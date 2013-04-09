using System.Collections.Generic;
using NUnit.Framework;

namespace Replica.Tests
{
	[TestFixture]
	public sealed class EnumerableTTests
	{
		[Test]
		public void ValueTypeEnumerableT_Default()
		{
			var src = new ValueTypeEnumerableTContainer();
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Ints, dst.Ints);
		}

		[Test]
		public void ValueTypeEnumerableT_Initialized()
		{
			var src = new ValueTypeEnumerableTContainer { Ints = new List<int> { 1, 2, 3 } };
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Ints.Count, dst.Ints.Count);
			for (var i = 0; i < src.Ints.Count; i++)
			{
				Assert.AreEqual(src.Ints[i], dst.Ints[i]);
			}
		}

		[Test]
		public void StringEnumerableT_Default()
		{
			var src = new StringEnumerableTContainer();
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Strings, dst.Strings);
		}

		[Test]
		public void StringEnumerableT_Initialized()
		{
			var src = new StringEnumerableTContainer { Strings = new List<string> { "1", "2", "3" } };
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Strings.Count, dst.Strings.Count);
			for (var i = 0; i < src.Strings.Count; i++)
			{
				Assert.AreEqual(src.Strings[i], dst.Strings[i]);
			}
		}

		[Test]
		public void RefTypeEnumerableT_Default()
		{
			var src = new RefTypeEnumerableTContainer();
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Classes, dst.Classes);
		}

		[Test]
		public void RefTypeEnumerableT_Initialized()
		{
			var src = new RefTypeEnumerableTContainer
			{
				Classes = new List<OnePropertyClass>
				{
					new OnePropertyClass { IntProtperty = 1 },
					new OnePropertyClass { IntProtperty = 2 },
					new OnePropertyClass { IntProtperty = 3 }
				}};
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Classes.Count, dst.Classes.Count);
			for (var i = 0; i < src.Classes.Count; i++)
			{
				Assert.AreNotSame(src.Classes[i], dst.Classes[i]);
				Assert.AreEqual(src.Classes[i].IntProtperty, dst.Classes[i].IntProtperty);				
			}
		}
	}
}
