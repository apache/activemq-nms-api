using System;
using System.Text;

namespace ActiveMQ
{
	public sealed class Tracer 
	{
		private static ITrace s_trace = null;

		// prevent instantiation of this class. All methods are static.
		private Tracer()
		{
		}

		public static ITrace Trace
		{
			get { return s_trace; }
			set { s_trace = value; }
		}

        public static void Debug(object message)
		{
			if (s_trace!=null && s_trace.IsDebugEnabled)
				s_trace.Debug(message.ToString());
		}

        public static void DebugFormat(string format, params object[] args)
		{
			if (s_trace != null && s_trace.IsDebugEnabled)
				s_trace.Debug(string.Format(format, args));
		}

		public static void Info(object message)
		{
			if (s_trace != null && s_trace.IsInfoEnabled)
				s_trace.Info(message.ToString());
		}

        public static void InfoFormat(string format, params object[] args)
		{
			if (s_trace != null && s_trace.IsInfoEnabled)
				s_trace.Info(string.Format(format, args));
		}

        public static void Warn(object message)
		{
			if (s_trace != null && s_trace.IsWarnEnabled)
				s_trace.Warn(message.ToString());
		}

        public static void WarnFormat(string format, params object[] args)
		{
			if (s_trace != null && s_trace.IsWarnEnabled)
				s_trace.Warn(string.Format(format, args));
		}

        public static void Error(object message)
		{
			if (s_trace != null && s_trace.IsErrorEnabled)
				s_trace.Error(message.ToString());
		}

        public static void ErrorFormat(string format, params object[] args)
		{
			if (s_trace != null && s_trace.IsErrorEnabled)
				s_trace.Error(string.Format(format, args));
		}

        public static void Fatal(object message)
		{
			if (s_trace != null && s_trace.IsFatalEnabled)
				s_trace.Fatal(message.ToString());
		}

        public static void FatalFormat(string format, params object[] args)
		{
			if (s_trace != null && s_trace.IsFatalEnabled)
				s_trace.Fatal(string.Format(format, args));
		}

		public static bool IsDebugEnabled 
		{
			get { return s_trace != null && s_trace.IsDebugEnabled; } 
		}

		public static bool IsInfoEnabled
		{
			get { return s_trace != null && s_trace.IsDebugEnabled; } 
		}

		public static bool IsWarnEnabled
		{
			get { return s_trace != null && s_trace.IsDebugEnabled; } 
		}

		public static bool IsErrorEnabled
		{
			get { return s_trace != null && s_trace.IsDebugEnabled; } 
		}

		public static bool IsFatalEnabled
		{
			get { return s_trace != null && s_trace.IsDebugEnabled; } 
		}
	}
}
