using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Cysharp.Threading.Tasks;

namespace Save2IDB.Async
{
    public static class IDBFileTask
    {
        public static async UniTask<IDBFileStream.AwaitWrapper> OpenAsync(
            string path, FileMode mode, FileAccess access = default, FileShare share = FileShare.Read)
        {
            var operationHandle = IDBFile.OpenAsync(path, mode, access, share);
            await operationHandle;
            return new IDBFileStream.AwaitWrapper(operationHandle.Result);
        }

        public static async UniTask<bool> ExistsAsync(string path)
        {
            var operationHandle = IDBFile.ExistsAsync(path);
            await operationHandle;
            return operationHandle.Result;
        }

        public static async UniTask DeleteAsync(string path) => await IDBFile.DeleteAsync(path);
        public static async UniTask MoveAsync(string sourceFileName, string destFileName) => await IDBFile.MoveAsync(sourceFileName, destFileName);
        public static async UniTask CopyAsync(string sourceFileName, string destFileName, bool overwrite = false)
            => await IDBFile.CopyAsync(sourceFileName, destFileName, overwrite);
    }
}
