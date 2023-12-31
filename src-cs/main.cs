#nullable disable
#pragma warning disable format, CS8981
using static Program; using System; using System.Collections; using System.Collections.Generic; using System.Diagnostics; using System.Globalization; using System.IO; using System.Linq; using System.Numerics; using System.Runtime.InteropServices; using System.Text; using System.Threading; using static System.Math; using MI = System.Runtime.CompilerServices.MethodImplAttribute; using bigint = System.Numerics.BigInteger;
#pragma warning restore format


public static partial class Program {
  public static readonly Random Rand = new Random(0);
  public const long TL = 3700;
  public static readonly Stopwatch Clock = new Stopwatch();
  [MI(256)] public static bool TimeCheck(out long time) => (time = Clock.ElapsedMilliseconds) < TL;

  public static int L, N, S;
  public static P[] Exits;
  public static int C, D;
  public static P[] Moves;

  [MI(512)]
  public static void main() {
    Clock.Start();

    L = cin; N = cin; S = cin;
    Exits = new P[N];
    for (int i = 0; i < N; i++) Exits[i] = (cin, cin);
    SetCD();

    Console.WriteLine($"# L={L}, N={N}, S={S}, D={D}");

    Moves = new P[] {
      new P(-1, 0) * D,
      new P(0, -1) * D,
      new P(1, 0) * D,
      new P(0, 1) * D
    };

    // 配置
    var placed = CreateTemperatures();
    JudgeIO.Place(L, placed);
    // 計測
    var measured_ard = MeasureAround();
    // 回答
    var ans = Solve(placed, measured_ard);
    JudgeIO.Answer(ans);
  }


  /// <summary>C と D の値を決める</summary>
  static void SetCD() {
    C = 10000 / N / 4;
    D = 4;
    if (L < 23) {
      if (S < 10) D = 1;
      else if (S < 200) D = 2;
      else if (S < 400) D = 3;
      else D = 4;
    } else if (L < 37) {
      if (S < 10) D = 1;
      else if (S < 200) D = 4;
      else if (S < 400) D = N < 80 ? 7 : 8;
      else D = N < 80 ? 7 : 8;
    } else {
      if (S < 10) D = N < 80 ? 1 : 2;
      else if (S < 200) D = 6;
      else if (S < 400) D = N < 80 ? 9 : 11;
      else D = 11;
    }

    if (D == 1) C = 10;
    if (D == 2) C = 20;
  }


  /// <summary>グリッドの中央を高温、周縁を低温にする</summary>
  /// <remarks>O(L^2)</remarks>
  static int[,] CreateTemperatures() {
    var res = new int[L, L];
    P center = new P(L / 2, L / 2);
    double diff_per_dist = 1000.0 / Sqrt(L * L / 2.0);

    for (int i = 0; i < L; i++) {
      for (int j = 0; j < L; j++) {
        var dist = center.DistE(new P(i, j));
        res[i, j] = (int)Round(1000 - diff_per_dist * dist);
      }
    }

    return res;
  }


  /// <summary>各出口から D マス離れた周囲を計測</summary>
  /// <remarks>O()</remarks>
  [MI(512)]
  static double[,] MeasureAround() {
    // 初期化
    var res = new double[N, Moves.Length];

    // 各入口について
    for (int i_in = 0; i_in < N; i_in++) {
      // それぞれの移動に
      for (int i_move = 0; i_move < Moves.Length; i_move++) {
        // C 回計測
        var measured = new int[C];
        for (int i_cnt = 0; i_cnt < C; i_cnt++) {
          measured[i_cnt] = JudgeIO.Measure(i_in, Moves[i_move]);
        }

        // 外れ値を上下 1 個捨てて平均を記録
        Array.Sort(measured);
        int sum = 0;
        for (int i = 1; i < C - 1; i++) sum += measured[i];
        res[i_in, i_move] = (double)sum / (C - 2);
      }
    }

    return res;
  }


