using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CTGestureParserExpression : IEnumerable<CTGestureParserExpression>
{
	private List<CTGestureParserToken> _tokens;
	
	public List<CTGestureParserToken> Tokens()
	{
		return _tokens;	
	}
	
	public CTGestureParserExpression()
	{
		_tokens = new List<CTGestureParserToken>();
	}

	public static CTGestureParserExpression operator +(CTGestureParserExpression expr1, CTGestureParserExpression expr2)
	{
		expr1._tokens.AddRange(expr2._tokens);
		return expr1;
	}

	public static CTGestureParserExpression operator +(CTGestureParserExpression expr1, CTGestureParserToken t)
	{
		expr1._tokens.Add(t);
		return expr1;
	}
	
	public bool Equals(CTGestureParserExpression expr2)
	{
		return _tokens.SequenceEqual(expr2._tokens);
	}
	
	public IEnumerator<CTGestureParserExpression> GetEnumerator()
	{
		var subExpression = new CTGestureParserExpression();
		foreach (var t in _tokens)
		{
			if (t.tokenType == CTGestParserTokenType.eval)
			{
				subExpression += t;	
			}
			else
			{
				yield return subExpression;
				subExpression = new CTGestureParserExpression();
			}
		}
		
		yield return subExpression;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();	
	}
	
	public string ToTokenString()
	{
		return string.Join(" ", _tokens.Select(t => t.ToTokenString()).ToArray());
	}
}
