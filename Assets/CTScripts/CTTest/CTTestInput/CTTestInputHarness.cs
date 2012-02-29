using UnityEngine;
using System.Collections;

public class CTTestInputHarness : CTTestHarness {
    private CTTestInputRecorder _recorder;
	protected CTTestInputDisplay _display;
	private CTTestInputPlayer _player;
	public bool isRecording;
	
	void Start () {
		_recorder = new CTTestInputRecorder();
		_display = new CTTestInputDisplay(gameObject);
		_player = new CTTestInputPlayer();
		
		if (isRecording)
		{
			_recorder.StartRecording();	
		}
		else
		{
		    Invoke("Play", 1);	
		}
	}
	
	public void Play()
	{
		_player.StartPlaying();	
	}
	
	void Update()
	{
		if (!isRecording)
			_player.Update();
	}
}
