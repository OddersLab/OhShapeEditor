using UnityEngine;
using UnityEngine.UI;

//Draw audio wave.
public class RenderSong : MonoBehaviour
{
    #region Inspector

    public Color WaveColorU = Color.red;
    public Color WaveColorD = Color.red;
	public Color WaveGrid	= Color.red;

    #endregion

    #region References

    private Color[] clearBrush = new Color[1];

    private float[] songData;
    private float[] levelDataMax;
    private float[] levelDataMin;

    private RectTransform _waveRectTransform;
    private OhShapeEditor _ohShapeEditor;

    #endregion

    #region Monobehaviour

    private void Awake()
    {
        _waveRectTransform = GetComponent<RectTransform>();
        _ohShapeEditor     = FindObjectOfType<OhShapeEditor>();
    }

    #endregion

    #region Render Wave Clip

    public void Render(Vector2 renderTimeRange)
    {
        var windowWidth = (int)_waveRectTransform.rect.width;
        var windowHeight = 100;        

        Log.AddLine("WWidth: " + windowWidth.ToString());
        //Log.AddLine("WTime: " + windowRenderTime.ToString("000.000"));

        if (!ClipInfo.Clip) return;

        Texture2D tex = NewPaintWaveformSpectrum(ClipInfo.Clip, 0.5f, windowWidth, windowHeight, renderTimeRange);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        GetComponent<Image>().overrideSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        GetComponent<Image>().color = Color.white;
    }

    public void ClearWaveImage()
    {
        GetComponent<Image>().overrideSprite = null;
        GetComponent<Image>().color = new Color(.2f, .2f, .2f);
    }

    public void MakeLevelBuffers(float zoom)
    {
        AudioClip audio = ClipInfo.Clip;
        if (audio == null) return;

        var windowWidth = (int)_waveRectTransform.rect.width;
        var totalSamples = audio.samples * audio.channels;

        var pixelRes = (totalSamples / windowWidth) / zoom;
        var bufferSize = (int)(totalSamples / pixelRes);

        songData = new float[totalSamples];
        levelDataMax = new float[bufferSize];
        levelDataMin = new float[bufferSize];

        audio.GetData(songData, 0);

        for (int p = 0; p < bufferSize; p++)
        {
            float min = 1000000;
            float max = -1000000;

            int index = (int)(pixelRes * p);

            for (int i = 0; i < (int)pixelRes; i++)
            {
                float data = songData[index + i];
                if (data > max)
                    max = data;
                if (data < min)
                    min = data;
            }

            levelDataMax[p] = max;
            levelDataMin[p] = min;

        }

    }

    #endregion

    #region Render Texture

    private Texture2D NewPaintWaveformSpectrum(AudioClip audio, float saturation, int width, int height, Vector2 time)
    {

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        //Erase texture
        ClearTexture(ref tex, width, height);
        int h = height / 2;

        var sampleStart = SecondToSample(audio, levelDataMax, time.x);
        //Draw wave
        for (int x = 0; x < width - 1; x++)
        {

            float max = 0f;
            float min = 0f;

            try
            {
                max = levelDataMax[Mathf.FloorToInt(x + sampleStart)];
                min = levelDataMin[Mathf.FloorToInt(x + sampleStart)];
            }
            catch
            {

            }

            var maxY = (int)(max * (float)height * saturation);
            var minY = (int)(min * (float)height * saturation);

            DrawLine(tex, x, h + maxY, x, h + minY, WaveColorD, WaveColorU);
        }

        tex.Apply();
        return tex;
    }

    private void ClearTexture(ref Texture2D tex, int width, int height)
    {
        if (clearBrush.Length != tex.GetPixels().Length)
        {
            var i2 = width * height;
            clearBrush = new Color[i2];
            for (int i = 0; i < i2; i++)
            {
                clearBrush[i] = Color.clear;
            }
        }

        tex.SetPixels(clearBrush);
    }

    #endregion

    #region Utils

    //Helper funtions
    private float SampleToSeconds(AudioClip audio, float[] samples, int sampleIndex)
    {
        var rate = ((samples.Length / audio.channels) / audio.length);
        return sampleIndex / rate;
    }

    private int SecondToSample(AudioClip audio, float[] samples, float second)
    {
        var rate = ((samples.Length / audio.channels) / audio.length);
        return (int)(second * rate * audio.channels);
    }

    private void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color colA, Color colB)
    {
        int dy = (int)(y1 - y0);
        int dx = (int)(x1 - x0);
        int stepx, stepy;

        if (dy < 0) { dy = -dy; stepy = -1; }
        else { stepy = 1; }
        if (dx < 0) { dx = -dx; stepx = -1; }
        else { stepx = 1; }
        dy <<= 1;
        dx <<= 1;

        float fraction = 0;
        tex.SetPixel(x0, y0, colA);

        if (dx > dy)
        {
            fraction = dy - (dx >> 1);
            while (Mathf.Abs(x0 - x1) > 1)
            {
                if (fraction >= 0)    
                {
                    y0 += stepy;
                    fraction -= dx;
                }
                x0 += stepx;
                fraction += dy;



                tex.SetPixel(x0, y0, colA);
            }
        }
        else
        {
            fraction = dx - (dy >> 1);
            while (Mathf.Abs(y0 - y1) > 1)
            {
                if (fraction >= 0)
                {
                    x0 += stepx;
                    fraction -= dy;
                }
                y0 += stepy;
                fraction += dx;

                var lerp1 = Mathf.Abs(fraction * 0.01f);
                var lerp2 = Mathf.Abs((y0 - y1) * 0.01f);
                var col = Color.Lerp(colA, colB, lerp2 * lerp1);

                tex.SetPixel(x0, y0, col);
            }
        }
    }

    #endregion
}