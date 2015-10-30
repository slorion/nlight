// Author(s): Sébastien Lorion

using NLight.Threading.Tasks;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace NLight.Threading
{
	public static class ThreadHelper
	{
		public static Task RunInNewThread(Action action, string threadName = null, ThreadPriority priority = ThreadPriority.Normal, ApartmentState apartmentState = ApartmentState.STA, CultureInfo culture = null, CultureInfo uiCulture = null)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			var tcs = new TaskCompletionSource<object>();

			var thread =
				new Thread(() => tcs.Run(action))
				{
					Name = threadName,
					Priority = priority,
					CurrentCulture = culture ?? CultureInfo.CurrentCulture,
					CurrentUICulture = uiCulture ?? CultureInfo.CurrentUICulture
				};

			thread.SetApartmentState(apartmentState);
			thread.Start();

			return tcs.Task;
		}	
	}
}