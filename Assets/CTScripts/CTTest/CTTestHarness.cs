using UnityEngine;
using System.Collections;
using System.Reflection;

public class CTTestHarness : MonoBehaviour {
	public void RunTests<T>(object obj)
	{
		MethodInfo[] methodInfos = typeof(T).GetMethods();
		foreach(var info in methodInfos)
		{
			if (info.Name.StartsWith("Test"))
			{
				info.Invoke(obj, null);
			}
		}
	}
	
	#region Test Utilities
	
	public static void AssertEquals(object obj1, object obj2)
	{
		if (!obj1.Equals(obj2))
			Debug.LogError("Test Failed: "+obj1 + " =/= " + obj2);
	}
	
	#endregion
}
