using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Threading;

namespace Save2IDB.Async
{
    public abstract class IDBFileStream : BaseIDBFileStream
    {
        public abstract string Name { get; }

        private bool isClosed;

        // ���ۃ��\�b�h��ǉ�����\�������邽�߁A�R���X�g���N�^�� internal �ɂ���
        internal IDBFileStream() { }

        public abstract new IDBOperationHandle FlushAsync(CancellationToken cancellationToken);

        public new IDBOperationHandle FlushAsync() => FlushAsync(CancellationToken.None);

        protected override void Dispose(bool disposing)
        {
            if (!isClosed)
            {
                if (disposing)
                {
                    if (CanWrite)
                    {
                        // �}�l�[�W�h���\�[�X�̔j��
                        Debug.LogWarning(
                            $"�������݉\�� {Name} �� {nameof(IDBFileStream)} �� {nameof(DisposeAsync)} ���g�p�����ɔj������܂����B" +
                            $"���ɂ���Ă͏������񂾓��e���ۑ�����Ȃ��ꍇ������܂��B");
                    }
                }

                // �A���}�l�[�W�h���\�[�X�̔j��
                isClosed = true;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// ���̃��\�b�h�����s�����ꍇ <see cref="Stream.Dispose"/> �����s����K�v�͂Ȃ�
        /// </summary>
        public virtual new IDBOperationHandle DisposeAsync()
        {
            if (CanWrite)
            {
                // �������݃��[�h�̏ꍇ�͏������݂����������� Close
                var flush = FlushAsync(CancellationToken.None);
                return new IDBOperationHandle(flush, x =>
                {
                    isClosed = true;
                    Dispose(true);
                    x.Done();
                });
            }
            else
            {
                isClosed = true;
                Dispose(true);
                return IDBOperationHandle.DoneInstance();
            }
        }

        /// <summary>
        /// <see cref="IDBFileStream.DisposeAsync"/> �� await using �X�e�[�g�����g�őҋ@�\�ȃ��b�p�[�N���X
        /// </summary>
        public sealed class AwaitWrapper
        {
            private readonly IDBFileStream baseStream;

            public string Name => baseStream.Name;
            public long Position { get => baseStream.Position; set => baseStream.Position = value; }
            public long Length => baseStream.Length;
            public bool CanWrite => baseStream.CanWrite;
            public bool CanTimeout => baseStream.CanTimeout;
            public bool CanSeek => baseStream.CanSeek;
            public bool CanRead => baseStream.CanRead;
            public int ReadTimeout { get => baseStream.ReadTimeout; set => baseStream.ReadTimeout = value; }
            public int WriteTimeout { get => baseStream.WriteTimeout; set => baseStream.WriteTimeout = value; }

            public AwaitWrapper(IDBFileStream baseStream)
            {
                this.baseStream = baseStream ?? throw new System.ArgumentNullException(nameof(baseStream));
            }

            public IDBOperationHandle FlushAsync(CancellationToken cancellationToken) => baseStream.FlushAsync(cancellationToken);
            public IDBOperationHandle FlushAsync() => baseStream.FlushAsync(CancellationToken.None);
            public IDBOperationHandle DisposeAsync() => baseStream.DisposeAsync();

            //public void Close() => baseStream.Close();
            public void CopyTo(Stream destination, int bufferSize) => baseStream.CopyTo(destination, bufferSize);
            public void CopyTo(Stream destination) => baseStream.CopyTo(destination);
            //public void Dispose() => baseStream.Dispose();
            public int Read(byte[] buffer, int offset, int count) => baseStream.Read(buffer, offset, count);
            public int Read(System.Span<byte> buffer) => baseStream.Read(buffer);
            public int ReadByte() => baseStream.ReadByte();
            public long Seek(long offset, SeekOrigin origin) => baseStream.Seek(offset, origin);
            public void SetLength(long value) => baseStream.SetLength(value);
            public void Write(byte[] buffer, int offset, int count) => baseStream.Write(buffer, offset, count);
            public void Write(System.ReadOnlySpan<byte> buffer) => baseStream.Write(buffer);

            //public object GetLifetimeService() => baseStream.GetLifetimeService();
            //public object InitializeLifetimeService() => baseStream.InitializeLifetimeService();

            public static implicit operator IDBFileStream(AwaitWrapper wrapper) => wrapper.baseStream;
        }
    }
}
