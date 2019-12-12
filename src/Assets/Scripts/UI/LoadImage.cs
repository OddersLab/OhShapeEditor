using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class LoadImage : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var file = "CheatSheet.png";
		if (File.Exists(file))
		{
			byte[] byteArray = File.ReadAllBytes(file);
			Texture2D sampleTexture = new Texture2D(2, 2);
			bool isLoaded = sampleTexture.LoadImage(byteArray);

			if (isLoaded)
			{
				Image i = GetComponent<Image>();
				i.color = Color.white;
				i.preserveAspect = true;
				Sprite sprite = Sprite.Create(sampleTexture, new Rect(0, 0, sampleTexture.width, sampleTexture.height), new Vector2(0.5f, 0.5f));

				i.sprite = sprite;
			}
		}
	}

}
