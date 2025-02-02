using System;
using UnityEngine;

public class ExceptionAbout<_> : ApplicationException {
    public ExceptionAbout() {}
    public ExceptionAbout(string message) : base(message) {}
    public ExceptionAbout(string message, Exception innerException) : base(message, innerException) {}
}
