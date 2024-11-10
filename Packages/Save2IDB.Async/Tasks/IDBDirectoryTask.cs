using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

namespace Save2IDB.Async
{
    public static class IDBDirectoryTask
    {
        public static async UniTask<string[]> GetFilesAsync(string path)
        {
            var operationHandle = IDBDirectory.GetFilesAsync(path);
            await operationHandle;
            return operationHandle.Result;
        }

        public static async UniTask<string[]> GetDirectoriesAsync(string path)
        {
            var operationHandle = IDBDirectory.GetDirectoriesAsync(path);
            await operationHandle;
            return operationHandle.Result;
        }

        public static async UniTask<string[]> GetFileSystemEntriesAsync(string path)
        {
            var operationHandle = IDBDirectory.GetFileSystemEntriesAsync(path);
            await operationHandle;
            return operationHandle.Result;
        }

        public static async UniTask CreateDirectoryAsync(string path) => await IDBDirectory.CreateDirectoryAsync(path);
        public static async UniTask DeleteAsync(string path, bool recursive = false) => await IDBDirectory.DeleteAsync(path, recursive);
    }
}
