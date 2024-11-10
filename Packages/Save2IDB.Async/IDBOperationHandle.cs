using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Save2IDB.Async
{
    public class IDBOperationHandle : IEnumerator
    {
        public IDBOperationStatus Status { get; protected set; }
        public string ErrorMsg { get; protected set; }

        private readonly object _current;
        object IEnumerator.Current => _current;

        public event System.Action Completed;

        internal IDBOperationHandle()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _current = new WaitWhile(() => Status == IDBOperationStatus.InProgress);
            Status = IDBOperationStatus.InProgress;
#else
            // WebGL でなければ最初から失敗扱い
            _current = null;
            Status = IDBOperationStatus.Failed;
#endif
        }

        internal IDBOperationHandle(IDBOperationHandle handle, System.Action<IDBOperationHandle> action)
            : base()
        {
            handle.Completed = () => action(this);

            if (handle.Status == IDBOperationStatus.Succeeded) { handle.Completed(); }
        }

        internal static IDBOperationHandle DoneInstance()
        {
            var operationHandle = new IDBOperationHandle();
            operationHandle.Done();
            return operationHandle;
        }

        internal void Done()
        {
            Status = IDBOperationStatus.Succeeded;
            Completed?.Invoke();
        }

        internal void Error(string errorMsg)
        {
            ErrorMsg = errorMsg;
            Status = IDBOperationStatus.Failed;
            Completed?.Invoke();
        }

        bool IEnumerator.MoveNext()
        {
            return Status == IDBOperationStatus.InProgress;
        }

        void IEnumerator.Reset()
        {
            ErrorMsg = default;
            Status = IDBOperationStatus.InProgress;
        }
    }

    public class IDBOperationHandle<T> : IDBOperationHandle, IEnumerator
    {
        public T Result { get; private set; }

        object IEnumerator.Current => null;

        internal IDBOperationHandle() { }

        internal void Done(T result)
        {
            Result = result;
            Status = IDBOperationStatus.Succeeded;
        }

        void IEnumerator.Reset()
        {
            Result = default;
            ErrorMsg = default;
            Status = IDBOperationStatus.InProgress;
        }
    }
}
