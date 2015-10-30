// Author(s): Sébastien Lorion

using System;
using System.Diagnostics;

namespace NLight.Reactive
{
	public class BehaviorSubjectSlim<T>
		: SubjectSlim<T>
	{
		private bool _hasValue;
		private T _value;

		public BehaviorSubjectSlim()
		{
		}

		public BehaviorSubjectSlim(T value)
			: this()
		{
			_hasValue = true;
			_value = value;
		}

		public T Value { get { return _value; } }

		protected override void OnNextCore(T value)
		{
			base.OnNextCore(value);
			_value = value;
			_hasValue = true;
		}

		protected override void OnSubscriptionAdded(IObserver<T> observer)
		{
			base.OnSubscriptionAdded(observer);

			if (this.IsCompleted)
			{
				try { observer.OnCompleted(); }
				catch (Exception ex) { Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Reactive_ErrorWhenNotifyingObserver, nameof(observer.OnCompleted), observer, ex); }
			}
			else if (this.Error != null)
			{
				try { observer.OnError(this.Error); }
				catch (Exception ex) { Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Reactive_ErrorWhenNotifyingObserver, nameof(observer.OnError), observer, ex); }
			}
			else if (_hasValue)
			{
				try { observer.OnNext(this.Value); }
				catch (Exception ex) { Log.Source.TraceEvent(TraceEventType.Warning, 0, Resources.LogMessages.Reactive_ErrorWhenNotifyingObserver, nameof(observer.OnNext), observer, ex); }
			}
		}
	}
}