  /// <summary>ある入口と出口を紐づけた時の誤差を計算</summary>
  /// <remarks>O(1)</remarks>
  [MI(256)]
  static double CalcDiff(int id_in, int id_out, int[,] placed, double[,] measured_ard) {
    double res = 0;
    for (int i_move = 0; i_move < Moves.Length; i_move++) {
      P moved_pos = Exits[id_out] + Moves[i_move];
      res += Abs(
        measured_ard[id_in, i_move]
        - placed[(moved_pos.Y + L) % L, (moved_pos.X + L) % L]
      );
    }
    return res;
  }


  /// <summary>初期解を構成する</summary>
  /// <remarks>O()</remarks>
  [MI(512)]
  static (int[] Ans, double Score) Init(int[,] placed, double[,] measured_ard) {
    var ans = new int[N];
    double score = 0;
    var used_out = new bool[N];

    // 各入口について
    for (int i_in = 0; i_in < N; i_in++) {
      double min_diff = INF64;
      // 各出口について
      for (int i_out = 0; i_out < N; i_out++) {
        if (used_out[i_out]) continue;
        // 紐づけたときの差が最小となる出口に紐づけ
        double diff = CalcDiff(i_in, i_out, placed, measured_ard);
        if (min_diff.ChMin(diff)) ans[i_in] = i_out;
      }
      used_out[ans[i_in]] = true;
      score += min_diff;
    }

    return (ans, score);
  }


  /// <summary>入口と出口の対応を 1 組入れ替える</summary>
  /// <remarks>O(1) たぶん重め</remarks>
  [MI(256)]
  static (int a, int b, double new_score) Modify(
    int[] ans, double score, int[,] placed, double[,] measured_ard
  ) {
    int a = Rand.Next(0, N), b = Rand.Next(0, N);
    double now_a = CalcDiff(a, ans[a], placed, measured_ard);
    double now_b = CalcDiff(b, ans[b], placed, measured_ard);
    Swap(ref ans[a], ref ans[b]);
    double next_a = CalcDiff(a, ans[a], placed, measured_ard);
    double next_b = CalcDiff(b, ans[b], placed, measured_ard);
    return (a, b, score + next_a + next_b - now_a - now_b);
  }


  /// <summary>Modify を元に戻す</summary>
  /// <remarks>O()</remarks>
  [MI(256)]
  private static void Undo(int[] ans, int a, int b) => Swap(ref ans[a], ref ans[b]);


  /// <summary>焼きなましによって差を最小化する</summary>
  /// <remarks>O()</remarks>
  [MI(512)]
  static int[] Solve(int[,] placed, double[,] measured_ard) {
    // スコアは小さいほどいい
    var (ans, score) = Init(placed, measured_ard);

    double temp_from = 4000, temp_to = 1;
    long start_time = Clock.ElapsedMilliseconds;
    int i = 0;

    while (TimeCheck(out long time)) {
      i++;
      var (a, b, new_score) = Modify(ans, score, placed, measured_ard);

      double temp =
        temp_from + (temp_to - temp_from) * (time - start_time) / (TL - start_time);

      double prob = Exp((score - new_score) / temp);

      if (prob <= Rand.NextDouble()) {
        Undo(ans, a, b);
      }
    }

    Console.WriteLine($"# looped {i} times");
    return ans;
  }

}


public readonly struct P : IEquatable<P> {
  public readonly int Y, X;
  [MI(256)] public P(int x, int y) { Y = x; X = y; }
  [MI(256)] public static implicit operator P((int X, int Y) t) => new P(t.X, t.Y);
  [MI(256)] public static P operator +(P a, P b) => new P(a.Y + b.Y, a.X + b.X);
  [MI(256)] public static P operator -(P a, P b) => new P(a.Y - b.Y, a.X - b.X);
  [MI(256)] public static P operator *(P a, int b) => new P(a.Y * b, a.X * b);
  [MI(256)] public static P operator /(P a, int b) => new P(a.Y / b, a.X / b);
  [MI(256)] public static bool operator ==(P a, P b) => a.Equals(b);
  [MI(256)] public static bool operator !=(P a, P b) => !a.Equals(b);
  [MI(256)] public readonly double DistE(P p) { double dy = (double)Y - p.Y, dx = (double)X - p.X; return Math.Sqrt(dx * dx + dy * dy); }
  [MI(256)] public readonly long DistE2(P p) { long dy = (long)Y - p.Y, dx = (long)X - p.X; return dx * dx + dy * dy; }
  [MI(256)] public readonly long DistM(P p) => Math.Abs((long)Y - p.Y) + Math.Abs((long)X - p.X);
  [MI(256)] public override readonly string ToString() => Y.ToString() + " " + X.ToString();
  [MI(256)] public readonly bool Equals(P b) => this.Y == b.Y && this.X == b.X;
  [MI(256)] public override readonly bool Equals(object o) => base.Equals(o);
  [MI(256)] public override readonly int GetHashCode() => HashCode.Combine(this.X, this.Y);
}


