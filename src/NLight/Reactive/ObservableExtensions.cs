// Author(s): Sébastien Lorion

using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NLight.Reactive
{
	public static class ObservableExtensions
	{
		private static readonly object _true = new object();
		private static readonly object _false = null;

		public static IObservable<TOut> IgnoreElementsWhileConverting<TIn, TOut>(this IObservable<TIn> source, Func<TIn, Task<TOut>> convert, Action<TIn> ignoreAction = null)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (convert == null) throw new ArgumentNullException(nameof(convert));

			object isConverting = _false;

			return source
				.SelectMany(
					async data =>
					{
						if (Interlocked.CompareExchange(ref isConverting, _true, _false) == _true)
						{
							if (ignoreAction != null)
								ignoreAction(data);

							return Tuple.Create(false, default(TOut));
						}
						else
						{
							try
							{
								return Tuple.Create(true, await convert(data).ConfigureAwait(false));
							}
							finally
							{
								Interlocked.CompareExchange(ref isConverting, _false, _true);
							}
						}
					})
				.Where(a => a.Item1)
				.Select(a => a.Item2);
		}

		public static IObservable<Tuple<TSource, TSource>> WithPrevious<TSource>(this IObservable<TSource> source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return source.Scan(
				Tuple.Create(default(TSource), default(TSource)),
				(previous, current) => Tuple.Create(previous.Item2, current));
		}
	}
}