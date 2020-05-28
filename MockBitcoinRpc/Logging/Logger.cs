using System.Diagnostics;

namespace MockBitcoinRpc.Logging
{
	public class Log
	{
		private readonly TraceSource _source;
		private static readonly TraceListener Ctl = new ColorConsoleTraceListener(0);

		public Log(TraceSource source)
		{
			_source = source;
			_source.Listeners.Add(Ctl);
		}

		public void Info(string format, params object[] p)
			=> _source.TraceEvent(TraceEventType.Information, 0, format, p);

		public void Verbose(string format, params object[] p)
			=> _source.TraceEvent(TraceEventType.Verbose, 0, format, p);

		public void Warn(string format, params object[] p)
			=> _source.TraceEvent(TraceEventType.Warning, 0, format, p);

		public void Error(string format, params object[] p)
			=> _source.TraceEvent(TraceEventType.Error, 0, format, p);
	}
}
