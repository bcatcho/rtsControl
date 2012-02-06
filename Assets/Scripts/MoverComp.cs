using UnityEngine;
using System.Collections;

public class MoverComp : MonoBehaviour
{
	public float pixelsPerFrame = 100;
	private Vector3 _destination;
	private bool _isMoving = false;
	
	void Update()
	{
		if (_isMoving) {
			transform.position = Vector3.MoveTowards(transform.position, _destination, pixelsPerFrame * Time.deltaTime);
			
			_isMoving = (gameObject.transform.position != _destination);
		}
	}
	
	public void Move2D(Vector2 destination)
	{
		transform.position = destination;	
	}
	
	public void MoveTowards(Vector3 destination)
	{
		_destination = destination;
		_isMoving = true;
	}
}
