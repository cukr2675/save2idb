using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Linq;

namespace Save2IDB.Samples
{
    public class EasyTest : RuntimeTestClass
    {
        [RuntimeTest]
        public void WriteRead()
        {
            CreateDirectoryFor(AutoPath);
            using (var stream = IDBFile.Open(AutoPath, FileMode.Create))
            {
                stream.Write(TestBytes);
            }
            Debug.Log($"Writed to {AutoPath}");

            AssertWriteTo(AutoPath, TestBytes);
        }

        [RuntimeTest]
        public void Update()
        {
            CreateDirectoryFor(AutoPath);
            var updateBytes = Enumerable.Range(0, 256).OfType<byte>().Reverse().ToArray();
            using (var stream = IDBFile.Open(AutoPath, FileMode.Open))
            {
                stream.Write(updateBytes);
            }
            Debug.Log($"Updated {AutoPath}");

            AssertWriteTo(AutoPath, updateBytes);
        }

        [RuntimeTest]
        public void Delete()
        {
            CreateDirectoryFor(AutoPath);
            File.Delete(AutoPath);
            Debug.Log($"Deleted {AutoPath}");

            if (File.Exists(AutoPath)) throw new RuntimeTestFailedException($"Delete failed.");
        }
    }
}
