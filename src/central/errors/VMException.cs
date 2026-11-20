using System;
using VMRuntime.Logging;

namespace VMRuntime.Errors
{
    public class VMException : Exception
    {
        public VMException(string message) : base(message) { }
        public VMException(string message, Exception inner) : base(message, inner) { }

        public virtual void Report()
        {
            string formatted = ToString();
            PrintManager.Current.PrintError(formatted);
            LogManager.Current.LogError(formatted);
        }
    }
}