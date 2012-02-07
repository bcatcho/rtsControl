using UnityEngine;
using System.Collections;

public abstract class GameActorAbility : MonoBehaviour
{
	public abstract string Name();

	public virtual bool HasAbility(string ability)
	{
		return false;
	}

	public virtual bool Message(string messageName)
	{
		return false;	
	}
}
