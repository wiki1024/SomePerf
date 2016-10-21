using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Perf.Lib;

namespace BenchRunner
{
    public static class MagicSequence
    {
        static MagicSequence()
        {
            //Initialize();
        }

        public static void Initialize()
        {
            var bits = GenerateBits();
            //Count(bits);
            var context = new Context { Heading = 9, Normal = 9999, Reversed = 9999 };
            context.Builder.Append(99999);
            _created++;
            _remain--;
            bits[99999] = false;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (GenerateOne(bits, context))
            {
                //if ((_created % 10000) == 0)
                //{
                //    watch.Stop();
                //    Console.WriteLine("T:{0}, C:{1}, R:{2} (S:{3:0.000}s)", _total, _created, _remain, watch.Elapsed.TotalSeconds);
                //    watch.Start();
                //}
            }
            watch.Stop();
            Console.WriteLine("T:{0}, C:{1}, R:{2} (S:{3:0.000}s)", _total, _created, _remain, watch.Elapsed.TotalSeconds);
        }

        private const string TEMPLATE = @"
namespace TestStringFormatPerformance
{
    public class SmartCopyBase
    {
        protected const string Sequence = ""{1}"";
        protected static readonly int[] Pointers = 
            {2};
    }
}";

        private static bool GenerateOne(bool[] bits, Context context)
        {
            for (int level = 10; level <= 100000; level *= 10)
            {
                context.Heading = (context.Reversed / (level / 10)) % 10;
                //if (TryNormalPart(bits, context, level)) return true;
                if (TryMixPart(bits, context, level)) return true;
                ///if (TryReversePart(bits, context, level)) return true;
            }
            //Dump(bits);
            //StringBuilder builder = new StringBuilder(",");
            //for (int i = 0; i < bits.Length; i++)
            //{
            //    if (bits[i] == false) continue;
            //    builder.Append(i.ToString("00000") + ",");
            //}
            //String x = builder.ToString();
            //string n = context.Normal.ToString("0000");
            //string r = context.Reversed.ToString("0000");
            //for (int i = 1; i <= 4; i++)
            //{
            //    if (x.Contains("," + n.Substring(4 - i)) ||
            //        x.Contains(r.Substring(0, i) + ","))
            //    {
            //        Console.WriteLine("!" + n + "," + r + ":" + i);
            //    }
            //}
            //context.Heading.ToString()
            //Count(bits);
            //for (int i = 9, h = 90000; i >= 0; i--, h -= 10000)
            //{
            //    for (int j = 9990; j >= 0; j -= 10)
            //    {
            //        if (TryNormal(bits, context, 100000, h + j, h + j)) return true;
            //        for (int k = 9; k >= i; k--)
            //        {
            //            if (TryNormal(bits, context, 100000, h + j + k, h + j + k)) return true;
            //        }
            //    }
            //}
            //Console.WriteLine("H:{0}, N:{1}, R:{2}", context.Heading, context.Normal, context.Reversed);
            //Dump(bits);
            return false;
        }

        private static void Count(bool[] bits)
        {
            Console.WriteLine(bits.Count(item => item));
        }

        private static void Dump(bool[] bits)
        {
            int cnt = 0;
            //for (int i = 0, h = 0; i <= 9; i++, h += 10000)
            //{
            //    for (int j = 0; j <= 9990; j += 10)
            //    {
            //        if (bits[i + j]) { Console.Write((i + j) + ","); cnt++; if ((cnt & 7) == 0) Console.WriteLine(); }
            //        for (int k = i; k <= 9; k++)
            //        {
            //            if (bits[i + j + k]) { Console.Write((i + j + k) + ","); cnt++; if ((cnt & 7) == 0) Console.WriteLine(); }
            //        }
            //    }
            //}
            for (int i = 0; i < 100000; i++)
            {
                if (bits[i]) { Console.Write((i) + ","); cnt++; if ((cnt & 7) == 0) Console.WriteLine(); }
            }
            Console.WriteLine();
        }

        private static bool TryMixPart(bool[] bits, Context context, int level)
        {
            var enumeratorNormal = TryNormalPartMix(bits, context, level).GetEnumerator();
            var normalHasNext = enumeratorNormal.MoveNext();
            var lastNormal = enumeratorNormal.Current;
            var enumeratorReversed = TryReversePartMix(bits, context, level).GetEnumerator();
            var reversedHasNext = enumeratorReversed.MoveNext();
            var lastReversed = enumeratorReversed.Current;
            while (normalHasNext || reversedHasNext)
            {
                if (!normalHasNext)
                {
                    if (TryReverse(bits, context, level, lastReversed.Value, lastReversed.AppendingValue)) return true;
                    reversedHasNext = enumeratorReversed.MoveNext();
                    lastReversed = enumeratorReversed.Current;
                    continue;
                }
                if (!reversedHasNext || lastNormal.Value >= lastReversed.Value)
                {
                    if (TryNormal(bits, context, level, lastNormal.Value, lastNormal.AppendingValue)) return true;
                    normalHasNext = enumeratorNormal.MoveNext();
                    lastNormal = enumeratorNormal.Current;
                    if (reversedHasNext && lastNormal.Value == lastReversed.Value)
                    {
                        reversedHasNext = enumeratorReversed.MoveNext();
                        lastReversed = enumeratorReversed.Current;
                    }
                    continue;
                }
                if (TryReverse(bits, context, level, lastReversed.Value, lastReversed.AppendingValue)) return true;
                reversedHasNext = enumeratorReversed.MoveNext();
                lastReversed = enumeratorReversed.Current;
            }
            return false;
        }

