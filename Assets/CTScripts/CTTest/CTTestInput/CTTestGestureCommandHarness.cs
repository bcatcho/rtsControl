using UnityEngine;
using System.Collections.Generic;

public class CTTestGestureCommandHarness : CTTestHarness {
	// Use this for initialization
	void Start () {
		RunTests<CTTestGestureCommandHarness>(this);	
	}
	
	public void TestCommandComposition_AddOneSubcommand_ItIsExecutedRecursively()
	{
		var cmd = new CTGestureCommand("base");
		cmd.TouchDown();
		AssertEquals(cmd.Process(MakeRawTouchDown()), CTGestureCommandResult.gestureRecognized);
	}
	
	public void TestCommandComposition_AddTwoSubcommands_TheyExecuteInOrderAdded()
	{
		var cmd = new CTGestureCommand("base")
			.TouchDown()
			.Then
			.TouchDown();
		
		cmd.Process(MakeRawTouchDown());
		var result = cmd.Process(MakeRawTouchDown());
		
		AssertEquals(result, CTGestureCommandResult.gestureRecognized);
	}
	
	public void TestCommandComposition_TouchesDontMatchGesture_FailsAfterSecondTouch()
	{
		var cmd = new CTGestureCommand("base")
			.TouchDown()
			.Then
			.TouchDown();
		
		cmd.Process(MakeRawTouchDown());
		var result = cmd.Process(new RawTouch {phase = TouchPhase.Ended});
		
		AssertEquals(result, CTGestureCommandResult.failed);
	}
	
	public void TestCommandComposition_CustomCommandLast_RecognizesGesture()
	{
		var customCmd = new CTGestureCommand("touchDown")
			.TouchDown();
		
		var cmd = new CTGestureCommand("base")
			.TouchDown()
			.Then
			.Cmd(customCmd);
		
		cmd.Process(MakeRawTouchDown());
		var result = cmd.Process(new RawTouch {phase = TouchPhase.Began});
		
		AssertEquals(result, CTGestureCommandResult.gestureRecognized);
	}
	
	public void TestCommandComposition_CustomCommandFirst_RecognizesGesture()
	{
		var customCmd = new CTGestureCommand("touchDown")
			.TouchDown();
		
		var cmd = new CTGestureCommand("base")
			.Cmd(customCmd)
			.Then
			.TouchDown();
		
		cmd.Process(MakeRawTouchDown());
		var result = cmd.Process(new RawTouch {phase = TouchPhase.Began});
		
		AssertEquals(result, CTGestureCommandResult.gestureRecognized);
	}	
	
	public void TestCommandComposition_CustomCommandWithTooLittleInput_GestureNotYetRecognized()
	{
		var customCmd = new CTGestureCommand("touchDown")
			.TouchDown();
		
		var cmd = new CTGestureCommand("base")
			.Cmd(customCmd)
			.Then.Cmd(customCmd)
			.Then.TouchDown();
		
		cmd.Process(MakeRawTouchDown());
		var result = cmd.Process(new RawTouch {phase = TouchPhase.Began});
		
		AssertEquals(result, CTGestureCommandResult.needsMoreInput);
	}
	
	public void TestCommandComposition_TwoCustomCommands_GestureRecognized()
	{
		var customCmd = new CTGestureCommand("touchDown")
			.TouchDown();
		
		var cmd = new CTGestureCommand("base")
			.Cmd(customCmd)
			.Then.Cmd(customCmd)
			.Then.TouchDown();
		
		cmd.Process(MakeRawTouchDown());
		cmd.Process(MakeRawTouchDown());
		var result = cmd.Process(new RawTouch {phase = TouchPhase.Began});
		
		AssertEquals(result, CTGestureCommandResult.gestureRecognized);
	}
	
	public void TestCommandComposition_TwoCustomCommandsNoTerminals_GestureRecognized()
	{
		var customCmd = new CTGestureCommand("touchDown")
			.TouchDown();
		
		var cmd = new CTGestureCommand("base")
			.Cmd(customCmd)
			.Cmd(customCmd)
			.Then.TouchDown();
		
		cmd.Process(MakeRawTouchDown());
		var result = cmd.Process(new RawTouch {phase = TouchPhase.Began});
		
		AssertEquals(result, CTGestureCommandResult.gestureRecognized);
	}

	public void TestEvaluator_TouchUp_GestureRecognized()
	{
		var cmd = new CTGestureCommand("base")
			.TouchUp();
		
		var result = cmd.Process(new RawTouch {phase = TouchPhase.Ended});
		
		AssertEquals(result, CTGestureCommandResult.gestureRecognized);
	}
	
	public void TestEvaluator_TouchDownTouchUp_GestureRecognized()
	{
		var cmd = new CTGestureCommand("base")
			.TouchDown()
			.Then.TouchUp();
		
		var result = cmd.Process(new RawTouch {phase = TouchPhase.Began});
		AssertEquals(result, CTGestureCommandResult.needsMoreInput);
		
		result = cmd.Process(new RawTouch {phase = TouchPhase.Ended});
		AssertEquals(result, CTGestureCommandResult.gestureRecognized);
	}

	public void TestEvaluator_AnyTouchDownTouchUp_GestureNotRecognizedUntilItRecievesMove()
	{
		var cmd = new CTGestureCommand("base")
			.Any(x => x.TouchUp() + x.TouchDown());
		
		var result = cmd.Process(new RawTouch {phase = TouchPhase.Ended});
		AssertEquals(result, CTGestureCommandResult.needsMoreInput);
		
		result = cmd.Process(new RawTouch {phase = TouchPhase.Began});
		AssertEquals(result, CTGestureCommandResult.needsMoreInput);
		
		result = cmd.Process(new RawTouch {phase = TouchPhase.Moved});
		AssertEquals(result, CTGestureCommandResult.gestureRecognized);
	}
	
	public void TestCommandComposition_DownThenAnyMovedThenUp_GestureRecognized()
	{
		// not good. THEN should be able to go after an ANY
		var cmd = new CTGestureCommand("tap")
			.TouchDown()
			.Then
			.Any(t => t.TouchMoved())
			.TouchUp();
		
		var result = cmd.Process(new RawTouch {phase = TouchPhase.Began});
		AssertEquals(result, CTGestureCommandResult.needsMoreInput);
		
		result = cmd.Process(new RawTouch {phase = TouchPhase.Moved});
		result = cmd.Process(new RawTouch {phase = TouchPhase.Moved});
		AssertEquals(result, CTGestureCommandResult.needsMoreInput);
		
		result = cmd.Process(new RawTouch {phase = TouchPhase.Ended});
		AssertEquals(result, CTGestureCommandResult.gestureRecognized);
	}
	
	private RawTouch MakeRawTouchDown()
	{
		return new RawTouch() {
			phase = TouchPhase.Began
		};
	}
}