using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate GameMessageResult GameMessageReciever(GameMessage msg);

public class GameMessage
{
	public Object sender;
	public Object reciever;
	public string name;
	public object data;
}

public enum GameMessageResult
{
	handledMessage,
	didNotHandleMessage
}

public class GameMessenger : MonoBehaviour
{
	private static GameMessenger _current;
	private GameMessageQueue<GameMessage> _queue;
	private Dictionary<string, GameMessageReciever> _listeners;
	
#region Static Methods
	public static void Send(string messageName, Object sender, object data)
	{
		var msg = new GameMessage() {
			name = messageName,
			sender = sender,
			data = data
		};
		_current.Send(msg);
	}
	
	public static void Send(string messageName, Object sender, Object reciever, object data)
	{
		var msg = new GameMessage() {
			name = messageName,
			sender = sender,
			reciever = reciever,
			data = data
		};
		_current.Send(msg);
	}

	public static void SendNow(string messageName, Object sender, object data)
	{
		var msg = new GameMessage() {
			name = messageName,
			sender = sender,
			data = data
		};
		_current.BroadcastMessage(msg);
	}
	
	/// <summary>
	/// 	Convenience static method for Register
	/// </summary>
	/// <param name='messageName'>
	/// Message name.
	/// </param>
	/// <param name='instance'>
	/// Object Instance that holds the delegate.
	/// </param>
	/// <param name='del'>
	/// The delegate that will recieve the messages
	/// </param>
	public static void Reg(string messageName, object instance, GameMessageReciever del)
	{
		_current.Register(messageName, instance, del);	
	}
	
	/// <summary>
	/// Convenience static method for Deregister
	/// </summary>
	/// <param name='messageName'>
	/// The message name
	/// </param>
	/// <param name='instance'>
	/// The object instance that contains the delegate
	/// </param>
	/// <param name='del'>
	/// The delegate that handles the message
	/// </param>
	public static void DeReg(string messageName, GameMessageReciever del)
	{
		_current.Deregister(messageName, del);
	}
	
	public static GameMessenger Current()
	{
		return _current;	
	}
 #endregion
	
	void Start()
	{
		_current = this;
		_queue = new GameMessageQueue<GameMessage>();
		_listeners = new Dictionary<string, GameMessageReciever>();
	}
	
	public void Register(string messageName, object instance, GameMessageReciever del)
	{
		if (!_listeners.ContainsKey(messageName)) {
			GameMessageReciever gmrDelegate = null;
			_listeners.Add(messageName, gmrDelegate);	
		}
		
		_listeners[messageName] += del;
	}
	
	public void Deregister(string messageName, GameMessageReciever del)
	{
		if (_listeners.ContainsKey(messageName)) {
			if (_listeners[messageName] != null)
				_listeners[messageName] -= del;
		}	
	}
	
	public void Send(GameMessage msg)
	{
		_queue.Add(msg);	
	}
	
	void Update()
	{
		foreach (GameMessage msg in _queue) {
			BroadcastMessage(msg);	
		}
	}

	private void HandleMessage(GameMessage msg)
	{
	}

	private	void BroadcastMessage(GameMessage msg)
	{
		if (_listeners.ContainsKey(msg.name)) {
			var messageCategory = _listeners[msg.name];
			if (messageCategory != null)
				messageCategory(msg);
		}
	}
	
	
	//TODO figure this out later
	private	void BroadcastMessageUntilHandled(GameMessage msg)
	{
		if (_listeners.ContainsKey(msg.name)) {
			var messageCategory = _listeners[msg.name];
			if (messageCategory != null)
			{
				foreach(GameMessageReciever del in messageCategory.GetInvocationList())
				{
					Debug.Log(del.Invoke(msg));
				}
			}
		}
	}
	
	private class GameMessageQueue<T> : IEnumerable
	{
		private Queue<T> _queue;
		
		public GameMessageQueue()
		{
			_queue = new Queue<T>();	
		}

		public void Add(T msg)
		{
			_queue.Enqueue(msg);	
		}

		public void Clear()
		{
			_queue.Clear();	
		}
		
		public IEnumerator GetEnumerator()
		{
			while (_queue.Count > 0) {
				yield return _queue.Dequeue();	
			}
		}
	}
}