        private class MixTesting
        {
            public int Value;
            public int AppendingValue;
        }

        private static IEnumerable<MixTesting> TryReversePartMix(bool[] bits, Context context, int level)
        {
            int power = (int)Math.Log10(level);
            int midLevel = (int)Math.Pow(10, 5 - power);
            int currentTail = context.Reversed * 10 / level;
            for (int head = (context.Heading == 0 ? 9 : context.Heading) * 10000; head >= 0; head -= 10000)
            {
                for (int mid = (level / 10 - 1) * midLevel; mid >= 0; mid -= midLevel)
                {
                    int h = head + mid;
                    yield return new MixTesting { Value = h + currentTail, AppendingValue = h };
                }
            }
        }

        private static IEnumerable<MixTesting> TryNormalPartMix(bool[] bits, Context context, int level)
        {
            int currentHead = (context.Normal * level) % 100000;
            for (int mid = level - 10; mid >= 0; mid -= 10)
            {
                for (int tail = 9; tail >= context.Heading; tail--)
                {
                    int t = mid + tail;
                    yield return new MixTesting { Value = currentHead + t, AppendingValue = t };
                }
                yield return new MixTesting { Value = currentHead + mid, AppendingValue = mid };
            }
        }

        //private static bool TryReversePart(bool[] bits, Context context, int level)
        //{
        //    int power = (int)Math.Log10(level);
        //    int midLevel = (int)Math.Pow(10, 5 - power);
        //    int currentTail = context.Reversed % midLevel;
        //    for (int head = (context.Heading - 1) * 10000; head >= 0; head -= 10000)
        //    {
        //        for (int mid = (level / 10 - 1) * midLevel; mid >= 0; mid -= midLevel)
        //        {
        //            int h = head + mid;
        //            int v = h + currentTail;
        //            if (TryReverse(bits, context, level, v, h)) return true;
        //        }
        //    }
        //    return false;
        //}

        //private static bool TryNormalPart(bool[] bits, Context context, int level)
        //{
        //    if (context.Heading == 9) return false;
        //    int currentHead = (context.Normal * level) % 100000;
        //    for (int mid = level - 10; mid >= 0; mid -= 10)
        //    {
        //        for (int tail = 9; tail > context.Heading; tail--)
        //        {
        //            int t = mid + tail;
        //            int v = currentHead + t;
        //            if (TryNormal(bits, context, level, v, t)) return true;
        //        }
        //        if (TryNormal(bits, context, level, currentHead + mid, mid)) return true;
        //    }
        //    return false;
        //}

        private static bool TryNormal(bool[] bits, Context context, int level, int v, int t)
        {
            if (bits[v] == false) return false;
            bits[v] = false;
            context.Builder.Append(t.ToString(level.ToString().Substring(1)));
            context.Normal = v % 10000;
            //context.Heading = context.Normal / 1000;
            context.Reversed = Reverse(context.Normal);
            _remain--;
            _created++;
            return true;
        }

        private static bool TryReverse(bool[] bits, Context context, int level, int v, int h)
        {
            if (bits[v] == false) return false;
            bits[v] = false;
            context.Builder.Append(Reverse(h).ToString(level.ToString().Substring(1)));
            context.Reversed = v / 10;
            //context.Heading = context.Reversed % 10;
            context.Normal = Reverse(context.Reversed);
            _remain--;
            _created++;
            return true;
        }

        private static int Reverse(int value)
        {
            int result = 0;
            for (int i = 0; i < 4; i++)
            {
                result *= 10;
                //if (value == 0) continue;
                int next = value / 10;
                result += value - next * 10;
                value = next;
            }
            return result;
        }

        private static bool[] GenerateBits()
        {
            var bits = new bool[100000];
            for (int i = 0, h = 0; i <= 9; i++, h += 10000)
            {
                for (int j = 0; j <= 9990; j += 10)
                {
                    if (i != 0)
                    {
                        bits[h + j] = true;
                        _total++;
                    }
                    for (int k = i; k <= 9; k++)
                    {
                        bits[h + j + k] = true;
                        _total++;
                    }
                }
            }
            _remain = _total;
            return bits;
        }

        private static int ToPointer(int currentPosition, bool reverse, int value)
        {
            int size = value == 0 ? 1 : (int)Math.Log10(value) + 1;
            if (reverse == false) currentPosition -= 5;
            return ((int)(reverse ? 0x80000000 : 0)) | (currentPosition << 3) | size;
        }

