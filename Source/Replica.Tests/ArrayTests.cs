using NUnit.Framework;

namespace Replica.Tests
{
	[TestFixture]
	public sealed class ArrayTests
	{
		[Test]
		public void ValueTypeArray_Default()
		{
			var src = new ValueTypeArrayContainer();
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Ints, dst.Ints);
		}

		[Test]
		public void ValueTypeArray_Initialized()
		{
			var src = new ValueTypeArrayContainer { Ints = new[] { 1, 2, 3} };
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Ints.Length, dst.Ints.Length);
			for (var i = 0; i < src.Ints.Length; i++)
			{
				Assert.AreEqual(src.Ints[i], dst.Ints[i]);
			}
		}

		[Test]
		public void StringArray_Default()
		{
			var src = new StringArrayContainer();
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Strings, dst.Strings);
		}

		[Test]
		public void StringArray_Initialized()
		{
			var src = new StringArrayContainer { Strings = new[] { "1", "2", "3" } };
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Strings.Length, dst.Strings.Length);
			for (var i = 0; i < src.Strings.Length; i++)
			{
				Assert.AreEqual(src.Strings[i], dst.Strings[i]);
			}
		}

		[Test]
		public void RefTypeArray_Default()
		{
			var src = new RefTypeArrayContainer();
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Classes, dst.Classes);
		}

		[Test]
		public void RefTypeArray_Initialized()
		{
			var src = new RefTypeArrayContainer { Classes = new[]
				{
					new OnePropertyClass { IntProtperty = 1 },
					new OnePropertyClass { IntProtperty = 2 },
					new OnePropertyClass { IntProtperty = 3 }
				}};
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Classes.Length, dst.Classes.Length);
			for (var i = 0; i < src.Classes.Length; i++)
			{
				Assert.AreNotSame(src.Classes[i], dst.Classes[i]);
				Assert.AreEqual(src.Classes[i].IntProtperty, dst.Classes[i].IntProtperty);				
			}
		}
	}
}