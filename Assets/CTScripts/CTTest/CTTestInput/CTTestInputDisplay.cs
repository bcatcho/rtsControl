using UnityEngine;
using System.Collections.Generic;

public class CTTestInputDisplay : Object {
	private LineRenderer line;
	private GUIText text;
	private List<Vector3> _trailVerts;
	private GameObject gameObject;
	
	public CTTestInputDisplay(GameObject attchedGameObject) {
		gameObject = attchedGameObject;
		line = gameObject.GetComponentInChildren<LineRenderer>();
		text = gameObject.GetComponentInChildren<GUIText>();
		_trailVerts = new List<Vector3>();
		GameMessenger.Reg("newTestInputRecording", this, Message_newTestInputRecording);
		GameMessenger.Reg("touch", this, Message_touch);
	}
	
	public GameMessageResult Message_touch(GameMessage msg)
	{
		var touch = (RawTouch)msg.data;
		AddToTrail(touch);
		return GameMessageResult.handledMessage;
	}
	
	public GameMessageResult Message_newTestInputRecording(GameMessage msg)
	{
		var testName = (string)msg.data;
		text.text = "[Test] " + testName;
		Clear();
		return GameMessageResult.handledMessage;
	}
	
	private void Clear()
	{
		_trailVerts.Clear();	
	}
	
	public void DrawTrail()
	{
		line.SetVertexCount(_trailVerts.Count);
		for (int i = 0; i < _trailVerts.Count; ++i) {
			line.SetPosition(i, _trailVerts[i]);
		}
	}
	
	public void AddToTrail(RawTouch touch)
	{
		if (_trailVerts.Count == 0) {
			_trailVerts.Add(V2toV3(touch.position, -100));
		}
		else {
			var newPoint = V2toV3(touch.position, -100);
			var endPoint = _trailVerts[_trailVerts.Count - 1];
			if (Vector3.Distance(endPoint, newPoint) > 2) {
				_trailVerts.Add(newPoint);	
			}
		}
		
		DrawTrail();	
	}
	
	private Vector3 V2toV3(Vector2 v2, float z)
	{
		return new Vector3(v2.x, v2.y, z);	
	}
}
