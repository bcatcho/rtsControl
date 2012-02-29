using UnityEngine;
using System.Collections.Generic;
using System.Text;

[System.Serializable]
public class CTTestInputTestCase  {
	public List<RawTouch> inputHistory = new List<RawTouch>();
	public string testCaseName = @"untitiled input test";
	public float timeOffset = 0;
	
	public CTTestInputTestCase() {}
	
	public void AppendInput(RawTouch input)
	{
		if (inputHistory.Count == 0)
		{
			timeOffset = input.time;
		}
		input.time -= timeOffset;
		inputHistory.Add(input);	
	}
	
	public string ToYAML()
	{
/*
example
  testCases:
  - touches:
    - rawPosition: {x: 359.46875, y: 252.71666, z: 0}
      deltaPosition: {x: 0, y: 0, z: 0}
      deltaTime: .00744870817
      fingerId: 0
      phase: 0
      position: {x: 4.00001526, y: 32.9133186}
      tapCount: 1
      time: 0
*/
		var str = new StringBuilder();
		str.AppendLine("======= START TEST CASE ===========");
		str.AppendLine("  testCases:");
		str.AppendLine("  - touches:");
		
		foreach (var t in inputHistory)
		{
			str.AppendFormat("    - rawPosition: {{x: {0}, y: {1}, z: {2}}}\n", t.rawPosition.x, t.rawPosition.y, t.rawPosition.z);
      		str.AppendFormat("      deltaPosition: {{x: {0}, y: {1}, z: {2}}}\n", t.deltaPosition.x, t.deltaPosition.y, t.deltaPosition.z);
		    str.AppendFormat("      deltaTime: {0}\n", t.deltaTime);
		    str.AppendFormat("      fingerId: {0}\n", t.fingerId);
		    str.AppendFormat("      phase: {0:D}\n", t.phase);
		    str.AppendFormat("      position: {{x: {0}, y: {0}}}\n", t.position.x, t.position.y);
		    str.AppendFormat("      tapCount: {0}\n", t.tapCount);
		    str.AppendFormat("      time: {0}\n", t.time);
		}
		str.AppendLine("======== END TEST CASE ============");
		return str.ToString();
	}
	
}
