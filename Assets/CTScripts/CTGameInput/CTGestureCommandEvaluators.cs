using UnityEngine;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;

public enum GParserStatus {
	Failed,
	Inconclusive,
	Success
}

public static class GParserStatusExtensions
{
	public static GParserStatus Plus(this GParserStatus status1, GParserStatus status2)
	{
		if (status1 == GParserStatus.Failed || status2 == GParserStatus.Failed)
			return GParserStatus.Failed;
		
		if (status1 == GParserStatus.Inconclusive || status2 == GParserStatus.Inconclusive)
			return GParserStatus.Inconclusive;
		
		return GParserStatus.Success;
	}
}

public delegate GParserStatus GParser<T>(GParserContext<T> context);

public class GParserCmd<T>
{
	public GParserCmd<T> next;
	public GParser<T> parser;
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

public class GParserCompiler<T>
{
	public GParserCompiler<T> previous;
	public GParser<T> parser;
}

public static class GParserCompilerExtensions
{
	public static GParserCmd<T> End<T>(this GParserCompiler<T> me)
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
	
	public static GParserCompiler<T> TouchDown<T>(this GParserCompiler<T> me) where T : RawTouch
	{
		GParser<T> f = t => t.current.phase == TouchPhase.Began ? GParserStatus.Success : GParserStatus.Failed;
		return me.And(f);
	}

	public static GParserCompiler<T> TouchUp<T>(this GParserCompiler<T> me) where T : RawTouch
	{
		GParser<T> f = t => t.current.phase == TouchPhase.Ended ? GParserStatus.Success : GParserStatus.Failed;
		return me.And(f);
	}
	
	public static GParserCompiler<T> Cmd<T>(this GParserCompiler<T> me, GParserCmd<T> cmd) 
	{
		return me.And(cmd);
	}
	
	public static GParserCompiler<T> Then<T>(this GParserCompiler<T> me)
	{
		return new GParserCompiler<T>{ previous = me };	
	}
	
	public static GParserCompiler<T> ThatStartsWithin<T>(this GParserCompiler<T> me, float milliseconds) where T : RawTouch
	{
		var madeItThroughOnce = false;
		GParser<T> parser = ctx => {
			if (madeItThroughOnce)
				return GParserStatus.Success;
			
			var time0 = ctx.parsed[ctx.start].time;
			if (ctx.current.time - time0 <= milliseconds)
			{
				madeItThroughOnce = true;
				return GParserStatus.Success;
				
			}
			
			return GParserStatus.Failed;
		};
		
		return me.And(parser);
	}
	
	public static GParserCompiler<T> ThenWithin<T>(this GParserCompiler<T> me, float milliseconds) where T : RawTouch
	{
		GParser<T> parser = ctx => {
			var time0 = ctx.parsed[ctx.start].time;
			return ctx.current.time - time0 > milliseconds ? GParserStatus.Failed : GParserStatus.Success;
		};
		
		return new GParserCompiler<T> { previous = me, parser = parser };
	}

	public static GParserCompiler<T> And<T>(this GParserCompiler<T> me, GParser<T> p)
	{
		me.parser = me.parser.And(p);
		return me;
	}
	
	public static GParserCompiler<T> And<T>(this GParserCompiler<T> me, GParserCmd<T> cmd)
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
		if (me.status == GParserStatus.Success)
		{
			if (me.next != null)
			{
				me.status = GParserStatus.Inconclusive;
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
	public static GParser<T> And<T>(this GParser<T> pMe, GParser<T> pNext)
	{
		if (pMe == null) 
			return pNext;
				
		return input => {
			var result = pMe(input);
			if (result == GParserStatus.Failed)
				return result;
			
			return result.Plus(pNext(input));
		};
	}
	
	public static GParser<T> And<T>(this GParser<T> pMe, GParserCmd<T> cmd)
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
			if (result == GParserStatus.Failed)
				return result;
			
			cmd.Parse(input);
			return result.Plus(cmd.status);
		};
	}
}