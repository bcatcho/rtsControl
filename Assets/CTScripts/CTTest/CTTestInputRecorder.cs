using UnityEngine;
using System.Collections.Generic;

// [] introduce concept of a "recording"
// [] add a 
public class CTTestInputRecorder : MonoBehaviour
{
	public LineRenderer line;
	public List<RawTouch> _touches;
	private List<Vector3> _trailVerts;
	
	// Use this for initialization
	void Start()
	{
		_touches = new List<RawTouch>();
		GameMessenger.Reg("touch", this, Message_Touch);
		_trailVerts = new List<Vector3>();
	}
	
	public GameMessageResult Message_Touch(GameMessage msg)
	{
		var touch = (RawTouch)msg.data;
		
		if (_touches.Count > 0 && touch.phase == TouchPhase.Began) {
			var lastTouch = _touches[_touches.Count - 1];
			//Debug.Log(touch.time + "  " + lastTouch.time);
			if (touch.time - lastTouch.time > .5) { // if there hasn't been a touch in a while it's a new gesture
				_trailVerts.Clear();
				_touches.Clear();
			}
		}
		
		if (_touches.Count == 0) {
			_touches.Add(touch);
		}
			    
		_touches.Add(touch);
		AddToTrail(touch);
		return GameMessageResult.didNotHandleMessage;
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
