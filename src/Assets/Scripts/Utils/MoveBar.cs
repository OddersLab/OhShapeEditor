using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Move log scrollbar to bottom
public class MoveBar : MonoBehaviour {

	public ScrollRect sr;
	public void Move(ScrollRect scrollR = null)
	{
		//I need move it on next frame
		if (sr.IsActive()) StartCoroutine(MoveE());
	}
	IEnumerator MoveE()
	{
		yield return new WaitForEndOfFrame();
		sr.normalizedPosition = new Vector2(0, 0);
	}
}
