using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OhShapeEditor : MonoBehaviour
{
    #region Inspector

    [Header("General Configuration")]
    [SerializeField]
    private int ZoomMin; // 1
    [SerializeField]
    private int ZoomMax; // 256
    [SerializeField]
    private int WallMarklineVisibleAtZoomLevel; // 3
    [SerializeField]
    private int WallIdVisibleAtZoomLevel; // 3
    [SerializeField]
    private int WallSuperpositionAtZoomLevel;


    [Header("UI References")]
    public GameObject FullScreenButtons;
    public GameObject NormalScreenButtons;
    public GameObject NotStartedBackground;
    public ZoomController ZoomController;
    public Button SaveButton;
    public RectTransform WaveWalls;
    public ScrollRect SongScrollBar;
    public Toggle GridToggle;
    public Text GridOffsetInputText;
    public Text GridBPMInputText;
    public InputField Volume;
    public InputField Zoom;

    [Header("Scroll Configuration")]
    [Range(0, 1)]
    public float ScrollAmount = 0.8f;

    #endregion

    #region Song properties

    private RectTransform _wave;
    private RectTransform _songWalls;
    private RectTransform _timeGrid;
    private RectTransform _cursor;
    private float _cursorTime = 0f;

    #endregion

    #region References

    private AudioManager _audioManager;
    private RenderSong _renderSong;
    private SongManager _songManager;
    private PropertiesManager _propertiesManager;
    private VideoManager _videoManager;
    private TimeGridManager _timeGridManager;
    private ObjectsLibraryManager _objectsLibraryManager;
    private DragSelectionHandler _dragSelectionHandler;

    private bool _allowQuiting;
    private float _zoom = 1.00f;
    private int _indexZoom = 0;
    private float _currentScrollTime = 0;
    private Vector2 _waveTimeRange = Vector2.zero;
    private Canvas _mainCanvas;

    private WallObject _currentWallObject;
    private List<WallObject> _wallObjects;
    private List<WallObject> _selectedWallObjects;

    private HashSet<WallObject> _clipboard;

    #endregion

    #region Flags
    
    private int _moveWallsVerticallyInTheNextFrame = 0;


    #endregion

    public AudioManager AudioManager
    {
        get
        {
            return _audioManager;
        }

        set
        {
            _audioManager = value;
        }
    }

    #region MonoBehaviour


    private void Awake()
    {
        _songManager = GetComponent<SongManager>();
        _audioManager = GetComponent<AudioManager>();
        _propertiesManager = GetComponent<PropertiesManager>();
        _videoManager = GetComponent<VideoManager>();
        _timeGridManager = GetComponent<TimeGridManager>();
        _objectsLibraryManager = GetComponent<ObjectsLibraryManager>();
        _dragSelectionHandler = GetComponent<DragSelectionHandler>();

        _wave = WaveWalls.Find("Wave").GetComponent<RectTransform>();
        _songWalls = WaveWalls.Find("Song Walls").GetComponent<RectTransform>();
        _timeGrid = WaveWalls.Find("TimeGrid").GetComponent<RectTransform>();
        _cursor = WaveWalls.Find("Song Walls/Cursor Song").GetComponent<RectTransform>();

        _renderSong = _wave.Find("RenderSong").GetComponent<RenderSong>();
        _mainCanvas = GetComponent<Canvas>();

        _wallObjects = new List<WallObject>();
        _selectedWallObjects = new List<WallObject>();
    }

    private void Start()
    {
        // Suscriptions
        WindowManager.instance.ScreenSizeChangeEvent += Instance_ScreenSizeChangeEvent;

        NormalScreenButtons.SetActive(!Screen.fullScreen);
        FullScreenButtons.SetActive(Screen.fullScreen);

        //Create (if is necesary) and initialize folders.
        FileManager.CreateFolders();
    }

    private void Update()
    {
        //Update cursor in screen.
        if (_audioManager.IsPlaying())
        {
            var playTime = _audioManager.GetCurrentClipTime();

            if (_waveTimeRange.y < playTime || _waveTimeRange.x > playTime)
            {
                UpdateClipRenderTimes();
                StartCoroutine(BarToPosition(_audioManager.GetCurrentClipTime()));
            }

            var pos = ClipInfo.SecToPixel(playTime);
            _cursor.anchorMin = new Vector2(pos, 0);
            _cursor.anchorMax = new Vector2(pos, 1);
            _cursor.anchoredPosition = Vector2.zero;

            _propertiesManager.UpdateClipTime(playTime.ToString("F2"));
        }

        Inputs();
    }

    private void FixedUpdate()
    {
        if (_moveWallsVerticallyInTheNextFrame == 1)
        {
            _moveWallsVerticallyInTheNextFrame = 2;
        }
        else if (_moveWallsVerticallyInTheNextFrame == 2)
        {
            sortWallObjectListAndSibling();
            moveWallsVertically();
            _moveWallsVerticallyInTheNextFrame = 0;
        }
    }

    private void OnApplicationQuit()
    {
        //Prevent accidental quiting.
        if (!_allowQuiting)
        {
            Application.CancelQuit();
            OnExit();
        }
    }

    #endregion

    #region Inputs

    private void Inputs()
    {
        bool canDoEvent = !isInputFieldFocused();
        if (!canDoEvent)
        {
            return;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {

            if (_audioManager.IsPlaying())
            {
                OnPauseClip();
            }
            else
            {
                OnPlayClip();
            }
        }
        //Create New object whit key N, at current time if song is playing if not at cursor position
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (_audioManager.IsPlaying())
            {
                OnAddWallObject(_audioManager.GetCurrentClipTime());
            }
            else
            {
                OnAddWallObject(_cursorTime);
            }
        }

       if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
       {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.S)) {
                    OnSaveAs();
                }

                return;
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                Copy();
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                Paste();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                OnSave();
            }
       }

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            OnDeleteWallObject();
        }
    }

    private bool isInputFieldFocused()
    {
        CustomInputField[] customInputField = GameObject.FindObjectsOfType<CustomInputField>();
        for (var i = 0; i < customInputField.Length; i++)
        {
            if (customInputField[i].isFocused)
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Modal Windows

    public void CreateNewFile(Text textInputField)
    {
        if (FileManager.CreateFile(textInputField.text))
        {
            //Load recent created file.
            DialogsWindowsManager.Instance.HideWindows();
            // ResetEditor();

            StartCoroutine(LoadSong(FileManager.CurrentFilename));
        }
    }

    public void SetVideoFileToLoad(string text)
    {
        FileManager.CurrentVideoFilename = text;
    }

    public void SetAudioClipToLoad(string text)
    {
        FileManager.CurrentAudioClipFilename = text;
    }

    public void LoadVideoFile()
    {
        if (_songManager.CurrentSong != null)
        {
            _songManager.CurrentSong.Video = FileManager.CurrentVideoFilename;
        }
        _videoManager.VideoName.text = FileManager.CurrentVideoFilename;
        _videoManager.SetVideo(FileManager.CurrentVideoFilename);
    }

    public void DeleteVideoFile()
    {
        if (_songManager.CurrentSong != null)
        {
            _songManager.CurrentSong.Video = null;
        }
        _videoManager.Close();
    }

    public void LoadAudioClip()
    {
        StartCoroutine(LoadClipRoutine());
    }

    public void SaveAndExit()
    {
        OnSave();
        Exit();
    }

    public void Exit()
    {
        _allowQuiting = true;
        Application.Quit();
    }

    #endregion

    #region UI Events

    public void OnNewSong()
    {
        DialogsWindowsManager.Instance.ShowWindow(DialogsWindowsManager.Window.NewFile);
    }

    public void OnLoadFile(string filter)
    {
        if (FileManager.OpenDialog(filter))
        {
            // TODO fix this
            if (filter.Contains(".ogg"))
            {
                LoadAudioClip();
            }
            else if (filter.Contains(".mp4;*.MOV"))
            {
                LoadVideoFile();
            }
            else
            {
                _videoManager.ShowVideoManager(true);
                StartCoroutine(LoadSong(FileManager.CurrentFilename));
            }
        }
    }

    IEnumerator SaveAnimation()
    {
        Cursor.visible = false;
        SaveButton.GetComponentInChildren<Text>().text = "SAVING...";

        yield return new WaitForSeconds(0.5f);


        SaveButton.GetComponentInChildren<Text>().text = "SAVED";
        Cursor.visible = true;
        // SaveButton.enabled = false;
        // SaveButton.enabled = true;
        yield return new WaitForSeconds(0.8f);
        SaveButton.GetComponentInChildren<Text>().text = "SAVE";

    }

    public void OnSave()
    {
        //Save
        if (_songManager.CurrentSong == null || SaveButton.GetComponentInChildren<Text>().text.Length > 4) return; // TODO Change the end of the if and do it with states
        _propertiesManager.UpdateCurrentSong();
        _songManager.SaveSongYaml();
        StartCoroutine(SaveAnimation());
    }

    public void OnSaveAs()
    {
        if (FileManager.SaveDialog())
        {
            OnSave();
        }
    }

    public void OnLoadVideo()
    {
        /*var fileList = FileManager.ReadDirectoryMP4();
		var container = DialogsWindowsManager.Instance.ContentLoadVideoFiles;
		var button = DialogsWindowsManager.Instance.ButtonVideoFileTemplate;

		CreateButtons(fileList, container, button);

		DialogsWindowsManager.Instance.ShowWindow(DialogsWindowsManager.Window.LoadVideo);*/
        SetVideoFileToLoad(FileManager.CurrentFilename + FileManager.CurrentFilenameExtension);
    }

    public void OnLoadAudioClip()
    {
        /*var fileList = FileManager.ReadDirectoryOgg();
		var container = DialogsWindowsManager.Instance.ContentLoadAudioClipFiles;
		var button = DialogsWindowsManager.Instance.ButtonAudioClipTemplate;

		CreateButtons(fileList, container, button);

		DialogsWindowsManager.Instance.ShowWindow(DialogsWindowsManager.Window.LoadAudio);
		*/

        SetAudioClipToLoad(FileManager.CurrentFilename + FileManager.CurrentFilenameExtension);
    }
    //Zoom
    public void OnZoom(int zoom)
    {
        int currentZoom = (int)_zoom;
        double log = Math.Log(currentZoom, 2);

        int exponentDelta = zoom > 0 ? zoom : ((log % 1 == 0) ? -1 : 0);
        int exponent = (int)(log + exponentDelta);
        int zoomValue = (int)Math.Pow(2, exponent);

        zoomValue = Mathf.Clamp(zoomValue, ZoomMin, ZoomMax);

        Zoom.text = zoomValue.ToString();
        UpdateZoom(zoomValue);
    }

    public void ResetZoom()
    {
        Zoom.text = "1";
        UpdateZoom();
    }

    public void OnZoom(string value)
    {
        int zoom;
        if (!Int32.TryParse(value, out zoom)) return;
        if (zoom < ZoomMin || zoom > ZoomMax)
        {
            Zoom.text = Mathf.Clamp(zoom, ZoomMin, ZoomMax).ToString();
            return;
        }

        UpdateZoom(zoom);
    }

    public void OnVolumeChange(string value)
    {
        int volumeMin = 0;
        int volumeMax = 100;

        int volume;
        if (!Int32.TryParse(value, out volume)) return;
        if (volume < volumeMin || volume > volumeMax)
        {
            Volume.text = Mathf.Clamp(volume, volumeMin, volumeMax).ToString();
            return;
        }
        _audioManager.UpdateVolume((float)volume / 100);
    }

    public void OnVolumeChange(int value)
    {
        int currentVolume = Int32.Parse(Volume.text);
        int volumeValue = Mathf.Clamp((currentVolume + value), 0, 100);
        Volume.text = volumeValue.ToString();
        _audioManager.UpdateVolume((float)volumeValue / 100);
    }

    public void OnMoveScrollWave(int direction)
    {
        // Applt % of the song that we want to move in the the scroll
        var range = (_waveTimeRange.y - _waveTimeRange.x) * ScrollAmount;

        if (direction < 0)
            StartCoroutine(BarToPosition(_waveTimeRange.x - range));
        else
            StartCoroutine(BarToPosition(_waveTimeRange.x + range));
    }

    public void OnMoveScrollWave()
    {
        _currentScrollTime = (ClipInfo.ClipTimeSize - (ClipInfo.ClipTimeSize / _zoom)) * SongScrollBar.normalizedPosition.x;
        RenderWave();
    }

    public void OnPlayClip()
    {
        _audioManager.PlayClipAtTime(_cursorTime);

        _videoManager.Play();
        _videoManager.SetTime(_cursorTime);
    }

    public void OnPauseClip()
    {
        _cursorTime = _audioManager.GetCurrentClipTime();
        _audioManager.PauseClip();

        SetCursorPosition();
        // StartCoroutine(BarToPosition(_cursorTime));

        _videoManager.Stop();
    }

    public void OnStopClip()
    {
        _audioManager.StopClip();

        SetCursorPosition();
        StartCoroutine(BarToPosition(_cursorTime));

        _videoManager.Stop();
    }

    //Called for click in edior SongWaveContainer or SongObjectContainer
    public void OnWaveClicked()
    {
        if (Input.GetMouseButtonUp(1)) return; // if Mouse button is right
        if (ClipInfo.ClipTimeSize == 0) return;
        clearListOfSelectedObject();
        float cursorTime = PixelToSec(Input.mousePosition.x / _mainCanvas.scaleFactor);
        OnCursorChange(cursorTime);
    }

    public void OnCursorChange(float cursorTime)
    {
        _cursorTime = cursorTime;

        SetCursorPosition();

        _propertiesManager.UpdateCurrentSong();
        _propertiesManager.UpdateUISongProperties();
        _propertiesManager.UpdateClipTime(_cursorTime.ToString("F2"));
        // _objectsLibraryManager.HideWallEdition(true);
    }

    public void OnWallObjectClicked(WallObject wallObject)
    {
        addWallObjectToSelectedList(wallObject, true);
        // _objectsLibraryManager.HideWallEdition(false);
        _objectsLibraryManager.SetPanel(0);
        _objectsLibraryManager.UpdateUIWallTime(wallObject.Time);
        _objectsLibraryManager.SelectWallTab(WallsUtils.GetWallType(wallObject.WallObjectId), wallObject.WallObjectId);
    }

    public void OnWallObjectMove(int sign)
    {
        // Add Substrac 1 ms
        _currentWallObject.AddTimeAmount(sign * 0.01f);
        _objectsLibraryManager.UpdateUIWallObjectProperties(_currentWallObject.WallObjectId, _currentWallObject.Time);
        moveWallsVerticallyInTheNextFrame();
    }

    public void OnWallObjectDrag(WallObject wallObject)
    {
        // if (!Input.GetMouseButton(0)) return; // if Mouse button is not right
        if (Input.GetMouseButton(0))
        {
            if (_selectedWallObjects.IndexOf(wallObject) < 0)
            {
                OnWallObjectClicked(wallObject);
            }
            Vector2 worldPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponentInParent<RectTransform>(), Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out worldPosition);

            float draggedWallTime = PixelToSec(Input.mousePosition.x / _mainCanvas.scaleFactor);

            if (Mathf.Abs(_cursorTime - draggedWallTime) < 2f / _zoom)
            {
                draggedWallTime = _cursorTime;
            }
            else
            {
                if (_timeGridManager.Snap)
                {
                    foreach (RectTransform gridLine in _timeGridManager.GridLines)
                    {
                        if (Mathf.Abs(float.Parse(gridLine.name) - draggedWallTime) < 2f / _zoom)
                        {
                            draggedWallTime = float.Parse(gridLine.name);
                            break;
                        }
                    }
                }
            }

            float diffTime = draggedWallTime - wallObject.Time;
            
            foreach (WallObject selectedWallObject in _selectedWallObjects)
            {
                float wallTime = selectedWallObject.Time + diffTime;
                if (wallTime >= ClipInfo.ClipTimeSize - 0.01f) wallTime = ClipInfo.ClipTimeSize - 0.01f;
                if (wallTime < 0) wallTime = 0f;

                updateWallTime(selectedWallObject, wallTime);
            }
        }

        /*OnWallObjectClicked(wallObject);
        Vector2 worldPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponentInParent<RectTransform>(), Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out worldPosition);

        float wallTime = PixelToSec(Input.mousePosition.x / _mainCanvas.scaleFactor);

        if (Mathf.Abs(_cursorTime - wallTime) < 2f / _zoom)
        {
            wallTime = _cursorTime;
        }
        updateWallTime(wallTime);*/
    }

    public void OnAddWallObject()
    {
        OnAddWallObject(_cursorTime);
    }

    public void OnAddWallObject(float time)
    {
        if (ClipInfo.ClipTimeSize == 0) return;

        string wallObjectId = _currentWallObject != null ? _currentWallObject.WallObjectId : "WP.C33CUC";

        _currentWallObject = _songManager.CreateWallObject(wallObjectId, time, true, this);
        _currentWallObject.GetComponent<Transform>().Find("MarkLine").gameObject.SetActive(_zoom >= WallMarklineVisibleAtZoomLevel);
        _currentWallObject.GetComponent<Transform>().Find("Toggle/Wall Id").gameObject.SetActive(_zoom >= WallIdVisibleAtZoomLevel);

        // _objectsLibraryManager.UpdateWallObjectSprite(_currentWallObject.WallObjectId);
        addWallObjectToSelectedList(_currentWallObject);
        OnWallObjectClicked(_currentWallObject);
    }

    /*public void OnDeleteWallObjectDialog()
    {
        DialogsWindowsManager.Instance.ShowWindow(DialogsWindowsManager.Window.DeleteObject);
    }
    */

    public void OnDeleteWallObject()
    {
        /*
        _wallObjects.Remove(_currentWallObject);
        removeWallObjectFromSelectedList(_currentWallObject);
        _currentWallObject = null; // TODO I don't know if this is valid
        Destroy(_currentWallObject.gameObject);
        // Destroy(_objectsLibraryManager.gameObject);
        */

        foreach (WallObject wallObject in _selectedWallObjects)
        {
            _wallObjects.Remove(wallObject);
            Destroy(wallObject.gameObject);
        }
        _selectedWallObjects.Clear(); // Set outside of the iteration because it will throw an error
        _objectsLibraryManager.SetPanel(1);
        _currentWallObject = null; // TODO I don't know if this is valid
        moveWallsVerticallyInTheNextFrame();

    }

    public void OnUpdatingWallId(string id)
    {
        WallsUtils.Walltype walltype = WallsUtils.GetWallType(id);
        _objectsLibraryManager.SelectWallTab(walltype, id);
    }

    public void OnUpdateWallId(string id)
    {
        if (_currentWallObject == null) return;
        OnUpdatingWallId(id);
        _currentWallObject.ChangeName(_objectsLibraryManager.CurrentWall.Id);

        // WallsUtils.Walltype walltype = WallsUtils.GetWallType(id);
        // _objectsLibraryManager.SelectWallTab(walltype);

    }

    public void OnUpdateWallTime(float time)
    {
        if (_currentWallObject == null) return;
        updateWallTime(_currentWallObject.Time + time);
    }

    public void OnUpdateWallTime(string strTime)
    {
        updateWallTime(float.Parse(strTime));
    }

    private void updateWallTime(float time)
    {
        if (_currentWallObject == null) return;

        _currentWallObject.ChangeTime(time);
        _objectsLibraryManager.UpdateUIWallTime(time);
        moveWallsVerticallyInTheNextFrame();
    }

    private void updateWallTime(WallObject wallObject, float time)
    {
        if (wallObject == null) return;

        wallObject.ChangeTime(time);
        // _objectsLibraryManager.UpdateUIWallTime(time);
        moveWallsVerticallyInTheNextFrame();
    }

    public void OnExit()
    {
        if (_songManager.CurrentSong != null && _songManager.CheckChanges())
        {
            DialogsWindowsManager.Instance.ShowWindow(DialogsWindowsManager.Window.SaveAndQuit);
            return;
        }
        DialogsWindowsManager.Instance.ShowWindow(DialogsWindowsManager.Window.Quit);
    }

    public void OnToggleFullscreen()
    {
        NormalScreenButtons.SetActive(Screen.fullScreen);
        FullScreenButtons.SetActive(!Screen.fullScreen);
        WindowManager.instance.ToggleFullScreen();
    }

    //Video
    public void OnVideoOffsetChanged(string newOffset)
    {
        float offset;
        if (!float.TryParse(newOffset, out offset)) return;

        _videoManager.SetVideoOffset(offset);
        if (_songManager.CurrentSong != null)
        {
            _songManager.CurrentSong.VOffset = offset;
        }
    }

    public void OnAddVideoOffset(float amount)
    {
        _videoManager.AddVideoOffset(amount);
        float offset = _videoManager.GetVideoOffset();
        if (_songManager.CurrentSong != null)
        {
            _songManager.CurrentSong.VOffset = offset;
        }
    }
    #endregion

    #region Clip Wave Render

    private void UpdateZoom(float value = 1)
    {
        _zoom = value;

        _songWalls.anchorMax = new Vector2(_zoom, 1);

        _songManager.SetWallMarkLinesVisible(_zoom >= WallMarklineVisibleAtZoomLevel ? true : false);
        _songManager.SetWallIdVisible(_zoom >= WallIdVisibleAtZoomLevel ? true : false);

        _timeGrid.anchorMax = new Vector2(_zoom, 1);

        _renderSong.MakeLevelBuffers(_zoom);
        // RenderWave();

        //Update Scroll bar s
        RefreshClipScroll();
        moveWallsVerticallyInTheNextFrame();
        // UpdateZoomText();
    }

    private void UpdateClipRenderTimes()
    {
        var windowStart = -_songWalls.anchoredPosition.x / _songWalls.rect.width;
        var timeStart = windowStart * ClipInfo.ClipTimeSize;

        var windowEnd = (WaveWalls.rect.width - _songWalls.anchoredPosition.x) / _songWalls.rect.width;
        var timeEnd = windowEnd * ClipInfo.ClipTimeSize;

        ////Actualize time range    
        _waveTimeRange = new Vector2(timeStart, timeEnd);
    }

    private void RefreshClipScroll()
    {
        // I need move it on next frame
        // Move Bar to the same second of the song in the last zoom        
        StartCoroutine(BarToPosition(_cursorTime));
    }

    private IEnumerator BarToPosition(float time)
    {
        //TODO multiple calls (Investigate)
        print(_zoom);
        if (_zoom > 1)
        {
            // Calculate the % of the time in the scroll
            var x = time / (ClipInfo.ClipTimeSize - (ClipInfo.ClipTimeSize / _zoom));
            x = Mathf.Clamp01(x);

            _currentScrollTime = time;
            SongScrollBar.normalizedPosition = new Vector2(x, 0);

            yield return new WaitForEndOfFrame();

        }

        RenderWave();
    }

    #endregion

    #region Toolbar

    private void UpdateZoomText()
    {
        ZoomController.UpdateZoomTextValue(string.Format("{0}.0x", _zoom));
    }

    #endregion

    #region Grid

    public void OnGridVisibilityChange(Boolean isVisible)
    {
        _timeGridManager.SetGridVisible(isVisible);
        _songManager.CurrentSong.Grid = isVisible;
    }

    public void OnGridBPMChange(string bpm)
    {
        _timeGridManager.ChangeGridBPM(Int32.Parse(bpm));
        _songManager.CurrentSong.GridBpm = Int32.Parse(bpm);
    }

    public void OnGridOffsetChange(string offset)
    {
        _timeGridManager.ChangeGridOffset(float.Parse(offset));
        _songManager.CurrentSong.GridOffset = float.Parse(offset);
    }
    
    public void OnSnapChange(Boolean isActive)
    {
        _timeGridManager.Snap = isActive;
    }

    #endregion

    #region Objects Library

    #region Editor Builder Change Tab
    private void OnObjectsLibraryChangeTab(bool active, WallsUtils.Walltype wallType)
    {
        if (!active) return;
        _objectsLibraryManager.SelectWallTab(wallType);

        OnUpdateWallId(_objectsLibraryManager.CurrentWall.Id);
    }

    public void OnObjectsLibraryChangeTabToShape(bool active)
    {
        OnObjectsLibraryChangeTab(active, WallsUtils.Walltype.WP);
    }

    public void OnObjectsLibraryChangeTabToHit(bool active)
    {
        OnObjectsLibraryChangeTab(active, WallsUtils.Walltype.WH);
    }

    public void OnObjectsLibraryChangeTabToDodge(bool active)
    {
        OnObjectsLibraryChangeTab(active, WallsUtils.Walltype.WA);
    }

    public void OnObjectsLibraryChangeTabToCoin(bool active)
    {
        OnObjectsLibraryChangeTab(active, WallsUtils.Walltype.CN);
    }

    #endregion


    public void OnObjectsLibraryShapeChange()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;
        string id = _objectsLibraryManager.CurrentWall.Id;

        int position = _objectsLibraryManager.GetPositionOfCharToChange(WallsUtils.Walltype.WP, button.name);
        char character = button.name.ToCharArray()[button.name.Length - 1];

        string newCode = _objectsLibraryManager.WallShape.ModifyStringCharacter(character, position);
        
        if (newCode == id) return;
        _objectsLibraryManager.UpdateUIWallObjectProperties(newCode);
        OnUpdateWallId(newCode);
        // TODO Change name in input
    }

    public void OnObjectsLibraryShapeChange66(bool change)
    {

        GameObject button = EventSystem.current.currentSelectedGameObject;

        change = button.GetComponent<Toggle>().isOn;
        string id = _objectsLibraryManager.CurrentWall.Id;

        string newCode = id.Substring(0, 4) + (change ? "66" : "33") + id.Substring(6);

        if (newCode == id) return;
        _objectsLibraryManager.UpdateUIWallObjectProperties(newCode);
        OnUpdateWallId(newCode);
        // TODO Change name in input

    }

    public void OnObjectsLibraryHitChange()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;

        int position = _objectsLibraryManager.GetPositionOfCharToChange(WallsUtils.Walltype.WH, button.name);
        char character = button.name.ToCharArray()[button.name.Length - 1];

        string newCode = _objectsLibraryManager.WallHit.ModifyStringCharacter(character, position);
        if (newCode == _objectsLibraryManager.CurrentWall.Id) return;
        _objectsLibraryManager.UpdateLibraryChangeHits(newCode);

        _objectsLibraryManager.UpdateUIWallObjectProperties(newCode);

        OnUpdateWallId(newCode);
    }

    public void OnObjectsLibraryHitHandsChange(string hand)
    {
        int position = 4;
        char character = hand[0];

        GameObject button = EventSystem.current.currentSelectedGameObject;
        char buttonCharacter = button.name.ToCharArray()[button.name.Length - 1];
        Toggle toggle = button.GetComponent<Toggle>();
        if (_objectsLibraryManager.WallHit.Id[position] == 'B')
        {
            character = character == 'L' ? 'R' : 'L';
        }
        else
        {
            character = _objectsLibraryManager.WallHit.Id[position] == character ? character : 'B';
        }

        string newCode = _objectsLibraryManager.WallHit.ModifyStringCharacter(character, position);
        if (newCode == _objectsLibraryManager.CurrentWall.Id) return;
        _objectsLibraryManager.UpdateLibraryChangeHits(newCode);
        _objectsLibraryManager.UpdateUIWallObjectProperties(newCode);

        OnUpdateWallId(newCode);
    }

    public void OnObjectsLibraryDodgeChange()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;
        string newCode = _objectsLibraryManager.WallDodge.Id;

        int position = _objectsLibraryManager.GetPositionOfCharToChange(WallsUtils.Walltype.WA, button.name);
        char character = button.name.ToCharArray()[button.name.Length - 1];

        newCode = "WA." + (character == '0' ? "LI" : character == '1' ? "L" : character == '2' ? "U" : character == '3' ? "R" : "RI") + "." + _objectsLibraryManager.WallDodge.Duration;

        _objectsLibraryManager.UpdateDodgeSprite(newCode);
        _objectsLibraryManager.UpdateUIWallObjectProperties(newCode);

        OnUpdateWallId(newCode);
    }


    public void OnObjectsLibraryChangeDodgeDuration(string duration)
    {
        int intDuration = Int32.Parse(duration);
        if (intDuration <= 0) return;
        _objectsLibraryManager.UpdateDodgeDuration(intDuration);
        // _objectsLibraryManager.UpdateUIWallObjectProperties(_objectsLibraryManager.GetDodge().Code);
        // OnUpdateWallId(_objectsLibraryManager.GetDodge().Code);
        _objectsLibraryManager.UpdateUIWallObjectProperties(_objectsLibraryManager.WallDodge.Id);
        _currentWallObject.ChangeName(_objectsLibraryManager.WallDodge.Id);
    }

    public void OnObjectsLibraryMoveCoin()
    {
        _objectsLibraryManager.MoveCoinInCoordinates();
        _objectsLibraryManager.UpdateUIWallObjectProperties(_objectsLibraryManager.WallCoin.Id);
        // OnUpdateWallId(_objectsLibraryManager.GetCoinCode());
        _currentWallObject.ChangeName(_objectsLibraryManager.WallCoin.Id);
    }

    public void OnObjectsLibraryMoveCoinX(string x)
    {
        int number;
        if (!Int32.TryParse(x, out number)) return;
        if (_objectsLibraryManager.WallCoin.X == number) return;
        _objectsLibraryManager.MoveCoinInCoordinatesX(number);
        _objectsLibraryManager.UpdateUIWallObjectProperties(_objectsLibraryManager.GetCoinCode());
        _currentWallObject.ChangeName(_objectsLibraryManager.WallCoin.Id);
    }

    public void OnObjectsLibraryMoveCoinY(string y)
    {
        int number;
        if (!Int32.TryParse(y, out number)) return;
        if (_objectsLibraryManager.WallCoin.Y == number) return;
        _objectsLibraryManager.MoveCoinInCoordinatesY(number);
        _objectsLibraryManager.UpdateUIWallObjectProperties(_objectsLibraryManager.GetCoinCode());
        _currentWallObject.ChangeName(_objectsLibraryManager.WallCoin.Id);
    }

    #endregion

    #region Selection Drag

    public void OnSelectionDragOnBeginDrag(BaseEventData eventData)
    {
        if (!Input.GetMouseButton(1)) return; // if Mouse button is not right
        clearListOfSelectedObject();
        _dragSelectionHandler.OnBeginDrag(eventData);
    }


    public void OnSelectionDragOnDrag(BaseEventData eventData)
    {
        if (!Input.GetMouseButton(1)) return; // if Mouse button is not right
        _dragSelectionHandler.OnDrag(eventData);
        List<WallObject> wallObjects = _dragSelectionHandler.GetWallObjectsInsideTheSelectionBox(eventData, _wallObjects);
        refreshListOfSelectedObjects(wallObjects);
    }

    public void OnSelectionDragOnEndDrag(BaseEventData eventData)
    {
        if (!Input.GetMouseButtonUp(1)) return; // if Release Mouse button is not right

        List<WallObject> wallObjects = _dragSelectionHandler.GetWallObjectsInsideTheSelectionBox(eventData, _wallObjects);
        foreach(WallObject wallObject in wallObjects)
        {
            addWallObjectToSelectedList(wallObject);
        }
    }

    #endregion

    #region Utils

    public void refreshListOfSelectedObjects(List<WallObject> wallObjects)
    {
        List<WallObject> objectsToToggleOn = new List<WallObject>();
        List<WallObject> objectsToToggleOff = new List<WallObject>();
        foreach(WallObject wallObject in _selectedWallObjects)
        {
            if (wallObjects.IndexOf(wallObject) < 0)
            {
                objectsToToggleOff.Add(wallObject);
            }
        }

        foreach(WallObject wallObject in wallObjects)
        {
            if (_selectedWallObjects.IndexOf(wallObject) < 0)
            {
                objectsToToggleOn.Add(wallObject);
            }
        }

        foreach(WallObject objectToToggleOff in objectsToToggleOff)
        {
            removeWallObjectFromSelectedList(objectToToggleOff);
        }

        foreach(WallObject objectToToggleOn in objectsToToggleOn)
        {
            addWallObjectToSelectedList(objectToToggleOn);
        }

    }

    public void clearListOfSelectedObject()
    {
        foreach (WallObject wallObject in _selectedWallObjects)
        {
            wallObject.GetComponentInChildren<Toggle>().isOn = false;
        }
        _selectedWallObjects.Clear();
        _objectsLibraryManager.SetPanel(1);
    }
    public void addWallObjectToListOfObjects(WallObject wallObject)
    {
        _wallObjects.Add(wallObject);
        sortWallObjectListAndSibling();
        moveWallsVerticallyInTheNextFrame();
    }

    private void sortWallObjectListAndSibling()
    {
        _wallObjects.Sort(WallObject.sortWallByName());

        for (int i = 0; i < _wallObjects.Count; i++) {
            if (_wallObjects[i].transform.GetSiblingIndex() != i)
            {
               _wallObjects[i].transform.SetSiblingIndex(i);
            }
        }
    }

    private void moveWallsVerticallyInTheNextFrame()
    {
        _moveWallsVerticallyInTheNextFrame = 1;
    }

    private void moveWallsVertically()
    {
        StartCoroutine(ImoveWallsVerticallys());
    }

    private IEnumerator ImoveWallsVerticallys() // TODO Set in WallObject.cs
    {
        yield return new WaitForFixedUpdate();

        int multiplier = 1;
        if (_wallObjects.Count == 0) yield break;
        WallObject selectedWallObject = _wallObjects[0];
        float baseY = Mathf.Infinity;

        foreach (WallObject wallObject in _wallObjects)
        {
            float yPosition = wallObject.GetComponentInChildren<Toggle>().transform.localPosition.y;
            if (yPosition < baseY)
            {
                baseY = yPosition;
            }
        }
        
        foreach(WallObject wallObject in _wallObjects)
        {
            Toggle selectedToggle = selectedWallObject.GetComponentInChildren<Toggle>();
            Toggle currentToggle = wallObject.GetComponentInChildren<Toggle>();

            BoxCollider2D selectedCollider = selectedWallObject.GetComponent<BoxCollider2D>();
            BoxCollider2D currentCollider = wallObject.GetComponent<BoxCollider2D>();
            bool isTouching = selectedCollider.IsTouching(currentCollider);

            float height = 20;
            if (isTouching)
            {
                if (_zoom >= WallSuperpositionAtZoomLevel || wallObject.name == selectedWallObject.name)
                {
                    if (multiplier < 4) multiplier ++;
                }
                else
                {
                    multiplier = 0;
                }

                if (currentToggle.transform.localPosition.y == baseY + height * multiplier)
                {
                    continue;
                }
                currentToggle.transform.localPosition = new Vector3(currentToggle.transform.localPosition.x, baseY + height * multiplier);
            }
            else
            {
                currentToggle.transform.localPosition = new Vector3(currentToggle.transform.localPosition.x, baseY);
                selectedWallObject = wallObject;
                multiplier = 0;
            }
        }
    }

    private void addWallObjectToSelectedList(WallObject wallObject, bool clear = false)
    {
        if (clear) clearListOfSelectedObject();
        if (_selectedWallObjects.IndexOf(wallObject) >= 0) return; // Works like a Set. Not duplicate elements
        _selectedWallObjects.Add(wallObject);
        if (_selectedWallObjects.Count == 1)
        {
            _currentWallObject = wallObject; // Change above of the if?
            // _objectsLibraryManager.HideWallEdition(false);
            _objectsLibraryManager.SetPanel(0);
        }
        if (_selectedWallObjects.Count > 1) _objectsLibraryManager.SetPanel(2);

        wallObject.GetComponentInChildren<Toggle>().isOn = true;

    }

    private void removeWallObjectFromSelectedList(WallObject wallObject)
    {
        _selectedWallObjects.Remove(wallObject);

        // if (_selectedWallObjects.Count == 0) _objectsLibraryManager.HideWallEdition(true);
        if (_selectedWallObjects.Count == 0) _objectsLibraryManager.SetPanel(1);
        if (_selectedWallObjects.Count == 1) _objectsLibraryManager.SetPanel(0);

        wallObject.GetComponentInChildren<Toggle>().isOn = false;

    }

    private IEnumerator LoadClipRoutine()
    {
        var clipName = FileManager.CurrentAudioClipFilename;
        yield return _audioManager.LoadClipRoutine(FileManager.Path + clipName);
        if (_songManager.CurrentSong != null)
        {
            _songManager.CurrentSong.Clip = FileManager.GetFileNameWithoutExtension(clipName);
            _propertiesManager.UpdateUISongProperties();
        }

        _renderSong.ClearWaveImage();
        _renderSong.MakeLevelBuffers(1);
        RenderWave();
    }

    private IEnumerator LoadSong(string fileName)
    {
        ResetEditor();

        _songManager.DesealizeSong(fileName);

        _propertiesManager.SetSongProperites(_songManager.CurrentSong);

        NotStartedBackground.SetActive(false);

        yield return _audioManager.LoadClipRoutine(FileManager.Path + _songManager.CurrentSong.Clip + FileManager.audioExtension);

        _renderSong.MakeLevelBuffers(1);
        RenderWave();
        _songManager.LoadSongObjects();
        _timeGridManager.Setup(_songManager.CurrentSong.Grid, _songManager.CurrentSong.GridBpm, _songManager.CurrentSong.GridOffset);

        _songManager.CurrentSong.AudioTime = ClipInfo.ClipTimeSize;
        if (_songManager.CurrentSong.Video != null)
        {
            _videoManager.Setup(_songManager.CurrentSong.Video, _songManager.CurrentSong.VOffset);
        }
        else
            _videoManager.Close();
        yield return null;
        sortWallObjectListAndSibling();
        moveWallsVertically();
    }

    public void RenderWave()
    {
        UpdateClipRenderTimes();
        _renderSong.Render(_waveTimeRange);
    }

    private void ResetEditor()
    {
        ResetZoom();
        _currentScrollTime = 0;
        _propertiesManager.HideProperties();
        // _objectsLibraryManager.HideWallEdition(true);
        _objectsLibraryManager.SetPanel(1);

        ////Delete previus objects.
        clearListOfSelectedObject();
        _wallObjects.Clear();
        _songManager.DeleteSongObjects();
        _renderSong.ClearWaveImage();
    }

    public void SetCursorPosition()
    {
        //TextCurrentTime.text = string.Format("{0:000.00}", CursorPosition);

        var newTime = ClipInfo.SecToPixel(_cursorTime);
        _cursor.anchorMin = new Vector2(newTime, 0);
        _cursor.anchorMax = new Vector2(newTime, 1);
        _cursor.anchoredPosition = Vector2.zero;

        _videoManager.SetTime(_cursorTime);
        _audioManager.UpdatePlayTime(_cursorTime);


    }

    //Helpers
    //Get a pixel position and return his time in song.
    private float PixelToSec(float pixel)
    {
        if (ClipInfo.ClipTimeSize == 0) return 0;
        var windowClick = (pixel - _songWalls.anchoredPosition.x) / _songWalls.rect.width;
        var second = windowClick * ClipInfo.ClipTimeSize;
        //Only 2 decimals precision
        second *= 100;
        second = Mathf.Round(second);
        second *= 0.01f;
        return second;
    }

    private void CreateButtons(string[] fileList, GameObject container, GameObject button)
    {
        button.SetActive(false);
        RectTransform rt = container.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, fileList.Length * 33); // Size of button.

        for (int i = 0; i < container.transform.childCount; i++)
        {
            var ch = container.transform.GetChild(i).gameObject;
            if (ch.activeSelf)
                Destroy(ch);
        }
        foreach (var f in fileList)
        {
            var b = Instantiate(button, container.transform) as GameObject;
            b.SetActive(true);
            b.GetComponentInChildren<Text>().text = f;
        }
    }

    public void Copy()
    {
        _clipboard = new HashSet<WallObject>(_selectedWallObjects);
    }

    public void Paste() 
    {
        // TODO what happens if time + songTime?
        // TODO set as not active
        float initTime = Mathf.Infinity;
        clearListOfSelectedObject();
        foreach (WallObject wallObject in _clipboard)
        {
            if (wallObject.Time < initTime) initTime = wallObject.Time;
        }
        foreach (WallObject wallObject in _clipboard)
        {
            float time = _cursorTime + wallObject.Time - initTime;
            time = time > ClipInfo.ClipTimeSize ? ClipInfo.ClipTimeSize : time;
            WallObject currentWallObject = _songManager.CreateWallObject(wallObject.WallObjectId, time, true, this);
            addWallObjectToSelectedList(currentWallObject);
            currentWallObject.GetComponent<Transform>().Find("MarkLine").gameObject.SetActive(_zoom >= WallMarklineVisibleAtZoomLevel);
            currentWallObject.GetComponent<Transform>().Find("Toggle/Wall Id").gameObject.SetActive(_zoom >= WallIdVisibleAtZoomLevel);
        }
    }
    #endregion

    #region Suscriptions

    private void Instance_ScreenSizeChangeEvent(int Width, int Height)
    {
        if (_renderSong == null) return;
        _renderSong.MakeLevelBuffers(_zoom);
        RenderWave();
    }

    #endregion
}
