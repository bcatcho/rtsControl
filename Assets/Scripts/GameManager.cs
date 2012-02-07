using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	public GameObject prefab;
	private ArrayList _sprites = new ArrayList();
	
	void Awake()
	{
		Application.targetFrameRate = 60;
	}
	
	void Start()
	{
		StartCoroutine("LoadSprites");
	}
	
	IEnumerator LoadSprites()
	{
		_sprites.Add(CreateNewSprite(new Vector3(100, 100, 0), 256f));
		return null;
	}
	
	void Update()
	{
	}
	
	private GameObject CreateNewSprite(Vector3 position, float size)
	{
		var theSprite = Instantiate(prefab, position, Quaternion.identity) as GameObject;
		//var packedSpriteComponent = theSprite.GetComponentInChildren<PackedSprite>();
		//packedSpriteComponent.SetSize(size, size);
		
		// update the bounding collider
		var ot = theSprite.GetComponentInChildren<OTSprite>();
		ot.size = new Vector2(size, size);
		//bc.size = new Vector3(size, size, 1);
		
		var act = theSprite.GetComponentInChildren<GameActor>();
		act.AddAbility<GameActorAbilitySelectable>(); 
		
		return theSprite;
	}
}
