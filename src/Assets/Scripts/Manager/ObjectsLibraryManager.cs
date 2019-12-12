using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ObjectsLibraryManager : MonoBehaviour
{

    #region Inspector

    [Header("Properies Windows References")]
    public Transform WallObjectProperties;

    public Transform ToggleGroupWallType;

    #endregion

    #region Wall Object properties

    public Transform ObjectsLibrary;

    #endregion

    #region Dodge Duration Input

    public GameObject DodgeDurationInput;

    #endregion

    #region Coin Coordinates Input

    public GameObject XInput;
    public GameObject YInput;

    #endregion

    private Image _wallImage;
    private Toggle[] _toggles;
    private WallsUtils.Walltype _currentWallType;
    private GameObject _currentTab;
    private GameObject _wallObjectId;
    private GameObject _time;
    private GameObject _backgroundImage;
    private GameObject _shapeImage;
    private GameObject _dodgeImage;
    private GameObject _coinImageBackground;
    private GameObject _coinImage;
    private GameObject _customImage;

    public WallEdit CurrentWall;

    public WallShape WallShape { get; set; }
    public WallHit WallHit { get; set; }
    public WallDodge WallDodge { get; set; }

    public Coin WallCoin { get; set; }

    public WallCustom WallCustom { get; set; }

    #region Update properties

    private void Awake()
    {
        _toggles = ObjectsLibrary.Find("ImageCheatSheet/ObjectSelected/Shape").GetComponentsInChildren<Toggle>();
        _currentWallType = WallsUtils.Walltype.WC; // TODO Set null

        Transform wallObjectProperties = WallObjectProperties.Find("ImageCheatSheet/Properties");
        Transform wallObjectContent = WallObjectProperties.Find("ImageCheatSheet/ObjectSelected");

        _wallObjectId = wallObjectProperties.Find("Id").gameObject;
        _time = wallObjectProperties.Find("Time").gameObject;
        // _backgroundImage = wallObjectContent.Find("Shape/ImageBackground").gameObject;
        _shapeImage = wallObjectContent.Find("Shape/ImageBackground/WallImage").gameObject;
        _dodgeImage = wallObjectContent.Find("Dodge/ImageBackground").gameObject;

        _coinImageBackground = wallObjectContent.Find("Coin/ImageBackground").gameObject;
        _coinImage = wallObjectContent.Find("Coin/ImageBackground/Image").gameObject;

        _currentTab = wallObjectContent.Find("Shape").gameObject;
        _currentTab.SetActive(true);

        WallShape = new WallShape();
        WallHit = new WallHit();
        WallDodge = new WallDodge();
        WallCoin = new Coin();
        WallCustom = new WallCustom();
        CurrentWall = WallShape;

    }

    public void MoveWallObjectSprite()
    {
        float factor = 42.5f; // TODO Change this factor to public settings in editor

        RectTransform wallImage = _shapeImage.GetComponent<RectTransform>();
        wallImage.anchoredPosition = new Vector2(factor*WallShape.getPosition(), wallImage.anchoredPosition.y);
    }

    public int GetPositionOfCharToChange(WallsUtils.Walltype wallType, string name)
    {
        switch (wallType)
        {
            case WallsUtils.Walltype.NONE:
                break;
            case WallsUtils.Walltype.WP:
                return WallShape.GetPositionOfCharToChange(name);
            case WallsUtils.Walltype.WA:
                break;
            case WallsUtils.Walltype.WH:
                return WallHit.GetPositionOfCharToChange(name);
            case WallsUtils.Walltype.WC:
                break;
            case WallsUtils.Walltype.CN:
                break;
            case WallsUtils.Walltype.EV:
                break;
            default:
                break;
        }

        return -1;

    }

    private void enableChanges() // TODO set in shape class
    {
        foreach(Toggle toggle in _toggles)
        {
            string newCode = "";
            if (toggle.name == "Shape66")
            {
                newCode = WallShape.Id.Substring(0, 4) + "66" + WallShape.Id.Substring(6);
            }
            else
            {
                char[] wallId = WallShape.Id.ToCharArray();

                char changeCharacter = toggle.name.Substring(toggle.name.Length - 1).ToCharArray()[0];
                int position = GetPositionOfCharToChange(WallsUtils.Walltype.WP, toggle.name);

                wallId[position] = changeCharacter;

                newCode = new string(wallId);
            }
            Sprite sprite = WallsUtils.getWallSprite(newCode);
            toggle.interactable = sprite != null;
            Image imageButton = toggle.GetComponent<Transform>().Find("Background/Image").GetComponent<Image>();

            imageButton.color = new Color(imageButton.color.r, imageButton.color.g, imageButton.color.b, toggle.interactable ? 1f : 0.5f);
        }
        // WallsUtils.CheckWallId(WallsUtils.Walltype.WP, _wallCode);
    }

    private void changeHitIcons() // TODO set in hit class
    {
        Image[] hitImages = _currentTab.gameObject.GetComponent<Transform>().Find("Hit Icons").gameObject.GetComponentsInChildren<Image>();

        List<int> validHits = WallHit.GetValidHits(WallHit.Id);

        for (int i = 0; i < hitImages.Length; i++)
        {
            Image image = hitImages[i];
            Color32 color = image.color;
            if (validHits.IndexOf(i) >= 0) {
                color.a = 255;
                image.color = color;
            }
            else
            {
                color.a = 75;
                image.color = color;
            }

        }
    }


    private void UpdateUIToggles(GameObject toggleGroupGO)
    {
        ToggleGroup[] toggleGroups = toggleGroupGO.GetComponentsInChildren<ToggleGroup>();
        foreach(ToggleGroup toggleGroup in toggleGroups)
        {
            Toggle[] toggles = toggleGroup.gameObject.GetComponentsInChildren<Toggle>();
            foreach(Toggle toggle in toggles)
            {
                int position = GetPositionOfCharToChange(CurrentWall.WallType, toggle.name);
                if (CurrentWall.WallType == WallsUtils.Walltype.WH && position == 4 && toggle.name.Contains("Hand"))
                {
                    switch (CurrentWall.Id[position])
                    {
                        case 'B':
                            if (!toggle.isOn)
                            {
                                toggle.Select();
                                ChangeToggle(toggle.gameObject, true);
                            }
                            break;

                        default:
                            if (toggle.name[toggle.name.Length - 1] == CurrentWall.Id[position])
                            {
                                toggle.Select();
                                ChangeToggle(toggle.gameObject, true);
                            }
                            else
                            {
                                toggle.Select();
                                ChangeToggle(toggle.gameObject, false);
                            }
                            break;
                    }

                }
                else if (CurrentWall.WallType == WallsUtils.Walltype.WA)
                {
                    string dodgePosition = CurrentWall.Id.Split('.')[1];
                    dodgePosition = (dodgePosition == "LI" ? "0" : dodgePosition == "L" ? "1" : dodgePosition == "U" ? "2" : dodgePosition == "R" ? "3" : "4");

                    if (toggle.name.EndsWith(dodgePosition))
                    {
                        toggle.Select();
                        ChangeToggle(toggle.gameObject, true);
                    }
                    else
                    {
                        ChangeToggle(toggle.gameObject, false);
                    }
                }
                else
                {
                    if (toggle.name == "Shape66")
                    {
                        if (CurrentWall.Id.Contains("66"))
                        {
                            toggle.Select();
                            ChangeToggle(toggle.gameObject, true);
                        }
                        else
                        {
                            ChangeToggle(toggle.gameObject, false);
                        }
                    }
                    else if (toggle.name[toggle.name.Length - 1] == CurrentWall.Id[position])
                    {
                        toggle.Select();
                        ChangeToggle(toggle.gameObject, true);
                    }
                    else
                    {
                        ChangeToggle(toggle.gameObject, false);
                    }
                }
            }
        }
    }
    public void SelectWallTab(WallsUtils.Walltype wallType, string id = null)
    {
        _currentTab.SetActive(false);

        string currentType = _wallObjectId.GetComponentInChildren<InputField>().text;

        GameObject currentToggleGroup;

        switch (wallType)
        {
            case WallsUtils.Walltype.NONE:
                return;
            case WallsUtils.Walltype.WP:
                // _backgroundImage.GetComponent<Image>().color = new Color32(139, 212, 255, 255);
                _currentTab = ObjectsLibrary.Find("ImageCheatSheet/ObjectSelected/Shape").gameObject;
                currentToggleGroup = ToggleGroupWallType.Find("Shape").gameObject;
                CurrentWall = WallShape;
                CurrentWall.Id = id == null ? CurrentWall.Id : id;

                UpdateUIWallObjectProperties(CurrentWall.Id);
                UpdateUIWallObjectSprite(CurrentWall.Id);
                enableChanges();
                MoveWallObjectSprite();
                UpdateUIToggles(_currentTab);
                break;
            case WallsUtils.Walltype.WA:
                // _backgroundImage.GetComponent<Image>().color = new Color32(22, 11, 111, 255);
                _currentTab = ObjectsLibrary.Find("ImageCheatSheet/ObjectSelected/Dodge").gameObject;
                currentToggleGroup = ToggleGroupWallType.Find("Dodge").gameObject;
                // ChangeToggle(currentToggleGroup);

                CurrentWall = WallDodge;
                CurrentWall.Id = id == null ? CurrentWall.Id : id;

                UpdateUIWallObjectProperties(CurrentWall.Id);
                UpdateDodgeSprite(CurrentWall.Id);
                UpdateUIToggles(_currentTab);

                break;
            case WallsUtils.Walltype.WH:
                // _backgroundImage.GetComponent<Image>().color = new Color32(191, 39, 45, 255);
                // _image.GetComponent<Image>().color = new Color32(191, 39, 45, 255);
                _currentTab = ObjectsLibrary.Find("ImageCheatSheet/ObjectSelected/Hit").gameObject;
                currentToggleGroup = ToggleGroupWallType.Find("Hit").gameObject;
                // ChangeToggle(currentToggleGroup);
                CurrentWall = WallHit;
                CurrentWall.Id = id == null ? CurrentWall.Id : id;

                UpdateUIWallObjectProperties(CurrentWall.Id);
                UpdateUIToggles(_currentTab);
                UpdateLibraryChangeHits(CurrentWall.Id);

                break;
            case WallsUtils.Walltype.WC:
                /*_currentTab = ObjectsLibrary.Find("ImageCheatSheet/ObjectSelected/Custom").gameObject;
                currentToggleGroup = ToggleGroupWallType.Find("Custom").gameObject;
                CurrentWall = WallCustom;
                CurrentWall.Id = id == null ? CurrentWall.Id : id;

                UpdateUIWallObjectProperties(CurrentWall.Id);
                UpdateUIToggles(_currentTab);
                UpdateLibraryChangeHits(CurrentWall.Id);*/

                return;
            case WallsUtils.Walltype.CN:
                // _backgroundImage.GetComponent<Image>().color = new Color32(22, 33, 44, 255);
                _currentTab = ObjectsLibrary.Find("ImageCheatSheet/ObjectSelected/Coin").gameObject;
                currentToggleGroup = ToggleGroupWallType.Find("Coin").gameObject;
                // ChangeToggle(currentToggleGroup);
                CurrentWall = WallCoin;
                CurrentWall.Id = id == null ? CurrentWall.Id : id;


                UpdateUIWallObjectProperties(CurrentWall.Id);
                UpdateCoin(WallCoin.X, WallCoin.Y);
                break;
            case WallsUtils.Walltype.EV:
                return;
            default:
                return;
        }
        ChangeToggle(currentToggleGroup);



        _currentTab.SetActive(true);
    }

    public string GetCoinCode()
    {
        return WallCoin.Id;
    }

    public void MoveCoinInCoordinates()
    {
        Vector2 worldPosition;
        RectTransform coinImage = _coinImage.GetComponent<RectTransform>();
        RectTransform coinImageBackground = _coinImageBackground.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(coinImageBackground, Input.mousePosition, _coinImageBackground.GetComponent<Canvas>().worldCamera, out worldPosition);

        float widthBackground = coinImageBackground.rect.width;
        float heightBackground = coinImageBackground.rect.height;
        float widthCoin = coinImage.rect.width;
        float heightCoin = coinImage.rect.height;

        Vector2 coinPosition = Coin.GetCoinPosition(worldPosition, coinImageBackground.rect, coinImage.rect);

        Vector2 backgroundContainer = new Vector2(widthBackground - widthCoin, heightBackground - heightCoin);
        Vector2 coinCoordinates = Coin.GetCoinCoordinates(coinPosition, backgroundContainer);

        coinImage.GetComponent<Transform>().localPosition = coinPosition;

        WallCoin.X = (int)Mathf.Round(coinCoordinates.x);
        WallCoin.Y = (int)Mathf.Round(coinCoordinates.y);

        SetUpPropertyInput(XInput, WallCoin.X.ToString());
        SetUpPropertyInput(YInput, WallCoin.Y.ToString());
    }

    public void UpdateCoin(int x, int y)
    {
        RectTransform coinImageBackground = _coinImageBackground.GetComponent<RectTransform>();
        RectTransform coinImage = _coinImage.GetComponent<RectTransform>();

        float widthBackground = coinImageBackground.rect.width;
        float widthCoin = coinImage.rect.width;
        float heightBackground = coinImageBackground.rect.height;
        float heightCoin = coinImage.rect.height;

        Vector2 backgroundContainer = new Vector2(widthBackground - widthCoin, heightBackground - widthCoin);

        Vector2 position = Coin.GetCoinInWorldPosition(new Vector2(x, y), new Rect(coinImageBackground.position, backgroundContainer));
        position = Coin.GetCoinPosition(position, coinImageBackground.rect, coinImage.rect);

        coinImage.GetComponent<Transform>().localPosition = new Vector2(position.x, position.y);

        WallCoin.X = x;
        WallCoin.Y = y;

        SetUpPropertyInput(XInput, WallCoin.X.ToString());
        SetUpPropertyInput(YInput, WallCoin.Y.ToString());
    }

    public void MoveCoinInCoordinatesX(int x)
    {
        UpdateCoin(x, WallCoin.Y);
    }

    public void MoveCoinInCoordinatesY(int y)
    {
        UpdateCoin(WallCoin.X, y);
    }

    public void SetPanel(int panel)
    {
        WallObjectProperties.Find("ImageCheatSheet").gameObject.SetActive(panel == 0);
        WallObjectProperties.Find("NewWall").gameObject.SetActive(panel == 1);
        WallObjectProperties.Find("MultipleWalls").gameObject.SetActive(panel == 2);
    }

    public void HideWallEdition(bool hide)
    {
        WallObjectProperties.Find("ImageCheatSheet").gameObject.SetActive(!hide);
        WallObjectProperties.Find("NewWall").gameObject.SetActive(hide);
    }

    #region Update Properties
    public void UpdateUIWallObjectProperties(string id, float time)
    {
        SetUpPropertyInput(_wallObjectId, id);
        SetUpPropertyInput(_time, time.ToString());
        CurrentWall.UpdateId(id);
    }

    public void UpdateUIWallTime(float time)
    {
        SetUpPropertyInput(_time, time.ToString("F2"));
    }

    public void UpdateUIWallObjectProperties(string id)
    {
        SetUpPropertyInput(_wallObjectId, id);
        CurrentWall.UpdateId(id);
    }

    #endregion

    public void UpdateUIWallObjectSprite(string id)
    {
        Sprite sprite = WallsUtils.getWallSprite(id);
        if (sprite == null) return;
        _shapeImage.GetComponentInChildren<Image>().sprite = sprite;
    }

    public void UpdateLibraryChangeHits(string id)
    {
        WallHit.Id = id;
        changeHitIcons();
    }

    public void UpdateDodge(string id)
    {
        WallDodge.Id = id;
        UpdateDodgeSprite(id);
    }

    public void UpdateDodgeDuration(int duration)
    {
        WallDodge.Duration = duration;
        DodgeDurationInput.GetComponent<InputField>().text = duration.ToString();
    }

    public void UpdateDodgeDuration(string duration)
    {
        WallDodge.Duration = Int32.Parse(duration);
        DodgeDurationInput.GetComponent<InputField>().text = duration;
    }

    public void UpdateDodgeSprite(string id) // TODO Fix
    {
        Sprite sprite = WallsUtils.getWallSprite(id);
        if (sprite == null) return;
        string[] splittedId = id.Split('.');
        if (splittedId.Length < 2) return;
        WallDodge.WallPosition = splittedId[1];
        _dodgeImage.GetComponentInChildren<Image>().sprite = sprite;
        UpdateDodgeDuration(splittedId[2]);
    }


    #endregion

    #region Setup Properties
    private void SetUpPropertyInput(GameObject template, string value)
    {
        InputField inputField = template.GetComponentInChildren<InputField>();
        InputField.SubmitEvent onEndEdit = inputField.onEndEdit;
        inputField.onEndEdit = null;
        // if (inputField.text == value) return;
        inputField.text = value;
        inputField.onEndEdit = onEndEdit;
    }

    private void ChangeToggle(GameObject template, bool active = true)
    {
        Toggle toggle = template.GetComponent<Toggle>();
        if (toggle.isOn == active) return;
        template.GetComponent<Toggle>().isOn = active;
    }

    public void DoNothing(bool nothing)
    {
        
    }

    #endregion
}