using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Perf.Lib;

namespace BenchRunner
{
    public class ExtremeTextWriterUtils : ITextWriterUtils
    {
        private static readonly string[] _zeroPaddingStrings;
        private static readonly string[] _nonZeroPaddingStrings;
        private static readonly string _minString;


        static ExtremeTextWriterUtils()
        {
            _zeroPaddingStrings = Enumerable.Range(0, 10000).Select(i => i.ToString("0000", CultureInfo.InvariantCulture)).ToArray();
            _nonZeroPaddingStrings = Enumerable.Range(0, 10000).Select(i => i.ToString(CultureInfo.InvariantCulture)).ToArray();
            _minString = int.MinValue.ToString(CultureInfo.InvariantCulture);
        }

        [ThreadStatic]
        private static char[] _buffer;

        private static char[] GetBuffer()
        {
            if (_buffer == null)
            {
                _buffer = new char[11];
                _buffer[0] = '-';
            }
            return _buffer;
        }

        private static void WriteCore(ITextWriter writer, int start, int value)
        {
            var buffer = new char[11];// GetBuffer();
            var high = value / 100000000;
            int offset;
            if (high > 0)
            {
                offset = CopyToBuffer(buffer, 1, _nonZeroPaddingStrings[high]);
                value -= high * 100000000;
                high = value / 10000;
                offset = CopyToBuffer(buffer, offset, _zeroPaddingStrings[high]);
                value -= high * 10000;
                offset = CopyToBuffer(buffer, offset, _zeroPaddingStrings[value]);
            }
            else
            {
                high = value / 10000;
                if (high > 0)
                {
                    offset = CopyToBuffer(buffer, 1, _nonZeroPaddingStrings[high]);
                    value -= high * 10000;
                    offset = CopyToBuffer(buffer, offset, _zeroPaddingStrings[value]);

                }
                else
                {
                    offset = CopyToBuffer(buffer, 1, _nonZeroPaddingStrings[value]);
                }
            }
            writer.Write(buffer, start, offset - start);
        }

        private static unsafe int CopyToBuffer(char[] buffer, int offset, string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                buffer[offset++] = s[i];
            }

            return offset;

        }

        public void Write(ITextWriter writer, int value)
        {
            if (value == int.MinValue) { writer.Write(_minString); return; }
            if (value < 0) { WriteCore(writer, 0, -value); return; }
            if (value <= 9999)
            {
                writer.Write(_nonZeroPaddingStrings[value]);
                return;
            }
            WriteCore(writer, 1, value);
        }

    }

    public class SampletTextWriterUtils : ITextWriterUtils
    {
        public void Write(ITextWriter writer, int value)
        {
            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
