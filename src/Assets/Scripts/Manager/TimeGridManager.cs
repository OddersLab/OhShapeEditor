using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class TimeGridManager : MonoBehaviour
{

	#region Inspector

	public GameObject TimeGridBarPrefab;    
	public Transform BarsParent;

    [Space]
    //UI Input variables
    public Toggle EnableGrid;
    public InputField GridBpmValue;
    public InputField GridOffset;

	#endregion


	private List<RectTransform> _gridLines = new List<RectTransform>();
    private float _bpm = 0;
    private float _offset = 0.00f;
    private Boolean _showGridLines = true;
	
	public bool Snap = false;

	public List<RectTransform> GridLines
	{
		get
		{
			return _gridLines.FindAll(line => line.gameObject.activeSelf);
		}
		private set
		{
			_gridLines = value;
		}
	}

    public void Setup(Boolean showGridLines, float bpm, float offset)
    {
        _showGridLines = showGridLines; 
        _bpm = bpm;
        _offset = offset;

        EnableGrid.isOn = showGridLines;
        GridBpmValue.text = bpm.ToString();
        GridOffset.text = offset.ToString("F2");

        LoadGridObjects();
    }

	public void LoadGridObjects()
	{
		//Delete previus objects.
		DeleteGridObjects();

		CreateGridObjects();

	}

	private void CreateGridObjects()
	{
        // float clipTimeSizeInMinutes = ClipInfo.ClipTimeSize / 60;
        float numberOfLines = ClipInfo.ClipTimeSize * (_bpm / 60);

        for (int i = 0; i <= (int)numberOfLines; i++) {
            float time = ClipInfo.ClipTimeSize / numberOfLines * i + _offset;
			time = (float)Math.Round(time, 2);
			float pos = ClipInfo.SecToPixel(time);

			GameObject timeGridLine = CreateGridBarObject (time.ToString("F2"));

			RectTransform rectTransform = timeGridLine.GetComponent<RectTransform> ();

            SetPosition(rectTransform, pos);
			rectTransform.gameObject.SetActive(_showGridLines);
			_gridLines.Add (rectTransform);
		}
    }


    public void ChangeGridOffset(float offset)
	{
		float clipTimeSizeInMinutes = ClipInfo.ClipTimeSize / 60;
		_offset = offset;
		float width = BarsParent.GetComponent<RectTransform> ().rect.width;

		for (int i = 0 ; i < _gridLines.Count ; i ++) {
			float time = ClipInfo.ClipTimeSize / _gridLines.Count * i + offset;
			time = (float)Math.Round(time, 2);
			float pos = ClipInfo.SecToPixel(time);

			_gridLines[i].name = time.ToString("F2");
			SetPosition(_gridLines[i], pos);
		}
	}

    public void SetGridVisible(Boolean isVisible)
    {
        _showGridLines = isVisible;

        for (int i = 0; i < _gridLines.Count; i ++)
        {
            _gridLines[i].gameObject.SetActive(_showGridLines);
        }
    }

	public void ChangeGridBPM(float bpm)
	{
		DeleteGridObjects();
        _bpm = bpm;
		CreateGridObjects();

	}

	private void SetPosition(RectTransform rt, float pixelPosition)
	{
		rt.anchorMin = new Vector2(pixelPosition, 0.0f);
		rt.anchorMax = new Vector2(pixelPosition, 1.0f);
		rt.offsetMin = new Vector2(0, 0);
		rt.offsetMax = new Vector2(1.1f, 0);
		rt.anchoredPosition = new Vector2(0, 0);
		rt.localScale = Vector3.one;

	}

	public GameObject CreateGridBarObject(string objName)
	{

		//Create SongObject basic.
		GameObject go = Instantiate(TimeGridBarPrefab);
		go.name = string.Format(objName);
		go.tag = "GridBar";
		go.SetActive(true);
		go.transform.SetParent(BarsParent); // Don't know

		return go;
	}

	public void DeleteGridObjects()
	{
		_gridLines.Clear();
		foreach (GameObject g in GameObject.FindGameObjectsWithTag("GridBar"))
		{
			Destroy(g);
		}
	}

}
