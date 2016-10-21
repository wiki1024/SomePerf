using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Perf.Lib;

namespace BenchRunner
{
    public class TestDateTimeToString
    {
        private DateTime _source;

        private Func<string, string> _funcString = (x) => x;
        //[Setup]
        //public void SetupData()
        //{
        //    _source = DateTime.Now;
        //}

        //[Benchmark]
        //public string NormalToString()
        //{
        //    //var source = DateTime.Now;
        //    return _source.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff", CultureInfo.InvariantCulture);
        //}

        //[Benchmark]
        //public string PointerToString()
        //{
        //    //var source = DateTime.Now;
        //    return CustomFormatter.GetDefaultFormat(_source);
        //}

        [Benchmark]
        public string NormalCall()
        {
            return ReturnString("hehe");
        }

        [Benchmark]
        public string Delegate()
        {

            return _funcString("hehe");
        }

        private string ReturnString(string x)
        {
            return x;
        }
    }
}
