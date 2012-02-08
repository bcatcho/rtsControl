using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void GameMessageReciever(GameMessage msg);

public class GameMessage
{
	public Object sender;
	public Object reciever;
	public string name;
	public object data;
}

public class GameMessenger : MonoBehaviour
{
	// [X]two queues for messages
	//		-- abscracted into "MessageQueue"
	// registering/deregistering listeners
	// logging
	// dictionary of messageName to delegate
	private static GameMessenger _current;
	private GameMessageQueue<GameMessage> _queue;
	private Dictionary<string, Dictionary<object, GameMessageReciever>> _listeners;
	private Queue<KeyValuePair<Dictionary<object, GameMessageReciever>, object>> _listenersToRemove;
	
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
	
	public static GameMessenger Current()
	{
		return _current;	
	}
	
	void Start()
	{
		_current = this;
		_queue = new GameMessageQueue<GameMessage>();
		_listeners = new Dictionary<string, Dictionary<object, GameMessageReciever>>();
		_listenersToRemove = new Queue<KeyValuePair<Dictionary<object, GameMessageReciever>, object>>();
	}
	
	public void Register(string messageName, object instance, GameMessageReciever del)
	{
		if (!_listeners.ContainsKey(messageName)) {
			_listeners.Add(messageName, new Dictionary<object, GameMessageReciever>());	
		}
		
		_listeners[messageName].Add(instance, del);
	}
	
	public void Deregister(string messageName, object instance)
	{
		if (_listeners.ContainsKey(messageName)) {
			if (_listeners[messageName].ContainsKey(instance)) {
				_listenersToRemove.Enqueue(new KeyValuePair<Dictionary<object, GameMessageReciever>, object>(_listeners[messageName], instance));
			}
		}	
	}
	
	public void Send(GameMessage msg)
	{
		_queue.Add(msg);	
	}
	
	void Update()
	{
		foreach (GameMessage msg in _queue) {
			if (msg.name != null) {
				if (msg.reciever != null)
					HandleMessage(msg);
				else
					BroadcastMessage(msg);	
			}
		}
	}

	private void HandleMessage(GameMessage msg)
	{
		var messageCategory = _listeners[msg.name];
		if (messageCategory != null) {
			var instanceMethod = messageCategory[msg.reciever];
			
			if (instanceMethod != null) {
				instanceMethod(msg);
				ProcessQueuedDeregisterRequests();
			}
		}
	}

	private	void BroadcastMessage(GameMessage msg)
	{
		if (_listeners.ContainsKey(msg.name)) {
			var messageCategory = _listeners[msg.name];
			
			foreach (KeyValuePair<object,GameMessageReciever> instanceMethodPair in messageCategory) {
				instanceMethodPair.Value(msg);
			}
			
			// events can deregister so process those queued up
			ProcessQueuedDeregisterRequests();
		}
	}
	
	private void ProcessQueuedDeregisterRequests()
	{
		while (_listenersToRemove.Count > 0) {
			var item = _listenersToRemove.Dequeue();
			item.Key.Remove(item.Value);
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

