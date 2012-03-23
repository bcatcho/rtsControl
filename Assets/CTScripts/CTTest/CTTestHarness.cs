using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;

public class CTTestHarness : MonoBehaviour {
	
	private void RunTests()
	{
		MethodInfo[] methodInfos = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly) ;
		foreach(var info in methodInfos)
		{
			if (!info.IsDefined(typeof(CTTestIgnore), true))
			{
				info.Invoke(this, null);
			}
		}
	}
	
	public virtual void Start()
	{
		RunTests();
	}
	
	#region Test Utilities
	
	public static void AssertEquals(object obj1, object obj2)
	{
		if (!obj1.Equals(obj2))
			Debug.LogError("Test Failed: "+obj1 + " =/= " + obj2);
	}
	
	#endregion
}


[System.AttributeUsage(System.AttributeTargets.Method)]
public class CTTestIgnore : System.Attribute
{
    public CTTestIgnore() {}
}