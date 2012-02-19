using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	void Awake()
	{
		Application.targetFrameRate = 60;
	}
	
	void Start()
	{
		GameMessenger.Send("spawnActor", this, "boxletPrefab");
	}
	
	void Update()
	{
	}
}
