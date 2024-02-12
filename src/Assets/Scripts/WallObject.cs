using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

//Manage objects, (walls, coins...) 
[Serializable]
public class SerializableWallObjects
{
    [Serializable]
    public class SerializableWallObject
    {
        public string Name;
        public float Time;

        public SerializableWallObject(string name, float time)
        {
            Name = name;
            Time = time;
        }
    }

    public List<SerializableWallObject> serializableWallObjects = new List<SerializableWallObject>();
}

public class WallObject : MonoBehaviour
{
    private const float DEFAULT_WIDTH = 30f;
    
    #region Inspector

    public Color ColorPose = new Color(0.2f, 0.56f, 0.91f, 1.0f);
    public Color ColorHit = new Color(0.91f, 0.2f, 0.2f, 1.0f);
    public Color ColorAvoid = new Color(0.91f, 0.2f, 0.2f, 1.0f);
    public Color ColorCustom = new Color(0.91f, 0.2f, 0.2f, 1.0f);
    public Color ColorCoin = new Color(0.91f, 0.2f, 0.2f, 1.0f);
    public Color ColorEvent = new Color(0.91f, 0.2f, 0.2f, 1.0f);
    public Color ColorNull = new Color(0.91f, 0.2f, 0.2f, 1.0f);

    public Color PressedColorPose = new Color(0.2f, 0.56f, 0.91f, 1.0f);
    public Color PressedColorHit = new Color(0.91f, 0.2f, 0.2f, 1.0f);
    public Color PressedColorAvoid = new Color(0.91f, 0.2f, 0.2f, 1.0f);
    public Color PressedColorCustom = new Color(0.91f, 0.2f, 0.2f, 1.0f);
    public Color PressedColorCoin = new Color(0.91f, 0.2f, 0.2f, 1.0f);
    public Color PressedColorEvent = new Color(0.91f, 0.2f, 0.2f, 1.0f);
    public Color PressedColorNull = new Color(0.91f, 0.2f, 0.2f, 1.0f);

    [Space]
    public BoxCollider2D Collider;
    public BoxCollider2D ToggleCollider;
    
    #endregion

    #region Wall Info

    [HideInInspector]
    public string WallObjectId;
    [HideInInspector]
    public float Time;

    #endregion

    #region References

    private RectTransform _rect;
    private Toggle _toggle;
    private Text _wallIdText;
    private Text _wallTypeText;
    private RectTransform _wallMarkLine;
    private Color _mainColor;
    private Color _secundaryColor;
    private string _textToggle = "?";

    #endregion

    #region Monobehaviour

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        Transform toggle = transform.Find("Toggle");
        _wallMarkLine = transform.Find("MarkLine") as RectTransform;

