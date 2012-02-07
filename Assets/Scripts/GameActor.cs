using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 	An in-game object that has behavior, capabilities and state and recieves messages.
/// </summary>
public class GameActor : MonoBehaviour
{
	private List<GameActorAbility> abilities = new List<GameActorAbility>();
	/// <summary>
	/// Determines whether this instance has ability the specified ability.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance has ability; otherwise, <c>false</c>.
	/// </returns>
	/// <param name='ability'>
	/// Ability.
	/// </param>
	public bool HasAbility(string ability)
	{
		return abilities.Find(act => act.HasAbility(ability)) != null;
	}
	
	public void AddAbility<T>() 
		where T : GameActorAbility
	{
		var abilityFound = abilities.Find(act => act.GetType() == typeof(T));
		if (abilityFound == null)                             
			abilities.Add(gameObject.AddComponent<T>());
	}

	public void Message(string messageName)
	{
		foreach (GameActorAbility a in abilities) {
			if (a.Message(messageName))
				break;
		}
	}
	
	void Update()
	{
	
	}
}
