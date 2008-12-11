using System;
using System.Diagnostics;

namespace Apache.NMS.Test
{
	public class NmsTracer : Apache.NMS.ITrace
    {
        #region ITrace Members
        public void Debug(string message)
        {
            Trace.WriteLine("DEBUG: " + message);
        }

        public void Error(string message)
        {
            Trace.WriteLine("ERROR: " + message);
        }

        public void Fatal(object message)
        {
            Trace.WriteLine("FATAL: " + message);
        }

        public void Info(string message)
        {
            Trace.WriteLine("INFO:  " + message);
        }

        public void Warn(string message)
        {
            Trace.WriteLine("WARN:  " + message);
        }

        public bool IsDebugEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsFatalEnabled
        {
            get { return true; }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsWarnEnabled
        {
            get { return true; }
        }

        #endregion
    }
}
