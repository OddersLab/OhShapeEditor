using UnityEngine;
using UnityEngine.UI;

public sealed class DialogsWindowsManager : MonoBehaviour
{

	public static DialogsWindowsManager Instance;


	public enum Window
	{
		Info,
		Quit,
        SaveAndQuit,
		NewFile,
		LoadAudio,
		LoadVideo,
		DeleteObject
	}

    #region Inspector 

    //MessagesBox
    [Header("Messages box")]
	public RectTransform MSGboxInfo;
    public RectTransform MSGboxQuit;
    public RectTransform MSGboxSaveAndQuit;
    public RectTransform MSGboxNewFile;
	public RectTransform MSGboxLoadVideo;
	public RectTransform MSGboxLoadAudioClip;
	public RectTransform MSGboxDeleteObject;
	[Header("Background object")]
	public GameObject Blur;

	[Header("Video file explorer")]
	public GameObject ContentLoadVideoFiles;
	public GameObject ButtonVideoFileTemplate;

	[Header("Audio file explorer")]
	public GameObject ContentLoadAudioClipFiles;
	public GameObject ButtonAudioClipTemplate;

    #endregion

    #region Windows Positions

    private Vector2 _hidePosition = new Vector2(0, 2000);
	private RectTransform _currentDialog;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        // Check if instance already exists
        //if not, set instance to this		
        if (Instance == null)
            Instance = this;

        //If instance already exists and it's not this:
        //Then destroy this. This enforces our singleton pattern, 
        //meaning there can only ever be one instance of a GameManager.
        else if (Instance != this)

            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Dialog Window Logic

    public void InfoMessage(string message)
	{
		MSGboxInfo.Find("MsgBoxContainer/TextInfoMessage").GetComponent<Text>().text = message;
		ShowWindow(Window.Info);
	}

	public void ShowWindow(Window window)
	{		
		HideWindows();
		SetWindow(window);
		_currentDialog.anchoredPosition = new Vector2(0, 0);
		_currentDialog.gameObject.SetActive(true);
		Blur.SetActive(true);
	}
	public void HideWindows()
	{
		if (_currentDialog == null) return;
		_currentDialog.anchoredPosition = _hidePosition;
		Blur.SetActive(false);
		_currentDialog.gameObject.SetActive(false);
		_currentDialog = null;		
	}

	private void SetWindow(Window window)
	{
		switch (window)
		{
			case Window.Info:
				_currentDialog = MSGboxInfo;
				break;
			case Window.Quit:
				_currentDialog = MSGboxQuit;
				break;
            case Window.SaveAndQuit:
                _currentDialog = MSGboxSaveAndQuit;
                break;
            case Window.NewFile:
				_currentDialog = MSGboxNewFile;
				break;
			case Window.LoadAudio:
				_currentDialog = MSGboxLoadAudioClip;
				break;
			case Window.LoadVideo:
				_currentDialog = MSGboxLoadVideo;
				break;
			case Window.DeleteObject:
				_currentDialog = MSGboxDeleteObject;
				break;
		}
	}

    #endregion
}
