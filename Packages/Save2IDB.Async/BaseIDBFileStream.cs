using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Save2IDB.Async
{
    public abstract class BaseIDBFileStream : Stream
    {
        public override void Flush() { }

        ///// <summary>
        ///// WebGL �̂Ƃ� IndexedDB �ւ̏������݂Ŕ񓯊������ɂ���K�v�����邽�߂��̃��\�b�h�ł̍X�V�̓T�|�[�g���܂���B
        ///// <see cref="StreamWriter"/> ����Ăяo����邱�Ƃ�z�肷�邽�ߗ�O�𓊂��邱�Ƃ�����܂���B
        ///// (����� <see cref="IDBFileStream.FlushAsync()"/> ���g�p���Ă�������)
        ///// </summary>
        //public sealed override void Flush() { }

        ///// <summary>
        ///// IL2CPP �ł͕W���񓯊��������T�|�[�g����Ȃ����ߗ�O�𓊂���̂�
        ///// </summary>
        ///// <exception cref="System.NotSupportedException"></exception>
        //public sealed override System.IAsyncResult BeginRead(byte[] buffer, int offset, int count, System.AsyncCallback callback, object state)
        //    => throw new System.NotSupportedException(
        //        $"{nameof(IDBFileStream)} �� {nameof(BeginRead)} ���g�p���邱�Ƃ͂ł��܂���B");

        ///// <summary>
        ///// IL2CPP �ł͕W���񓯊��������T�|�[�g����Ȃ����ߗ�O�𓊂���̂�
        ///// </summary>
        ///// <exception cref="System.NotSupportedException"></exception>
        //public sealed override System.IAsyncResult BeginWrite(byte[] buffer, int offset, int count, System.AsyncCallback callback, object state)
        //    => throw new System.NotSupportedException(
        //        $"{nameof(IDBFileStream)} �� {nameof(BeginWrite)} ���g�p���邱�Ƃ͂ł��܂���B");

        ///// <summary>
        ///// IL2CPP �ł͕W���񓯊��������T�|�[�g����Ȃ����ߗ�O�𓊂���̂�
        ///// </summary>
        ///// <exception cref="System.NotSupportedException"></exception>
        //public sealed override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        //    => throw new System.NotSupportedException(
        //        $"{nameof(IDBFileStream)} �� {nameof(CopyToAsync)} ���g�p���邱�Ƃ͂ł��܂���B");

        ///// <summary>
        ///// IL2CPP �ł͕W���񓯊��������T�|�[�g����Ȃ����ߗ�O�𓊂���̂�
        ///// </summary>
        ///// <exception cref="System.NotSupportedException"></exception>
        //public sealed override ValueTask DisposeAsync()
        //    => throw new System.NotSupportedException(
        //        $"{nameof(IDBFileStream)} �� {nameof(DisposeAsync)} ���g�p���邱�Ƃ͂ł��܂���B" +
        //        $"����� {nameof(IDBFileStream.DisposeAsync)} ���g�p���Ă��������B");

        ///// <summary>
        ///// IL2CPP �ł͕W���񓯊��������T�|�[�g����Ȃ����ߗ�O�𓊂���̂�
        ///// </summary>
        ///// <exception cref="System.NotSupportedException"></exception>
        //public sealed override int EndRead(System.IAsyncResult asyncResult)
        //    => throw new System.NotSupportedException(
        //        $"{nameof(IDBFileStream)} �� {nameof(EndRead)} ���g�p���邱�Ƃ͂ł��܂���B");

        ///// <summary>
        ///// IL2CPP �ł͕W���񓯊��������T�|�[�g����Ȃ����ߗ�O�𓊂���̂�
        ///// </summary>
        ///// <exception cref="System.NotSupportedException"></exception>
        //public sealed override void EndWrite(System.IAsyncResult asyncResult)
        //    => throw new System.NotSupportedException(
        //        $"{nameof(IDBFileStream)} �� {nameof(EndWrite)} ���g�p���邱�Ƃ͂ł��܂���B");

        ///// <summary>
        ///// IL2CPP �ł͕W���񓯊��������T�|�[�g����Ȃ����ߗ�O�𓊂���̂�
        ///// </summary>
        ///// <exception cref="System.NotSupportedException"></exception>
        //public sealed override Task FlushAsync(CancellationToken cancellationToken)
        //    => throw new System.NotSupportedException(
        //        $"{nameof(IDBFileStream)} �� {nameof(FlushAsync)} ���g�p���邱�Ƃ͂ł��܂���B" +
        //        $"����� {nameof(IDBFileStream.FlushAsync)} ���g�p���Ă��������B");

        ///// <summary>
        ///// IL2CPP �ł͕W���񓯊��������T�|�[�g����Ȃ����ߗ�O�𓊂���̂�
        ///// </summary>
        ///// <exception cref="System.NotSupportedException"></exception>
        //public sealed override ValueTask<int> ReadAsync(System.Memory<byte> buffer, CancellationToken cancellationToken = default)
        //    => throw new System.NotSupportedException(
        //        $"{nameof(IDBFileStream)} �� {nameof(ReadAsync)} ���g�p���邱�Ƃ͂ł��܂���B");

        ///// <summary>
        ///// IL2CPP �ł͕W���񓯊��������T�|�[�g����Ȃ����ߗ�O�𓊂���̂�
        ///// </summary>
        ///// <exception cref="System.NotSupportedException"></exception>
        //public sealed override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        //    => throw new System.NotSupportedException(
        //        $"{nameof(IDBFileStream)} �� {nameof(ReadAsync)} ���g�p���邱�Ƃ͂ł��܂���B");

        ///// <summary>
        ///// IL2CPP �ł͕W���񓯊��������T�|�[�g����Ȃ����ߗ�O�𓊂���̂�
        ///// </summary>
        ///// <exception cref="System.NotSupportedException"></exception>
        //public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        //    => throw new System.NotSupportedException(
        //        $"{nameof(IDBFileStream)} �� {nameof(WriteAsync)} ���g�p���邱�Ƃ͂ł��܂���B");

        ///// <summary>
        ///// IL2CPP �ł͕W���񓯊��������T�|�[�g����Ȃ����ߗ�O�𓊂���̂�
        ///// </summary>
        ///// <exception cref="System.NotSupportedException"></exception>
        //public sealed override ValueTask WriteAsync(System.ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        //    => throw new System.NotSupportedException(
        //        $"{nameof(IDBFileStream)} �� {nameof(WriteAsync)} ���g�p���邱�Ƃ͂ł��܂���B");
    }
}
