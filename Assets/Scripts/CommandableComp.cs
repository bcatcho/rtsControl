using UnityEngine;
using System.Collections;

public class CommandableComp : MonoBehaviour
{
	private bool _isSelected = false;
	private float  _previousSize; // HACK

	public bool IsSelected()
	{
		return _isSelected;	
	}
	
	public void ToggleSelected()
	{
		_isSelected = !_isSelected;
		
		if (_isSelected) {
//			var packedSpriteComponent = gameObject.GetComponentInChildren<PackedSprite>();
//			_previousSize = packedSpriteComponent.width;
//			var newSize = _previousSize * 1.5f;
//			
//			packedSpriteComponent.SetSize(newSize, newSize);
//				
//			// update the bounding collider
//			var bc = gameObject.GetComponentInChildren(typeof(BoxCollider)) as BoxCollider;
//			bc.size = new Vector3(newSize, newSize, 1f);		
//		}
//		else {
//			var packedSpriteComponent = gameObject.GetComponentInChildren<PackedSprite>();
//			var newSize = _previousSize;
//			
//			packedSpriteComponent.SetSize(newSize, newSize);
//				
//			// update the bounding collider
//			var bc = gameObject.GetComponentInChildren(typeof(BoxCollider)) as BoxCollider;
//			bc.size = new Vector3(newSize, newSize, 1f);	
		}
	}
}
