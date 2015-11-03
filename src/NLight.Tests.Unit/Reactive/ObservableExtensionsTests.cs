// Author(s): Sébastien Lorion

using NLight.Reactive;
using NUnit.Framework;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace NLight.Tests.Unit.Reactive
{
	public static class ObservableExtensionsTests
	{
		[Test]
		public static async Task IgnoreElementsWhileConvertingTest()
		{
			var input = new SubjectSlim<int>();

			var output = input
				.IgnoreElementsWhileConverting(
					async i =>
					{
						await Task.Delay(75).ConfigureAwait(false);
						return i * 1000;
					})
				.Replay();

			using (output.Connect())
			{
				for (int i = 0; i < 10; i++)
				{
					input.OnNext(i);
					await Task.Delay(50).ConfigureAwait(false);
				}
				input.OnCompleted();

				Assert.That(output.ToEnumerable(), Is.EqualTo(new[] { 0, 2000, 4000, 6000, 8000 }));
			}
		}

		[Test]
		public static void WithPreviousTest()
		{
			var input = new SubjectSlim<int?>();
			var output = input.WithPrevious().Replay();

			using (output.Connect())
			{
				for (int i = 0; i < 5; i++)
					input.OnNext(i);
				input.OnCompleted();

				Assert.That(output.ToEnumerable(), Is.EqualTo(
					new[] {
						Tuple.Create<int?, int?>(null, 0),
						Tuple.Create<int?, int?>(0, 1),
						Tuple.Create<int?, int?>(1, 2),
						Tuple.Create<int?, int?>(2, 3),
						Tuple.Create<int?, int?>(3, 4)
					}));
			}
		}
	}
}