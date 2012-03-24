using UnityEngine;
using System.Collections.Generic;

public class CTGestureParserCompiler
{
	private Dictionary<string, CTGestDef> definitions;
	
	public CTGestureParserCompiler() 
	{
		definitions = new Dictionary<string, CTGestDef>();
	}
	
	public void AddDefinitions(List<CTGestDef> defs)
	{
		foreach (var def in defs)
		{
			definitions.Add(def.name, def);
		}
	}
	
	public CTGestureParserExpression ReduceDefinition(CTGestDef def)
	{
		var expr = new CTGestureParserExpression();
		
		foreach (var t in def.expression.Tokens())
		{
			if 	(t.tokenType == CTGestParserTokenType.def)
			{
				var replacementDef = definitions[t.name];
				expr += ReduceDefinition(replacementDef);
			}
			else {
				expr += t;
			}
		}
		
		return expr;
	}

	public CTGestureParserGraph Compile()
	{
		var graph = new CTGestureParserGraph();
		foreach (var def in definitions.Values)
		{
			foreach (var subExpression in ReduceDefinition(def))
			{
				graph.AddExpression(subExpression);
			}
			graph.AddTerminal(def.name);	
			graph.Rewind();
		}
		
		return graph;
	}
}
