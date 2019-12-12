/*
 * 
 * Create your own video library here or sustitute this file with the real library in order to start using the videoplayer
 * 
 */

using UnityEngine;

public class UniversalMediaPlayer : MonoBehaviour
{
	public string Path { get; internal set; }
	public bool AbleToPlay { get; internal set; }
	public bool IsPlaying { get; internal set; }
	public int FrameRate { get; internal set; }
	public long Time { get; internal set; }
	public bool Mute { get; internal set; }
	public bool IsReady { get; internal set; }
	public float Length { get; internal set; }

	internal void Play()
	{
			
	}

	internal void Pause()
	{

	}

	public static bool IsValidLibrary()
	{
		return false;
	}
}

