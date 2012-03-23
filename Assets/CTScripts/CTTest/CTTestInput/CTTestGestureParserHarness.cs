using UnityEngine;
using System.Collections.Generic;

public class CTTestGestureParserHarness : CTTestHarness 
{
	public void CTGestDef__WithEvalsAndBinary__TokenStringIsCorrect()
	{
		var eval = "tap".GestDef().TouchDown().Then().TouchUp();

		AssertEquals(eval.ToTokenString(), "tap := touchDown > touchUp");		
	}
	
	public void CTGestDef__EvalWithParameters__TokenStringIsCorrect()
	{
		var eval = "tap".GestDef()
			.TouchDown().Then()
			.TouchUp().Then()
			.EndAfter(10);

		AssertEquals(eval.ToTokenString(), "tap := touchDown > touchUp > endAfter(10)");		
	}
	
	public void CTGestDef__MultipleEvals__TokenStringIsCorrect()
	{
		var eval = "tap".GestDef().TouchDown().TouchUp();

		AssertEquals(eval.ToTokenString(), "tap := touchDown touchUp");		
	}
	
	public void CTGestParserCompilerReduce__NestedDefinitions__ReducesNestedDefinitionToEvals()
	{
		var compiler = new CTGestureParserCompiler();
		var defs = new List<CTGestDef> 
		{
			"tap".GestDef().TouchDown().Then().TouchUp(),
			"touchtap".GestDef().TouchDown().Def("tap"),
		};
		
		compiler.AddDefinitions(defs);
		var reducedDef = new CTGestDef("touchtapReduced") {
			expression = compiler.ReduceDefinition(defs[1]),
		};
		
		AssertEquals(reducedDef.ToTokenString(), "touchtapReduced := touchDown touchDown > touchUp");
	}
	
	public void CTGestParserCompilerBuildGraph__MultipleEvals__CombinedToOneGraphNode()
	{
		var compiler = BuildCompiler(new List<CTGestDef>{
			"tap".GestDef().TouchDown().TouchUp()
		});
		
		var graph = compiler.Compile();
		
		AssertEquals(graph.CurrentNode().ToTokenString(), "touchDown touchUp");
	}
	
	public void CTGestParserCompilerBuildGraph__EvalThenEval__MakesTwoGraphNodes()
	{
		var compiler = BuildCompiler(new List<CTGestDef>{
			"tap".GestDef().TouchDown().Then().TouchUp()
		});
		
		var graph = compiler.Compile();
		
		AssertEquals(graph.CurrentNode().ToTokenString(), "touchDown");
		
		graph.Advance();
		AssertEquals(graph.CurrentNode().ToTokenString(), "touchUp");
	}
	
	public void CTGestParserCompilerBuildGraph__WholeGesture__EndsWithATerminal()
	{
		var compiler = BuildCompiler(new List<CTGestDef>{
			"tap".GestDef().TouchDown().Then().TouchUp()
		});
		
		var graph = compiler.Compile();	
		graph.Advance();
		graph.Advance();
		AssertEquals(graph.CurrentNode().ToTokenString(), "$tap");
	}
	
	public void CTGestParserCompilerBuildGraph__TwoOverlappingGestures__SecondGestureAttachedToTheFailParamOfFirst()
	{
		var compiler = BuildCompiler(new List<CTGestDef>{
			"tap".GestDef().TouchDown().Then().TouchUp().Then().EndAfter(50),
			"doubleTap".GestDef().Def("tap").Then().Def("tap")
		});
		
		var graph = compiler.Compile();
		graph.Pass().Pass().Pass();
		AssertEquals(graph.GetCurrentTokenString(), "$tap");
		
		graph.Rewind();
		graph.Pass().Pass().Fail(); // fail to be a tap -> head towards double tap
		graph.Pass().Pass().Pass();
		AssertEquals(graph.GetCurrentTokenString(), "$doubleTap");
	}
	
	
	
	[CTTestIgnore]
	private CTGestureParserCompiler BuildCompiler(List<CTGestDef> defs)
	{
		var compiler = new CTGestureParserCompiler();
		compiler.AddDefinitions(defs);
		return 	compiler;
	}
		
	[CTTestIgnore]
	private RawTouch t(
		TouchPhase phase = TouchPhase.Moved,
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
