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
    private GameObject _scenary;
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
        _scenary = songContent.Find("Scenary").gameObject;

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
        setUpPropertySlider(_speed, (int)speed);
    }

    public void UpdateSpeedTitle(string speed)
    {
        setUpPropertyInput(_speed, speed);
    }

    public void UpdateSpeedTitle(int speed)
    {
        int currentSpeed = Int32.Parse(_speed.GetComponentInChildren<InputField>().text);
        setUpPropertyInput(_speed, (currentSpeed + speed).ToString());
    }

    public void UpdateCurrentSong()
    {
		SongTitle.text = FileManager.CurrentFilename + FileManager.CurrentFilenameExtension;

        _currentSong.Title = getPropertyInput(_songTitle);
		_currentSong.Difficulty = getUpPropertyDifficulty();
		_currentSong.Speed = Int32.Parse(getPropertyInput(_speed));
        _currentSong.Scenary = Int32.Parse(getUpPropertyScenary());
		_currentSong.Clip = getPropertyText(_audio);
		_currentSong.Offset = float.Parse(getPropertyInput(_offset));
        _currentSong.Author = getPropertyInput(_author);
        _currentSong.Preview = float.Parse(getPropertyInput(_preview));
		_currentSong.ForceDebug = getPropertyDebugMode(_debugMode);
    }

    public void UpdateUIWallObjectProperties(string id, float time)
    {
        /*
        SongProperties.gameObject.SetActive(false);
        WallObjectProperties.gameObject.SetActive(true);
        */
        setUpPropertyInput(_wallObjectId, id);
        setUpPropertyInput(_time, time.ToString());
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

        setUpPropertyText(_audio, currentSong); 
        setUpPropertyInput(_offset, _currentSong.Offset.ToString());
        setUpPropertyInput(_author, _currentSong.Author);
        setUpPropertyInput(_preview, _currentSong.Preview.ToString());
        setUpPropertyDifficulty(_difficulty, _currentSong.Difficulty);
        setUpPropertyScenary(_scenary, _currentSong.Scenary);
		setUpPropertyDebugMode(_debugMode, _currentSong.ForceDebug);
		setUpPropertyInput(_songTitle, _currentSong.Title);
		setUpPropertyInput(_speed, speed);
	}

	public void UpdateClipTime(string time)
    {
        CurrentCliptime.text = time + " s";
    }

	public void SetUpPropertyDifficulty(int state)
	{
		setUpPropertyDifficulty(_difficulty, GetUpPropertyDifficulty(state));
	}

    public void SetUpPropertyScenary(int scenary)
    {
        setUpPropertyScenary(_scenary, _currentSong.Scenary);
    }

    #endregion

    #region Setup properties

    private void setUpPropertyInput(GameObject template, string value)
    {
        template.GetComponentInChildren<InputField>().text = value;
    }

    private void setUpPropertyText(GameObject template, string value)
    {
        template.GetComponentInChildren<Text>().text = value;
    }

    private void setUpPropertySlider(GameObject template, int value)
    {
		template.GetComponentInChildren<Text>().text = string.Format("{0} km/h", value);
        // template.GetComponentInChildren<Slider>().value = value;
    }

    private void setUpPropertyFloat(GameObject template, float value)
    {
        template.GetComponentInChildren<Text>().text = value.ToString();
    }

    private void setUpPropertyDebugMode(GameObject template, bool value)
    {
        template.GetComponentInChildren<Toggle>().isOn = value;
    }

    private void setUpPropertyDifficulty(GameObject template, string state)
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

		setUpPropertyInput(_speed, _currentSong.Speed.ToString());
		template.GetComponentInChildren<Dropdown>().value = difficulty;
    }

    private void setUpPropertyScenary(GameObject template, int scenary)
    {
        template.GetComponentInChildren<Dropdown>().value = scenary + 1;
    }

    #endregion

    #region Read properties

    private string getPropertyInput(GameObject template)
    {
        return template.GetComponentInChildren<InputField>().text;
    }

    private string getPropertyText(GameObject template)
    {
        return template.GetComponentInChildren<Text>().text;
    }

    private float getPropertySlider(GameObject template)
    {
        return template.GetComponentInChildren<Slider>().value;
    }

    private bool getPropertyDebugMode(GameObject template)
    {
        return template.GetComponentInChildren<Toggle>().isOn;
    }

    private string getUpPropertyDifficulty()
    {
		int dropDownValue = _difficulty.GetComponentInChildren<Dropdown>().value;
		return GetUpPropertyDifficulty(dropDownValue);
    }

    private string getUpPropertyScenary()
    {
        int dropDownValue = _scenary.GetComponentInChildren<Dropdown>().value - 1;
        return dropDownValue.ToString();
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
