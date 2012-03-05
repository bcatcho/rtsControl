using UnityEngine;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;

public partial class CTGestureCommand
{
	public CTGestureCommand Then{
		get{
			children.Add(new CTGestureCommandTerminal());
			return this;
		}
	}
	
	public CTGestureCommand Any(Expression<Func<CTGestureCommand,CTGestureCommand>> expr)
	{
		var compiled = expr.Compile();
		var cmd = compiled(new CTGestureCommand("tmp"));
		var anyCmd = new CTGestureCommandAny(cmd.Children());
		children.Add(anyCmd);
		return this;
	}
	
	public CTGestureCommand TouchDown()
	{
		children.Add(new CTGestureCommandTouchDown());
		return this;
	}
	
	public CTGestureCommand TouchMoved()
	{
		children.Add(new CTGestureCommandTouchMoved());
		return this;
	}
	
	public CTGestureCommand TouchUp()
	{
		children.Add(new CTGestureCommandTouchUp());
		return this;
	}
	
	// TODO: Reconsider this. This is very counterintuitive.
	public static CTGestureCommand operator +(CTGestureCommand cmd1, CTGestureCommand cmd2)
	{
		cmd1.Add(cmd2);
		return cmd1;
	}
}

public class CTGestureCommandTerminal : CTGestureCommand
{
	public CTGestureCommandTerminal() : base("Terminal") {}
	
	public override CTGestureCommandResult Process(RawTouch t)
	{
		return CTGestureCommandResult.terminal;
	}
}

public class CTGestureCommandTouchDown : CTGestureCommand
{
	public CTGestureCommandTouchDown() : base("TouchDown") {}
	
	public override CTGestureCommandResult Process(RawTouch t)
	{
		if (t.phase == TouchPhase.Began)
			return CTGestureCommandResult.gestureRecognized;
		
		return CTGestureCommandResult.failed;
	}
}

public class CTGestureCommandTouchUp : CTGestureCommand
{
	public CTGestureCommandTouchUp() : base("TouchUp") {}
	
	public override CTGestureCommandResult Process(RawTouch t)
	{
		if (t.phase == TouchPhase.Ended)
			return CTGestureCommandResult.gestureRecognized;
		
		return CTGestureCommandResult.failed;
	}
}

public class CTGestureCommandTouchMoved : CTGestureCommand
{
	public CTGestureCommandTouchMoved() : base("TouchMoved") {}
	
	public override CTGestureCommandResult Process(RawTouch t)
	{
		if (t.phase == TouchPhase.Moved)
			return CTGestureCommandResult.gestureRecognized;
		
		return CTGestureCommandResult.failed;
	}
}

public class CTGestureCommandAny : CTGestureCommand
{
	public CTGestureCommandAny(List<CTGestureCommand> commands) : base("Any") {
		children.AddRange(commands);
	}
	
	public override CTGestureCommandResult Process(RawTouch t)
	{
		foreach(var cmd in children)
		{
			if (cmd.Process(t) == CTGestureCommandResult.gestureRecognized)
				return CTGestureCommandResult.needsMoreInput;
		}
		
		return CTGestureCommandResult.gestureRecognized;
	}	
}