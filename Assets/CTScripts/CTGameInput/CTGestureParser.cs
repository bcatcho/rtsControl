using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CTGestureParser 
{
	public CTGestureParser() {}
}


public class CTGestDef
{
	public CTGestureParserExpression expression;
	public string name;
	
	public CTGestDef(string name)
	{
		expression = new CTGestureParserExpression();
		this.name = name;
	}
	
	public CTGestDef AddFluent(CTGestureParserToken next)
	{
		expression += next;
		return this;
	}
	
	public string ToTokenString()
	{
		return name + " := " + expression.ToTokenString();
	}
}

public class CTGestureParserToken : System.IEquatable<CTGestureParserToken>
{
	public string name;
	public CTGestParserTokenType tokenType;
	public List<string> parameters;
	
	public CTGestureParserToken(string name, CTGestParserTokenType tokenType)
	{
		this.name = name;
		this.tokenType = tokenType;
		parameters = new List<string>();
	}
	
	public string ToTokenString()
	{
		var str = name;
		if (parameters.Count > 0)
		{
			str += "(" + string.Join(", ", parameters.ToArray()) + ")";
		}
		return str;		
	} 

	public override int GetHashCode()
	{
		return (int)tokenType * name.GetHashCode();
	}
	
	public bool Equals(CTGestureParserToken other)
	{
		return name == other.name && tokenType == other.tokenType && parameters.SequenceEqual(other.parameters);
	}
}

public static class CTGestureParserTokenFactory
{
	public static CTGestureParserToken Then()
	{
		return new CTGestureParserToken(">", CTGestParserTokenType.binary);	
	}
}

public static class CTGestDefExtensions
{
	public static CTGestDef Def(this CTGestDef me, string defName)
	{
		return me.AddFluent(new CTGestureParserToken(defName, CTGestParserTokenType.def));
	}
	
	public static CTGestDef TouchDown(this CTGestDef me)
	{
		return me.AddFluent(new CTGestureParserToken("touchDown", CTGestParserTokenType.eval));
	}
	
	public static CTGestDef TouchUp(this CTGestDef me)
	{
		return me.AddFluent(new CTGestureParserToken("touchUp", CTGestParserTokenType.eval));
	}
	
	public static CTGestDef NoInput(this CTGestDef me)
	{
		return me.AddFluent(new CTGestureParserToken( "e", CTGestParserTokenType.empty));
	}
	
	public static CTGestDef Stationary(this CTGestDef me)
	{
		return me.AddFluent(new CTGestureParserToken("stationary", CTGestParserTokenType.eval));
	}
	
	public static CTGestDef Movement(this CTGestDef me)
	{
		return me.AddFluent(new CTGestureParserToken("movement", CTGestParserTokenType.eval));
	}
	
	public static CTGestDef TimeOut(this CTGestDef me)
	{
		return me.AddFluent(new CTGestureParserToken( "timeOut", CTGestParserTokenType.eval));	
	}
	
	public static CTGestDef EndAfter(this CTGestDef me, int milliseconds)
	{
		var token = new CTGestureParserToken( "endAfter", CTGestParserTokenType.eval);
		token.parameters.Add(milliseconds.ToString());
		return me.AddFluent(token);	
	}
	
	public static CTGestDef Then(this CTGestDef me)
	{
		return me.AddFluent(CTGestureParserTokenFactory.Then());	
	}
}

public static class CTStringExtenxsions
{
	public static CTGestDef GestDef(this string me)
	{
		return new CTGestDef(me);	
	}
}

public enum CTGestParserTokenType
{
	empty,
	eval,
	binary,
	function,
	def
}
