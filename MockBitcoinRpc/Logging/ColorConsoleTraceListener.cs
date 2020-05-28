using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MockBitcoinRpc.Logging
{
	public class ColorConsoleTraceListener : TraceListener
	{
		private static readonly Dictionary<TraceEventType, ConsoleColor> EventColor = new Dictionary<TraceEventType, ConsoleColor>
		{
			[TraceEventType.Verbose] = ConsoleColor.DarkGray,
			[TraceEventType.Information] = ConsoleColor.Gray,
			[TraceEventType.Warning] = ConsoleColor.Yellow,
			[TraceEventType.Error] = ConsoleColor.DarkRed,
			[TraceEventType.Critical] = ConsoleColor.Red,
			[TraceEventType.Start] = ConsoleColor.DarkCyan,
			[TraceEventType.Stop] = ConsoleColor.DarkCyan
		};

		public ColorConsoleTraceListener(int identLevel)
		{
			_il = identLevel;
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
			TraceEvent(eventCache, source, eventType, id, "{0}", message);
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
		{
			IndentLevel = _il;
			IndentSize = 2;
			var originalColor = Console.ForegroundColor;
			Console.ForegroundColor = GetEventColor(eventType, originalColor);
			Console.WriteLine($"{DateTime.Now.TimeOfDay} {source} {eventType}: {string.Format(format, args)}");
			Console.ForegroundColor = originalColor;
		}

		private static ConsoleColor GetEventColor(TraceEventType eventType, ConsoleColor defaultColor)
		{
			return !EventColor.ContainsKey(eventType) ? defaultColor : EventColor[eventType];
		}

		public override void Write(string message)
		{
			Console.Write(message);
		}

		public override void WriteLine(string message)
		{
			Console.WriteLine(message);
		}

		public int _il { get; set; }
	}
}
