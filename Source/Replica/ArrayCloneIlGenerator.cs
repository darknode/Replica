using System;
using System.Reflection.Emit;

namespace Replica
{
	internal sealed class ArrayCloneIlGenerator : ITypeCloneIlGenerator
	{
		public bool CanClone(Type targetType)
		{
			return targetType.IsArray;
		}

		public void GenerateIl(ICloneEngine engine, Type targetType, DynamicMethod method)
		{
			var elementType = targetType.GetElementType();
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			engine.CreateNullArgumentCheckIl(generator);

			var source = generator.DeclareLocal(targetType);	//local_1
			var target = generator.DeclareLocal(targetType);	//local_2
			var index = generator.DeclareLocal(typeof(int));	//local_3

			var endLabel = generator.DefineLabel();
			var refTable = engine.CreateRefTableCheck(generator);
			engine.TryGetFromRefTable(generator, refTable, target, endLabel);

			var labelCheck = generator.DefineLabel();
			var labelAssign = generator.DefineLabel();

			// Create array instance
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Stloc, source);
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldlen);
			generator.Emit(OpCodes.Conv_I4);
			generator.Emit(OpCodes.Newarr, elementType);
			generator.Emit(OpCodes.Stloc, target);

			engine.StoreRefs(generator, refTable, target);

			// Iterate by source array
			// Array index variable
			generator.Emit(OpCodes.Ldc_I4_0);
			generator.Emit(OpCodes.Stloc, index);
			// Go to check
			generator.Emit(OpCodes.Br_S, labelCheck);
			// Assign
			generator.MarkLabel(labelAssign);
			// Ld stack items
			generator.Emit(OpCodes.Ldloc, target);
			generator.Emit(OpCodes.Ldloc, index);
			// St stack items
			generator.Emit(OpCodes.Ldloc, source);
			generator.Emit(OpCodes.Ldloc, index);

			if (elementType.IsValueType)
			{
				generator.Emit(OpCodes.Ldelem, elementType);
				generator.Emit(OpCodes.Stelem, elementType);
			}
			else
			{				
				generator.Emit(OpCodes.Ldelem_Ref);
				engine.CallCloneOrUnwrap(elementType, generator, refTable);
				generator.Emit(OpCodes.Stelem_Ref);
			}

			generator.Emit(OpCodes.Ldloc, index);
			generator.Emit(OpCodes.Ldc_I4_1);
			generator.Emit(OpCodes.Add);
			generator.Emit(OpCodes.Stloc, index);
			// Check index < array.Len
			generator.MarkLabel(labelCheck);
			generator.Emit(OpCodes.Ldloc, index);
			generator.Emit(OpCodes.Ldloc, source);
			generator.Emit(OpCodes.Ldlen);
			generator.Emit(OpCodes.Conv_I4);
			generator.Emit(OpCodes.Blt_S, labelAssign);

			generator.MarkLabel(endLabel);
			generator.Emit(OpCodes.Ldloc, target);
			generator.Emit(OpCodes.Ret);
		}
	}
}
