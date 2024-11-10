using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Save2IDB.Samples
{
    public class RuntimeTestFailedException : System.Exception
    {
        public RuntimeTestFailedException(string message) : base(message) { }
    }
}
