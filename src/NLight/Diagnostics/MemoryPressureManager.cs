// Author(s): Sébastien Lorion

using System;
using System.Threading;

namespace NLight.Diagnostics
{
	/// <summary>
	/// Manages the garbage collector memory pressure.
	/// See <see cref="GC.AddMemoryPressure(long)"/> and <see cref="GC.RemoveMemoryPressure(long)"/>.
	/// </summary>
	public static class MemoryPressureManager
	{
		// only add pressure in 512KB chunks
		private const long _threshold = 512 * 1024;

		private static long _pressure;
		private static long _committedPressure;

		private static readonly object _lock = new object();

		/// <summary>
		/// Informs the runtime of a large allocation of unmanaged memory that should be taken into account when scheduling garbage collection.
		/// </summary>
		/// <param name="bytesAllocated">The incremental amount of unmanaged memory that has been allocated.</param>
		public static void AddMemoryPressure(long bytesAllocated)
		{
			Interlocked.Add(ref _pressure, bytesAllocated);
			PressureCheck();
		}

		/// <summary>
		/// Informs the runtime that unmanaged memory has been released and no longer needs to be taken into account when scheduling garbage collection.
		/// </summary>
		/// <param name="bytesAllocated">The amount of unmanaged memory that has been released.</param>
		public static void RemoveMemoryPressure(long bytesAllocated)
		{
			AddMemoryPressure(-bytesAllocated);
		}

		private static void PressureCheck()
		{
			// double check lock pattern
			if (Math.Abs(_pressure - _committedPressure) >= _threshold)
			{
				lock (_lock)
				{
					long diff = _pressure - _committedPressure;

					if (Math.Abs(diff) >= _threshold)
					{
						if (diff < 0)
							GC.RemoveMemoryPressure(-diff);
						else
							GC.AddMemoryPressure(diff);

						_committedPressure += diff;
					}
				}
			}
		}
	}
}