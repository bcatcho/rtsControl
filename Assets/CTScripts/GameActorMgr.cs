using UnityEngine;
using System.Collections.Generic;

public class GameActorMgr : MonoBehaviour
{
	public List<GameObject> sceneActors;
	
	// Use this for initialization
	void Start()
	{
		GameMessenger.Current().Register("spawnActor", this, Message_spawnActor);
	}
	
	public GameMessageResult Message_spawnActor(GameMessage msg)
	{
		string actorName = msg.data as string;
		var actorToSpawn = sceneActors.Find(actr => actr.name == actorName);
		if (actorToSpawn != null) {
			GameObject.Instantiate(actorToSpawn);
		}
		else {
			Debug.LogWarning("could not find actor " + actorName);	
		}
		
		return GameMessageResult.handledMessage;
	}

	// Update is called once per frame
	void Update()
	{
	
	}
	
	private GameObject CreateNewSprite(GameObject prefab, Vector3 position, float size)
	{
		
		var theSprite = Instantiate(prefab, position, Quaternion.identity) as GameObject;
		
		// update the bounding collider
		var ot = theSprite.GetComponentInChildren<OTSprite>();
		ot.size = new Vector2(size, size);
		
		return theSprite;
	}
}
