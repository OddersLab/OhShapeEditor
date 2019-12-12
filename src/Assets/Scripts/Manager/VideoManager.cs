using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;


//Manage video.
public class VideoManager : MonoBehaviour
{
#region Inspector

    //UI Output variables
    public Transform VideoContainer;
	public Text TimeText;
	[Space]
	//UI Input variables
	public Text VideoName;
    public InputField OffsetValue;
	public Toggle MuteToggle;
	[Space]
    //Player prefab
    public UniversalMediaPlayer PlayerPrefab;

    [Space]
    public Transform VideoWallsLibrary;

#endregion

#region references

    //Private variables
    private string _videoFilename;
    private float _currentTime = 0f;
    private float _offsetTime = 0f;
    private bool _playingState = false;
    private UniversalMediaPlayer _player;

    #endregion

    #region Initialize

    //Initial setup, when load.
    public void Setup(string video, float offset)
    {
        if (_player == null) return;
        Close();

        VideoName.text = video;
		OffsetValue.text = offset.ToString("F2");

        _offsetTime = offset;

		//Set the video
        SetVideo(video);
    }

    public void SetVideo(string videoFilename)
    {
        if (_player == null) return;
        _videoFilename = videoFilename;
        var file = FileManager.Path + _videoFilename;

        if (!File.Exists(file))
        {
			DialogsWindowsManager.Instance.InfoMessage("Video Not Exist");
            return;
        }

        ShowVideoManager(true);

        _player.Path = Path.GetFullPath(FileManager.Path + _videoFilename);

        Mute();
        Play();
        StartCoroutine(getframe());
    }

#endregion

#region Video Control

	public void Close()
    {
        if (_player == null) return;
        Destroy(_player.gameObject);
        _player = Instantiate(PlayerPrefab, VideoContainer);
        _videoFilename = "";
        _offsetTime = 0f;

        VideoName.text = "";
        OffsetValue.text = "0.00";

        _player.Path = "";
        ShowVideoManager(false);
    }

    public void Play()
    {
        if (_player == null) return;
        if (!File.Exists(_player.Path)) return;
        _playingState = true;
        _player.Play();

    }

    public void Stop()
    {
        if (_player == null) return;
        _playingState = false;
        _player.Pause();
    }

#endregion

#region Utils
	//Make sure to get a first frame for video preview
	IEnumerator getframe()
    {
	
        while (!_player.AbleToPlay)
        {
            yield return null;
        }

        Play();

        while (!_player.IsPlaying)
        {
            yield return null;
        }

        yield return new WaitForSeconds((float)1/(_player.FrameRate*4));

        Stop();
    }

	public void SetVideoOffset(float offset)
	{
        _offsetTime = offset;
	}
	public float GetVideoOffset()
	{
		return _offsetTime;
	}
	public void AddVideoOffset(float amount)
	{
		_offsetTime += amount;
		OffsetValue.text = _offsetTime.ToString("F2");
	}

#endregion

#region MonoBehaviour

	private void Awake()
    {
        
        if (UniversalMediaPlayer.IsValidLibrary())
        {
            _player = Instantiate(PlayerPrefab, VideoContainer);
            return;
        }

        _player = null;
        VideoWallsLibrary.Find("VideoBlocked").gameObject.SetActive(true);
        VideoWallsLibrary.Find("NewVideo").gameObject.SetActive(false);
        VideoWallsLibrary.Find("VideoLoaded").gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_player == null) return;
        
        if (_player.Time > 9999999999)
        {
            TimeText.text = "0 s";
        }
        else
        {
            TimeText.text = (_player.Time * 0.001f).ToString("F2") + " s";
        }
    }

#endregion

#region Audio

	//Called by UI mute button toggle.
	public void MuteController(bool state)
	{
		if (state)
			Mute();
		else
			UnMute();
	}

	public void Mute()
    {
        if (_player == null) return;
        _player.Mute = true;
		MuteToggle.isOn = true;
    }
    public void UnMute()
    {
        if (_player == null) return;
        _player.Mute = false;
		MuteToggle.isOn = false;
	}

#endregion

#region Time

	public void SetTime(float newTime)
    {
        if (_player == null) return;
        if (!_player.IsReady) return;

        _currentTime = newTime + _offsetTime;
        if (_currentTime * 1000 >= _player.Length)
        {
            Log.AddLine("Out of video time");
            return;
        }

        _player.Time = (long)(_currentTime * 1000);

        if (!_playingState)
        {
            StartCoroutine(getframe());
        }
    }

    #endregion

    #region Show Video Manager

    public void ShowVideoManager(bool show)
    {
        if (_player == null) return;
        VideoWallsLibrary.Find("NewVideo").gameObject.SetActive(!show);
        VideoWallsLibrary.Find("VideoLoaded").gameObject.SetActive(show);
    }
    #endregion

    #region Library Installed
    public bool IsLibraryInstalled()
    {
        return _player != null;
    }

    #endregion
}
