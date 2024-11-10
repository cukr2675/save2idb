using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Save2IDB.Async
{
    public class ManagedIDBFileStream : IDBFileStream
    {
        private readonly FileStream baseStream;

        public override string Name => baseStream.Name;
        public override bool CanRead => baseStream.CanRead;
        public override bool CanSeek => baseStream.CanSeek;
        public override bool CanWrite => baseStream.CanWrite;
        public override long Length => baseStream.Length;
        public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

        internal ManagedIDBFileStream(FileStream baseStream) => this.baseStream = baseStream;

        public override int Read(Span<byte> buffer) => baseStream.Read(buffer);
        public override int Read(byte[] array, int offset, int count) => baseStream.Read(array, offset, count);
        public override int ReadByte() => baseStream.ReadByte();
        public override long Seek(long offset, SeekOrigin origin) => baseStream.Seek(offset, origin);
        public override void SetLength(long value) => baseStream.SetLength(value);
        public override void Write(byte[] array, int offset, int count) => baseStream.Write(array, offset, count);
        public override void Write(ReadOnlySpan<byte> buffer) => baseStream.Write(buffer);
        public override void WriteByte(byte value) => baseStream.WriteByte(value);

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => baseStream.BeginRead(buffer, offset, count, callback, state);
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => baseStream.BeginWrite(buffer, offset, count, callback, state);
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => baseStream.CopyToAsync(destination, bufferSize, cancellationToken);
        public override int EndRead(IAsyncResult asyncResult) => baseStream.EndRead(asyncResult);
        public override void EndWrite(IAsyncResult asyncResult) => baseStream.EndWrite(asyncResult);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => baseStream.ReadAsync(buffer, offset, count, cancellationToken);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => baseStream.ReadAsync(buffer, cancellationToken);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => baseStream.WriteAsync(buffer, offset, count, cancellationToken);
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            => baseStream.WriteAsync(buffer, cancellationToken);

        public override IDBOperationHandle FlushAsync(CancellationToken cancellationToken)
        {
            baseStream.Flush();
            return IDBOperationHandle.DoneInstance();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { baseStream.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
