using System;
using System.Collections.Generic;

namespace Replica.Tests
{
	public sealed class ValueTypeArrayContainer
	{
		public int[] Ints { get; set; }
	}

	public sealed class StringArrayContainer
	{
		public string[] Strings { get; set; }
	}

	public sealed class RefTypeArrayContainer
	{
		public OnePropertyClass[] Classes { get; set; }
	}

	public sealed class ValueTypeEnumerableTContainer
	{
		public List<int> Ints { get; set; }
	}

	public sealed class StringEnumerableTContainer
	{
		public List<string> Strings { get; set; }
	}

	public sealed class RefTypeEnumerableTContainer
	{
		public List<OnePropertyClass> Classes { get; set; }
	}

	public sealed class ValueTypeGenericDictionaryContainer
	{
		public Dictionary<int, int> Ints { get; set; }
	}

	public sealed class StringGenericDictionaryContainer
	{
		public Dictionary<string, string> Strings { get; set; }
	}

	public sealed class RefTypeGenericDictionaryContainer
	{
		public Dictionary<OnePropertyClass, OnePropertyClass> Classes { get; set; }
	}

	public sealed class ValueTypePropertyContainer
	{
		public bool Bool { get; set; }
		public sbyte SignedByte { get; set; }
		public byte Byte { get; set; }
		public short Short { get; set; }
		public ushort UsignedShort { get; set; }
		public int Int { get; set; }
		public uint UsignedInt { get; set; }
		public long Long { get; set; }
		public ulong UsignedLong { get; set; }
		public float Float { get; set; }
		public double Double { get; set; }
		public decimal Decimal { get; set; }
		public DateTime DateTime { get; set; }
		public char Char { get; set; }
		public SimpleEnum SimpleEnum { get; set; }
		public FlagsEnum FlagsEnum { get; set; }
		public Guid Guid { get; set; }
		public TimeSpan TimeSpan { get; set; }
		
	}

	public sealed class NullableValueTypePropertyContainer
	{
		public bool? Bool { get; set; }
		public sbyte? SignedByte { get; set; }
		public byte? Byte { get; set; }
		public short? Short { get; set; }
		public ushort? UsignedShort { get; set; }
		public int? Int { get; set; }
		public uint? UsignedInt { get; set; }
		public long? Long { get; set; }
		public ulong? UsignedLong { get; set; }
		public float? Float { get; set; }
		public double? Double { get; set; }
		public decimal? Decimal { get; set; }
		public DateTime? DateTime { get; set; }
		public char? Char { get; set; }
		public SimpleEnum? SimpleEnum { get; set; }
		public FlagsEnum? FlagsEnum { get; set; }
		public Guid? Guid { get; set; }
		public TimeSpan? TimeSpan { get; set; }
	}

	public sealed class ValueTypeFieldContainer
	{
		public bool Bool;
		public sbyte SignedByte;
		public byte Byte;
		public short Short;
		public ushort UsignedShort;
		public int Int;
		public uint UsignedInt;
		public long Long;
		public ulong UsignedLong;
		public float Float;
		public double Double;
		public decimal Decimal;
		public DateTime DateTime;
		public char Char;
		public SimpleEnum SimpleEnum;
		public FlagsEnum FlagsEnum;
		public Guid Guid;
		public TimeSpan TimeSpan;
	}

	public sealed class NullableValueTypeFieldContainer
	{
		public bool? Bool;
		public sbyte? SignedByte;
		public byte? Byte;
		public short? Short;
		public ushort? UsignedShort;
		public int? Int;
		public uint? UsignedInt;
		public long? Long;
		public ulong? UsignedLong;
		public float? Float;
		public double? Double;
		public decimal? Decimal;
		public DateTime? DateTime;
		public char? Char;
		public SimpleEnum? SimpleEnum;
		public FlagsEnum? FlagsEnum;
		public Guid? Guid;
		public TimeSpan? TimeSpan;
	}

	public sealed class OnePropertyClass
	{
		public int IntProtperty { get; set; }
	}

	public enum SimpleEnum
	{
		None = 0,
		Val1 = 1,
		Val2 = 2,
		Val3 = 3
	}

	[Flags]
	public enum FlagsEnum
	{
		None = 0,
		Flag1 = 1,
		Flag2 = 2,
		All = Flag1 | Flag2
	}	
}