static class JudgeIO {
  [MI(512)]
  public static void Place(int l, int[,] temperature) {
    for (int i = 0; i < l; i++) {
      for (int j = 0; j < l; j++) {
        Console.Write(temperature[i, j]);
        if (j < l - 1) Console.Write(' ');
      }
      Console.WriteLine();
    }
    cout.Flush();
  }

  [MI(256)]
  public static int Measure(int i, P p) {
    Console.WriteLine($"{i} {p.Y} {p.X}");
    cout.Flush();
    int v = cin;
    if (v == -1) {
      Console.WriteLine($"# [WA] Received -1 (i={i}, y={p.Y}, x={p.X})");
      Environment.Exit(1);
    }
    return v;
  }

  public static void Answer(int[] ans) {
    Console.WriteLine("-1 -1 -1");
    Console.WriteLine(string.Join('\n', ans));
    cout.Flush();
  }
}



#region library
#pragma warning disable
public static partial class Program {
#if DEBUG
  public const bool DEBUG = true;
#else
  public const bool DEBUG = false;
#endif
  public const bool MANY_RECURSIONS = false;
  public const int INF32 = (1 << 30) - 1;
  public const long INF64 = 1L << 60;
  public static readonly CIn cin = new CIn(Console.OpenStandardInput());
  public static readonly COut cout = new COut(Console.OpenStandardOutput()) { AutoFlush = DEBUG ? false : false };
  public static readonly COut cerr = new COut(Console.OpenStandardError()) { AutoFlush = true };
  public static void Main() { Console.SetOut(cout); if (MANY_RECURSIONS) { var t = new Thread(main, 134217728); t.Start(); t.Join(); } else main(); cout.Flush(); }
  [MI(256)] public static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);
  [MI(256)] public static bool Change<T>(this ref T a, T b) where T : struct, IEquatable<T> { if (a.Equals(b)) return false; else { a = b; return true; } }
  [MI(256)] public static bool ChMax<T>(this ref T a, T b) where T : struct, IComparable { if (a.CompareTo(b) < 0) { a = b; return true; } return false; }
  [MI(256)] public static int ChMax<T>(this ref T a, params T[] others) where T : struct, IComparable { int idx = -1; for (int i = 0; i < others.Length; i++) if (a.ChMax(others[i])) idx = i; return idx; }
  [MI(256)] public static bool ChMin<T>(this ref T a, T b) where T : struct, IComparable { if (a.CompareTo(b) > 0) { a = b; return true; } return false; }
  [MI(256)] public static int ChMin<T>(this ref T a, params T[] others) where T : struct, IComparable { int idx = -1; for (int i = 0; i < others.Length; i++) if (a.ChMin(others[i])) idx = i; return idx; }
  [MI(256)] public static long nCr(this long n, int r) { if (r < 0 || n < r) return 0; long x = 1; for (int i = 0; i < r; i++) x *= n - i; for (int i = 1; i <= r; i++) x /= i; return x; }
  [MI(256)] public static long nPr(this long n, int r) { if (r < 0 || n < r) return 0; long x = 1; while (n > r) x *= n--; return x; }
  [MI(256)] public static long PowN(this long x, long exponent) { long res = 1; while (exponent > 0) { if ((exponent & 1) == 1) res *= x; x *= x; exponent >>= 1; } return res; }
  [MI(256)] public static int DivCeil(this int dividend, int divisor) => (dividend + divisor - 1) / divisor;
  [MI(256)] public static long DivCeil(this long dividend, long divisor) => (dividend + divisor - 1) / divisor;
  [MI(256)] public static int RangeSum(int min, int max) => (max - min + 1) * (min + max) / 2;
  [MI(256)] public static long RangeSum(long min, long max) => (max - min + 1) * (min + max) / 2;
  [MI(256)] public static long Gcd(long a, long b) { while (b != 0) (a, b) = (b, a % b); return a; }
  [MI(256)] public static long Lcm(long a, long b) => a / Gcd(a, b) * b;
  [MI(256)] public static Span<T> AsSpan<T>(this T[,] array) => MemoryMarshal.CreateSpan(ref array[0, 0], array.Length);
  [MI(256)] public static Span<T> AsSpan<T>(this T[,,] array) => MemoryMarshal.CreateSpan(ref array[0, 0, 0], array.Length);
  [MI(256)] public static void Fill<T>(this T[,] array, T value) => array.AsSpan().Fill(value);
  [MI(256)] public static void Fill<T>(this T[,,] array, T value) => array.AsSpan().Fill(value);
  [MI(256)] public static void Fill<T>(this IList<IList<T>> list, int length0, int length1, T v) { for (int i = 0; i < length0; ++i) for (int j = 0; j < length1; ++j) list[i][j] = v; }
  [MI(256)] public static void Fill<T>(this IList<IList<IList<T>>> list, int length0, int length1, int length2, T value) { for (int i = 0; i < length0; ++i) for (int j = 0; j < length1; ++j) for (int k = 0; k < length2; ++k) list[i][j][k] = value; }
  [MI(256)] public static void Init<T>(this T[,] array, Func<int, int, T> generator) { int x = array.GetLength(0), y = array.GetLength(1); for (int i = 0; i < x; ++i) for (int j = 0; j < y; ++j) array[i, j] = generator(i, j); }
  [MI(256)] public static void Init<T>(this IList<IList<T>> list, int length0, int length1, Func<int, int, T> generator) { for (int i = 0; i < length0; ++i) for (int j = 0; j < length1; ++j) list[i][j] = generator(i, j); }
  [MI(256)] public static void Init<T>(this T[,,] array, int length0, int length1, int length2, Func<int, int, int, T> generator) { for (int i = 0; i < length0; ++i) for (int j = 0; j < length1; ++j) for (int k = 0; k < length2; ++k) array[i, j, k] = generator(i, j, k); }
  [MI(256)] public static void Init<T>(this IList<IList<IList<T>>> list, int length0, int length1, int length2, Func<int, int, int, T> generator) { for (int i = 0; i < length0; ++i) for (int j = 0; j < length1; ++j) for (int k = 0; k < length2; ++k) list[i][j][k] = generator(i, j, k); }
  [MI(256)] public static T[,] Transpose<T>(this T[,] array) { var res = new T[array.GetLength(1), array.GetLength(0)]; for (int i = 0; i < array.GetLength(1); i++) for (int j = 0; j < array.GetLength(0); j++) res[i, j] = array[j, i]; return res; }
  [MI(256)] public static T[,] Transpose<T>(this IList<IList<T>> list) { var res = new T[list[0].Count, list.Count]; for (int i = 0; i < list[0].Count; i++) for (int j = 0; j < list.Count; j++) res[i, j] = list[j][i]; return res; }
  [MI(256)] public static Dictionary<V, int> CountBy<K, V>(this IEnumerable<K> seq, Func<K, V> func) where V : notnull { var dict = new Dictionary<V, int>(); foreach (var item in seq) { var key = func(item); dict[key] = dict.TryGetValue(key, out int count) ? count + 1 : 1; } return dict; }
  [MI(256)] public static IEnumerable<R> Scan<S, R>(this IEnumerable<S> seq, R defaultValue, Func<R, S, R> func) { yield return defaultValue; foreach (var item in seq) { defaultValue = func(defaultValue, item); yield return defaultValue; } }
  [MI(256)] public static int BinarySearch(int good, int bad, Predicate<int> condition) { while (Abs(bad - good) > 1) { var mid = (good + bad) >> 1; if (condition(mid)) good = mid; else bad = mid; } return good; }
  [MI(256)] public static int BinarySearch(int good, int bad, Func<int, int, int, bool> condition) { while (Abs(bad - good) > 1) { var mid = (good + bad) >> 1; if (condition(good, mid, bad)) good = mid; else bad = mid; } return good; }
  [MI(256)] public static long BinarySearch(long good, long bad, Predicate<long> condition) { while (Abs(bad - good) > 1) { var mid = (good + bad) >> 1; if (condition(mid)) good = mid; else bad = mid; } return good; }
  [MI(256)] public static long BinarySearch(long good, long bad, Func<long, long, long, bool> condition) { while (Abs(bad - good) > 1) { var mid = (good + bad) >> 1; if (condition(good, mid, bad)) good = mid; else bad = mid; } return good; }
  [MI(256)] public static double BinarySearch(double good, double bad, double precision, Predicate<double> condition) { while (Abs(bad - good) > precision) { var mid = (good + bad) / 2; if (condition(mid)) good = mid; else bad = mid; } return good; }
  [MI(256)] public static double BinarySearch(double good, double bad, double precision, Func<double, double, double, bool> condition) { while (Abs(bad - good) > precision) { var mid = (good + bad) / 2; if (condition(good, mid, bad)) good = mid; else bad = mid; } return good; }
  [MI(256)] public static T PopLast<T>(this List<T> list) { T res = list[^1]; list.RemoveAt(list.Count - 1); return res; }
  [MI(256)] public static IEnumerable<T> PopRange<T>(this List<T> list, Range rng) { var (off, len) = rng.GetOffsetAndLength(list.Count); var res = list.GetRange(off, len); list.RemoveRange(off, len); return res; }
  public static P[] Dir4 { [MI(256)] get => new[] { new P(-1, 0), new P(0, -1), new P(1, 0), new P(0, 1) }; }
  public static P[] Dir8 { [MI(256)] get => new[] { new P(-1, 0), new P(-1, -1), new P(0, -1), new P(1, -1), new P(1, 0), new P(1, 1), new P(0, 1), new P(-1, 1) }; }
}

