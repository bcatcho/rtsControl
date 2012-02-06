using UnityEngine;
using System.Collections;

public abstract class GameActorAbility : Object
{
	public abstract string Name();

	public virtual bool HasAbility(string ability)
	{
		return false;
	}
}
