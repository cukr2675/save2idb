using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Linq;

namespace Save2IDB.Samples
{
    public class StorageTest : RuntimeTestClass
    {
        [RuntimeTest]
        public void CreateIterFilesOf100KB(int iter)
        {
            var autoPath = AutoPath;
            CreateDirectoryFor(Path.Combine(autoPath, "0"));

            var fileOf100KB = Enumerable.Repeat((byte)'1', 100000).ToArray();

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < iter; i++)
            {
                var path = Path.Combine(autoPath, i.ToString());
                using var stream = IDBFile.Open(path, FileMode.Create);
                stream.Write(fileOf100KB);
            }
            stopwatch.Stop();
            Debug.Log($"Create {iter} files(100KB): {stopwatch.ElapsedMilliseconds}ms");
        }

        [RuntimeTest]
        public void CreateIterFilesOf1MB(int iter)
        {
            var autoPath = AutoPath;
            CreateDirectoryFor(Path.Combine(autoPath, "0"));

            var fileOf1MB = Enumerable.Repeat((byte)'1', 1000000).ToArray();

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < iter; i++)
            {
                var path = Path.Combine(autoPath, i.ToString());
                using var stream = IDBFile.Open(path, FileMode.Create);
                stream.Write(fileOf1MB);
            }
            stopwatch.Stop();
            Debug.Log($"Create {iter} files(1MB): {stopwatch.ElapsedMilliseconds}ms");
        }

        [RuntimeTest]
        public void DeleteIterFiles(int iter)
        {
            var autoPath = AutoPath;
            CreateDirectoryFor(Path.Combine(autoPath, "0"));

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < iter; i++)
            {
                var path = Path.Combine(autoPath, i.ToString());
                File.Delete(path);
            }
            stopwatch.Stop();
            Debug.Log($"Delete {iter} files: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
