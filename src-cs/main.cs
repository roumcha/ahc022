#nullable disable
#pragma warning disable format, CS8981
using static Program; using System; using System.Collections; using System.Collections.Generic; using System.Diagnostics; using System.Globalization; using System.IO; using System.Linq; using System.Numerics; using System.Runtime.InteropServices; using System.Text; using System.Threading; using static System.Math; using MI = System.Runtime.CompilerServices.MethodImplAttribute; using bigint = System.Numerics.BigInteger;
#pragma warning restore format


public static partial class Program {
  public static void main() {
    int l = cin, n = cin, s = cin;
    var exits = new P[n];
    for (int i = 0; i < n; i++) exits[i] = (cin, cin);
    var solver = new Solver(l, n, s, exits);
    solver.Solve();
  }
}


public readonly struct Solver {
  public readonly int L, N, S;
  public readonly P[] Exits;

  public Solver(int l, int n, int s, P[] exits) {
    L = l; N = n; S = s; Exits = exits;
  }


  [MI(512)]
  public void Solve() {
    var temperatures = this.CreateTemperatures();
    JudgeIO.Place(L, temperatures);
    var estimates = this.Predict(temperatures);
    JudgeIO.Answer(estimates);
  }


  private readonly int[,] CreateTemperatures() {
    var temperatures = new int[L, L];
    for (int i = 0; i < N; i++) {
      var exit = Exits[i];
      temperatures[exit.Y, exit.X] = i * 10;

      // 周辺 9 マスを同じ温度にする
      foreach (var dir in Dir8) {
        var p = exit + dir;
        temperatures[(p.Y + L) % L, (p.X + L) % L] = i * 10;
      }
    }
    return temperatures;
  }


  [MI(512)]
  private int[] Predict(int[,] temperatures) {
    var estimates = new int[N];

    for (int i_in = 0; i_in < N; i_in++) {
      // 周辺 9 マスを計測（外れ値の影響を小さくするため、ルートを取って平均計算）
      const double root = 3;
      double v = Pow(JudgeIO.Measure(i_in, (0, 0)), 1.0 / root);
      foreach (var dir in Dir8) v += Pow(JudgeIO.Measure(i_in, dir), 1.0 / root);
      v = Pow(v / 9, root);

      // 誤差最小の出口に紐づけ
      double min_diff = 9999;
      for (int i_out = 0; i_out < N; i_out++) {
        int y = Exits[i_out].Y, x = Exits[i_out].X;

        if (min_diff.ChMin(Abs(temperatures[y, x] - v))) {
          estimates[i_in] = i_out;
        }
      }

      Console.WriteLine($"# measure in={i_in}, out={estimates[i_in]}, v={v}");
    }

    return estimates;
  }

}


public readonly struct Torus {
  private readonly int _len1, _len2;
  private readonly int[,] _values;

  [MI(256)]
  public Torus(int len1, int len2) {
    _len1 = len1;
    _len2 = len2;
    _values = new int[len1, len2];
  }

  public readonly int this[int i, int j] {
    [MI(256)]
    get => _values[((i % _len1) + _len1) % _len1, ((j % _len2) + _len2) % _len2];
    [MI(256)]
    set => _values[((i % _len1) + _len1) % _len1, ((j % _len2) + _len2) % _len2] = value;
  }
}


public readonly struct P : IEquatable<P> {
  public readonly int Y, X;
  [MI(256)] public P(int x, int y) { Y = x; X = y; }
  [MI(256)] public static implicit operator P((int X, int Y) t) => new P(t.X, t.Y);
  [MI(256)] public static P operator +(in P a, in P b) => new P(a.Y + b.Y, a.X + b.X);
  [MI(256)] public static P operator -(in P a, in P b) => new P(a.Y - b.Y, a.X - b.X);
  [MI(256)] public static P operator *(in P a, int b) => new P(a.Y * b, a.X * b);
  [MI(256)] public static P operator /(in P a, int b) => new P(a.Y / b, a.X / b);
  [MI(256)] public static bool operator ==(in P a, in P b) => a.Equals(b);
  [MI(256)] public static bool operator !=(in P a, in P b) => !a.Equals(b);
  [MI(256)] public readonly double DistE(in P p) { double dy = (double)Y - p.Y, dx = (double)X - p.X; return Math.Sqrt(dx * dx + dy * dy); }
  [MI(256)] public readonly long DistE2(in P p) { long dy = (long)Y - p.Y, dx = (long)X - p.X; return dx * dx + dy * dy; }
  [MI(256)] public readonly long DistM(in P p) => Math.Abs((long)Y - p.Y) + Math.Abs((long)X - p.X);
  [MI(256)] public override readonly string ToString() => Y.ToString() + " " + X.ToString();
  [MI(256)] public readonly bool Equals(P b) => this.Y == b.Y && this.X == b.X;
  [MI(256)] public override readonly bool Equals(object o) => base.Equals(o);
  [MI(256)] public override readonly int GetHashCode() => HashCode.Combine(this.X, this.Y);
}


static class JudgeIO {
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
  public static int Measure(int i, in P p) {
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
