using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class RawTouch
{
	public Ray ray;
	public Vector2 deltaPosition;
	public float deltaTime;
	public int fingerId;
	public TouchPhase phase;
	public Vector2 position;
	public int tapCount;
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
		List<RawTouch> rawInput = CollectRawInput();
		rawInputInterpreter.InterperetInput(rawInput);
		rawInputInterpreter.Update();
	}
	
	private List<RawTouch> CollectRawInput()
	{
		List<RawTouch> touches = new List<RawTouch>(); // TODO is this too slow?
		foreach (Touch touch in Input.touches) {
			RawTouch rawTouch = new RawTouch()
			{
				deltaPosition = touch.deltaPosition,
				deltaTime = touch.deltaTime,
				fingerId = touch.fingerId,
				phase = touch.phase,
				tapCount = touch.tapCount
			};
			rawTouch.ray = _currentCamera.ScreenPointToRay(touch.position);
			rawTouch.position = rawTouch.ray.origin;
			touches.Add(rawTouch);
		}
		
		return touches;
	}	
}

class RawInputInterpreter
{
	private GameInputStateMachine _interpretState;
	
	public RawInputInterpreter()
	{
		_interpretState = new GameInputStateMachine();
		_interpretState.AddState("waiting", new GameInputState_Waiting());
		_interpretState.AddState("began", new GameInputState_Began());
		_interpretState.AddState("dragging", new GameInputState_Dragging());
		_interpretState.AddState("ending", new GameInputState_Ending());
		_interpretState.TransitionTo("waiting");
	}
	
	public void InterperetInput(List<RawTouch> touches)
	{
		foreach (var touch in touches) {
			if (touch.fingerId == 0) {
				_interpretState.TransitionWithTouch(touch);
			}
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
			var objectFound = Physics.Raycast(touchRay, out hitInfo, 2000f);//, LayerMask.NameToLayer("actor"));
		
			if (objectFound) {
				stateMachine.trackingObject = hitInfo.collider.gameObject;
				
				// TODO i think we should cache references to game objects in an actor manager thing.
				// talking to actors needs to be cached, i think. searching for components every time may be slow (reflection)
				var actr = stateMachine.trackingObject.GetComponentInChildren<GameActor>();
				if (actr.HasAbility("GameActorAbilitySelectable")) {
					actr.Message("select");	
				}
			}	
		}
		
		public override void TransitionWithTouch(RawTouch touch)
		{
			if (touch.phase == TouchPhase.Ended && stateMachine.trackingObject != null) {
				stateMachine.TransitionTo("ending");
			}
			else if (touch.phase == TouchPhase.Ended) {
				stateMachine.TransitionTo("waiting");	
			}
			else if (touch.phase == TouchPhase.Moved && stateMachine.trackingObject != null) {
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
				var actr = stateMachine.trackingObject.GetComponentInChildren<GameActor>();
				if (actr != null && actr.HasAbility("GameActorAbilitySelectable")) {
					_timeOfTouchEnded = UnityEngine.Time.time;
					actr.Message("select");	
				}
			}
		}
		
		public override void Update()
		{
			if (Time.time - _timeOfTouchEnded > .02) {
				//Debug.Log("tap");
				stateMachine.TransitionTo("waiting");	
			}
		} 
	}
	
	private class GameInputState_Dragging : GameInputState
	{	
		private MoverComp trackingObjectMover;
		
		public override void EnterState()
		{
			trackingObjectMover = stateMachine.trackingObject.GetComponentInChildren<MoverComp>();	
		}
		
		public override void TransitionWithTouch(RawTouch touch)
		{
			if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
				stateMachine.TransitionTo("ending");
			}
			else if (touch.phase == TouchPhase.Moved) {
				trackingObjectMover.Move2D(touch.ray.origin);
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

