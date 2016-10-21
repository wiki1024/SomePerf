using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Perf.Lib.blocks
{
    public class PDFStream : Stream
    {
        private readonly bool _canRead = true;
        private readonly bool _canSeek = false;
        private readonly bool _canWrite = true;

        private readonly MemoryPoolBlock _head;
        private MemoryPoolBlock _current;
        private MemoryPoolBlock _read;
        private MemoryPool _memory;

        private int _readIndex = 0;

        private bool _disposedValue = false;
        public PDFStream(MemoryPool memory)
        {
            _memory = memory;
            _head = memory.Lease();
            _current = _head;
            _read = _head;
        }
        public override void Flush()
        {

        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var block = _read;
            var remaining = count;
            var readOffset = offset;
            var bytesRead = 0;
            var toRead = 0;
            while (true)
            {
                var remainInBlock = block.End - block.Start - _readIndex;
                var isLastBlock = block.Next == null;
                if (remaining < remainInBlock)
                {
                    toRead = remaining;
                    Buffer.BlockCopy(block.Array, block.Start+_readIndex, buffer, readOffset, toRead);
                    bytesRead += toRead;
                    _readIndex += toRead;
                    readOffset += toRead;
                    remaining -= toRead;
                    return bytesRead;
                }
                else
                {
                    toRead = remainInBlock;
                    if (toRead > 0)
                    {
                        Buffer.BlockCopy(block.Array, block.Start+_readIndex, buffer, readOffset, toRead);
                    }
                    bytesRead += toRead;
                    _readIndex += toRead;
                    readOffset += toRead;
                    remaining -= toRead;
                    if (isLastBlock) return bytesRead;
                    block = block.Next;
                    _read = block;
                    _readIndex = 0;
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count < Vector<byte>.Count)
            {
                WriteShort(buffer, offset, count);
            }
            else
            {

                var pool = _memory;
                var block = _current;

                var bytesLeftInBlock = block.Data.Count - (block.End - block.Start);
                var toCopy = count;
                var inputOffset = offset;
                while (toCopy > 0)
                {
                    if (bytesLeftInBlock == 0)
                    {
                        var nextBlock = pool.Lease();
                        Volatile.Write(ref block.Next, nextBlock);
                        block = nextBlock;
                        _current = block;
                        bytesLeftInBlock = block.Data.Count;
                    }

                    var copied = PointerPerf.VectorCopy(buffer, inputOffset, block.Array, block.End, toCopy < bytesLeftInBlock ? toCopy : bytesLeftInBlock);
                    block.End += copied;
                    bytesLeftInBlock -= copied;
                    toCopy -= copied;
                    inputOffset += copied;

                }
            }


        }

        public unsafe void WriteShort(byte[] buffer, int offset, int count)
        {
            fixed (byte* ptSource = buffer)
            {
                var pool = _memory;
                var block = _current;

                var bytesLeftInBlock = block.Data.Count - (block.End - block.Start);

                var input = ptSource;
                var inputEnd = ptSource + count;
                while (input < inputEnd)
                {
                    if (bytesLeftInBlock == 0)
                    {
                        var nextBlock = pool.Lease();
                        Volatile.Write(ref block.Next, nextBlock);
                        block = nextBlock;
                        _current = block;
                        bytesLeftInBlock = block.Data.Count;
                    }

                    var output = (block.DataFixedPtr + block.End);
                    var copied = 0;

                    for (; input < inputEnd && copied < bytesLeftInBlock; copied++)
                    {
                        *(output++) = *(input++);
                        block.End++;
                    }

                    bytesLeftInBlock -= copied;
                }

            }
        }

        public override bool CanRead
        {
            get { return _canRead; }
        }

        public override bool CanSeek
        {
            get { return _canSeek; }
        }

        public override bool CanWrite
        {
            get { return _canWrite; }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override void Close()
        {
            if (!_disposedValue)
            {
                _disposedValue = true;


                var block = _head;
                while (block != null)
                {
                    var returnBlock = block;
                    block = block.Next;

                    returnBlock.Pool.Return(returnBlock);
                }

            }
        }
    }
}


//public override unsafe int Read(byte[] buffer, int offset, int count)
//{
//    var n = buffer.Length >= count ? count : buffer.Length;
//    fixed (byte* ptTarget = buffer)
//    {
//        var block = _read;
//        var readBytes = 0;
//        var output = ptTarget;
//        while (true)
//        {

//            var input = block.DataFixedPtr + block.Start + _position;
//            var inputEnd = block.DataFixedPtr + block.End;

//            for (; readBytes < n && input < inputEnd; readBytes++)
//            {
//                *(output++) = *(input++);
//                _position++;
//            }
//            if (readBytes == n) return readBytes;
//            if (readBytes < n && block.Next == null) return readBytes;
//            block = block.Next;
//            _read = block;
//            _position = 0;
//        }
//    }
//}

