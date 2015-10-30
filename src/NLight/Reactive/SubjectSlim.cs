// Author(s): Sébastien Lorion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace NLight.Reactive
{
	public partial class SubjectSlim<T>
		: ISubject<T>
	{
		private readonly object _lock = new object();
		private static volatile List<IObserver<T>> _observers = new List<IObserver<T>>();

		public bool IsCompleted { get; private set; }
		public Exception Error { get; private set; }

		public void OnNext(T value)
		{
			CheckStillActive();

			OnNextCore(value);

			var observers = _observers;
			foreach (var obs in observers)
			{
				try { obs.OnNext(value); }
				catch (Exception ex) { Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Reactive_ErrorWhenNotifyingObserver, nameof(obs.OnNext), obs, ex); }
			}
		}
		protected virtual void OnNextCore(T value) { }

		public void OnCompleted()
		{
			CheckStillActive();

			this.IsCompleted = true;

			OnCompletedCore();

			var observers = _observers;
			foreach (var obs in observers)
			{
				try { obs.OnCompleted(); }
				catch (Exception ex) { Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Reactive_ErrorWhenNotifyingObserver, nameof(obs.OnCompleted), obs, ex); }
			}

			UnsubscribeAll();
		}
		protected virtual void OnCompletedCore() { }

		public void OnError(Exception error)
		{
			if (error == null) throw new ArgumentNullException(nameof(error));

			CheckStillActive();

			this.Error = error;

			OnErrorCore();

			var observers = _observers;
			foreach (var obs in observers)
			{
				try { obs.OnError(error); }
				catch (Exception ex) { Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Reactive_ErrorWhenNotifyingObserver, nameof(obs.OnError), obs, ex); }
			}

			UnsubscribeAll();
		}
		protected virtual void OnErrorCore() { }

		protected virtual void OnSubscriptionAdded(IObserver<T> observer) { }
		protected virtual void OnSubscriptionRemoved(IObserver<T> observer) { }

		public IDisposable Subscribe(IObserver<T> observer)
		{
			if (observer == null) throw new ArgumentNullException(nameof(observer));

			if (this.IsCompleted || this.Error != null)
			{
				OnSubscriptionAdded(observer);
				return Disposable.Empty;
			}
			else
			{
				lock (_lock)
				{
					var observers = new List<IObserver<T>>(_observers);
					observers.Add(observer);
					_observers = observers;
				}

				OnSubscriptionAdded(observer);
				return new Subscription(this, observer);
			}
		}

		private void Unsubscribe(IObserver<T> observer)
		{
			if (observer == null) throw new ArgumentNullException(nameof(observer));

			lock (_lock)
			{
				var observers = new List<IObserver<T>>(_observers);
				observers.Remove(observer);
				_observers = observers;
			}

			OnSubscriptionRemoved(observer);
		}

		private void UnsubscribeAll()
		{
			var observers = _observers;

			lock (_lock) { _observers = new List<IObserver<T>>(); }

			foreach (var obs in observers)
				OnSubscriptionRemoved(obs);
		}

		protected void CheckStillActive()
		{
			if (this.IsCompleted)
				throw new InvalidOperationException(Resources.ExceptionMessages.Reactive_SequenceAlreadyCompleted);

			if (this.Error != null)
				throw new InvalidOperationException(Resources.ExceptionMessages.Reactive_SequenceAlreadyCompletedWithError);
		}
	}
}