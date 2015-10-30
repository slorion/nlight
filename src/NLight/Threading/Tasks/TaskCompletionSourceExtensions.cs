// Author(s): Sébastien Lorion

using System;
using System.Threading.Tasks;

namespace NLight.Threading.Tasks
{
	public static class TaskCompletionSourceExtensions
	{
		public static void Run<T>(this TaskCompletionSource<T> tcs, Action operation) => Run(tcs, () => { operation(); return default(T); });

		public static void Run<T>(this TaskCompletionSource<T> tcs, Func<T> operation)
		{
			if (tcs == null) throw new ArgumentNullException(nameof(tcs));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			try
			{
				tcs.SetResult(operation());
			}
			catch (Exception ex)
			{
				tcs.SetException(ex);
			}
		}
	}
}