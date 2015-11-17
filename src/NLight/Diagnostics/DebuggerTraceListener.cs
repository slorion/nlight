// Author(s): Sébastien Lorion
// inspired by https://github.com/akkadotnet/akka.net/pull/1424

using System.Diagnostics;

namespace NLight.Diagnostics
{
	public class DebuggerTraceListener
		: TraceListener
	{
		public bool BreakOnErrorEnabled { get; set; } = true;

		public override void Write(string message) => Debug.Write(message);
		public override void WriteLine(string message) => Debug.WriteLine(message);

		public override void Fail(string message)
		{
			base.Fail(message);
			BreakOnError(TraceEventType.Error);
		}

		public override void Fail(string message, string detailMessage)
		{
			base.Fail(message, detailMessage);
			BreakOnError(TraceEventType.Error);
		}

		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			base.TraceData(eventCache, source, eventType, id, data);
			BreakOnError(eventType);
		}

		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
		{
			base.TraceData(eventCache, source, eventType, id, data);
			BreakOnError(eventType);
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
		{
			base.TraceEvent(eventCache, source, eventType, id);
			BreakOnError(eventType);
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
		{
			base.TraceEvent(eventCache, source, eventType, id, format, args);
			BreakOnError(eventType);
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
			base.TraceEvent(eventCache, source, eventType, id, message);
			BreakOnError(eventType);
		}

		private void BreakOnError(TraceEventType eventType)
		{
			if (this.BreakOnErrorEnabled && Debugger.IsAttached)
			{
				if (eventType == TraceEventType.Critical || eventType == TraceEventType.Error)
					Debugger.Break();
			}
		}
	}
}