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
	
	public void TestFunctionalPlayground_CombiningDelegates_Success()
	{
		GParse<RawTouch> td = t => t.current.phase == TouchPhase.Began ? GParserStatus.success : GParserStatus.failed;
		GParse<RawTouch> x = t => t.current.fingerId == 0 ? GParserStatus.success : GParserStatus.failed;
		GParse<RawTouch> td_x = td.And(x);
		
		var cmd = new GParserPhrase<RawTouch>().And(td_x).End();
		cmd.Parse(new RawTouch { phase = TouchPhase.Began, fingerId = 0});
		
		AssertEquals(cmd.status, GParserStatus.success);
	}
	
	public void TestFunctionalPlayground_CombiningDelegates_ShortCircuitFailure()
	{
		GParse<RawTouch> td = t => t.current.phase == TouchPhase.Began ? GParserStatus.success : GParserStatus.failed;
		GParse<RawTouch> x = t => t.current.fingerId == 0 ? GParserStatus.success : GParserStatus.failed;
		GParse<RawTouch> y = t => t.current.time == 0 ? GParserStatus.success : GParserStatus.failed;
		GParse<RawTouch> tdxy = td.And(x).And(y);
		
		
		var cmd = new GParserPhrase<RawTouch>().And(tdxy).End();
		cmd.Parse(new RawTouch { phase = TouchPhase.Began, fingerId = 0, time = 1});
		
		AssertEquals(cmd.status, GParserStatus.failed);
	}
	
	public void TestFunctionalPlayground_MakingCommands()
	{
		var cmd = new GParserPhrase<RawTouch>().TouchDown().Then().TouchUp().End();
		cmd.Parse(new RawTouch { phase = TouchPhase.Began } );
		AssertEquals(cmd.status, GParserStatus.inconclusive);
	}

	public void TestFunctionalPlayground_MultistepCommands()
	{
		var cmd = new GParserPhrase<RawTouch>().TouchDown().Then().TouchUp().End();
		
		cmd.Parse(new RawTouch { phase = TouchPhase.Began } );
		cmd.Parse(new RawTouch { phase = TouchPhase.Ended } );
		AssertEquals(cmd.status, GParserStatus.success);
	}
	
	public void TestFunctionalPlayground_MultistepCommands_failiure()
	{
		var cmd = new GParserPhrase<RawTouch>().TouchDown().Then().TouchUp().End();
		
		cmd.Parse(new RawTouch { phase = TouchPhase.Began } );
		cmd.Parse(new RawTouch { phase = TouchPhase.Began } );
		AssertEquals(cmd.status, GParserStatus.failed);
	}
	
	public void TestFunctionalPlayground_NestedCommands()
	{
		var cmdTdTd = new GParserPhrase<RawTouch>().TouchDown().Then().TouchDown().End();
		var cmd = new GParserPhrase<RawTouch>().Cmd(cmdTdTd).Then().TouchUp().End();
		
		cmd.Parse(new RawTouch { phase = TouchPhase.Began } );
		cmd.Parse(new RawTouch { phase = TouchPhase.Began } );
		cmd.Parse(new RawTouch { phase = TouchPhase.Ended } );
		AssertEquals(cmd.status, GParserStatus.success);
	}
	
	public void TestFunctionalPlayground_WaitForTouchUp_TimeLimitExcededReturnsFailure()
	{
		var cmd = new GParserPhrase<RawTouch>()
			.TouchDown()
			.ThenWithin(-10).TouchUp()
			.End();
		
		cmd.Parse(new RawTouch { phase = TouchPhase.Began } );
		cmd.Parse(new RawTouch { phase = TouchPhase.Moved } );
		cmd.Parse(new RawTouch { phase = TouchPhase.Stationary } );
		cmd.Parse(new RawTouch { phase = TouchPhase.Ended } );
		AssertEquals(cmd.status, GParserStatus.failed);
	}
	
	public void TestFunctionalPlayground_WaitForTouchUp_TimeLimitNotExceededReturnsSuccess()
	{
		var cmd = new GParserPhrase<RawTouch>()
			.TouchDown()
			.ThenWithin(100).TouchUp()
			.End();
		
		cmd.Parse(new RawTouch { phase = TouchPhase.Began } );
		cmd.Parse(new RawTouch { phase = TouchPhase.Moved } );
		cmd.Parse(new RawTouch { phase = TouchPhase.Stationary } );
		cmd.Parse(new RawTouch { phase = TouchPhase.Ended } );
		AssertEquals(cmd.status, GParserStatus.success);
	}	
	
	private RawTouch MakeRawTouchDown()
	{
		return new RawTouch() {
			phase = TouchPhase.Began
		};
	}
}