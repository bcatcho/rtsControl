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
	
	public void AddExpression(CTGestureParserExpression expr)
	{
		current = current.AddBranch(expr);
	}
		
	public void AddTerminal(string terminalName)
	{
		current.AddTerminal(terminalName);
	}

	public CTGestureParserGraph GoBranch(int index = 0)
	{
		current = current.branches[index];
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
	public List<CTGestureParserGraphNode> branches;
	public CTGestureParserGraphNode parent;
	public CTGestParserGraphNodeType nodeType;
	public string terminalName;
	
	public CTGestureParserGraphNode(CTGestureParserGraphNode parent = null)
	{
		this.parent = parent;
		branches = new List<CTGestureParserGraphNode>();
		expr = new CTGestureParserExpression();
	}
	
	public CTGestureParserGraphNode AddBranch(CTGestureParserExpression expression)
	{
		// if you are trying to chain on to a terminal, back track and chain from there.
		// this happens with subcommands
		if (this.nodeType == CTGestParserGraphNodeType.terminal)
		{
			return parent.AddBranch(expression);
		}
		
		foreach (var b in branches)
		{
			if (b.expr.Equals(expression))
			{
				return b;	
			}
		}
		
		branches.Add(new CTGestureParserGraphNode(this) {
			expr = expression,
		});
		
		return branches.Last();
	}
	
	public void AddTerminal(string terminalName)
	{
		if (branches.Count > 0)
			Debug.LogError("Can't end gesture in the middle of another");
		else if (this.terminalName != null)
			Debug.LogError(string.Format("Duplicate gestures exist. Tried to terminate {0}, but found the terminal for {1}", terminalName, this.terminalName));
		else
		{
			this.terminalName = terminalName;
			nodeType = CTGestParserGraphNodeType.terminal;
		}
	}
	
	public string ToTokenString()
	{
		if (nodeType == CTGestParserGraphNodeType.terminal)
			return expr.ToTokenString() +" $"+terminalName;
		
		return expr.ToTokenString();
	}
}

public enum CTGestParserGraphNodeType
{
	eval,
	terminal,
	wildcard
}