        public static string Sequence { get; private set; }

        private static int _total;
        private static int _remain;
        private static int _created;

        /*
        63157 -> [N:3157, R:7513]
        3157 [H:3, V: 3157] 31577-9 : 31578 -> [N:1578(H:1,V:1578), R:8751 -> (H:1,V:8751)
        7513 [H:3, V: 7513] 1-37513 : 27513 -> [N:1572(H:1,V:1572), R:2751 -> (H:1,V:2751)
        */

        private class Context
        {
            //public readonly char[] Buffer = new char[98304];
            //private int Offset;
            //public void Write(int value, int length)
            //{
            //    Buffer
            //}
            public StringBuilder Builder = new StringBuilder(200000);
            public int[] Pointers = new int[100000];
            public int Heading;
            public int Normal;
            public int Reversed;
        }
    }

    public class MultiThreadPerformanceTester<T> where T : ITextWriterUtils, new()
    {

        private const int _testValue = -1234567890;
        protected Thread[] _threads = new Thread[Environment.ProcessorCount];
        protected int _round;

        protected virtual string ExtraInfo { get { return null; } }

        public virtual void Test(int round)
        {
            _round = round;

            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i] = new Thread(TestCore);
            }

            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i].Start();
            }
            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i].Join();
            }
            watch.Stop();
            Console.WriteLine((round / watch.Elapsed.TotalSeconds) + " op/s [" + typeof(T).Name + "] Multi thread" + ExtraInfo);
        }

        protected virtual void TestCore()
        {
            var writer = new TextWriterAdapter(TextWriter.Null);
            var util = new T();
            int end = _round / _threads.Length;
            for (int i = 0; i < end; i++)
            {
                util.Write(writer, _testValue);
            }
        }
    }

    public class MultiThreadRandomPerformanceTester<T> : MultiThreadPerformanceTester<T> where T : ITextWriterUtils, new()
    {
        public MultiThreadRandomPerformanceTester(RandomSet randomSet)
        {
            _randomSet = randomSet;
        }

        protected readonly RandomSet _randomSet;

        protected override void TestCore()
        {
            var writer = new TextWriterAdapter(TextWriter.Null);
            var util = new T();
            int end = _round / _threads.Length;
            for (int i = 0; i < end; i++)
            {
                util.Write(writer, _randomSet.Get(i));
            }
        }

        protected override string ExtraInfo
        {
            get
            {
                return ", " + _randomSet.Name;
            }
        }
    }

    public class RandomSet
    {
        static RandomSet()
        {
            LargeNumbers = new RandomSet(int.MaxValue/200, int.MaxValue, "largeRnd");
            SmallNumbers = new RandomSet(-200000, 200000, "smallRnd");
        }

        public RandomSet(int min, int max, string name)
        {
            Name = name;
            var rnd = new Random();
            _numbers = Enumerable.Repeat(0, 2 << 20).Select(i => rnd.Next(min, max)).ToArray();
        }

        private readonly int[] _numbers;

        public int Get(int i)
        {
            return _numbers[i & 0xFFFFF];
        }

        public static RandomSet LargeNumbers { get; private set; }
        public static RandomSet SmallNumbers { get; private set; }

        public string Name { get; private set; }
    }

    public class MTRSBTester<T> : MultiThreadRandomPerformanceTester<T> where T : ITextWriterUtils, new()
    {

        public MTRSBTester(RandomSet randomSet) : base(randomSet) { }

        private const int MAX_ROUND = 1000000;
        private ConcurrentBag<StringBuilder> _stringBuilders = new ConcurrentBag<StringBuilder>();
        public override void Test(int round)
        {
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                _stringBuilders.Add(new StringBuilder(8000000));
            }
            base.Test(round);
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete();
        }

        protected override string ExtraInfo
        {
            get
            {
                return ", sb, " + _randomSet.Name;
            }
        }

        protected override void TestCore()
        {
            StringBuilder stringBuilder;
            _stringBuilders.TryTake(out stringBuilder);
            var writer = new TextWriterAdapter(new StringWriter(stringBuilder));
            var util = new T();
            int end = _round / _threads.Length;
            for (int i = 0; i < end;)
            {
                util.Write(writer, _randomSet.Get(i));
                i++;
                if ((i & 0xFFFF) == 0) stringBuilder.Length = 0;
            }
        }
    }

    public class SingleThreadRandomPerformanceTester<T> where T : ITextWriterUtils, new()
    {
        public void Test(int round, RandomSet randomSet)
        {
            var writer = new TextWriterAdapter(TextWriter.Null);
            var util = new T();
            // Cold start;            
            util.Write(writer, 0);

            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < round; i++)
            {
                util.Write(writer, randomSet.Get(i));
            }
            watch.Stop();
            Console.WriteLine((round / watch.Elapsed.TotalSeconds) + " op/s [" + typeof(T).Name + "] Single thread, " + randomSet.Name);
        }
    }
}
