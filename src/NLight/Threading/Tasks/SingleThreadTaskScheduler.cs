// Author(s): Sébastien Lorion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace NLight.Threading.Tasks
{
	public sealed class SingleThreadTaskScheduler
		: TaskScheduler, IDisposable
	{
		private readonly Thread _thread;
		private readonly BlockingCollection<Task> _queue = new BlockingCollection<Task>();

		public SingleThreadTaskScheduler(string name = null, ThreadPriority priority = ThreadPriority.Normal, ApartmentState apartmentState = ApartmentState.STA)
		{
#if DEBUG
			_allocStackTrace = new StackTrace();
#endif

			_thread = new Thread(
				() =>
				{
					try
					{
						foreach (var task in _queue.GetConsumingEnumerable())
							TryExecuteTask(task);
					}
					finally
					{
						_queue.Dispose();
					}
				});

			_thread.IsBackground = true;
			_thread.Name = name;
			_thread.Priority = priority;
			_thread.SetApartmentState(apartmentState);

			_thread.Start();
		}

		public override int MaximumConcurrencyLevel => 1;

		protected override IEnumerable<Task> GetScheduledTasks() => _queue.ToArray();

		protected override void QueueTask(Task task)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			ValidateIsDisposed();

			if (!_thread.IsAlive)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionMessages.Threading_UnderlyingThreadNotAlive, _thread.Name, _thread.ManagedThreadId));

			_queue.Add(task);
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			ValidateIsDisposed();

			if (Thread.CurrentThread != _thread)
				return false;
			else
			{
				TryExecuteTask(task);
				return true;
			}
		}

		#region IDisposable members

#pragma warning disable CS0649
		private readonly StackTrace _allocStackTrace;
#pragma warning restore CS0649

		public bool IsDisposed { get; private set; }

		public TimeSpan DisposeTimeout { get; set; } = Timeout.InfiniteTimeSpan;

		private void ValidateIsDisposed()
		{
			if (this.IsDisposed)
				throw new ObjectDisposedException(nameof(SingleThreadTaskScheduler));
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this.IsDisposed && _queue != null)
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

						if (!_thread.Join(this.DisposeTimeout))
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
				finally
				{
					this.IsDisposed = true;
				}
			}
		}

		~SingleThreadTaskScheduler()
		{
			Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Shared_InstanceNotDisposedCorrectly, _allocStackTrace);
			Dispose(false);
		}

		#endregion
	}
}