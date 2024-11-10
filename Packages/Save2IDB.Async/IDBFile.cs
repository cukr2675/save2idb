using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using AOT;

namespace Save2IDB.Async
{
    public static class IDBFile
    {
        private delegate void ExistsThenCallback(System.IntPtr ohPtr, bool exists);

        [DllImport("__Internal")]
        private static extern bool Save2IDB_Async_Exists(
            System.IntPtr ohPtr, string path, ExistsThenCallback thenCallback, IDBCommon.CatchCallback catchCallback);

        [DllImport("__Internal")]
        private static extern void Save2IDB_Async_Delete(
            System.IntPtr ohPtr, string path, IDBCommon.ThenCallback thenCallback, IDBCommon.CatchCallback catchCallback);

        [DllImport("__Internal")]
        private static extern bool Save2IDB_Async_Move(
            System.IntPtr ohPtr, string sourceFileName, string destFileName,
            IDBCommon.ThenCallback thenCallback, IDBCommon.CatchCallback catchCallback);

        [DllImport("__Internal")]
        private static extern bool Save2IDB_Async_Copy(
            System.IntPtr ohPtr, string sourceFileName, string destFileName, bool overwrite,
            IDBCommon.ThenCallback thenCallback, IDBCommon.CatchCallback catchCallback);

        /// <param name="access">
        /// default: <paramref name="mode"/> == <see cref="FileMode.Append"/> ? <see cref="FileAccess.Write"/> : <see cref="FileAccess.ReadWrite"/>
        /// </param>
        public static IDBOperationHandle<IDBFileStream> OpenAsync(
            string path, FileMode mode, FileAccess access = default, FileShare share = FileShare.Read)
        {
            if (access == default) { access = mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite; }

#if UNITY_WEBGL && !UNITY_EDITOR
            return WebGLIDBFileStream.OpenAsync(path, mode, access);
#else
            var operationHandle = new IDBOperationHandle<IDBFileStream>();
            operationHandle.Done(new ManagedIDBFileStream(File.Open(path, mode, access, share)));
            return operationHandle;
#endif
        }

        public static IDBOperationHandle<bool> ExistsAsync(string path)
        {
            var operationHandle = new IDBOperationHandle<bool>();
#if UNITY_WEBGL && !UNITY_EDITOR
            var ohPtr = Unsafe.As<IDBOperationHandle<bool>, System.IntPtr>(ref operationHandle);
            Save2IDB_Async_Exists(ohPtr, path, ExistsThen, IDBCommon.Catch);
#else
            operationHandle.Done(File.Exists(path));
#endif
            return operationHandle;
        }

        [MonoPInvokeCallback(typeof(ExistsThenCallback))]
        private static void ExistsThen(System.IntPtr ohPtr, bool exists)
        {
            var operationHandle = Unsafe.As<System.IntPtr, IDBOperationHandle<bool>>(ref ohPtr);
            operationHandle.Done(exists);
        }

        public static IDBOperationHandle DeleteAsync(string path)
        {
            var operationHandle = new IDBOperationHandle();
#if UNITY_WEBGL && !UNITY_EDITOR
            var ohPtr = Unsafe.As<IDBOperationHandle, System.IntPtr>(ref operationHandle);
            Save2IDB_Async_Delete(ohPtr, path, IDBCommon.Then, IDBCommon.Catch);
#else
            File.Delete(path);
            operationHandle.Done();
#endif
            return operationHandle;
        }

        public static IDBOperationHandle MoveAsync(string sourceFileName, string destFileName)
        {
            var operationHandle = new IDBOperationHandle();
#if UNITY_WEBGL && !UNITY_EDITOR
            var ohPtr = Unsafe.As<IDBOperationHandle, System.IntPtr>(ref operationHandle);
            Save2IDB_Async_Move(ohPtr, sourceFileName, destFileName, IDBCommon.Then, IDBCommon.Catch);
#else
            File.Move(sourceFileName, destFileName);
            operationHandle.Done();
#endif
            return operationHandle;
        }

        public static IDBOperationHandle CopyAsync(string sourceFileName, string destFileName, bool overwrite = false)
        {
            var operationHandle = new IDBOperationHandle();
#if UNITY_WEBGL && !UNITY_EDITOR
            var ohPtr = Unsafe.As<IDBOperationHandle, System.IntPtr>(ref operationHandle);
            Save2IDB_Async_Copy(ohPtr, sourceFileName, destFileName, overwrite, IDBCommon.Then, IDBCommon.Catch);
#else
            File.Copy(sourceFileName, destFileName, overwrite);
            operationHandle.Done();
#endif
            return operationHandle;
        }

        private static IDBOperationHandle ReplaceAsync(
            string sourceFileName, string destFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        {
            throw new System.NotImplementedException();
//            var operationHandle = new IDBOperationHandle();
//#if UNITY_WEBGL && !UNITY_EDITOR
//            var ohPtr = Unsafe.As<IDBOperationHandle, System.IntPtr>(ref operationHandle);
//            Save2IDB_Async_Copy(ohPtr, sourceFileName, destFileName, overwrite, IDBCommon.Then, IDBCommon.Catch);
//#else
//            File.Replace(sourceFileName, destFileName, destinationBackupFileName, ignoreMetadataErrors);
//            operationHandle.Done();
//#endif
//            return operationHandle;
        }
    }
}
