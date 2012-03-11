using UnityEngine;
using System.Collections.Generic;

public enum CTGestureCommandResult 
{
	failed,
	terminal,
	gestureRecognized,
	needsMoreInput
}

public partial class CTGestureCommand 
{
	public string name;
	internal List<CTGestureCommand> children;
	private int currentChildIndex;
	
	public List<CTGestureCommand> Children()
	{
		return children;	
	}
	
	public CTGestureCommand(string commandName)
	{
		name = commandName;
		children = new List<CTGestureCommand>();
		currentChildIndex = 0;
	}
	
	public void Add(CTGestureCommand command)
	{
		children.Add(command);
	}
	
	public virtual CTGestureCommandResult Process(RawTouch t)
	{
		CTGestureCommand child;
		while(currentChildIndex < children.Count)
		{
			child = children[currentChildIndex];
			currentChildIndex++;
			var result = child.Process(t);
			
			if (result == CTGestureCommandResult.terminal)
			{
				return CTGestureCommandResult.needsMoreInput;
			}
			else if (result == CTGestureCommandResult.needsMoreInput)
			{
				currentChildIndex--;
				return CTGestureCommandResult.needsMoreInput;
			}
			else if (result == CTGestureCommandResult.failed)
			{
				return CTGestureCommandResult.failed;
			}
		}
		
		return CTGestureCommandResult.gestureRecognized;
	}
	
	public CTGestureCommand Cmd(CTGestureCommand command)
	{
		children.Add(command);
		return this;
	}
}

public static class CTGestureCommandExtensions
{
	
}