using System;

namespace Replica
{
	public static class CloneHelper
	{
		private readonly static CloneEngine Engine = new CloneEngine();

		static CloneHelper()
		{
		}

		public static T Clone<T>(T instance) where T : class, new()
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			return Engine.Clone(instance);
		}

		public static object Clone(object instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			return Engine.Clone(instance);
		}
	}
}
