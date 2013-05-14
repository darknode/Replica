using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Replica
{
	public sealed class CloneEngine : ICloneEngine
	{
		private readonly LookupStrategy _lookupStrategy;
		private readonly Type _refTableType;
		private static readonly List<ITypeCloneIlGenerator> Generators = new List<ITypeCloneIlGenerator>();
		private static readonly Dictionary<Type, DynamicClone> CachedIl = new Dictionary<Type, DynamicClone>();
		private static MethodInfo _stringCopy;
		private DynamicMethod _unwrapObjectMethod;
		private DynamicMethod _unwrapInterfaceMethod;
// ReSharper disable NotAccessedField.Local
		private Delegate _unwrapObjectDelegate;
		private Delegate _unwrapInterfaceDelegate;
// ReSharper restore NotAccessedField.Local

		public LookupStrategy Strategy
		{
			get { return _lookupStrategy; }
		}

		public CloneEngine() : this(new LookupStrategy(LookupType.PublicPropertiesAndFields))
		{
			
		}

		public CloneEngine(LookupStrategy lookupStrategy)
		{
			_lookupStrategy = lookupStrategy;
			_stringCopy = typeof(string).GetMethod("Copy");
			Generators.Add(new ArrayCloneIlGenerator());
			Generators.Add(new DictionaryTCloneIlGenerator());
			Generators.Add(new EnumerableTCloneIlGenerator());
			
			_refTableType = typeof(ConditionalWeakTable<,>).MakeGenericType(new[] { typeof(object), typeof(object) });

			BuildUnwrapObjectMethod();
			BuildUnwrapInterfaceMethod();
		}

		public T Clone<T>(T instance) where T : class, new()
		{
			var type = typeof(T);
			DynamicClone dynClone;
			if (!CachedIl.TryGetValue(type, out dynClone))
			{
				dynClone = CreateTypeMethod(type);
			}

			return ((Func<T, ConditionalWeakTable<object, object>, CloneEngine, T>)dynClone.CloneFunc)(instance, null, this);
		}

		public object Clone(object instance)
		{
			if (instance == null)
				return null;

			var type = instance.GetType();
			DynamicClone dynClone;
			if (!CachedIl.TryGetValue(type, out dynClone))
			{
				dynClone = CreateTypeMethod(type);
			}

			return dynClone.CloneFunc.DynamicInvoke(instance, null, this);
		}

		private void BuildUnwrapObjectMethod()
		{
			_unwrapObjectMethod = new DynamicMethod("UnwrapObjectMethod",
													typeof(object), new[] { typeof(object), _refTableType, typeof(CloneEngine) },
			                                        Assembly.GetExecutingAssembly().ManifestModule,
			                                        true);

			var generator = _unwrapObjectMethod.GetILGenerator();

			
			var lblRefType = generator.DefineLabel();
			var lblContinue = generator.DefineLabel();
			var realType = generator.DeclareLocal(typeof(Type));
			var value = generator.DeclareLocal(typeof(object));

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Stloc, value);
			generator.Emit(OpCodes.Ldloc, value);
			generator.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetType"));
			generator.Emit(OpCodes.Stloc, realType);			
#if DEBUG
			generator.Emit(OpCodes.Ldstr, "Recognized real type:");
			generator.Emit(OpCodes.Call, typeof(Debug).GetMethod("WriteLine", new[] { typeof(string) }));
			generator.Emit(OpCodes.Ldloc, realType);
			generator.Emit(OpCodes.Call, typeof(Debug).GetMethod("WriteLine", new[] { typeof(object) }));
#endif			
			generator.Emit(OpCodes.Ldloc, realType);
			generator.Emit(OpCodes.Call, typeof(Type).GetProperty("IsValueType").GetGetMethod());
			generator.Emit(OpCodes.Brfalse, lblRefType);
			// ValueType
			generator.Emit(OpCodes.Ldloc, value);
			generator.Emit(OpCodes.Ldloc, realType);
			generator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type) }));
			generator.Emit(OpCodes.Br, lblContinue);
			// RefType
			generator.MarkLabel(lblRefType);
			generator.Emit(OpCodes.Ldarg_2);
			generator.Emit(OpCodes.Ldloc, value);
			generator.Emit(OpCodes.Callvirt, typeof(CloneEngine).GetMethod("Clone", new[] { typeof(object) }));
			
			generator.MarkLabel(lblContinue);
			generator.Emit(OpCodes.Ret);

			var genericFunc = typeof(Func<,,,>);
			genericFunc = genericFunc.MakeGenericType(typeof(object), _refTableType, typeof(CloneEngine), typeof(object));
			_unwrapObjectDelegate = _unwrapObjectMethod.CreateDelegate(genericFunc);
		}

		private void BuildUnwrapInterfaceMethod()
		{
			_unwrapInterfaceMethod = new DynamicMethod("UnwrapInterfaceMethod",
													typeof(object), new[] { typeof(object), _refTableType, typeof(CloneEngine) },
													Assembly.GetExecutingAssembly().ManifestModule,
													true);

			var generator = _unwrapInterfaceMethod.GetILGenerator();
			var realType = generator.DeclareLocal(typeof(Type));
			var value = generator.DeclareLocal(typeof(object));

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Stloc, value);
			generator.Emit(OpCodes.Ldloc, value);
			generator.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetType"));
			generator.Emit(OpCodes.Stloc, realType);