public class CIn {
  public CIn(Stream stream) { _stream = stream; }
  readonly Stream _stream; readonly byte[] _buffer = new byte[1024]; int _len, _ptr; bool _end; public bool End { [MI(256)] get => _end; }
  [MI(256)] byte R() { if (_end) throw new EndOfStreamException(); if (_ptr >= _len) { _ptr = 0; if ((_len = _stream.Read(_buffer, 0, 1024)) <= 0) { _end = true; return 0; } } return _buffer[_ptr++]; }
  public char c { [MI(256)] get { byte b; do b = R(); while (b < 33 || 126 < b); return (char)b; } }
  public string s { [MI(256)] get { var sb = new StringBuilder(); for (char b = c; 33 <= b && b <= 126; b = (char)R()) sb.Append(b); return sb.ToString(); } }
  public long l { [MI(256)] get { long res = 0; byte b; var negative = false; do b = R(); while (b != '-' && (b < '0' || '9' < b)); if (b == '-') { negative = true; b = R(); } for (; true; b = R()) { if (b < '0' || '9' < b) return negative ? -res : res; else res = res * 10 + b - '0'; } } }
  public ulong ul { [MI(256)] get { ulong res = 0; byte b; do b = R(); while (b < '0' || '9' < b); for (; true; b = R()) { if (b < '0' || '9' < b) return res; else res = res * 10 + b - '0'; } } }
  public int i { [MI(256)] get => (int)this.l; }
  public uint u { [MI(256)] get => (uint)this.ul; }
  public float f { [MI(256)] get => float.Parse(this.s, CultureInfo.InvariantCulture); }
  public double d { [MI(256)] get => double.Parse(this.s, CultureInfo.InvariantCulture); }
  public decimal m { [MI(256)] get => decimal.Parse(this.s, CultureInfo.InvariantCulture); }
  public bigint b { [MI(256)] get => bigint.Parse(this.s, CultureInfo.InvariantCulture); }
  [MI(256)] public static implicit operator char(CIn cin) => cin.c;
  [MI(256)] public static implicit operator string(CIn cin) => cin.s;
  [MI(256)] public static implicit operator long(CIn cin) => cin.l;
  [MI(256)] public static implicit operator int(CIn cin) => (int)cin.l;
  [MI(256)] public static implicit operator ulong(CIn cin) => cin.ul;
  [MI(256)] public static implicit operator uint(CIn cin) => (uint)cin.ul;
  [MI(256)] public static implicit operator float(CIn cin) => cin.f;
  [MI(256)] public static implicit operator double(CIn cin) => cin.d;
  [MI(256)] public static implicit operator decimal(CIn cin) => cin.m;
  [MI(256)] public static implicit operator bigint(CIn cin) => cin.b;
}

