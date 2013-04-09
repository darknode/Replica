using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Replica.Tests
{
	[TestFixture]
	public sealed class GenericDictionaryTests
	{
		[Test]
		public void ValueTypeDictionaryTV_Default()
		{
			var src = new ValueTypeGenericDictionaryContainer();
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Ints, dst.Ints);
		}

		[Test]
		public void ValueTypeDictionaryTV_Initialized()
		{
			var src = new ValueTypeGenericDictionaryContainer { Ints = new Dictionary<int, int> { {1, 1} , {2, 2}, {3, 3} } };
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Ints.Count, dst.Ints.Count);

			var srcPairArray = src.Ints.ToArray();
			var dstPairArray = dst.Ints.ToArray();
			for (var i = 0; i < srcPairArray.Length; i++)
			{
				Assert.AreEqual(srcPairArray[i].Key, dstPairArray[i].Key);
				Assert.AreEqual(srcPairArray[i].Value, dstPairArray[i].Value);
			}
		}

		[Test]
		public void StringDictionaryTV_Default()
		{
			var src = new StringGenericDictionaryContainer();
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Strings, dst.Strings);
		}

		[Test]
		public void StringDictionaryTV_Initialized()
		{
			var src = new StringGenericDictionaryContainer { Strings = new Dictionary<string, string> { { "1", "1" }, { "2", "2" }, { "3", "3" } } };
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Strings.Count, dst.Strings.Count);
			foreach (var srcPair in src.Strings)
			{
				Assert.IsTrue(dst.Strings.ContainsKey(srcPair.Key));
				Assert.AreEqual(dst.Strings[srcPair.Key], srcPair.Value);
			}

			var srcPairArray = src.Strings.ToArray();
			var dstPairArray = dst.Strings.ToArray();
			for (var i = 0; i < srcPairArray.Length; i++)
			{
				Assert.AreEqual(srcPairArray[i].Key, dstPairArray[i].Key);
				Assert.AreEqual(srcPairArray[i].Value, dstPairArray[i].Value);
			}
		}

		[Test]
		public void RefTypeDictionaryTV_Default()
		{
			var src = new RefTypeGenericDictionaryContainer();
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Classes, dst.Classes);
		}

		[Test]
		public void RefTypeDictionaryTV_Initialized()
		{
			var src = new RefTypeGenericDictionaryContainer
			{
				Classes = new Dictionary<OnePropertyClass, OnePropertyClass>
				{
					{new OnePropertyClass { IntProtperty = 1 },new OnePropertyClass { IntProtperty = 1 }},
					{new OnePropertyClass { IntProtperty = 2 },new OnePropertyClass { IntProtperty = 2 }},
					{new OnePropertyClass { IntProtperty = 3 },new OnePropertyClass { IntProtperty = 3 }},
				}};
			var dst = CloneHelper.Clone(src);

			Assert.AreEqual(src.Classes.Count, dst.Classes.Count);

			var srcPairArray = src.Classes.ToArray();
			var dstPairArray = dst.Classes.ToArray();
			for (var i = 0; i < srcPairArray.Length; i++)
			{
				Assert.AreNotSame(srcPairArray[i].Key, dstPairArray[i].Key);
				Assert.AreEqual(srcPairArray[i].Key.IntProtperty, dstPairArray[i].Key.IntProtperty);

				Assert.AreNotSame(srcPairArray[i].Value, dstPairArray[i].Value);
				Assert.AreEqual(srcPairArray[i].Value.IntProtperty, dstPairArray[i].Value.IntProtperty);
			}
		}
	}
}
