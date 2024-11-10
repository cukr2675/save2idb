using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using AOT;

namespace Save2IDB.Async
{
    public static class IDBDirectory
    {
        private delegate void GetFileSystemEntriesThenCallback(System.IntPtr ohPtr, string entriesJson);

        [DllImport("__Internal")]
        private static extern bool Save2IDB_Async_GetFileSystemEntries(
            System.IntPtr ohPtr, string path, bool containFiles, bool containDirectories,
            GetFileSystemEntriesThenCallback thenCallback, IDBCommon.CatchCallback catchCallback);

        [DllImport("__Internal")]
        private static extern bool Save2IDB_Async_CreateDirectory(
            System.IntPtr ohPtr, string path, IDBCommon.ThenCallback thenCallback, IDBCommon.CatchCallback catchCallback);

        [DllImport("__Internal")]
        private static extern bool Save2IDB_Async_DeleteDirectory(
            System.IntPtr ohPtr, string path, bool recursive, IDBCommon.ThenCallback thenCallback, IDBCommon.CatchCallback catchCallback);

        public static IDBOperationHandle<string[]> GetFilesAsync(
            string path, string searchPattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var operationHandle = new IDBOperationHandle<string[]>();
#if UNITY_WEBGL && !UNITY_EDITOR
            var ohPtr = Unsafe.As<IDBOperationHandle<string[]>, System.IntPtr>(ref operationHandle);
            Save2IDB_Async_GetFileSystemEntries(ohPtr, path, true, false, GetFileSystemEntriesThenCall, IDBCommon.Catch);
#else
            operationHandle.Done(Directory.GetFiles(path, searchPattern, searchOption));
#endif
            return operationHandle;
        }

        public static IDBOperationHandle<string[]> GetDirectoriesAsync(
            string path, string searchPattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var operationHandle = new IDBOperationHandle<string[]>();
#if UNITY_WEBGL && !UNITY_EDITOR
            var ohPtr = Unsafe.As<IDBOperationHandle<string[]>, System.IntPtr>(ref operationHandle);
            Save2IDB_Async_GetFileSystemEntries(ohPtr, path, false, true, GetFileSystemEntriesThenCall, IDBCommon.Catch);
#else
            operationHandle.Done(Directory.GetDirectories(path, searchPattern, searchOption));
#endif
            return operationHandle;
        }

        public static IDBOperationHandle<string[]> GetFileSystemEntriesAsync(
            string path, string searchPattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var operationHandle = new IDBOperationHandle<string[]>();
#if UNITY_WEBGL && !UNITY_EDITOR
            var ohPtr = Unsafe.As<IDBOperationHandle<string[]>, System.IntPtr>(ref operationHandle);
            Save2IDB_Async_GetFileSystemEntries(ohPtr, path, true, true, GetFileSystemEntriesThenCall, IDBCommon.Catch);
#else
            operationHandle.Done(Directory.GetFileSystemEntries(path, searchPattern, searchOption));
#endif
            return operationHandle;
        }

        [MonoPInvokeCallback(typeof(GetFileSystemEntriesThenCallback))]
        private static void GetFileSystemEntriesThenCall(System.IntPtr ohPtr, string entriesJson)
        {
            var operationHandle = Unsafe.As<System.IntPtr, IDBOperationHandle<string[]>>(ref ohPtr);
            var entries = JsonUtility.FromJson<StringArray>($"{{\"vs\":{entriesJson}}}");
            operationHandle.Done(entries.vs);
        }

        public static IDBOperationHandle CreateDirectoryAsync(string path)
        {
            var operationHandle = new IDBOperationHandle();
#if UNITY_WEBGL && !UNITY_EDITOR
            var ohPtr = Unsafe.As<IDBOperationHandle, System.IntPtr>(ref operationHandle);
            Save2IDB_Async_CreateDirectory(ohPtr, path, IDBCommon.Then, IDBCommon.Catch);
#else
            Directory.CreateDirectory(path);
            operationHandle.Done();
#endif
            return operationHandle;
        }

        public static IDBOperationHandle DeleteAsync(string path, bool recursive = false)
        {
            var operationHandle = new IDBOperationHandle();
#if UNITY_WEBGL && !UNITY_EDITOR
            var ohPtr = Unsafe.As<IDBOperationHandle, System.IntPtr>(ref operationHandle);
            Save2IDB_Async_DeleteDirectory(ohPtr, path, recursive, IDBCommon.Then, IDBCommon.Catch);
#else
            Directory.Delete(path, recursive);
            operationHandle.Done();
#endif
            return operationHandle;
        }

        private class StringArray
        {
            public string[] vs;
        }
    }
}
