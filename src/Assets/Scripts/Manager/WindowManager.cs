using UnityEngine;

public class WindowManager : MonoBehaviour {

    public delegate void ScreenSizeChangeEventHandler(int width, int height);       //  Define a delgate for the event
    public event ScreenSizeChangeEventHandler ScreenSizeChangeEvent;                //  Define the Event

    protected virtual void OnScreenSizeChange(int Width, int Height)
    {
        //  Define Function trigger and protect the event for not null;
        if (ScreenSizeChangeEvent != null) ScreenSizeChangeEvent(Width, Height);
    }

    private Vector2 _lastScreenSize;
    private float _executeActionTime = 0f;
    private bool _executeAction = false;

    //  Singleton for call just one instance
    public static WindowManager instance = null;

    void Awake()
    {
        // Singleton instance
        instance = this;

        _lastScreenSize = new Vector2(Screen.width, Screen.height);
        _executeActionTime = Time.time;
    }

    void Update()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (_lastScreenSize != screenSize)
        {
            if (!Screen.fullScreen)
            {
                _lastScreenSize = screenSize;

            }

            _executeAction = true;
            _executeActionTime = Time.time + 0.2f;
        }

        if (_executeAction && Time.time > _executeActionTime)
        {
            _executeAction = false;

            //  Launch the event when the screen size change
            OnScreenSizeChange((int)screenSize.x, (int)screenSize.y);
        }
    }

    public void ToggleFullScreen()
    {
        int width = 1440;
        int height = 880;

        if (Screen.fullScreen)
        {
            width = (int)_lastScreenSize.x;
            height = (int)_lastScreenSize.y;
        }

        Screen.SetResolution(width, height, !Screen.fullScreen);
    }
}

