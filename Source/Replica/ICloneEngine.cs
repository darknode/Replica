using System;
using System.Reflection.Emit;

namespace Replica
{
	public interface ICloneEngine
	{
		LookupStrategy Strategy { get; }
		
		void CreateNullArgumentCheckIl(ILGenerator generator);
		LocalBuilder CreateRefTableCheck(ILGenerator generator);
		void TryGetFromRefTable(ILGenerator generator, LocalBuilder refTable, LocalBuilder result, Label retLabel);
		void StoreRefs(ILGenerator generator, LocalBuilder refTable, LocalBuilder result);
		void CallCloneOrUnwrap(Type valueType, ILGenerator generator, LocalBuilder refTable);
	}
}
