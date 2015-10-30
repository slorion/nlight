// Author(s): Sébastien Lorion

using System;
using System.Reactive.Subjects;

namespace NLight.Reactive
{
	public static class SubjectExtensions
	{
		public static DeferredSubject<T> ToDeferred<T>(this ISubject<T> subject)
		{
			if (subject == null) throw new ArgumentNullException(nameof(subject));

			return new DeferredSubject<T>(subject);
		}
	}
}