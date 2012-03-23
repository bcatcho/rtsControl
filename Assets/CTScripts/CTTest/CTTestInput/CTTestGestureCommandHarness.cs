using UnityEngine;
using System.Collections.Generic;

// HACK: the overall solution wont work because we mutate the parser. We would have to make a new parser for every new gesture.
// 		perhaps the context should contain all of that information that is mutable.
public class CTTestGestureCommandHarness : CTTestHarness {
	
	public void GParser_CombiningDelegates_Success()
	{
		GParser<RawTouch> td = t => t.current.phase == TouchPhase.Began ? GParserStatus.Success : GParserStatus.Failed;
		GParser<RawTouch> x = t => t.current.fingerId == 0 ? GParserStatus.Success : GParserStatus.Failed;
		GParser<RawTouch> td_x = td.And(x);
		
		var cmd = new GParserCompiler<RawTouch>().And(td_x).End();
		cmd.Parse(new RawTouch { phase = TouchPhase.Began, fingerId = 0});
		
		AssertEquals(cmd.status, GParserStatus.Success);
	}
	
	public void GParser_CombiningDelegates_ShortCircuitFailure()
	{
		GParser<RawTouch> td = t => t.current.phase == TouchPhase.Began ? GParserStatus.Success : GParserStatus.Failed;
		GParser<RawTouch> x = t => t.current.fingerId == 0 ? GParserStatus.Success : GParserStatus.Failed;
		GParser<RawTouch> y = t => t.current.time == 0 ? GParserStatus.Success : GParserStatus.Failed;
		GParser<RawTouch> tdxy = td.And(x).And(y);
		
		
		var cmd = new GParserCompiler<RawTouch>().And(tdxy).End();
		cmd.Parse(new RawTouch { phase = TouchPhase.Began, fingerId = 0, time = 1});
		
		AssertEquals(cmd.status, GParserStatus.Failed);
	}
	
	public void GParserCmd_MultistepCommand_TooLittleInputIsInconclusive()
	{
		var cmd = new GParserCompiler<RawTouch>().TouchDown().Then().TouchUp().End();
		cmd.Parse(t(TouchPhase.Began) );
		AssertEquals(cmd.status, GParserStatus.Inconclusive);
	}

	public void GParserCmd_MultistepCommand_EnoughInputIsSuccessful()
	{
		var cmd = new GParserCompiler<RawTouch>().TouchDown().Then().TouchUp().End();
		
		cmd.Parse(t(TouchPhase.Began) );
		cmd.Parse(t(TouchPhase.Ended) );
		AssertEquals(cmd.status, GParserStatus.Success);
	}
	
	public void GParserCmd_MultistepCommand_Bad2ndInputFails()
	{
		var cmd = new GParserCompiler<RawTouch>().TouchDown().Then().TouchUp().End();
		
		cmd.Parse(t(TouchPhase.Began) );
		cmd.Parse(t(TouchPhase.Began) );
		AssertEquals(cmd.status, GParserStatus.Failed);
	}
	
	public void GParserCmd_NestedCustomCommands_ExcutesInOrder()
	{
		var cmdTdTd = new GParserCompiler<RawTouch>().TouchDown().Then().TouchDown().End();
		var cmd = new GParserCompiler<RawTouch>().Cmd(cmdTdTd).Then().TouchUp().End();
		
		cmd.Parse(t(TouchPhase.Began) );
		cmd.Parse(t(TouchPhase.Began) );
		cmd.Parse(t(TouchPhase.Ended) );
		AssertEquals(cmd.status, GParserStatus.Success);
	}
	
	public void GParserCmd_TimeLimitedWildcard_TimeLimitExcededReturnsFailure()
	{
		var cmd = new GParserCompiler<RawTouch>()
			.TouchDown()
			.ThenWithin(-10).TouchUp()
			.End();
		
		cmd.Parse(t(TouchPhase.Began ) );
		cmd.Parse(t(TouchPhase.Moved ) );
		cmd.Parse(t(TouchPhase.Stationary ) );
		cmd.Parse(t(TouchPhase.Ended ) );
		AssertEquals(cmd.status, GParserStatus.Failed);
	}
	
	public void GParserCmd_TimeLimitedWildcardT_imeLimitNotExceededReturnsSuccess()
	{
		var cmd = new GParserCompiler<RawTouch>()
			.TouchDown()
			.ThenWithin(100).TouchUp()
			.End();
		
		cmd.Parse( t(TouchPhase.Began) );
		cmd.Parse( t(TouchPhase.Moved) );
		cmd.Parse( t(TouchPhase.Stationary) );
		cmd.Parse( t(TouchPhase.Ended) );
		AssertEquals(cmd.status, GParserStatus.Success);
	}	
	
	public void GParserCmd_DoubleTap()
	{
		var tap = new GParserCompiler<RawTouch>().TouchDown().ThenWithin(50).TouchUp();
		var doubleTap = new GParserCompiler<RawTouch>()
			.Cmd(tap.End()).Then()
			.Cmd(tap.End()).ThatStartsWithin(4)
			.End();
			
		// tap1
		doubleTap.Parse( t(TouchPhase.Began, 0) );
		doubleTap.Parse( t(TouchPhase.Moved, 1) );
		doubleTap.Parse( t(TouchPhase.Stationary, 2) );
		doubleTap.Parse( t(TouchPhase.Ended, 3) );
		// tap2 -- gap of 4 milliseconds
		doubleTap.Parse( t(TouchPhase.Began, 7) );
		doubleTap.Parse( t(TouchPhase.Moved, 8) );
		doubleTap.Parse( t(TouchPhase.Stationary, 9) );
		doubleTap.Parse( t(TouchPhase.Ended, 10) );
		
		AssertEquals(doubleTap.status, GParserStatus.Success);
	}	
	
	[CTTestIgnore]
	private RawTouch t(TouchPhase phase = TouchPhase.Moved,
		float time = 0, 
		float pX = 0, float pY = 0)
	{
		return new RawTouch() {
			phase = phase,
			position = new Vector2(pX, pY),
			time = time
		};
	}
}