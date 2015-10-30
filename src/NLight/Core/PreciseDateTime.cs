// Author(s): Sébastien Lorion

using System;
using System.Runtime.InteropServices;

namespace NLight.Core
{
	public static class PreciseDateTime
	{
		// kernel function is available in Windows 8/2012+
		// https://msdn.microsoft.com/en-us/library/windows/desktop/hh706895%28v=vs.85%29.aspx
		[DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
		private static extern void GetSystemTimePreciseAsFileTime(out long filetime);

		private static readonly Func<DateTimeOffset> _getNow;

		static PreciseDateTime()
		{
			try
			{
				_getNow =
					() =>
					{
						long filetime;
						GetSystemTimePreciseAsFileTime(out filetime);
						return DateTimeOffset.FromFileTime(filetime);
					};
			}
			catch (EntryPointNotFoundException)
			{
				_getNow = () => DateTimeOffset.Now;
			}
		}

		public static DateTimeOffset Now => _getNow();
	}
}