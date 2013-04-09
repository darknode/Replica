using System;
using System.Reflection.Emit;

namespace Replica
{
	internal sealed class DynamicClone
	{
		public Delegate CloneFunc { get; set; }
		public DynamicMethod Method { get; set; }

		public DynamicClone(Delegate cloneFunc, DynamicMethod method)
		{
			CloneFunc = cloneFunc;
			Method = method;
		}
	}
}