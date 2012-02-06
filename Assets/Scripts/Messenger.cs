using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Messenger : MonoBehaviour
{
	// [X]two queues for messages
	//		-- abscracted into "MessageQueue"
	// registering/deregistering listeners
	// logging
	// dictionary of messageName to delegate
	public static Messenger current;
	private MessageQueue _queue;
	private Dictionary<string, Dictionary<object, GameMessageReciever>> _listeners;
	
	void Start()
	{
		_queue = new MessageQueue();
		_listeners = new Dictionary<string, Dictionary<object, GameMessageReciever>>();
		current = this;
	}
	
	public void Register(string messageName, object instance, GameMessageReciever del)
	{
		if (!_listeners.ContainsKey(messageName)) {
			_listeners.Add(messageName, new Dictionary<object, GameMessageReciever>());	
		}
		
		_listeners[messageName].Add(instance, del);
	}
	
	public void Send(GameMessage msg)
	{
		_queue.Add(msg);	
	}
	
	void Update()
	{
		foreach (GameMessage msg in _queue) {
			if (msg != null && msg.name != null) {
				if (msg.reciever != null)
					HandleMessage(msg);
				else
					BroadcastMessage(msg);	
			}
		}
		_queue.Clear();
	}

	private void HandleMessage(GameMessage msg)
	{
		var messageCategory = _listeners[msg.name];
		if (messageCategory != null) {
			var instanceMethod = messageCategory[msg.reciever];
			
			if (instanceMethod != null)
				instanceMethod(msg);
		}
	}

	private	void BroadcastMessage(GameMessage msg)
	{
		var messageCategory = _listeners[msg.name];
		if (messageCategory != null) {
			foreach (KeyValuePair<object,GameMessageReciever> instanceMethodPair in messageCategory) {
				instanceMethodPair.Value(msg);
			}
		}
	}
	
	private class MessageQueue
	{
		private List<GameMessage> _queue;
		
		public MessageQueue()
		{
			
			_queue = new List<GameMessage>();	
		}

		public void Add(GameMessage msg)
		{
			_queue.Add(msg);	
		}

		public void Clear()
		{
			_queue.Clear();	
		}
		
		public IEnumerator GetEnumerator()
		{
			return _queue.GetEnumerator();	
		}
	}
}

public delegate void GameMessageReciever(GameMessage msg);

public class GameMessage
{
	public Object sender;
	public object reciever;
	public string name;
	public Object data;
}