public class COut : StreamWriter {
  public override IFormatProvider FormatProvider { [MI(256)] get => CultureInfo.InvariantCulture; }
  public COut(Stream stream) : base(stream, new UTF8Encoding(false, true)) { }
  public COut(Stream stream, Encoding encoding) : base(stream, encoding) { }
  [MI(256)] public void Print2D(IEnumerable<IEnumerable<char>> seq) { foreach (var x in seq) WriteLine(string.Concat(x)); }
  [MI(256)] public void Print2D<T>(IEnumerable<IEnumerable<T>> seq) { foreach (var x in seq) WriteLine(string.Join(' ', x)); }
  [MI(256)] public void Print2D(char[,] array) { var span = MemoryMarshal.CreateReadOnlySpan(ref array[0, 0], array.Length); int h = array.GetLength(0), w = array.GetLength(1); for (int i = 0; i < h; i++) WriteLine(new string(span[(i * w)..((i + 1) * w)])); }
  [MI(256)] public void Print2D<T>(T[,] array) { int h = array.GetLength(0), w = array.GetLength(1); for (int i = 0; i < h; i++) { var sb = new StringBuilder(2 * w); for (int j = 0; j < w; j++) { sb.Append(array[i, j]); if (j < w - 1) sb.Append(' '); } WriteLine(sb); } }
}

