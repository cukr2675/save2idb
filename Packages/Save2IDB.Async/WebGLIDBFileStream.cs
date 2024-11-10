using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using AOT;

namespace Save2IDB.Async
{
    public class WebGLIDBFileStream : IDBFileStream
    {
        private delegate void OpenThenCallback(System.IntPtr ohPtr, string statsJson);

        [DllImport("__Internal")]
        private static extern void Save2IDB_Async_FileStream_Open(
            System.IntPtr ohPtr, string path, string flags, OpenThenCallback thenCallback, IDBCommon.CatchCallback catchCallback);

        [DllImport("__Internal")]
        private static extern void Save2IDB_Async_FileStream_Close(int fileDescriptor);

        [DllImport("__Internal")]
        private static extern void Save2IDB_Async_FileStream_ReadFile(
            System.IntPtr ohPtr, string path, byte[] bytesPtr, int bytesLen, IDBCommon.ThenCallback thenCallback, IDBCommon.CatchCallback catchCallback);

        [DllImport("__Internal")]
        private static extern void Save2IDB_Async_FileStream_WriteFile(
            System.IntPtr ohPtr, string path, byte[] bytesPtr, int bytesLen, IDBCommon.ThenCallback thenCallback, IDBCommon.CatchCallback catchCallback);

        public override string Name => _name;
        public override bool CanRead => baseStream.CanRead;
        public override bool CanSeek => baseStream.CanSeek;
        public override bool CanWrite => baseStream.CanWrite;
        public override long Length => baseStream.Length;
        public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

        public bool IsSaved { get; private set; }

        private readonly MemoryStream baseStream;
        private string _name;
        private WebGLFileStats stats;

        private static readonly string invalidCharacterExceptionMessage = $"パスに無効な文字 [{string.Join(", ", invalidChars)}] が含まれています。";
        private static readonly string invalidChars = new string(Path.GetInvalidFileNameChars().Where(x => x != '/').ToArray());

        private WebGLIDBFileStream(OpenOperationHandle operationHandle)
        {
            baseStream = operationHandle.BaseStream;
            _name = operationHandle.Path;
            stats = operationHandle.Stats;

            if (operationHandle.Mode == FileMode.Append) { Seek(0, SeekOrigin.End); }
            else { Position = 0; }
        }

        public override void CopyTo(Stream destination, int bufferSize) => baseStream.CopyTo(destination, bufferSize);
        public override int Read(System.Span<byte> destination) => baseStream.Read(destination);
        public override int Read(byte[] buffer, int offset, int count) => baseStream.Read(buffer, offset, count);
        public override int ReadByte() => baseStream.ReadByte();
        public override long Seek(long offset, SeekOrigin loc) => baseStream.Seek(offset, loc);
        public override void SetLength(long value) => baseStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => baseStream.Write(buffer, offset, count);
        public override void Write(System.ReadOnlySpan<byte> buffer) => baseStream.Write(buffer);
        public override void WriteByte(byte value) => baseStream.WriteByte(value);

        public override IDBOperationHandle FlushAsync(CancellationToken cancellationToken)
        {
            if (!CanWrite) throw new System.InvalidOperationException($"書き込み禁止ストリームで {nameof(FlushAsync)} を実行することはできません。");

            var operationHandle = new IDBOperationHandle();
#if UNITY_WEBGL && !UNITY_EDITOR
            var ohPtr = Unsafe.As<IDBOperationHandle, System.IntPtr>(ref operationHandle);
            var buffer = baseStream.GetBuffer();
            Save2IDB_Async_FileStream_WriteFile(ohPtr, _name, buffer, (int)Length, IDBCommon.Then, IDBCommon.Catch);
#endif
            return operationHandle;
        }

        protected override void Dispose(bool disposing)
        {
            if (stats != null)
            {
                if (disposing) { baseStream.Dispose(); }

                Save2IDB_Async_FileStream_Close(stats.fd);
                _name = null;
                stats = null;
            }

            base.Dispose(disposing);
        }



        internal static IDBOperationHandle<IDBFileStream> OpenAsync(string path, FileMode mode, FileAccess access)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new System.ArgumentException($"{path} は無効なパスです。", nameof(path));
            if (invalidChars.Contains(path)) throw new System.ArgumentException(invalidCharacterExceptionMessage, nameof(path));
            if (mode < FileMode.CreateNew || FileMode.Append < mode) throw new System.ArgumentOutOfRangeException(nameof(mode));
            if (access < FileAccess.Read || FileAccess.ReadWrite < access) throw new System.ArgumentOutOfRangeException(nameof(access));
            if ((access & FileAccess.Write) == 0)
            {
                if (mode == FileMode.Truncate || mode == FileMode.CreateNew || mode == FileMode.Create || mode == FileMode.Append)
                    throw new System.ArgumentException($"{access} は {mode} では無効です。", nameof(access));
            }
            if ((access & FileAccess.Read) != 0)
            {
                if (mode == FileMode.Append) throw new System.ArgumentException($"{access} は {mode} では無効です。", nameof(access));
            }

            var operationHandle = new OpenOperationHandle() { Path = path, Mode = mode, Access = access };
#if UNITY_WEBGL && !UNITY_EDITOR
            var ohPtr = Unsafe.As<OpenOperationHandle, System.IntPtr>(ref operationHandle);
            Save2IDB_Async_FileStream_Open(ohPtr, path, GetFlags(mode), OpenThen, IDBCommon.Catch);
#endif
            return operationHandle;
        }

        [MonoPInvokeCallback(typeof(OpenThenCallback))]
        private static void OpenThen(System.IntPtr ohPtr, string statsJson)
        {
            // ファイルを開くことに成功したら中身を読み込む
            var operationHandle = Unsafe.As<System.IntPtr, OpenOperationHandle>(ref ohPtr);
            operationHandle.Stats = JsonUtility.FromJson<WebGLFileStats>(statsJson);
            operationHandle.BaseStream = new MemoryStream();
            operationHandle.BaseStream.SetLength(operationHandle.Stats.size);
            var buffer = operationHandle.BaseStream.GetBuffer();
            Save2IDB_Async_FileStream_ReadFile(ohPtr, operationHandle.Path, buffer, operationHandle.Stats.size, OpenReadThen, IDBCommon.Catch);
        }

        [MonoPInvokeCallback(typeof(IDBCommon.ThenCallback))]
        private static void OpenReadThen(System.IntPtr ohPtr)
        {
            // 中身を読み込むことに成功したらストリームを生成して返す
            var operationHandle = Unsafe.As<System.IntPtr, OpenOperationHandle>(ref ohPtr);
            var stream = new WebGLIDBFileStream(operationHandle);
            operationHandle.Done(stream);
        }

        private static string GetFlags(FileMode mode)
        {
            switch (mode)
            {
                case FileMode.CreateNew:
                    throw new System.NotImplementedException(); // ファイルが存在したら例外 + Create
                case FileMode.Create:
                    return "w+";
                case FileMode.Open:
                    return "r+";
                case FileMode.OpenOrCreate:
                    return "a+";
                case FileMode.Truncate:
                    throw new System.NotImplementedException(); // ファイルが存在しなかったら例外 + Create + 読み込み不可
                case FileMode.Append:
                    throw new System.NotImplementedException(); // OpenOrCreate + シーク範囲制限
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(mode));
            }
        }

        private class OpenOperationHandle : IDBOperationHandle<IDBFileStream>
        {
            public string Path { get; set; }
            public FileMode Mode { get; set; }
            public FileAccess Access { get; set; }
            public WebGLFileStats Stats { get; set; }
            public MemoryStream BaseStream { get; set; }
        }
    }
}
