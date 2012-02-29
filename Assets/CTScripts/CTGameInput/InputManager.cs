using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RawTouch
{
	public Ray ray;
	public Vector3 rawPosition;
	public Vector3 deltaPosition;
	public float deltaTime;
	public int fingerId;
	public TouchPhase phase;
	public Vector2 position;
	public int tapCount;
	public float time;
}

public class InputManager : MonoBehaviour
{
	private GameObject _selected;
	private RawInputInterpreter rawInputInterpreter;
	private Camera _currentCamera;
	
	void Start()
	{	
		rawInputInterpreter = new RawInputInterpreter();
		_currentCamera = Camera.main;
	}
	
	void Update()
	{
		CollectRawInput();
		rawInputInterpreter.Update();
	}
	
	private void CollectRawInput()
	{
		foreach (Touch touch in Input.touches) {
			RawTouch rawTouch = new RawTouch()
			{
				deltaPosition = touch.deltaPosition,
				deltaTime = touch.deltaTime,
				fingerId = touch.fingerId,
				phase = touch.phase,
				tapCount = touch.tapCount,
				time = Time.time,
				rawPosition = touch.position
			};
			rawTouch.ray = _currentCamera.ScreenPointToRay(touch.position);
			rawTouch.position = rawTouch.ray.origin;
			GameMessenger.SendNow("touch", this, rawTouch);
		}
	}	
}

class RawInputInterpreter
{
	private GameInputStateMachine _interpretState;
	
	public RawInputInterpreter()
	{
		GameMessenger.Reg("touch", this, Message_Touch);
		_interpretState = new GameInputStateMachine();
		_interpretState.AddState("waiting", new GameInputState_Waiting());
		_interpretState.AddState("began", new GameInputState_Began());
		_interpretState.AddState("dragging", new GameInputState_Dragging());
		_interpretState.AddState("ending", new GameInputState_Ending());
		_interpretState.TransitionTo("waiting");
	}
	
	public GameMessageResult Message_Touch(GameMessage msg)
	{
		var touch = (RawTouch)msg.data;
		InterperetInput(touch);
		return GameMessageResult.handledMessage;
	}
	
	public void InterperetInput(RawTouch touch)
	{
		if (touch.fingerId == 0) {
			_interpretState.TransitionWithTouch(touch);
		}
	}

	public void Update()
	{
		_interpretState.Update();	
	}

	private class GameInputState_Waiting : GameInputState
	{
		public override void EnterState()
		{
			stateMachine.trackingObject = null;
		}
		
		public override void TransitionWithTouch(RawTouch touch)
		{
			if (touch.phase == TouchPhase.Began) {
				stateMachine.TransitionTo("began");	
			}
		}
	}
	
	private class GameInputState_Began : GameInputState
	{
		public override void EnterState()
		{
			var hitInfo = new RaycastHit();
			var touchRay = stateMachine.lastTouch.ray;
			int mask = 1 << LayerMask.NameToLayer("actor");
			var objectFound = Physics.Raycast(touchRay, out hitInfo, 4000f, mask);
			if (objectFound) {
				GameMessenger.SendNow("touchStarted", stateMachine, null);
			}	
		}
		
		public override void TransitionWithTouch(RawTouch touch)
		{
			if (touch.phase == TouchPhase.Ended) {
				stateMachine.TransitionTo("ending");	
			}
			else if (touch.phase == TouchPhase.Moved) {
				stateMachine.TransitionTo("dragging");
			}
		}
	}
	
	private class GameInputState_Ending : GameInputState
	{
		private float _timeOfTouchEnded;

		public override void EnterState()
		{
			if (stateMachine.trackingObject != null) {
				_timeOfTouchEnded = Time.time;	
			}
		}
		
		public override void Update()
		{
			if (Time.time - _timeOfTouchEnded > .2) {
				stateMachine.TransitionTo("waiting");
			}
		}
		
		public override void ExitState()
		{
			GameMessenger.SendNow("touchEnded", stateMachine, null);	
		}
	}
	
	private class GameInputState_Dragging : GameInputState
	{	
		public override void TransitionWithTouch(RawTouch touch)
		{
			if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
				stateMachine.TransitionTo("ending");
			}
			else if (touch.phase == TouchPhase.Moved) {
				GameMessenger.SendNow("touchMoved", stateMachine, touch.ray);
			}
		}
	}
	
	private class GameInputStateMachine : StateMachine<GameInputState>
	{
		public GameObject trackingObject;
		public RawTouch lastTouch;
		
		public void TransitionWithTouch(RawTouch touch)
		{
			lastTouch = touch;
			currentState.TransitionWithTouch(lastTouch);
		}
	}
	
	private class GameInputState : State<GameInputStateMachine>
	{
		public virtual void TransitionWithTouch(RawTouch touch)
		{
		}
	}
}

