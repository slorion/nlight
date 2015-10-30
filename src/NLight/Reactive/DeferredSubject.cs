// Author(s): Sébastien Lorion

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace NLight.Reactive
{
	public class DeferredSubject<T>
		: ISubject<T>, IDisposable
	{
		private readonly BlockingCollection<Notification<T>> _queue = new BlockingCollection<Notification<T>>();
		private readonly ISubject<T> _inner;
		private readonly Task _listeningTask;

		public DeferredSubject(ISubject<T> subject)
		{
#if DEBUG
			_allocStackTrace = new StackTrace();
#endif

			if (subject == null) throw new ArgumentNullException(nameof(subject));

			_inner = subject;
			_listeningTask = Task.Run(
				() =>
				{
					try
					{
						foreach (var notification in _queue.GetConsumingEnumerable())
							notification.Accept(_inner);
					}
					finally
					{
						_queue.Dispose();
					}
				});
		}

		public void OnCompleted()
		{
			_queue.Add(Notification.CreateOnCompleted<T>());
			_queue.CompleteAdding();
		}

		public void OnError(Exception error)
		{
			_queue.Add(Notification.CreateOnError<T>(error));
			_queue.CompleteAdding();
		}

		public void OnNext(T value)
		{
			_queue.Add(Notification.CreateOnNext<T>(value));
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return _inner.Subscribe(observer);
		}

		#region IDisposable members

#pragma warning disable CS0649
		private readonly StackTrace _allocStackTrace;
#pragma warning restore CS0649

		public TimeSpan DisposeTimeout { get; set; } = Timeout.InfiniteTimeSpan;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_queue != null)
			{
				try
				{
					try
					{
						try
						{
							var count = _queue.Count;
							if (count > 0)
								Log.Source.TraceEvent(TraceEventType.Verbose, 0, Resources.LogMessages.Shared_NonProcessedItemsWhenDisposing, count);

							_queue.CompleteAdding();
						}
						catch (ObjectDisposedException) { }

						if (!_listeningTask.Wait(this.DisposeTimeout))
							Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Shared_NonProcessedItemsAfterDisposeTimeout, this.DisposeTimeout);
					}
					finally
					{
						_queue.Dispose();
					}
				}
				catch (Exception ex) when (!disposing)
				{
					Log.Source.TraceEvent(TraceEventType.Error, 0, Resources.LogMessages.Shared_ExceptionDuringFinalization, ex);
				}
			}
		}

		~DeferredSubject()
		{
			Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Shared_InstanceNotDisposedCorrectly, _allocStackTrace);
			Dispose(false);
		}

		#endregion
	}
}