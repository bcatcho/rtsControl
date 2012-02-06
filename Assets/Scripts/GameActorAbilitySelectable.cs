using UnityEngine;

public class GameActorAbilitySelectable : GameActorAbility
{
	public override string Name()
	{
		return "GameActorAbilitySelectable";	
	}
	
	public override bool HasAbility(string ability)
	{
		return ability == Name();
	}
}
