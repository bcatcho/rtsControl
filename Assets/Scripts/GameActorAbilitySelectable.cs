using UnityEngine;

public class GameActorAbilitySelectable : GameActorAbility
{
	private bool _isSelected = false;
	private OTSprite _mySprite;

	void Start()
	{
		GameMessenger.Current().Register("touchStarted", this, Message_TouchStarted);
		_mySprite = gameObject.GetComponent<OTSprite>();
		
	}
	
	public void Message_TouchStarted(GameMessage msg)
	{
		ToggleSelected();
		GameMessenger.Current().Register("touchEnded", this, Message_TouchEnded);
		GameMessenger.Current().Register("touchMoved", this, Message_TouchMoved);	
	}

	public void Message_TouchMoved(GameMessage msg)
	{
		if (msg.data != null) {
			var position = (Ray)msg.data;
			gameObject.transform.position = position.origin; 
		}
	}
	
	public void Message_TouchEnded(GameMessage msg)
	{
		ToggleSelected();
		GameMessenger.Current().Deregister("touchEnded", this);
		GameMessenger.Current().Deregister("touchMoved", this);	
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
