using System.Runtime.CompilerServices;

namespace System.Reflection
{
	public static class MethodInfoExtensions
	{
		public static bool IsAsync(this MethodInfo mi)
		{
			Type attType = typeof(AsyncStateMachineAttribute);

			var attrib = (AsyncStateMachineAttribute)mi.GetCustomAttribute(attType);

			return attrib is { };
		}

		public static object GetDefault(this Type type)
		{
			if(type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}
			return null;
		}
	}
}
