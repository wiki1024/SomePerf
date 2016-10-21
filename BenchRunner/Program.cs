using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Threading;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Perf.Lib;
using Perf.Lib.blocks;
using Perf.Lib.buffers;
using RPackage.Light;

namespace BenchRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            //var pool = new MemoryPool();
            // var pdfStream = new PDFStream(pool);
            //var source = File.ReadAllBytes(@"D:\dummy\hhuh103.txt");
            //pdfStream.Write(source, 0, source.Length);
            //   var formData = new List<Tuple<string, string>> { Tuple.Create("FullName", "Hehejjj") };
            //var path = @"C:\FormsAutomation\FormsDevelopmentTemplate\community-home-purchase.pdf";// @"D:\dummy\jjj.pdf";//
            //var source = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
            //pdfStream.Write(source, 0, 8);
            //pdfStream.Write(source, 0, source.Length);
            //var copy = new byte[66];
            //pdfStream.Read(copy,0,copy.Length);
            //DirectoryInfo templeteDir = new DirectoryInfo(@"C:\FormsAutomation\FormsDevelopmentTemplate");
            //var files = from f in templeteDir.EnumerateFiles()
            //            where f.Extension == ".pdf"
            //            orderby f.Length ascending
            //            select f;
            //foreach (var file in files)
            //{
            //var pdfStream = new PDFStream(pool);
            ////    Console.WriteLine(file.FullName);
            ////    Thread.Sleep(1000);
            //PDFFiller.FillSinglePDF(path, pdfStream, formData);

            //using (FileStream fs = File.Create(@"D:\dummy\hehejjj.txt"))
            //{
            //    pdfStream.CopyTo(fs);
            //}
            //pdfStream.Dispose();
            // }

            // TestPDFStream();

            //pdfStream.Dispose();
            //pool.Dispose();
            //var summary = BenchmarkRunner.Run<TestPDFStream>();
            // var summary = BenchmarkRunner.Run<TestDateTimeToString>();
            // var summary = BenchmarkRunner.Run<TestTextWritter>();
            //new MultiThreadLockPerformanceTester<MyFineLockCalculator>().Run(2);
            // new MultiThreadLockPerformanceTester<BigLockCalculator>().Run(10);
            // var hehe= new Char11Pool(10);
            //var ttt = new TestTextWritter();
            //ttt.SetupData();
            //ttt.ExtremWriter();
            //var hh = new HeheWriter();
            //int start;
            //int count;
            //var result = hh.GetCharResult(12345678, out start, out count);
            //var testString = new TestDateTimeToString();
            //Console.WriteLine(testString.NormalToString());
            //Console.WriteLine(testString.PointerToString());
            //var now = DateTime.Now;
            //Parallel.ForEach(Enumerable.Range(0, 10), new ParallelOptions { MaxDegreeOfParallelism = 4 }, (i) =>
            //  {
            //      for (int j = 0; j < 500000; j++)
            //      {
            //        // var s = now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff", CultureInfo.InvariantCulture);
            //        var s = CustomFormatter.GetDefaultFormat(now);
            //        //Console.WriteLine(j);
            //    }
            //  });
            //var t = new TestCopy();
            //var result = t.MemCopy();
            //for (int i = 0; i < 2000; i++)
            //{
            //    var result = t.MemCopy();
            //}
            //LetsFight();
            // var person = new Person {Name = "hehe"};
            //Func<string> fff = person.Funcc;
            TestPDFStream();

            Console.WriteLine("Done");
            Console.ReadLine();

        }

        static void TestPDFStream()
        {
            var formData = new List<Tuple<string, string>> { Tuple.Create("FullName", "Hehejjj") };
            var _oldFormData = new FormData { Fields = new Dictionary<string, string> { { "FullName", "Hehe" } } };
            var _fileFiller = new FileFiller();
            DirectoryInfo templeteDir = new DirectoryInfo(@"C:\FormsAutomation\FormsDevelopmentTemplate");
            var files = from f in templeteDir.EnumerateFiles()
                        where f.Extension == ".pdf"
                        orderby f.Length ascending
                        select f;
            //var watch = new Stopwatch();
            //watch.Start();
            Parallel.ForEach(Enumerable.Range(1, 100), new ParallelOptions { MaxDegreeOfParallelism = 4 }, (i) =>
            {

                using (FileStream fs = File.Create(string.Format(@"D:\dummy\zip\jjj_{0}.zip", i.ToString())))
                {

                    PDFFiller.FillManyToZipStream(files.Select(f => f.FullName), formData, fs);
                   // _fileFiller.FillManyToZipStream(files.Select(f => f.FullName), _oldFormData, fs);
                }
                // bytes.Dispose();
                Console.WriteLine(i);
            });
            //watch.Stop();
            //Console.WriteLine(watch.Elapsed.TotalSeconds);
            //using (FileStream fs = File.Create(string.Format(@"D:\dummy\zip\jjj_{0}.zip", 1)))
            //{

            //    PDFFiller.FillManyToZipStream(files.Select(f => f.FullName), formData, fs);
            //}
        }
        static void LetsFight()
        {
            new CorrectnessTester<SampletTextWriterUtils>().Test();
            new CorrectnessTester<HeheWriter>().Test();
            new CorrectnessTester<ExtremeTextWriterUtils>().Test();


            int round = 10000000;

            //new SingleThreadPerformanceTester<SampletTextWriterUtils>().Test(round);
            //new SingleThreadPerformanceTester<HeheWriter>().Test(round);
            //new SingleThreadPerformanceTester<ExtremeTextWriterUtils>().Test(round);
            //Console.WriteLine("-------\n");

            //new MultiThreadPerformanceTester<SampletTextWriterUtils>().Test(round);
            //new MultiThreadPerformanceTester<HeheWriter>().Test(round);
            //new MultiThreadPerformanceTester<ExtremeTextWriterUtils>().Test(round);

            //Console.WriteLine("-------\n");

            Test(round, RandomSet.SmallNumbers);
            Test(round, RandomSet.LargeNumbers);




            Console.WriteLine("DONE!");

        }

        private static void Test(int round, RandomSet randomSet)
        {
            new SingleThreadRandomPerformanceTester<SampletTextWriterUtils>().Test(round, randomSet);
            new SingleThreadRandomPerformanceTester<HeheWriter>().Test(round, randomSet);
            new SingleThreadRandomPerformanceTester<ExtremeTextWriterUtils>().Test(round, randomSet);
            Console.WriteLine("-------\n");

            new MultiThreadRandomPerformanceTester<SampletTextWriterUtils>(randomSet).Test(round);
            new MultiThreadRandomPerformanceTester<HeheWriter>(randomSet).Test(round);
            new MultiThreadRandomPerformanceTester<ExtremeTextWriterUtils>(randomSet).Test(round);
            Console.WriteLine("-------\n");

            new MTRSBTester<ExtremeTextWriterUtils>(randomSet).Test(round);
            new MTRSBTester<HeheWriter>(randomSet).Test(round);
            new MTRSBTester<SampletTextWriterUtils>(randomSet).Test(round);
            Console.WriteLine("-------\n");
        }
    }

    public class SingleThreadPerformanceTester<T> where T : ITextWriterUtils, new()
    {
        private const int _testValue = -1234567890;

        public void Test(int round)
        {
            var writer = new TextWriterAdapter(TextWriter.Null);
            var util = new T();
            // Cold start;            
            util.Write(writer, _testValue);

            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < round; i++)
            {
                util.Write(writer, _testValue);
            }
            watch.Stop();
            Console.WriteLine((round / watch.Elapsed.TotalSeconds) + " op/s [" + typeof(T).Name + "] Single thread");
        }
    }

    public class CorrectnessTester<T> where T : ITextWriterUtils, new()
    {

        public void Test()
        {
            Test(0);
            Test(int.MinValue);
            Test(int.MaxValue);
            Test("1234567890", 1);
            Test("-1234567890", 2);
        }

        private void Test(string test, int startFrom)
        {
            for (var i = startFrom; i < test.Length; i++)
            {
                Test(int.Parse(test.Substring(0, i)));
            }
        }

        public void Test(int number)
        {
            var correctString = number.ToString(CultureInfo.InvariantCulture);
            var writer = new StringWriter();
            var writerAdapter = new TextWriterAdapter(writer);
            var util = new T();
            util.Write(writerAdapter, number);
            var utilString = writer.ToString();
            var correct = correctString == utilString;
            Console.WriteLine(
                "[" + typeof(T).Name + "] " + correctString + " is " +
                (correct
                    ? "correct"
                    : ("in correct (" + utilString + ")")
                )
            );
        }
    }

     struct bucket
    {
        public Object key;
        public Object val;
        public int hash_coll;   // Store hash code; sign bit means there was a collision.
    }

    class Person
    {
        public string Name { get; set; }

        public string Funcc()
        {
            return this.Name;
        }

    }

}
