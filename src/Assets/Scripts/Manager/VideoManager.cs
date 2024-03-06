using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.Video;


//Manage video.
public class VideoManager : MonoBehaviour
{
    #region Public Variables
	[Header("Output")]
	public Text TimeText;

	[Header("Input")]
	public Text VideoName;
	public InputField OffsetValue;
	public Toggle MuteToggle;
	public VideoPlayer VideoPlayer;
	public AudioSource AudioSource;

	[Header("Components")]
	public GameObject NewVideo;
	public GameObject VideoLoaded;

	[HideInInspector] public float Offset;
	#endregion Public Variables


	#region Properties
	public bool IsPlaying { get; private set; }
	public float Time { get; private set; }
	public string Name { get; private set; }
	#endregion Properties


	#region Unity Methods
	private void Update()
	{
		TimeText.text = VideoPlayer.time.ToString("F2") + " s";
	}
	#endregion Unity Methods


	#region Main Methods
	public void Load(string video, float offset)
	{
		Close();

		VideoName.text = video;
		OffsetValue.text = offset.ToString("F2");

		Offset = offset;

		SetVideo(video);
	}
	public void SetVideo(string name)
	{
		Name = name;
		string path = FileManager.VideoPath + Name;

		if (!File.Exists(path))
		{
			DialogsWindowsManager.Instance.InfoMessage("Video Not Exist");
			return;
		}

		VideoPlayer.url = path;

		ShowVideoManager(true);

		VideoPlayer.enabled = true;
		Mute(true);
		Play();

		StartCoroutine(GetFristFrameCoroutine());
	}

	public void Close()
	{
		VideoPlayer.enabled = false;
		Name = "";
		Offset = 0f;

		VideoName.text = "";
		OffsetValue.text = "0.00";

		ShowVideoManager(false);
	}
	public void Play()
	{
		IsPlaying = true;
		VideoPlayer.Play();
	}
	public void Stop()
	{
		IsPlaying = false;
		VideoPlayer.Pause();
	}
	#endregion Main Methods


	#region Utility Methods
	// Make sure to get a first frame for video preview
	IEnumerator GetFristFrameCoroutine()
	{
		while (!VideoPlayer.enabled) yield return null;

		Play();

		while (!VideoPlayer.isPlaying) yield return null;

		yield return new WaitForSeconds(1f / (VideoPlayer.frameRate * 4));

		Stop();
	}

	public void AddVideoOffset(float amount)
	{
		Offset += amount;
		OffsetValue.text = Offset.ToString("F2");
	}

	// Called by UI mute button toggle
	public void Mute(bool mute)
	{
		AudioSource.mute = mute;
		MuteToggle.isOn = mute;
	}

	public void SetTime(float time)
	{
		Time = time + Offset;

		if (Time >= GetVideoURLTime())
		{
			VideoPlayer.time = GetVideoURLTime();
		}
		else
		{
			VideoPlayer.Play();
			VideoPlayer.time = Time;
		}

		if (!IsPlaying)
			StartCoroutine(GetFristFrameCoroutine());
	}

	public float GetVideoURLTime()
	{
		double time = VideoPlayer.frameCount / VideoPlayer.frameRate;
		if (double.IsNaN(time))
		{
			return float.MinValue;
		}
		else
		{
			TimeSpan videoUrlLength = TimeSpan.FromSeconds(time);
			return videoUrlLength.Seconds;	
		}
	}

	public void ShowVideoManager(bool show)
	{
		NewVideo.SetActive(!show);
		VideoLoaded.SetActive(show);
	}
	#endregion Utility Methods
}
