using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataSourceTool
{
    // Taken from here: https://social.msdn.microsoft.com/Forums/vstudio/en-US/c409b63b-37df-40ca-9322-458ffe06ea48/how-to-access-part-of-a-filestream-or-memorystream?forum=netfxbcl
    // Modified to be reused

    public class SubStream : Stream
    {
        private readonly long length;

        private Stream baseStream;
        private long position;

        public SubStream(Stream baseStream, long offset, long length)
        {
            if (baseStream == null)
                throw new ArgumentNullException(nameof(baseStream));
            if (baseStream.CanRead == false)
                throw new ArgumentException($"{nameof(baseStream)} must be readable");
            if (baseStream.CanSeek == false)
                throw new ArgumentException($"{nameof(baseStream)} must be seekable");

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            position = 0;

            this.baseStream = baseStream;
            this.length = length;

            baseStream.Seek(offset, SeekOrigin.Begin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            long remaining = length - position;

            if (remaining <= 0)
                return 0;

            if (remaining < count)
                count = (int)remaining;

            int read = baseStream.Read(buffer, offset, count);
            position += read;

            return read;
        }

        private void CheckDisposed()
        {
            if (baseStream == null)
                throw new ObjectDisposedException(GetType().Name);
        }

        public override long Length
        {
            get
            {
                CheckDisposed();
                return length;
            }
        }

        public override bool CanRead
        {
            get
            {
                CheckDisposed();
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                CheckDisposed();
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                CheckDisposed();
                return false;
            }
        }

        public override long Position
        {
            get
            {
                CheckDisposed();
                return position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            CheckDisposed();
            baseStream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (baseStream != null)
                {
                    try
                    {
                        baseStream.Dispose();
                    }
                    catch
                    {
                    }
                    baseStream = null;
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
