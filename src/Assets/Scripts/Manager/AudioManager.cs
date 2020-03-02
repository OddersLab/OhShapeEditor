using System.IO;
using UnityEngine;
using System.Collections;
using System;

#region Clip Info

public static class ClipInfo
{
    public static AudioClip Clip;
    public static float ClipTimeSize;

    //Get a time of song, and return x position in screen.
    public static float SecToPixel(float sec)
    {
        var x = sec / ClipTimeSize;
        return x;
    }
}

#endregion

//Store and manage audio clips and audio source.
public class AudioManager : MonoBehaviour
{
    #region references

    private AudioClip _clip;    
    private AudioSource _source;
    private SongManager _songManager;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _songManager = GetComponent<SongManager>();
    }

    #endregion

    #region Load Clip
    
    public IEnumerator LoadClipRoutine(string name)
    {
        if (File.Exists(name))
        {
            name = "file:///" + name;
            Log.AddLine(string.Format("Loading clip:{0}", name));
            using (var www = new WWW(name))
            {
                //Wait to be loaded.
                yield return www;
                _clip = www.GetAudioClip();
                _source.clip = _clip;

                ClipInfo.ClipTimeSize = _clip.length;
                ClipInfo.Clip = _clip;

                Log.AddLine("Succefull loaded.");
#if UNITY_EDITOR
                ShowSongData();
#endif
            }
        }
        else
        {
            Log.AddLine(string.Format("File {0} no exist.", name));                       
            ClipInfo.Clip = _source.clip = null;
            ClipInfo.ClipTimeSize = 0;
            DialogsWindowsManager.Instance.InfoMessage("Audio Clip Not Exists");
        }
    }

    #endregion

    #region AudioSource

    public void UpdateVolume(float value)
    {
        _source.volume = value;
    }

    public void PlayClipAtTime(float time = 0)
    {
        _source.Stop();
        _source.time = time;
        _source.Play();

    }
    public void UpdatePlayTime(float time)
    {
        if (_source.isPlaying)
            _source.time = time;
    }
    public void StopClip()
    {
        _source.Stop();
    }
	public void PauseClip()
	{
		_source.Pause();
	}

    public bool IsPlaying()
    {
        return _source.isPlaying;
    }
    public float GetCurrentClipTime()
    {
        return _source.time;
    }

    #endregion

    #region Debug

    //Show some audio clip info.
    public void ShowSongData()
    {
        float[] samples = new float[_clip.samples * _clip.channels];
        _clip.GetData(samples, 0);

        string data = string.Format("Duration: {0} seg\nSamples: {1}\nRate: {2}\n",
        _clip.length,
        samples.Length / _clip.channels,
        (samples.Length / _clip.channels) / _clip.length);

        Log.AddLine(data);
    }

    #endregion

}