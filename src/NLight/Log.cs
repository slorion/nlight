// Author(s): Sébastien Lorion

using System.Diagnostics;

namespace NLight
{
	internal static class Log
	{
		public static TraceSource Source => new TraceSource(typeof(Log).Namespace);
	}
}