using UnityEngine;
using UnityEngine.UI;

public class ZoomController : MonoBehaviour
{
    #region references

    private Text _valueText;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        _valueText = GetComponentInChildren<Text>();
    }

    #endregion

    #region slider update

    public void UpdateZoomTextValue(string text)
    {
        _valueText.text = text;
    }

    #endregion

}