public class LowerBound<T> : IComparer<T> where T : IComparable<T> { [MI(256)] public int Compare(T x, T y) => 0 <= x!.CompareTo(y) ? 1 : -1; }
public class UpperBound<T> : IComparer<T> where T : IComparable<T> { [MI(256)] public int Compare(T x, T y) => 0 < x!.CompareTo(y) ? 1 : -1; }

/// <remarks>デフォルトは小さい順</remarks>
class PriorityQueue_2<T> : PriorityQueueOp<T, ComparableComparer<T>>
  where T : IComparable<T> {
  [MI(256)] public PriorityQueue_2() : base(new ComparableComparer<T>()) { }
  [MI(256)] public PriorityQueue_2(int capacity) : base(capacity, new ComparableComparer<T>()) { }
  [MI(256)] public PriorityQueue_2(IComparer<T> comparer) : base(new ComparableComparer<T>(comparer)) { }
  [MI(256)] public PriorityQueue_2(int capacity, IComparer<T> comparer) : base(capacity, new ComparableComparer<T>(comparer)) { }
}

class PriorityQueueOp<T, TOp> : IPriorityQueueOp<T>
  where TOp : IComparer<T> {
  protected T[] data;
  protected readonly TOp _comparer;
  internal const int DefaultCapacity = 16;
  [MI(256)] public PriorityQueueOp() : this(default(TOp)) { }
  [MI(256)] public PriorityQueueOp(int capacity) : this(capacity, default(TOp)) { }
  [MI(256)] public PriorityQueueOp(TOp comparer) : this(DefaultCapacity, comparer) { }
  [MI(256)] public PriorityQueueOp(int capacity, TOp comparer) { if (comparer == null) throw new ArgumentNullException(nameof(comparer)); data = new T[Math.Max(capacity, DefaultCapacity)]; _comparer = comparer; }
  [DebuggerBrowsable(0)] public int Count { [MI(256)] get; [MI(256)] private set; } = 0;
  public T Peek => data[0];
  [MI(256)] internal void Resize() { Array.Resize(ref data, data.Length << 1); }
  [MI(256)] public void Enqueue(T value) { if (Count >= data.Length) Resize(); data[Count++] = value; UpdateUp(Count - 1); }
  [MI(256)] public bool TryDequeue(out T result) { if (Count == 0) { result = default(T); return false; } result = Dequeue(); return true; }
  [MI(256)] public T Dequeue() { var res = data[0]; data[0] = data[--Count]; UpdateDown(0); return res; }
  /// <summary><paramref name="value"/> を Enqueue(T) してから Dequeue します。</summary>
  [MI(256)] public T EnqueueDequeue(T value) { var res = data[0]; if (_comparer.Compare(value, res) <= 0) return value; data[0] = value; UpdateDown(0); return res; }
  /// <summary>Dequeue した値に <paramref name="func"/> を適用して Enqueue(T) します。</summary>
  [MI(256)] public void DequeueEnqueue(Func<T, T> func) { data[0] = func(data[0]); UpdateDown(0); }

  [MI(256)]
  protected internal void UpdateUp(int i) {
    var tar = data[i];
    while (i > 0) {
      var p = (i - 1) >> 1;
      if (_comparer.Compare(tar, data[p]) >= 0) break;
      data[i] = data[p]; i = p;
    }
    data[i] = tar;
  }

  [MI(256)]
  protected internal void UpdateDown(int i) {
    var tar = data[i];
    int n = Count, child = 2 * i + 1;
    while (child < n) {
      if (child != n - 1 &&
        _comparer.Compare(data[child], data[child + 1]) > 0) child++;
      if (_comparer.Compare(tar, data[child]) <= 0) break;
      data[i] = data[child]; i = child; child = 2 * i + 1;
    }
    data[i] = tar;
  }

  [MI(256)] public void Clear() => Count = 0;
  [MI(256)] public ReadOnlySpan<T> Unorderd() => data.AsSpan(0, Count);
  private class DebugView {
    private readonly PriorityQueueOp<T, TOp> pq;
    [MI(256)] public DebugView(PriorityQueueOp<T, TOp> pq) { this.pq = pq; }
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)] public T[] Items { [MI(256)] get { var arr = pq.Unorderd().ToArray(); Array.Sort(arr, pq._comparer); return arr; } }
  }
}

