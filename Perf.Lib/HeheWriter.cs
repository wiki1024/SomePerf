using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Perf.Lib
{
    public class HeheWriter : ITextWriterUtils
    {
        private static string[] _fourDigits = Enumerable.Range(0, 10000).Select((fourdigit) => fourdigit.ToString("0000", CultureInfo.InvariantCulture)).ToArray();
        private string _minValue = int.MinValue.ToString();
        private char[] _zeroChar = new char[1] { '0' };
     // private static readonly IBufferPool<char> Char11Pool =new ConcurrentQueueChar11Poll();

        public void Write(ITextWriter writer, int value)
        {
            
             WriteCore(writer,value);
           
        }

        public unsafe void WriteCore(ITextWriter writer, int value)
        {
            
           
            if (value == Int32.MinValue)
            {
                writer.Write(_minValue);
            }
            else if (value == 0)
            {

                writer.Write(_zeroChar) ;
            }
            else
            {
                int start;
                int count;
                var flag = false;
               char* result = stackalloc char[11];//size is to small pooling ends up waiting locking for too long 
                if (value < 0)
                {
                    flag = true;
                    value = -value;
                }


                //var result = new char[11];
                //fixed (char* pt = result)
                //{
                //    var split = value / 1000000;
                //    fixed (char* ptV = _fourDigits[split])
                //    {
                //        *(long*)(pt + 1) = *(long*)ptV;

                //    }
                //    value = value % 1000000;
                //    fixed (char* ptV = _fourDigits[value / 100])
                //    {
                //        *(long*)(pt + 5) = *(long*)ptV;
                //    }
                //    var v = _fourDigits[value % 100];
                //    pt[9] = v[2];
                //    pt[10] = v[3];
                //    for (start = split > 0 ? 1 : 5; start < 11; start++)
                //    {
                //        if (pt[start] != '0') break;
                //    }
                //    if (flag)
                //    {
                //        start--;
                //        pt[start] = '-';
                //    }
                //    count = 11 - start;
                //}
                //writer.Write(result, start, count);



                var split = value / 1000000;
                fixed (char* ptV = _fourDigits[split])
                {
                    *(long*)(result + 1) = *(long*)ptV;

                }
                value = value % 1000000;
                fixed (char* ptV = _fourDigits[value / 100])
                {
                    *(long*)(result + 5) = *(long*)ptV;
                }
                var v = _fourDigits[value % 100];
                result[9] = v[2];
                result[10] = v[3];
                for (start = split > 0 ? 1 : 5; start < 11; start++)
                {
                    if (result[start] != '0') break;
                }
                if (flag)
                {
                    start--;
                    result[start] = '-';
                }
                count = 11 - start;

                // writer.Write(new string(result, start, count));
                writer.Write(result + start, count);

            }
        }

    }

    public class Char11Pool:IBufferPool<char>
    {
        private readonly char[][] _buffers;
        private SpinLock _lock; // do not make this readonly; it's a mutable struct
        private int _index;
        private readonly int _bufferLength;

        public Char11Pool(int poolSize=50)
        {
            _lock=new SpinLock();
            _buffers=new char[poolSize][];
            _bufferLength = 11;
            for (_index = 0; _index < poolSize; _index++)
            {
                _buffers[_index]=new char[_bufferLength];
            }

        }

        public char[] Rent()
        {
            char[][] buffers = _buffers;
            char[] chars = null;

            bool lockTaken = false, allocateNew = false;
            try
            {
                _lock.Enter(ref lockTaken);

                if (_index > 0)
                {
                    chars = buffers[--_index];
                    buffers[_index] = null;
                }
                else
                {
                    allocateNew = true;
                }
            }
            finally
            {
                if (lockTaken) _lock.Exit(false);
            }

            // While we were holding the lock, we grabbed whatever was at the next available index, if
            // there was one.  If we tried and if we got back null, that means we hadn't yet allocated
            // for that slot, in which case we should do so now.
            if (allocateNew)
            {
                chars = new char[11];

            }
            return chars;

        }

        public void Return(char[] chars)
        {
            if (chars.Length != _bufferLength)
            {
                throw new ArgumentException("Invalid_Length_CharArrayNotFromPool", nameof(chars));
            }
            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);

                if (_index < _buffers.Length)
                {
                    _buffers[_index++] = chars;
                }
            }
            finally
            {
                if (lockTaken) _lock.Exit(false);
            }
        }
    }

    public interface IBufferPool<T>
    {
         T[] Rent();
         void Return(T[] buffer);

    }

    public class ConcurrentQueueChar11Poll : IBufferPool<char>
    {
        private readonly ConcurrentQueue<char[]> _queue;
        public ConcurrentQueueChar11Poll()
        {
            var _buffers = new char[50][];
            for (var _index = 0; _index < 50; _index++)
            {
                _buffers[_index] = new char[11];
            }
            _queue=new ConcurrentQueue<char[]>(_buffers);
        }
        public char[] Rent()
        {
            char[] block;
            if (_queue.TryDequeue(out block))
            {
                // block successfully taken from the stack - return it
//#if DEBUG
//                block.Leaser = memberName + ", " + sourceFilePath + ", " + sourceLineNumber;
//                block.IsLeased = true;
//#endif
                return block;
            }
            return new char[11];
        }

        public void Return(char[] chars)
        {
            _queue.Enqueue(chars);
        }
    }
}
