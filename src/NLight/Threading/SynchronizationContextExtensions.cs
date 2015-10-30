// Author(s): Sébastien Lorion

using NLight.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NLight.Threading
{
	public static class SynchronizationContextExtensions
	{
		public static Task Dispatch(this SynchronizationContext context, Action action, bool asynchronous = true)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (action == null) throw new ArgumentNullException(nameof(action));

			var tcs = new TaskCompletionSource<object>();

			if (asynchronous)
				context.Post(_ => tcs.Run(action), null);
			else
				context.Send(_ => tcs.Run(action), null);

			return tcs.Task;
		}
	}
}