#if DEBUG
			generator.Emit(OpCodes.Ldstr, "Recognized real type:");
			generator.Emit(OpCodes.Call, typeof(Debug).GetMethod("WriteLine", new[] { typeof(string) }));
			generator.Emit(OpCodes.Ldloc, realType);
			generator.Emit(OpCodes.Call, typeof(Debug).GetMethod("WriteLine", new[] { typeof(object) }));
#endif
			generator.Emit(OpCodes.Ldarg_2);
			generator.Emit(OpCodes.Ldloc, value);
			generator.Emit(OpCodes.Callvirt, typeof(CloneEngine).GetMethod("Clone", new[] { typeof(object) }));
			generator.Emit(OpCodes.Ret);

			var genericFunc = typeof(Func<,,,>);
			genericFunc = genericFunc.MakeGenericType(typeof(object), _refTableType, typeof(CloneEngine), typeof(object));
			_unwrapInterfaceDelegate = _unwrapInterfaceMethod.CreateDelegate(genericFunc);
		}

		private DynamicClone CreateTypeMethod(Type type)
		{
			if (type == typeof(object))
				throw new Exception("Type System.Object not supported.");

			if (type.IsValueType)
				throw new Exception("ValueTypes not supported.");

			DynamicClone clone;
			if (CachedIl.TryGetValue(type, out clone))
				return clone;

			var genericFunc = typeof(Func<,,,>);
			genericFunc = genericFunc.MakeGenericType(type, _refTableType, typeof(CloneEngine), type);

			var dynMethod = new DynamicMethod("DoClone",
											  type, new[] { type, _refTableType, typeof(CloneEngine) },
			                                  Assembly.GetExecutingAssembly().ManifestModule,
			                                  true);

			clone = new DynamicClone(null, dynMethod);
			CachedIl.Add(type, clone);

			var generator = Generators.FirstOrDefault(_ => _.CanClone(type));
			if (generator != null)
			{
				generator.GenerateIl(this, type, dynMethod);
			}
			else
			{
				CreateTypeIl(type, dynMethod);
			}

			clone.CloneFunc = dynMethod.CreateDelegate(genericFunc);

			return clone;
		}

		private void CreateTypeIl(Type type, DynamicMethod dynMethod)
		{
			var generator = dynMethod.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			CreateNullArgumentCheckIl(generator);

			if (type == typeof (string))
			{
				generator.Emit(OpCodes.Ldarg_0);
				generator.Emit(OpCodes.Call, _stringCopy);
				generator.Emit(OpCodes.Ret);
			}
			else
			{
				var ctorInfo = type.GetConstructor(Type.EmptyTypes);
				if (ctorInfo == null)
					throw new Exception(string.Format("{0}. Parameterless constructor does not exists.", type.FullName));

				var cloneVar = generator.DeclareLocal(type);

				var retLabel = generator.DefineLabel();
				var refTable = CreateRefTableCheck(generator);
				TryGetFromRefTable(generator, refTable, cloneVar, retLabel);

				generator.Emit(OpCodes.Newobj, ctorInfo);
				generator.Emit(OpCodes.Stloc, cloneVar);

				StoreRefs(generator, refTable, cloneVar);

				var fieldFlags = _lookupStrategy.LookupType == LookupType.PublicPropertiesAndFields
					                 ? BindingFlags.Instance | BindingFlags.Public
					                 : BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

				var fields = type.GetFields(fieldFlags);
				var valueTypeFields = fields.Where(_ => _.FieldType.IsValueType).ToList();
				var classTypeFields = fields.Where(_ => _.FieldType.IsClass || _.FieldType.IsInterface).ToList();

				foreach (var valueField in valueTypeFields)
				{
					generator.Emit(OpCodes.Ldloc, cloneVar);
					generator.Emit(OpCodes.Ldarg_0);
					generator.Emit(OpCodes.Ldfld, valueField);
					generator.Emit(OpCodes.Stfld, valueField);
				}

				foreach (var classField in classTypeFields)
				{
					generator.Emit(OpCodes.Ldloc, cloneVar);
					generator.Emit(OpCodes.Ldarg_0);
					generator.Emit(OpCodes.Ldfld, classField);
					CallCloneOrUnwrap(classField.FieldType, generator, refTable);

					generator.Emit(OpCodes.Stfld, classField);
				}

				if (_lookupStrategy.LookupType == LookupType.PublicPropertiesAndFields)
				{
					var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
					props = props.Where(_ => _.CanRead && _.CanWrite && _.IsSpecialName == false).ToArray();
					var valueTypeProps = props.Where(_ => _.PropertyType.IsValueType).ToList();
					var classTypeProps = props.Where(_ => _.PropertyType.IsClass || _.PropertyType.IsInterface).ToList();

					foreach (var valueProp in valueTypeProps)
					{
						var getter = valueProp.GetGetMethod();
						var setter = valueProp.GetSetMethod();
						// Check for private and internal modifiers
						if (getter == null || setter == null)
							continue;

						generator.Emit(OpCodes.Ldloc, cloneVar);
						generator.Emit(OpCodes.Ldarg_0);
						generator.Emit(OpCodes.Callvirt, getter);
						generator.Emit(OpCodes.Callvirt, setter);
					}

					foreach (var classProp in classTypeProps)
					{
						// Check for indexer properties
						var indexerParams = classProp.GetIndexParameters();
						if (indexerParams.Length > 0)
							continue;

						var getter = classProp.GetGetMethod();
						var setter = classProp.GetSetMethod();
						// Check for private and internal modifiers
						if (getter == null || setter == null)
							continue;

						generator.Emit(OpCodes.Ldloc, cloneVar);
						generator.Emit(OpCodes.Ldarg_0);
						generator.Emit(OpCodes.Callvirt, getter);

						CallCloneOrUnwrap(classProp.PropertyType, generator, refTable);

						generator.Emit(OpCodes.Callvirt, setter);
					}
				}

				generator.MarkLabel(retLabel);
				generator.Emit(OpCodes.Ldloc, cloneVar);
				generator.Emit(OpCodes.Ret);
			}
		}

		private LocalBuilder CreateRefTableCheck(ILGenerator generator)
		{
			var refTable = generator.DeclareLocal(_refTableType);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Stloc, refTable);
			generator.Emit(OpCodes.Ldloc, refTable);
			// Check for null argument
			var refTableNotNullLabel = generator.DefineLabel();
			generator.Emit(OpCodes.Brtrue, refTableNotNullLabel);
			// If refTable is null
			generator.Emit(OpCodes.Newobj, _refTableType.GetConstructor(Type.EmptyTypes));
			generator.Emit(OpCodes.Stloc, refTable);
			generator.MarkLabel(refTableNotNullLabel);
			return refTable;
		}

		private void TryGetFromRefTable(ILGenerator generator, LocalBuilder refTable, LocalBuilder result, Label retLabel)
		{			
			generator.Emit(OpCodes.Ldloc, refTable);
			// Load instance as Key
			generator.Emit(OpCodes.Ldarg_0);
			// Load address of local var for method out parameter
			generator.Emit(OpCodes.Ldloca_S, result);			
			generator.Emit(OpCodes.Call, _refTableType.GetMethod("TryGetValue"));
			// If value present - go to ret
			generator.Emit(OpCodes.Brtrue, retLabel);
		}

		private void StoreRefs(ILGenerator generator, LocalBuilder refTable, LocalBuilder result)
		{
			generator.Emit(OpCodes.Ldloc, refTable);
			// Load instance as Key
			generator.Emit(OpCodes.Ldarg_0);
			// Load new instance as Value
			generator.Emit(OpCodes.Ldloc, result);
			generator.Emit(OpCodes.Call, _refTableType.GetMethod("Add"));
		}

		private void CreateNullArgumentCheckIl(ILGenerator generator)
		{
			// Check for null argument
			var continueLabel = generator.DefineLabel();
			generator.Emit(OpCodes.Brtrue, continueLabel);
			// If arg0 is null - ret
			generator.Emit(OpCodes.Ldnull);
			generator.Emit(OpCodes.Ret);
			generator.MarkLabel(continueLabel);
		}

		private void CallCloneOrUnwrap(Type valueType, ILGenerator generator, LocalBuilder refTable)
		{
			if (valueType == typeof(object))
			{
				generator.Emit(OpCodes.Ldloc, refTable);
				generator.Emit(OpCodes.Ldarg_2);
				generator.Emit(OpCodes.Call, _unwrapObjectMethod);
			}
			else if (valueType.IsInterface)
			{
				generator.Emit(OpCodes.Ldloc, refTable);
				generator.Emit(OpCodes.Ldarg_2);
				generator.Emit(OpCodes.Call, _unwrapInterfaceMethod);
			}
			else
			{
				generator.Emit(OpCodes.Ldloc, refTable);
				generator.Emit(OpCodes.Ldarg_2);
				var dynClone = CreateTypeMethod(valueType);
				generator.Emit(OpCodes.Call, dynClone.Method);
			}
		}

		#region ICloneEngine

		LocalBuilder ICloneEngine.CreateRefTableCheck(ILGenerator generator)
		{
			return CreateRefTableCheck(generator);
		}

		void ICloneEngine.TryGetFromRefTable(ILGenerator generator, LocalBuilder refTable, LocalBuilder result, Label retLabel)
		{
			TryGetFromRefTable(generator, refTable, result, retLabel);
		}

		void ICloneEngine.StoreRefs(ILGenerator generator, LocalBuilder refTable, LocalBuilder result)
		{
			StoreRefs(generator, refTable, result);
		}

		void ICloneEngine.CreateNullArgumentCheckIl(ILGenerator generator)
		{
			CreateNullArgumentCheckIl(generator);
		}

		void ICloneEngine.CallCloneOrUnwrap(Type valueType, ILGenerator generator, LocalBuilder refTable)
		{
			CallCloneOrUnwrap(valueType, generator, refTable);
		}

		#endregion
	}
}