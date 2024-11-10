using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq;
using UnityEngine.Events;

namespace Save2IDB.Samples
{
    public abstract class RuntimeTestClass
    {
        protected string TestPath => Path.Combine(Application.persistentDataPath, "UnitTesting");

        /// <summary>
        /// Path.Combine(TestPath, nameof([class]))
        /// </summary>
        protected string AutoPath
        {
            get
            {
                //var stackTrace = new StackTrace(1);
                //var methodName = stackTrace.GetFrame(0).GetMethod().Name;
                var className = GetType().Name;
                return Path.Combine(TestPath, className); //, methodName);
            }
        }

        protected ReadOnlySpan<byte> TestBytes => _testBytes;
        private static readonly byte[] _testBytes = Encoding.UTF8.GetBytes("This is testing text.");

        protected void CreateDirectoryFor(string path)
        {
            var directory = Directory.GetParent(path);
            if (directory.Exists) return;

            directory.Create();
        }

        protected void AssertWriteTo(string path, ReadOnlySpan<byte> expectedBytes)
        {
            using var stream = IDBFile.Open(path, FileMode.Open);
            var actualBytes = new Span<byte>(new byte[expectedBytes.Length]);
            stream.Read(actualBytes);
            if (actualBytes.SequenceEqual(expectedBytes)) return;

            throw new RuntimeTestFailedException($"{nameof(actualBytes)} is not equal to {nameof(expectedBytes)}.");
        }

        public IEnumerable<(string name, UnityAction action)> GetUnitTests()
        {
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<RuntimeTestAttribute>();
                if (attribute == null) continue;

                yield return (method.Name, (UnityAction)method.CreateDelegate(typeof(UnityAction), this));
            }
        }

        public IEnumerable<(string name, Action<int> action)> GetPerformanceTests()
        {
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<RuntimeTestAttribute>();
                if (attribute == null) continue;

                yield return (method.Name, iter => method.Invoke(this, new object[] { iter }));
            }
        }
    }
}
