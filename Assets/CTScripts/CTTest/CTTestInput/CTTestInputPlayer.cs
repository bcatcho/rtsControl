using UnityEngine;
using System.Collections.Generic;

public class CTTestInputPlayer : Object {
	private CTTestInputTestCaseList _caseList;
	private CTTestInputTestCase _currentTest;
	public Queue<RawTouch> currentCase;
	public float timeOffset;
	public bool isPlaying;
	
	// Use this for initialization
	public CTTestInputPlayer() {
		_caseList = (CTTestInputTestCaseList)Resources.Load("TestCases");
		isPlaying = false;
	}
	
	private void LoadNextTest()
	{
		_currentTest = _caseList.testCases[0];
		
		currentCase = new Queue<RawTouch>();
		foreach (RawTouch t in _currentTest.inputHistory)
		{
			currentCase.Enqueue(t);	
		}
	}
	
	public void StartPlaying()
	{	
		LoadNextTest();
		isPlaying = true;
		var currentTouch = currentCase.Dequeue();
		timeOffset = Time.deltaTime;
		currentTouch.time = Time.deltaTime;
		currentTouch.ray = Camera.main.ScreenPointToRay(currentTouch.rawPosition);
		GameMessenger.SendNow("newTestInputRecording", this, _currentTest.testCaseName);
		GameMessenger.SendNow("touch", this, currentTouch);
	}
	
	public void Update() {
		if (isPlaying && currentCase.Count > 0)
		{
			var currentTouch = currentCase.Peek();
			var adjustedTime = currentTouch.time + timeOffset ;
			if (adjustedTime >= Time.deltaTime)
			{
				currentCase.Dequeue();
				currentTouch.time = Time.deltaTime;
				currentTouch.ray = Camera.main.ScreenPointToRay(currentTouch.rawPosition);
				GameMessenger.SendNow("touch", this, currentTouch);	
			}
		}
	}
}
