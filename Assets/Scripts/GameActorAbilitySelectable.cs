using UnityEngine;

public class GameActorAbilitySelectable : GameActorAbility
{
	private bool _isSelected = false;
	private OTSprite _mySprite;

	void Start()
	{
		
		_mySprite = gameObject.GetComponent<OTSprite>();
	}
	
	public void ToggleSelected()
	{
		_isSelected = !_isSelected;
		float scaledSize = (_isSelected) ? 1.1f : (1f / 1.1f);
		 
		_mySprite.size = _mySprite.size * scaledSize;
	}
	
	public override string Name()
	{
		return "GameActorAbilitySelectable";	
	}
	
	public override bool HasAbility(string ability)
	{
		return ability == Name();
	}
	
	public override bool Message(string messageName)
	{
		switch (messageName) {
			case "select":
				ToggleSelected();
				return true;
		}
		return false;
	}
}
