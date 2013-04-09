using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Replica
{
	internal sealed class DictionaryTCloneIlGenerator : ITypeCloneIlGenerator
	{
		public bool CanClone(Type targetType)
		{
			return targetType != typeof(string) &&
				   targetType.GetInterfaces().Any(_ => _.IsGenericType && _.GetGenericTypeDefinition() == typeof(IDictionary<,>));
		}

		public void GenerateIl(ICloneEngine engine, Type targetType, DynamicMethod method)
		{
			var genArgs = targetType.GetGenericArguments();
			var keyType = genArgs[0];
			var valueType = genArgs[1];
			var pairType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
			var enumeratorType = typeof(Dictionary<,>.Enumerator).MakeGenericType(keyType, valueType);

			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			engine.CreateNullArgumentCheckIl(generator);

			var labelRet = generator.DefineLabel();
			var labelEndFinally = generator.DefineLabel();
			var labelMove = generator.DefineLabel();
			var labelWhile = generator.DefineLabel();

			var source = generator.DeclareLocal(targetType);			//local_0
			var target = generator.DeclareLocal(targetType);			//local_1
			var enumerator = generator.DeclareLocal(enumeratorType);	//local_2
			var pair = generator.DeclareLocal(pairType);				//local_3
			var key = generator.DeclareLocal(keyType);					//local_4
			var value = generator.DeclareLocal(valueType);				//local_5

			var refTable = engine.CreateRefTableCheck(generator);
			engine.TryGetFromRefTable(generator, refTable, target, labelRet);

			generator.Emit(OpCodes.Newobj, targetType.GetConstructor(Type.EmptyTypes));
			generator.Emit(OpCodes.Stloc, target);

			engine.StoreRefs(generator, refTable, target);

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Stloc, source);
			generator.Emit(OpCodes.Ldloc, source);
			generator.Emit(OpCodes.Callvirt, targetType.GetMethod("GetEnumerator"));
			generator.Emit(OpCodes.Stloc, enumerator);

			var tryFinally = generator.BeginExceptionBlock();
			// try {
			generator.Emit(OpCodes.Br_S, labelMove);
			generator.MarkLabel(labelWhile);

			generator.Emit(OpCodes.Ldloca, enumerator);
			generator.Emit(OpCodes.Callvirt, enumeratorType.GetProperty("Current").GetGetMethod());
			generator.Emit(OpCodes.Stloc, pair);

			generator.Emit(OpCodes.Ldloca, pair);
			generator.Emit(OpCodes.Callvirt, pairType.GetProperty("Key").GetGetMethod());
			generator.Emit(OpCodes.Stloc, key);

			generator.Emit(OpCodes.Ldloca, pair);
			generator.Emit(OpCodes.Callvirt, pairType.GetProperty("Value").GetGetMethod());
			generator.Emit(OpCodes.Stloc, value);

			generator.Emit(OpCodes.Ldloc, target);

			generator.Emit(OpCodes.Ldloc, key);
			if (!keyType.IsValueType)
			{
				engine.CallCloneOrUnwrap(keyType, generator, refTable);
			}

			generator.Emit(OpCodes.Ldloc, value);
			if (!valueType.IsValueType)
			{
				engine.CallCloneOrUnwrap(valueType, generator, refTable);
			}

			generator.Emit(OpCodes.Callvirt, targetType.GetMethod("Add", new[] { keyType, valueType }));

			generator.MarkLabel(labelMove);
			generator.Emit(OpCodes.Ldloca, enumerator);
			generator.Emit(OpCodes.Callvirt, enumeratorType.GetMethod("MoveNext"));
			generator.Emit(OpCodes.Brtrue_S, labelWhile);
			generator.Emit(OpCodes.Leave_S, labelRet);

			// } finally {

			generator.BeginFinallyBlock();
			generator.Emit(OpCodes.Ldloca, enumerator);
			generator.Emit(OpCodes.Brfalse_S, labelEndFinally);
			generator.Emit(OpCodes.Ldloca, enumerator);
			generator.Emit(OpCodes.Callvirt, enumeratorType.GetMethod("Dispose"));
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
