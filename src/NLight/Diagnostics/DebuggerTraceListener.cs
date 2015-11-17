// Author(s): Sébastien Lorion
// inspired by https://github.com/akkadotnet/akka.net/pull/1424

using System.Diagnostics;

namespace NLight.Diagnostics
{
	/// <summary>
	/// A trace listener that will break in the debugger when the <see cref="TraceEventType"/> is equal or lower 
	/// than the value specified by <see cref="BreakOnEventType"/> (by default <see cref="TraceEventType.Error"/>).
	/// </summary>
	public class DebuggerTraceListener
		: TraceListener
	{
		public bool BreakOnEventEnabled { get; set; } = true;
		public TraceEventType BreakOnEventType { get; set; } = TraceEventType.Error;

		public override void Write(string message) => Debug.Write(message);
		public override void WriteLine(string message) => Debug.WriteLine(message);

		public override void Fail(string message)
		{
			base.Fail(message);
			BreakOnEvent(TraceEventType.Error);
		}

		public override void Fail(string message, string detailMessage)
		{
			base.Fail(message, detailMessage);
			BreakOnEvent(TraceEventType.Error);
		}

		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			base.TraceData(eventCache, source, eventType, id, data);
			BreakOnEvent(eventType);
		}

		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
		{
			base.TraceData(eventCache, source, eventType, id, data);
			BreakOnEvent(eventType);
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
		{
			base.TraceEvent(eventCache, source, eventType, id);
			BreakOnEvent(eventType);
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
		{
			base.TraceEvent(eventCache, source, eventType, id, format, args);
			BreakOnEvent(eventType);
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
			base.TraceEvent(eventCache, source, eventType, id, message);
			BreakOnEvent(eventType);
		}

		private void BreakOnEvent(TraceEventType eventType)
		{
			if (this.BreakOnEventEnabled && eventType <= this.BreakOnEventType && Debugger.IsAttached)
				Debugger.Break();
		}
	}
}