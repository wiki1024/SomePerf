using BenchmarkDotNet.Attributes;
using Perf.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenchRunner
{
    public class TestCopy
    {
        byte[] source;
        int length = 256 ;
       
        public TestCopy()
        {     
            source = new byte[length];
            for (int i = 0; i < length; ++i)
            {
                source[i] = (byte)i;
            }
        }

       

        //[Benchmark]
        //public byte[] PointerCopy()
        //{
        //    var target = new byte[length];
        //    PointerPerf.Copy(source, 0, target, 0, length);
        //    return target;
        //}

        //[Benchmark]
        //public byte[] NaiveCopy()
        //{
        //    var target = new byte[length];
        //    PointerPerf.NaiveCopy(source, 0, target, 0, length);
        //    return target;
        //}

        [Benchmark]
        public byte[] MemCopy()
        {
            var target = new byte[length];
            PointerPerf.MemCopy(source, 0, target, 0, length);
            return target;
        }

        [Benchmark]
        public byte[] VectorCopy()
        {
            var target = new byte[length];
            PointerPerf.VectorCopy(source, 0, target, 0, length);
            return target;
        }

        [Benchmark]
        public byte[] BlockCopy()
        {
            var target = new byte[length];
            Buffer.BlockCopy(source, 0, target, 0, length);
            return target;
        }

        //[Benchmark]
        //public byte[] ArrayCopy()
        //{
        //    var target = new byte[length];
        //    Array.Copy(source, 0, target, 0, length);
        //    return target;
        //}
    }
}
