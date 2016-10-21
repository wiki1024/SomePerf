using System;
using System.Globalization;
using System.IO;
using BenchmarkDotNet.Attributes;
using Perf.Lib;

namespace BenchRunner
{
    public class TestTextWritter
    {
        private TextWriterAdapter _writerAdapter;
        private Random _rn;
        private int _next;
        private HeheWriter _heheWriter;
        private ExtremeTextWriterUtils _extremWriter;
        private SampletTextWriterUtils _simpleWriter;

        public TestTextWritter()
        {
            _writerAdapter=new TextWriterAdapter(new StringWriter());
            _rn=new Random();
            _heheWriter=new HeheWriter();
            _simpleWriter = new SampletTextWriterUtils();
            _extremWriter = new ExtremeTextWriterUtils();
        }
        [Setup]
        public void SetupData()
        {
            _writerAdapter.Clean();
            _next = _rn.Next(int.MinValue, int.MaxValue);
        }

        [Benchmark]
        public void HeheWriter()
        {
           _heheWriter.Write(_writerAdapter,_next);
        }

        [Benchmark]
        public void SimpleWriter()
        {
            _simpleWriter.Write(_writerAdapter, _next);
        }

        [Benchmark]
        public void ExtremWriter()
        {
            _extremWriter.Write(_writerAdapter, _next);
        }
    }
}