        _toggle = toggle.GetComponent<Toggle>();
        _wallIdText = toggle.Find("Wall Id").GetComponent<Text>();
        _wallTypeText = toggle.Find("Wall Type").GetComponent<Text>();
        // allMyWallObjects.Add(this);
    }

    public float Width { get; private set; }
    #endregion

    #region Wall edition
    public void Init(string name, float time, bool isOn, OhShapeEditor editor)
    {        
        WallObjectId = name;
        Time = time;
        SetPosition();
        CheckWallId(name);

        _wallIdText.text = name;
        // addToggleGroup();
        addEventToWall(editor);
        _toggle.isOn = isOn;
    }

    public void ChangeName(string name)
    {
        WallObjectId = name;
        _wallIdText.text = WallObjectId;
        CheckWallId(name);
    }

    public void SetMarkLineVisible(Boolean isVisible)
    {
        _wallMarkLine.gameObject.SetActive(isVisible);
    }

    public void SetIdVisible(Boolean isVisible)
    {
        _wallIdText.gameObject.SetActive(isVisible);
    }

    public void ChangeTime(string time)
    {
        float numValue;
        if (float.TryParse(time, out numValue))
        {
            ChangeTime(numValue);
        }
        else
        {
            DialogsWindowsManager.Instance.InfoMessage("Error, bad time introduced.");            
        }
    }

    private void Update()
    {
        UpdateWidth();
    }

    public void UpdateWidth()
    {
        string id = WallObjectId.Substring(0, 2);
        if (id == "WA")
        {
            float min = OhShapeEditor.Instance.TimeStart;
            float max = OhShapeEditor.Instance.TimeEnd;

            string[] values = WallObjectId.Split('.', ',');
            float duration = int.Parse(values[2]) / 100f;

            float startTime = Time;
            float endTime = startTime + duration;
            
            float posInit = ClipInfo.SecToPixel(startTime);
            float posEnd = ClipInfo.SecToPixel(endTime);
            
            if ((startTime > min && startTime < max) || (endTime > min && endTime < max))
            {
                _rect.anchorMin = new Vector2(posInit, 0.0f);
                _rect.anchorMax = new Vector2(posEnd, 1.0f);
                _rect.anchoredPosition = new Vector2(0, 0);
                _rect.offsetMin = new Vector2(0, 0);
                _rect.offsetMax = new Vector2(0, 0);
                _rect.localScale = Vector3.one;
            }
        }
        else _rect.sizeDelta = new Vector2(DEFAULT_WIDTH, _rect.sizeDelta.y);
    }

    private float Increment(float x, float y, float z)
    {
        return 1f + (1f - x / y) / z;
    }

    public void ChangeTime(float time)
    {
        if (time < 0) time = 0;
        Time = time;
        SetPosition();        

        name = string.Format("{0:0000.00}", time);
    }

    public void AddTimeAmount(float time)
    {
        ChangeTime(Time + time);
    }

    #endregion

    #region Walls validations

    public void CheckWallId(string id)
    {
        _mainColor = ColorNull;
        _secundaryColor = PressedColorNull;
        SetTextToggle("?");

        //Pose:    WP.[L|C|R][nn][?][U|D]
        //Custom:  WC.[L|C|R][xxxx]
        //Hit:     WH.[L|C|R][L|B|R][U|D]
        //Avoid:   WA.xxxx.t
        //Coin:    CN.xx.yy        
        string[] splitedName = id.ToUpper().Split('.');

        try { 

            WallsUtils.Walltype wallType = (WallsUtils.Walltype)Enum.Parse(typeof(WallsUtils.Walltype), splitedName[0]);
            string wallId = splitedName[1];
            switch (wallType)
            {
                case WallsUtils.Walltype.WP:
                    CheckPose(wallType, wallId);
                    break;
                case WallsUtils.Walltype.WA:
                    string time = splitedName[2];
                    CheckAvoid(wallId, time);
                    break;
                case WallsUtils.Walltype.WH:
                    CheckHit(wallType, wallId);
                    break;
                case WallsUtils.Walltype.WC:
                    CheckCustom(wallType, wallId);
                    break;
                case WallsUtils.Walltype.CN:
                    string x = splitedName[1];
                    string y = splitedName[2];
                    CheckCoins(x, y);
                    break;
                case WallsUtils.Walltype.EV:
                    CheckEvent(wallId);
                    break;
            }            
        }
        catch (Exception e)
        {
            Log.AddLine("Bad Wall Id -> " + name + " at: " + Time + "Error" + e.StackTrace);
        }
        SetColor();
    }

    private void CheckPose(WallsUtils.Walltype type, string id)
    {
        if (WallsUtils.CheckWallId(type, id))
        {
            _mainColor = ColorPose;
            _secundaryColor = PressedColorPose;
            SetTextToggle("P");
        }
        else
        {
            Log.AddLine("Bad pose -> " + id + " at: " + Time);
            SetTextToggle("P?");
        }
    }

    private void CheckCustom(WallsUtils.Walltype type, string id)
    {
        if (WallsUtils.CheckWallId(type, id))
        {
            _mainColor = ColorCustom;
            _secundaryColor = PressedColorCustom;

            SetTextToggle("C");
        }
    }

    private void CheckHit(WallsUtils.Walltype type, string id)
    {

        if (WallsUtils.CheckWallId(type, id))
        {
            _mainColor = ColorHit;
            _secundaryColor = PressedColorHit;

            SetTextToggle("H");
        }
    }

    private void CheckAvoid(string id, string time)
    {
        if (WallsUtils.CheckAvoidWall(id, time))
        {
            _mainColor = ColorAvoid;
            _secundaryColor = PressedColorAvoid;

            SetTextToggle("A");
        }
    }

    private void CheckCoins(string x, string y)
    {
        if (WallsUtils.CheckCoinWall(x, y))
        {
            _mainColor = ColorCoin;
            _secundaryColor = PressedColorCoin;

            SetTextToggle("o");
        }
    }

    // TODO CHECK EVENTS WALLS
    private void CheckEvent(string id)
    {
        _mainColor = ColorEvent;
        _secundaryColor = PressedColorEvent;

        SetTextToggle("EV");
    }
    #endregion

    #region Utils

    private void addEventToWall(OhShapeEditor editor)
    {
        EventTrigger trigger = _toggle.GetComponentInParent<EventTrigger>();
       
        EventTrigger.Entry pointerClick = new EventTrigger.Entry();
        pointerClick.eventID = EventTriggerType.PointerClick;
        pointerClick.callback.AddListener((eventData) => {
            // _toggle.isOn = true;
            editor.OnWallObjectClicked(this);
        });

        EventTrigger.Entry drag = new EventTrigger.Entry();
        drag.eventID = EventTriggerType.Drag;
        drag.callback.AddListener((eventData) =>
        {
            editor.OnWallObjectDrag(this);
        });

        trigger.triggers.Add(pointerClick);
        trigger.triggers.Add(drag);
    }

    private void SetPosition()
    {
        RectTransform rt = GetComponent<RectTransform>();
        var pos = ClipInfo.SecToPixel(Time);

        rt.anchorMin = new Vector2(pos, 0.0f);
        rt.anchorMax = new Vector2(pos, 1.0f);
        rt.anchoredPosition = new Vector2(0, 0);
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(30, 0);
        rt.localScale = Vector3.one;
    }

    private void SetColor()
    {
        ColorBlock colorBlock = new ColorBlock();
        colorBlock.normalColor = _mainColor;
        colorBlock.highlightedColor = _secundaryColor;
        colorBlock.pressedColor = _secundaryColor;
        colorBlock.colorMultiplier = 1.0f;
        colorBlock.fadeDuration = 0.1f;
        _toggle.colors = colorBlock;
        _toggle.gameObject.GetComponentsInChildren<Image>()[1].color = _secundaryColor;
    }


    private void SetTextToggle(string text)
    {
        _textToggle = text;
        _wallTypeText.text = _textToggle;
    }

    #endregion

    #region Wall Utils

    private class sortByName : IComparer<WallObject>
    {
        int IComparer<WallObject>.Compare(WallObject a, WallObject b)
        {
            int comparation = String.Compare(a.name, b.name);
            if (comparation == 0)
            {
                return a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex());
            }
            return comparation;
        }
    }

    public static IComparer<WallObject> sortWallByName()
    {
        return new sortByName();
    }

    #endregion
}

