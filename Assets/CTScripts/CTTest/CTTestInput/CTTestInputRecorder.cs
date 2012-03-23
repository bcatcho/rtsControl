using UnityEngine;
using System.Collections.Generic;

public class CTTestInputRecorder : Object 
{
	private CTTestInputTestCase _testCase;
	private CTTestInputTestCaseList _caseList;
	
	// Use this for initialization
	public CTTestInputRecorder()
	{
		_testCase = new CTTestInputTestCase();
		_caseList = (CTTestInputTestCaseList)Resources.Load("TestCases");
	}
	
	public void StartRecording()
	{
		GameMessenger.Reg("touch", this, Message_Touch);
	}
	
	public GameMessageResult Message_Touch(GameMessage msg)
	{
		var touch = (RawTouch)msg.data;
		
		if (ThisIsANewGesture(touch)) { // if there hasn't been a touch in a while it's a new gesture
			GameMessenger.SendNow("newTestInputRecording", this, null);
			_caseList.testCases.Add(_testCase);
			Debug.Log(_testCase.ToYAML());		
			_testCase = new CTTestInputTestCase();
		}
			    
		_testCase.AppendInput(touch);
		return GameMessageResult.didNotHandleMessage;
	}
	
	private bool ThisIsANewGesture(RawTouch touch)
	{
		return touch.phase == TouchPhase.Began;
	}
}


