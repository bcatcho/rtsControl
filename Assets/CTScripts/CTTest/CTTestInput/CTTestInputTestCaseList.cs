using UnityEngine;
using System.Collections.Generic;

public class CTTestInputTestCaseList : ScriptableObject {
	public List<CTTestInputTestCase> testCases;
	
	public CTTestInputTestCaseList()
	{
		testCases = new List<CTTestInputTestCase>();
	}
}
