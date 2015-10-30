// Author(s): Sébastien Lorion

using System;
using System.Threading.Tasks;

namespace NLight.Threading.Tasks
{
	public static class TaskFactoryExtensions
	{
		public static Task StartNew(this TaskFactory factory, Action action, TaskScheduler scheduler)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));

			return factory.StartNew(action, factory.CancellationToken, factory.CreationOptions, scheduler);
		}

		public static Task<T> StartNew<T>(this TaskFactory factory, Func<T> action, TaskScheduler scheduler)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));

			return factory.StartNew(action, factory.CancellationToken, factory.CreationOptions, scheduler);
		}
	}
}