using UnityEngine;
using UnityEngine.UI;

//Show text in a log window for help development.
public static class Log {

	private static Text logText;
	private static MoveBar moveBar;

	public static void AddLine(string newLine)
	{
		// Debug.Log(newLine);
	
		// if (!logText) logText = GameObject.Find("LogText").GetComponent<Text>();
		// if (!moveBar) moveBar = GameObject.Find("ScrollViewLog").GetComponent<MoveBar>();

		/*logText.text += "\nlog:> " + newLine;
		if (moveBar) moveBar.Move();
		if (logText.text.Length > 3000) logText.text = "";*/
	}

}
