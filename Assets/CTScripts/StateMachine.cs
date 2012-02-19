using UnityEngine;
using System.Collections.Generic;

public class StateMachine<T> : Object, IStateMachine where T : IState
{
	protected T currentState;
	private Dictionary<string, T> _states;
	
	public StateMachine()
	{
		_states = new Dictionary<string, T>();
	}
	
	public void AddState(string stateName, T state)
	{
		if (_states.ContainsKey(stateName)) {
			Debug.LogWarning("StateName Already Exists in StateMachine : " + stateName + " " + state);
		}
		else {
			_states.Add(stateName, state);	
		}
	}
	
	public void TransitionTo(string stateName)
	{
		T nextState = _states[stateName];
		if (nextState != null)
			Transition(nextState);
	}

	private void Transition(T newState)
	{
		if (currentState != null) {
			currentState.ExitState();
		}
		
		currentState = newState;
		currentState.SetParent(this);
		currentState.EnterState();
	}

	public void Update()
	{
		if (currentState != null)
			currentState.Update();
	}	
}

public interface IStateMachine
{
	void TransitionTo(string newState);
}

public interface IState
{	
	void SetParent(IStateMachine parentStateMachine);

	void EnterState();

	void ExitState();

	void Update();
}

public abstract class State<T> : IState where T : IStateMachine
{
	public T stateMachine;
	
	public void SetParent(IStateMachine parentStateMachine)
	{
		stateMachine = (T)parentStateMachine;	
	}
	
	public virtual void EnterState()
	{
	}

	public virtual void ExitState()
	{
	}

	public virtual void Update()
	{
	}
}