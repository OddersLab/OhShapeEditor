using System;
using UnityEngine;
using UnityEngine.UI;

public class PropertiesManager : MonoBehaviour
{
    #region Inspector

    [Header("General Properites")]
    public Text CurrentCliptime;
    public Text SongTitle;

    [Header("Properies Windows References")]
    public Transform SongProperties;
    public Transform WallObjectProperties;

    #endregion

    #region Song Properties

    private GameObject _songTitle;
    private GameObject _speed;
    private GameObject _audio;
    private GameObject _offset;
    private GameObject _author;
    private GameObject _preview;
    private GameObject _difficulty;
	private GameObject _debugMode;

	#endregion

	#region Wall Object properties

	private GameObject _wallObjectId;
    private GameObject _time;

    #endregion

    #region references

    private DeserializedSong _currentSong;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        Transform songContent = SongProperties.Find("Properties");

        _songTitle = songContent.Find("SongTitle").gameObject;
        _author = songContent.Find("Author").gameObject;
        _preview = songContent.Find("Preview").gameObject;
        _speed = songContent.Find("Speed/Input").gameObject;
        _offset = songContent.Find("Offset").gameObject;
        
        _audio = songContent.Find("AudioFile/Audio").gameObject;

        
        
        _difficulty = songContent.Find("Difficulty").gameObject;
		_debugMode = songContent.Find("DebugMode").gameObject;

		Transform wallObjectContent = WallObjectProperties.Find("Content");
    }

    #endregion    

    #region Read Load Properties    

    public void HideProperties()
    {
        _currentSong = null;
        // WallObjectProperties.gameObject.SetActive(false);
        SongProperties.gameObject.SetActive(false);
    }

    public void SetSongProperites(DeserializedSong song)
    {
        _currentSong = song;
        UpdateUISongProperties();
    }

    #endregion

    #region Update properties

    public void UpdateSpeedSliderTitle(float speed)
    {
        SetUpPropertySlider(_speed, (int)speed);
    }

    public void UpdateSpeedTitle(string speed)
    {
		Debug.Log(_currentSong.Difficulty);
        SetUpPropertyInput(_speed, speed);
    }

    public void UpdateSpeedTitle(int speed)
    {
        int currentSpeed = Int32.Parse(_speed.GetComponentInChildren<InputField>().text);
        SetUpPropertyInput(_speed, (currentSpeed + speed).ToString());
    }

    public void UpdateCurrentSong()
    {
		SongTitle.text = FileManager.CurrentFilename + FileManager.CurrentFilenameExtension;

        _currentSong.Title = GetPropertyInput(_songTitle);
		_currentSong.Difficulty = GetUpPropertyDifficulty();
		_currentSong.Speed = Int32.Parse(GetPropertyInput(_speed));
		_currentSong.Clip = GetPropertyText(_audio);
		_currentSong.Offset = float.Parse(GetPropertyInput(_offset));
        _currentSong.Author = GetPropertyInput(_author);
        _currentSong.Preview = float.Parse(GetPropertyInput(_preview));
		_currentSong.ForceDebug = GetPropertyDebugMode(_debugMode);
    }

    public void UpdateUIWallObjectProperties(string id, float time)
    {
        /*
        SongProperties.gameObject.SetActive(false);
        WallObjectProperties.gameObject.SetActive(true);
        */
        SetUpPropertyInput(_wallObjectId, id);
        SetUpPropertyInput(_time, time.ToString());
	}

    public void UpdateUISongProperties()
    {
        // WallObjectProperties.gameObject.SetActive(false);
        SongProperties.gameObject.SetActive(true);

		SongTitle.text = FileManager.CurrentFilename + FileManager.CurrentFilenameExtension;
		// TODO change to not set .ogg manually
		string currentSong = _currentSong.Clip;
		if (!_currentSong.Clip.Contains(".ogg")) {
			currentSong += ".ogg";
		}

		string speed = _currentSong.Speed.ToString();

        SetUpPropertyText(_audio, currentSong); 
        SetUpPropertyInput(_offset, _currentSong.Offset.ToString());
        SetUpPropertyInput(_author, _currentSong.Author);
        SetUpPropertyInput(_preview, _currentSong.Preview.ToString());
        SetUpPropertyDifficulty(_difficulty, _currentSong.Difficulty);
		SetUpPropertyDebugMode(_debugMode, _currentSong.ForceDebug);
		SetUpPropertyInput(_songTitle, _currentSong.Title);
		SetUpPropertyInput(_speed, speed);
	}

	public void UpdateClipTime(string time)
    {
        CurrentCliptime.text = time + " s";
    }

	public void SetUpPropertyDifficulty(int state)
	{
		SetUpPropertyDifficulty(_difficulty, GetUpPropertyDifficulty(state));
	}

	#endregion

	#region Setup properties

	private void SetUpPropertyInput(GameObject template, string value)
    {
        template.GetComponentInChildren<InputField>().text = value;
    }

    private void SetUpPropertyText(GameObject template, string value)
    {
        template.GetComponentInChildren<Text>().text = value;
    }

    private void SetUpPropertySlider(GameObject template, int value)
    {
		template.GetComponentInChildren<Text>().text = string.Format("{0} km/h", value);
        // template.GetComponentInChildren<Slider>().value = value;
    }

    private void SetUpPropertyFloat(GameObject template, float value)
    {
        template.GetComponentInChildren<Text>().text = value.ToString();
    }

    private void SetUpPropertyDebugMode(GameObject template, bool value)
    {
        template.GetComponentInChildren<Toggle>().isOn = value;
    }

    private void SetUpPropertyDifficulty(GameObject template, string state)
    {
        int difficulty = 0;
        switch (state.ToLower())
        {
			case "beginner":
				difficulty = 0;
				_currentSong.Speed = 15;
				_currentSong.Difficulty = state.ToLower();
				break;
			case "easy":
				difficulty = 1;
				_currentSong.Speed = 20;
				_currentSong.Difficulty = state.ToLower();
				break;
			case "medium":
				difficulty = 2;
				_currentSong.Speed = 30;
				_currentSong.Difficulty = state.ToLower();
				break;
			case "hard":
				difficulty = 3;
				_currentSong.Speed = 40;
				_currentSong.Difficulty = state.ToLower();
				break;
		}

		SetUpPropertyInput(_speed, _currentSong.Speed.ToString());
		template.GetComponentInChildren<Dropdown>().value = difficulty;
    }
    #endregion

    #region Read properties

    private string GetPropertyInput(GameObject template)
    {
        return template.GetComponentInChildren<InputField>().text;
    }

    private string GetPropertyText(GameObject template)
    {
        return template.GetComponentInChildren<Text>().text;
    }

    private float GetPropertySlider(GameObject template)
    {
        return template.GetComponentInChildren<Slider>().value;
    }

    private bool GetPropertyDebugMode(GameObject template)
    {
        return template.GetComponentInChildren<Toggle>().isOn;
    }

    private string GetUpPropertyDifficulty()
    {
		int dropDownValue = _difficulty.GetComponentInChildren<Dropdown>().value;
		return GetUpPropertyDifficulty(dropDownValue);
    }

	private string GetUpPropertyDifficulty(int state)
	{
		string difficulty = "easy";

		switch (state)
		{
			case 0:
				difficulty = "beginner";
				break;
			case 1:
				difficulty = "easy";
				break;
			case 2:
				difficulty = "medium";
				break;
			case 3:
				difficulty = "hard";
				break;
		}

		return difficulty;
	}

	#endregion
}
