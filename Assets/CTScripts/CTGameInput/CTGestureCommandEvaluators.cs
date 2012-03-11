using UnityEngine;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;

#region non-functional
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

#endregion

#region functional

public enum GParserStatus {
	failed,
	inconclusive,
	success
}

public class GParser<T>
{
	public GParse<T> Parse;
	public GParserStatus status;
	public GParser()
	{
	}
}

public delegate GParserStatus GParse<T>(GParserContext<T> context);

public class GParserCmd<T>
{
	public GParserCmd<T> next;
	public GParse<T> parser;
	public GParserStatus status; 
	public GParserContext<T> context;	
	
	public GParserCmd()
	{
		context = new GParserContext<T>();
	}
}

public class GParserContext<T>
{
	public List<T> parsed;
	public int start;
	public T current;
	
	public GParserContext()
	{
		start = 0;
		parsed = new List<T>();
	}
	
	public void Add(T input)
	{
		if (current != null)
			parsed.Add(input);
		current = input;
	}
}

public class GParserPhrase<T>
{
	public GParserPhrase<T> previous;
	public GParse<T> parser;
}

public static class GParserPhraseExtensions
{
	public static GParserCmd<T> End<T>(this GParserPhrase<T> me)
	{
		var currentPhrase = me;
		GParserCmd<T> cmd = null;
		
		do 
		{
			cmd = new GParserCmd<T> { parser = currentPhrase.parser, next = cmd };
			currentPhrase = currentPhrase.previous;
		} 
		while (currentPhrase != null) ;
	
		return cmd;
	}
	
	public static GParserPhrase<T> TouchDown<T>(this GParserPhrase<T> me) where T : RawTouch
	{
		GParse<T> f = t => t.current.phase == TouchPhase.Began ? GParserStatus.success : GParserStatus.failed;
		me.And(f);
		return me;
	}

	public static GParserPhrase<T> TouchUp<T>(this GParserPhrase<T> me) where T : RawTouch
	{
		GParse<T> f = t => t.current.phase == TouchPhase.Ended ? GParserStatus.success : GParserStatus.failed;
		me.And(f);
		return me;
	}
	
	public static GParserPhrase<T> Cmd<T>(this GParserPhrase<T> me, GParserCmd<T> cmd) 
	{
		me.And(cmd);
		return me;
	}
	
	public static GParserPhrase<T> Then<T>(this GParserPhrase<T> me)
	{
		return new GParserPhrase<T>{ previous = me };	
	}
	
	public static GParserPhrase<T> ThenWithin<T>(this GParserPhrase<T> me, float milliseconds) where T : RawTouch
	{
		GParse<T> parser = ctx => {
			var time0 = ctx.parsed[ctx.start].time;
			return ctx.current.time - time0 > milliseconds ? GParserStatus.failed : GParserStatus.success;
		};
		
		return new GParserPhrase<T> { previous = me, parser = parser };
	}

	public static GParserPhrase<T> And<T>(this GParserPhrase<T> me, GParse<T> p)
	{
		me.parser = me.parser.And(p);
		return me;
	}
	
	public static GParserPhrase<T> And<T>(this GParserPhrase<T> me, GParserCmd<T> cmd)
	{
		me.parser = me.parser.And(cmd);
		return me;
	}
}


public static class GParserCmdExtensions
{
	public static void Parse<T>(this GParserCmd<T> me, T t)
	{
		me.context.current = t;
		me.status = me.parser(me.context);
		me.context.parsed.Add(t); 
		if (me.status == GParserStatus.success)
		{
			if (me.next != null)
			{
				me.status = GParserStatus.inconclusive;
				me.parser = me.next.parser;
				me.next = me.next.next;
				me.context.start = me.context.parsed.Count - 1;
			}
		}	
	}
	
	public static void Parse<T>(this GParserCmd<T> me, GParserContext<T> ctx)
	{
		me.Parse(ctx.current);
	}
}

public static class GParseExtensions
{
	public static GParse<A> And<A>(this GParse<A> pMe, GParse<A> pNext)
	{
		if (pMe == null) 
			return pNext;
				
		return input => {
			var result = pMe(input);
			return result == GParserStatus.success ? pNext(input) : result;
		};
	}
	
	public static GParse<T> And<T>(this GParse<T> pMe, GParserCmd<T> cmd)
	{
		if (pMe == null) 
		{	
			return input => {
				cmd.Parse(input);
				return cmd.status;
			};
		}
		return input => {
			var result = pMe(input);
			if (result == GParserStatus.success)
			{
				cmd.Parse(input);
				return cmd.status;
			}
			
			return result;
		};
	}
	
}

#endregion