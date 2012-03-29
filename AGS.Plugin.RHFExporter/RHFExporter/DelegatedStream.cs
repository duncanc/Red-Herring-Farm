using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RedHerringFarm
{
    public class DelegatedStream : Stream
    {
        public delegate int ByteReader(byte[] data, int pos, int len);
        public delegate void ByteWriter(byte[] data, int pos, int len);
        public delegate long PositionGetter();
        public delegate void PositionSetter(long newPos);
        public delegate long Seeker(long newPos, SeekOrigin origin);
        public delegate void Flusher();
        public delegate long LengthGetter();
        public delegate void LengthSetter(long length);

        public delegate IEnumerable<byte[]> ByteYielder();

        public static ByteReader CreateByteYielder(IEnumerable<byte[]> yielder)
        {
            IEnumerator<byte[]> input = yielder.GetEnumerator();
            byte[] srcData = null;
            int srcPos = 0;
            return delegate(byte[] dstData, int dstPos, int maxLen)
            {
                while (srcData == null
                    || srcPos >= srcData.Length)
                {
                    if (!input.MoveNext())
                    {
                        return 0;
                    }
                    srcData = input.Current;
                    srcPos = 0;
                }
                int numBytes = Math.Min(maxLen, srcData.Length - srcPos);
                Buffer.BlockCopy(srcData, srcPos, dstData, dstPos, numBytes);
                srcPos += numBytes;
                return numBytes;
            };
        }

        public ByteReader read;
        public ByteWriter write;
        public Seeker seek;
        public Flusher flush;
        public LengthGetter getLength;
        public LengthSetter setLength;
        public PositionGetter getPosition;
        public PositionSetter setPosition;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (read == null) throw new NotImplementedException();
            return read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (write == null) throw new NotImplementedException();
            write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return read != null; }
        }

        public override bool CanSeek
        {
            get { return seek != null; }
        }

        public override bool CanWrite
        {
            get { return write != null; }
        }

        public override void Flush()
        {
            if (flush != null) flush();
        }

        public override long Length
        {
            get
            {
                if (getLength == null) throw new NotImplementedException();
                return getLength();
            }
        }

        public override long Position
        {
            get
            {
                if (getPosition == null) throw new NotImplementedException();
                return getPosition();
            }
            set
            {
                if (setPosition == null) throw new NotImplementedException();
                setPosition(value);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (seek == null) throw new NotImplementedException();
            return seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if (setLength == null) throw new NotImplementedException();
            setLength(value);
        }
    }
}
