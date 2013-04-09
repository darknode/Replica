using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Replica
{
	public sealed class EnumerableTCloneIlGenerator : ITypeCloneIlGenerator
	{
		public bool CanClone(Type targetType)
		{
			return targetType != typeof(string) &&
				   targetType.GetInterfaces().Any(_ => _.IsGenericType && _.GetGenericTypeDefinition() == typeof(IEnumerable<>));
		}

		public void GenerateIl(ICloneEngine engine, Type targetType, DynamicMethod method)
		{
			var elementType = targetType.GetGenericArguments()[0];
			var generator = method.GetILGenerator();
			var genericEnumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);
			var genericListType = typeof(List<>).MakeGenericType(elementType);
			var genericEnumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);

			generator.Emit(OpCodes.Ldarg_0);
			engine.CreateNullArgumentCheckIl(generator);

			var labelRet = generator.DefineLabel();
			var labelEndFinally = generator.DefineLabel();
			var labelMove = generator.DefineLabel();
			var labelWhile = generator.DefineLabel();

			var source = generator.DeclareLocal(targetType);				//arg0
			var target = generator.DeclareLocal(genericListType);			//local_0
			var element = generator.DeclareLocal(elementType);				//local_1
			var enumerator = generator.DeclareLocal(genericEnumeratorType);	//local_2

			var refTable = engine.CreateRefTableCheck(generator);
			engine.TryGetFromRefTable(generator, refTable, target, labelRet);

			generator.Emit(OpCodes.Newobj, genericListType.GetConstructor(Type.EmptyTypes));
			generator.Emit(OpCodes.Stloc, target);

			engine.StoreRefs(generator, refTable, target);

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Stloc, source);
			generator.Emit(OpCodes.Ldloc, source);
			generator.Emit(OpCodes.Callvirt, genericEnumerableType.GetMethod("GetEnumerator"));
			generator.Emit(OpCodes.Stloc, enumerator);
			var tryFinally = generator.BeginExceptionBlock();
			// try {
			generator.Emit(OpCodes.Br_S, labelMove);
			generator.MarkLabel(labelWhile);
			generator.Emit(OpCodes.Ldloc, enumerator);
			generator.Emit(OpCodes.Callvirt, genericEnumeratorType.GetProperty("Current").GetGetMethod());
			generator.Emit(OpCodes.Stloc, element);
			generator.Emit(OpCodes.Ldloc, target);
			generator.Emit(OpCodes.Ldloc, element);
			if (!elementType.IsValueType)
			{
				engine.CallCloneOrUnwrap(elementType, generator, refTable);
			}
			generator.Emit(OpCodes.Callvirt, genericListType.GetMethod("Add"));
			generator.MarkLabel(labelMove);
			generator.Emit(OpCodes.Ldloc, enumerator);
			generator.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
			generator.Emit(OpCodes.Brtrue_S, labelWhile);
			generator.Emit(OpCodes.Leave_S, labelRet);
			// } finally {
			generator.BeginFinallyBlock();
			generator.Emit(OpCodes.Ldloc, enumerator);
			generator.Emit(OpCodes.Brfalse_S, labelEndFinally);
			generator.Emit(OpCodes.Ldloc, enumerator);
			generator.Emit(OpCodes.Callvirt, typeof(IDisposable).GetMethod("Dispose"));
			generator.MarkLabel(labelEndFinally);
			generator.Emit(OpCodes.Endfinally);
			// }
			generator.EndExceptionBlock();
			generator.MarkLabel(labelRet);
			generator.Emit(OpCodes.Ldloc, target);
			generator.Emit(OpCodes.Ret);
		}
	}
}
