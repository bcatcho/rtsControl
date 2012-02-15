using UnityEngine;

public class GameActorAbilitySelectable : MonoBehaviour
{
	private bool _isSelected = false;
	private OTSprite _mySprite;

	void Start()
	{
		GameMessenger.Reg("touchStarted", this, Message_TouchStarted);
		_mySprite = gameObject.GetComponent<OTSprite>();
		
	}
	
	public GameMessageResult Message_TouchStarted(GameMessage msg)
	{
		ToggleSelected();
		GameMessenger.Reg("touchEnded", this, Message_TouchEnded);
		GameMessenger.Reg("touchMoved", this, Message_TouchMoved);	
		return GameMessageResult.handledMessage;
	}

	public GameMessageResult Message_TouchMoved(GameMessage msg)
	{
		if (msg.data != null) {
			var position = (Ray)msg.data;
			gameObject.transform.position = position.origin; 
		}
		return GameMessageResult.handledMessage; 
	}
	
	public GameMessageResult Message_TouchEnded(GameMessage msg)
	{
		ToggleSelected();
		GameMessenger.DeReg("touchEnded", Message_TouchEnded);
		GameMessenger.DeReg("touchMoved", Message_TouchMoved);	
		return GameMessageResult.handledMessage;
	}
	
	private void ToggleSelected()
	{
		_isSelected = !_isSelected;
		float scaledSize = (_isSelected) ? 1.1f : (1f / 1.1f);
		 
		_mySprite.size = _mySprite.size * scaledSize;
	}
}
