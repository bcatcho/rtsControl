using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CTGestureParserGraph
{
	private CTGestureParserGraphNode root;
	private CTGestureParserGraphNode current;
	
	public CTGestureParserGraph() 
	{
		root = new CTGestureParserGraphNode();
		Rewind();
	}
	
	public void Rewind()
	{
		current = root;
	}
	
	public void Advance()
	{
		current = current.pass;	
	}
	
	public void AddNodeForEvals(CTGestureParserExpression expr)
	{
		var evalsString = expr.ToTokenString();
		var currentTokenString = current.ToTokenString();
		
		if (current.nodeType == CTGestParserGraphNodeType.terminal)
		{
			Graph_RewindAndDiverge();
			AddNodeForEvals(expr); // and try again
		}
		// you match the current node
		else if (evalsString == current.ToTokenString())
		{
			Graph_AdvanceOnDuplicate();
		}
		// the current node is not blank, but it doesn't match what you are doing
		else if (currentTokenString != string.Empty && evalsString != currentTokenString)
		{
			if (current.Overlaps(expr))
			{
				
			}
		}
		// the current node is blank
		else if (currentTokenString == string.Empty)
		{
			Graph_AppendNewEvals(expr);
		}
		else
		{
			Debug.LogError("unexpected state encountered");
		}
	}
	
	private void Graph_AdvanceOnDuplicate()
	{
		current = current.pass;
	}

	private void Graph_RewindAndDiverge()
	{
		current = current.parent; // backtrack
		// HACK: blindly hoping that current is the end of a gesture
		if (current.fail == null)
		{	
			current.fail = new CTGestureParserGraphNode(current);
		}
		current = current.fail; // set current to a non terminal
	}

	private void Graph_AppendNewEvals(CTGestureParserExpression expr)
	{
		current.expr += expr;
		current.pass = new CTGestureParserGraphNode(current);
		current = current.pass;
	}
	
	public void AddTerminal(string terminalName)
	{
		current.nodeType = CTGestParserGraphNodeType.terminal;
		current.terminalName = terminalName;
	}
	
	public CTGestureParserGraphNode CurrentNode()
	{
		return current;
	}

	public CTGestureParserGraph Pass()
	{
		current = current.pass;
		return this;
	}
	
	public CTGestureParserGraph Fail()
	{
		current = current.fail;
		return this;
	}
	
	public string GetCurrentTokenString()
	{
		return current.ToTokenString();	
	}
}

public class CTGestureParserGraphNode
{
	public CTGestureParserExpression expr;
	public CTGestureParserGraphNode pass;
	public CTGestureParserGraphNode fail;
	public CTGestureParserGraphNode parent;
	public CTGestParserGraphNodeType nodeType;
	public string terminalName;
	
	public CTGestureParserGraphNode(CTGestureParserGraphNode parent = null)
	{
		this.parent = parent;
		expr = new CTGestureParserExpression();
	}
	
	public string ToTokenString()
	{
		if (nodeType == CTGestParserGraphNodeType.terminal)
			return "$"+terminalName;
		
		return expr.ToTokenString();
	}

	public bool Overlaps(CTGestureParserExpression expr)
	{
		return false;
	}
}

public enum CTGestParserGraphNodeType
{
	eval,
	terminal,
	wildcard
}