interface IPriorityQueueOp<T> {
  int Count { [MI(256)] get; }
  T Peek { [MI(256)] get; }
  void Enqueue(T value);
  T Dequeue();
  bool TryDequeue(out T result);
  void Clear();
}

struct ComparableComparer<T> : IComparer<T>, IEquatable<ComparableComparer<T>> where T : IComparable<T> {
  private IComparer<T> Comparer { [MI(256)] get; }
  [MI(256)] public ComparableComparer(IComparer<T> comparer) { Comparer = comparer; }
  [MI(256)] public int Compare(T x, T y) => Comparer?.Compare(x, y) ?? x.CompareTo(y);
  #region Equatable
  [MI(256)] public override bool Equals(object obj) => obj is ComparableComparer<T> && Equals((ComparableComparer<T>)obj);
  [MI(256)] public bool Equals(ComparableComparer<T> other) { if (Comparer == null) return other.Comparer == null; return Comparer.Equals(other.Comparer); }
  [MI(256)] public override int GetHashCode() => Comparer?.GetHashCode() ?? 0;
  [MI(256)] public static bool operator ==(ComparableComparer<T> left, ComparableComparer<T> right) => left.Equals(right);
  [MI(256)] public static bool operator !=(ComparableComparer<T> left, ComparableComparer<T> right) => !left.Equals(right);
  #endregion Equatable
}

#endregion
