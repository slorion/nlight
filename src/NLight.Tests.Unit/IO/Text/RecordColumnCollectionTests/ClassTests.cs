// Author(s): Sébastien Lorion

using NLight.IO.Text;
using NUnit.Framework;

namespace NLight.Tests.Unit.IO.Text.RecordColumnCollectionTests
{
	public partial class ClassTests
	{
		[Test]
		public void AddItemWhenEmptyTest()
		{
			var column = new RecordColumn("test");

			var columns = new RecordColumnCollection<RecordColumn>();
			columns.Add(column);

			Assert.AreEqual(1, columns.Count);
			Assert.AreEqual(column, columns[0]);
			Assert.AreEqual(column, columns["test"]);
		}
	}
}