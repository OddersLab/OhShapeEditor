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
    private Image _waveImage;

    #endregion

    #region Monobehaviour

    private void Awake()
    {
        _waveRectTransform = GetComponent<RectTransform>();
        _ohShapeEditor = FindObjectOfType<OhShapeEditor>();
        _waveImage = GetComponent<Image>();
    }

    #endregion

    #region Render Wave Clip

    public void Render(Vector2 renderTimeRange)
    {
        var windowWidth = (int)_waveRectTransform.rect.width;
        var windowHeight = 100;        

        //Log.AddLine("WWidth: " + windowWidth.ToString());
        //Log.AddLine("WTime: " + windowRenderTime.ToString("000.000"));

        if (!ClipInfo.Clip) return;

        Texture2D tex = NewPaintWaveformSpectrum(ClipInfo.Clip, 0.5f, windowWidth, windowHeight, renderTimeRange);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        _waveImage.overrideSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        _waveImage.color = Color.white;
    }

    public void ClearWaveImage()
    {
        _waveImage.overrideSprite = null;
        _waveImage.color = new Color(.2f, .2f, .2f);
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
        ClearTexture(ref tex, width, height);

        int h = height / 2;
        var sampleStart = SecondToSample(audio, levelDataMax, time.x);

        float max = 0f, min = 0f;
        
        for (int x = 0; x < width - 1; x++)
        {
            int index = Mathf.Clamp(Mathf.FloorToInt(x + sampleStart), 0, levelDataMax.Length - 1);
            max = levelDataMax[index];
            min = levelDataMin[index];

            int maxY = Mathf.FloorToInt(max * height * saturation);
            int minY = Mathf.FloorToInt(min * height * saturation);

            DrawLine(tex, x, h + maxY, x, h + minY, WaveColorD, WaveColorU);
        }

        tex.Apply();
        return tex;
    }

    private void ClearTexture(ref Texture2D tex, int width, int height)
    {
        int totalPixels = width * height;
    
        if (clearBrush == null || clearBrush.Length != totalPixels)
        {
            clearBrush = new Color[totalPixels];
            for (int i = 0; i < totalPixels; i++)
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
        var rate = samples.Length / audio.length;
        return (int)(second * rate);
    }

    private void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color colA, Color colB)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (x0 != x1 || y0 != y1)
        {
            float lerp1 = Mathf.Abs(err) / Mathf.Sqrt(dx * dx + dy * dy);
            float lerp2 = Mathf.Abs(dx - err) / Mathf.Sqrt(dx * dx + dy * dy);
            Color col = Color.Lerp(colA, colB, lerp1 * lerp2);
            tex.SetPixel(x0, y0, col);

            int err2 = 2 * err;
            if (err2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (err2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    #endregion
}