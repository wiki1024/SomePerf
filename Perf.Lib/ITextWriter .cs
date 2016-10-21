using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perf.Lib
{
    public interface ITextWriter
    {
        void Write(string text);
        void Write(string text, int startIndex, int count);
        void Write(char[] chars);
        void Write(char[] chars, int startIndex, int count);
        unsafe void Write(char* value, int valueCount);
    }

    public interface ITextWriterUtils
    {
        void Write(ITextWriter writer, int value);
    }

    public class TextWriterAdapter : ITextWriter
    {
        public TextWriterAdapter(TextWriter writer)
        {
            _writer = writer;
            _stringWriter=(writer is StringWriter) ? (writer as StringWriter).GetStringBuilder():null;
        }

        private TextWriter _writer;

        private StringBuilder _stringWriter;
        public void Write(char[] chars)
        {
            _writer.Write(chars);
        }

        public void Write(string text)
        {
            _writer.Write(text);
        }

        public void Write(char[] chars, int startIndex, int count)
        {
            _writer.Write(chars, startIndex, count);
        }

        public void Write(string text, int startIndex, int count)
        {
            _writer.Write(text.Substring(startIndex, count));
        }

        public unsafe void Write(char* value, int valueCount)
        {
            if (_stringWriter != null)
            {
                
            _stringWriter.Append(value, valueCount);
            }
        }
        public void Clean()
        {
            (_writer as StringWriter).GetStringBuilder().Clear();
        }
    }
}
