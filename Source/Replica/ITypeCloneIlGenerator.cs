using System;
using System.Reflection.Emit;

namespace Replica
{
	public interface ITypeCloneIlGenerator
	{
		bool CanClone(Type targetType);
		void GenerateIl(ICloneEngine engine, Type targetType, DynamicMethod method);
	}
}
