using System.Reflection;
using NUnit.Framework;

namespace Replica.Tests
{
	[TestFixture]
	public sealed class CommonTests
	{
		[Test]
		public void LookupType_PublicPropertiesAndFields()
		{
			var lookupStrategy = new LookupStrategy(LookupType.PublicPropertiesAndFields);
			var source = new AccessLevelTestClass
			{
				Field = 4,
				PublicProperty = 5
			};

			source.InitPrivates();

			var engine = new CloneEngine(lookupStrategy);
			var target = engine.Clone(source);

			Assert.AreEqual(source.Field, target.Field);
			Assert.AreEqual(source.PublicProperty, target.PublicProperty);
			
			Assert.AreNotEqual(
				source.PrivateProperty2,
				target.PrivateProperty2,
				"S# {0}\n\rT# {1}", source, target
			);

			var type = typeof(AccessLevelTestClass);

			var sourceFieldValue = (int)type.GetField("_field", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);
			var targetFieldValue = (int)type.GetField("_field", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
			Assert.AreNotEqual(sourceFieldValue, targetFieldValue);

			var sourceProperty1Value = (int)type.GetProperty("PrivateProperty1", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source, null);
			var targetProperty1Value = (int)type.GetProperty("PrivateProperty1", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target, null);
			Assert.AreNotEqual(sourceProperty1Value, targetProperty1Value);
		}

		[Test]
		public void LookupType_PrivateAndPublicFields()
		{
			var lookupStrategy = new LookupStrategy(LookupType.PrivateAndPublicFields);
			var source = new AccessLevelTestClass
			{
				Field = 4,
				PublicProperty = 5
			};

			source.InitPrivates();

			var engine = new CloneEngine(lookupStrategy);
			var target = engine.Clone(source);

			Assert.AreEqual(source.Field, target.Field);
			Assert.AreEqual(source.PublicProperty, target.PublicProperty);

			Assert.AreEqual(source.PrivateProperty2, target.PrivateProperty2);

			var type = typeof(AccessLevelTestClass);

			var sourceFieldValue = (int)type.GetField("_field", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);
			var targetFieldValue = (int)type.GetField("_field", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
			Assert.AreEqual(sourceFieldValue, targetFieldValue);

			var sourceProperty1Value = (int)type.GetProperty("PrivateProperty1", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source, null);
			var targetProperty1Value = (int)type.GetProperty("PrivateProperty1", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target, null);
			Assert.AreEqual(sourceProperty1Value, targetProperty1Value);
		}

		[Test]
		public void RefTable_FoolProof()
		{
			var source = new AccessLevelTestClass();
			var target = CloneHelper.Clone(source);
			Assert.AreNotSame(source, target);
		}

		[Test]
		public void RefTable_RefRestore()
		{
			var source = new RefTestClass1();
			source.Link1 = new RefTestClass2();
			source.Link2 = new RefTestClass2();
			var cls3 = new RefTestClass3();

			source.Link1.Link = cls3;
			source.Link2.Link = cls3;

			var target = CloneHelper.Clone(source);
			
			Assert.AreNotSame(source, target);
			Assert.AreNotSame(source.Link1, target.Link1);
			Assert.AreNotSame(source.Link2, target.Link2);			
			Assert.AreNotSame(source.Link1.Link, target.Link1.Link);
			Assert.AreNotSame(source.Link1.Link, target.Link1.Link);

			Assert.AreSame(source.Link1.Link, source.Link2.Link);
			Assert.AreSame(target.Link1.Link, target.Link2.Link);
		}

		[Test]
		public void RefTable_CircularRef()
		{
			var source = new RefTestClass1();
			source.Link1 = new RefTestClass2();
			source.Link2 = new RefTestClass2();
			var cls3 = new RefTestClass3();

			source.Link1.Link = cls3;
			source.Link2.Link = cls3;

			cls3.Link = new RefTestClass4();
			cls3.Link.Link = source;

			var target = CloneHelper.Clone(source);
			Assert.True(true);
		}

		private class AccessLevelTestClass
		{
			private int _field;
			private int PrivateProperty1 { get; set; }
			public int PrivateProperty2 { get; private set; }

			public int PublicProperty { get; set; }
			public int Field;

			public void InitPrivates()
			{
				_field = 1;
				PrivateProperty1 = 2;
				PrivateProperty2 = 3;
			}

			public override string ToString()
			{
				return string.Format(
					"_field: {0}, Field: {1}, PrivateProperty1: {2}, PrivateProperty2: {3}, PublicProperty: {4}", 
					_field, Field, PrivateProperty1, PrivateProperty2, PublicProperty
				);
			}
		}

		private class RefTestClass1
		{
			public RefTestClass2 Link1 { get; set; }
			public RefTestClass2 Link2 { get; set; }
		}

		private class RefTestClass2
		{
			public RefTestClass3 Link { get; set; }
		}

		private class RefTestClass3
		{
			public RefTestClass4 Link { get; set; }
		}

		private class RefTestClass4
		{
			public RefTestClass1 Link { get; set; }
		}
	}
}
