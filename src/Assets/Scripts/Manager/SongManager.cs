using UnityEngine;
using System.Linq;
using System;

public class SongManager : MonoBehaviour
{

    #region Inspector

    public GameObject WallPrefab;    
    public Transform wallsParent;

    #endregion

    #region reference
    
    public DeserializedSong CurrentSong;
    
    #endregion

    #region Save Load YAML

    public void DesealizeSong(string filename)
    {
        CurrentSong = YamlImporter.Deserialize(FileManager.Path + filename + FileManager.ymlExtension);
        FileManager.SetFilename(filename);        
    }

    public bool CheckChanges()
    {
        string originalSong = FileManager.ReadYaml();

        string currentSong = generateYamlInfo();

        return originalSong.CompareTo(currentSong) != 0;
    }

    private string generateYamlInfo()
    {
        string songYaml =
            "%YAML 1.1\n" +
            "---\n\n" +
            "title: {0}\n" +
            "clip: {1}\n" +
            "speed: {2}\n" +
            "audioTime: {3}\n" +
            "scenary: {4}\n\n";

        songYaml += "video: {5}\n" +
            "vOffset: {6}\n\n";

        songYaml += "forceDebug: {7}\n" +
            "offset: {8}\n\n";

        songYaml += "author: {9}\n" +
            "difficulty: {10}\n" +
            "preview: {11}\n\n";

        songYaml += "grid: {12}\n" +
            "gridBpm: {13}\n" +
            "gridOffset: {14}\n\n\n";


        string songHead = string.Format(songYaml, CurrentSong.Title, CurrentSong.Clip.Replace(".ogg", ""), CurrentSong.Speed, CurrentSong.AudioTime, CurrentSong.Scenary,
            CurrentSong.Video, CurrentSong.VOffset,
            CurrentSong.ForceDebug, CurrentSong.Offset,
            CurrentSong.Author, CurrentSong.Difficulty, CurrentSong.Preview,
            CurrentSong.Grid, CurrentSong.GridBpm, CurrentSong.GridOffset);

        string songSeq =
            "levels:\n" +
            "  - level: 0\n" +
            "    sequence:\n\n";

        string songChunk =
            "      - second: {0:0.00}\n" +
            "        obj: {1}\n" +
            "        track: {2}\n";

        string songObjects = "";

        //Sort objects by gameobject name ( I named this with object time) .  
        var sortedList = GameObject.FindGameObjectsWithTag("Objects").OrderBy(go => go.name).ToList();

        foreach (GameObject g in sortedList)
        {
            var om = g.GetComponent<WallObject>();
            songObjects += string.Format(songChunk, om.Time, om.WallObjectId, 0);
        }

        songSeq += songObjects;
        string song = songHead + songSeq + "...\n";
        return song;
    }

    public void SaveSongYaml()
    {
        string song = generateYamlInfo();
        FileManager.SaveYaml(song);
    }

    public void LoadSongObjects()
    {
        OhShapeEditor editor = FindObjectOfType<OhShapeEditor>();

        //Delete previus objects.
        DeleteSongObjects();

        //objectTemplateParent = ObjectTemplate.transform.parent;
        var song = CurrentSong;

        //Get Levels list.
        if (song.Levels == null) return;

        var levelsList = new string[song.Levels.Count];
        var lvlCount = 0;
        foreach (var lvl in song.Levels)
        {
            levelsList[lvlCount++] = lvl.Level.ToString();
        }

        var currentLevel = 0;
        if (song.Levels[currentLevel].Sequence == null) return;
        //Get sequence list from currentLevel.
        var sequenceList = new GameObject[song.Levels[currentLevel].Sequence.Count];

        //Loop sequence of objects in selected level
        foreach (DeserializedSong.SongLevels.LevelSequence seq in song.Levels[currentLevel].Sequence)
        {
            string wallId = seq.Obj;
            //Error if objName is null.
            if (wallId == null) { Log.AddLine("No name in song object"); continue; }

            CreateWallObject(wallId.ToUpper(), seq.Second, false, editor);
        }
    }

    public WallObject CreateWallObject(string objName, float time, bool isOn, OhShapeEditor editor)
    {
        time = Mathf.Round(time * 100);
        time = time * 0.01f;

        //Create SongObject basic.
        GameObject go = Instantiate(WallPrefab);
        go.name = string.Format("{0:0000.00}", time);
        go.SetActive(true);
		go.transform.SetParent(wallsParent);

        WallObject wall = go.GetComponent<WallObject>();
        wall.Init(objName, time, isOn, editor);
        editor.addWallObjectToListOfObjects(wall);

        return wall;
    }

    public void SetWallMarkLinesVisible(Boolean isVisible)
    {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Objects"))
        {
            WallObject wall = g.GetComponent<WallObject>();
            wall.SetMarkLineVisible(isVisible);
        }
    }

    public void SetWallIdVisible(Boolean isVisible)
    {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Objects"))
        {
            WallObject wall = g.GetComponent<WallObject>();
            wall.SetIdVisible(isVisible);
        }
    }

    public void DeleteSongObjects()
    {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Objects"))
        {
            Destroy(g);
        }
    }
    
    #endregion

    #region Song edition

    public void SetVideoFile(string video)
    {
        CurrentSong.Video = video;
    }
    public void SetVideoOffset(string offset)
    {
        CurrentSong.VOffset = float.Parse(offset);
    }

    public void SetGridEnabled(Boolean isEnabled)
    {
        CurrentSong.Grid = isEnabled;
    }

    public void SetGridBPMValue(int bpm)
    {
        CurrentSong.GridBpm = bpm;
    }

    public void SetGridOffset(float offset)
    {
        CurrentSong.GridOffset = offset;
    }

    public void SetBPMData(Boolean isEnabled, int bpm, float offset)
    {
        SetGridEnabled(isEnabled);
        SetGridBPMValue(bpm);
        SetGridOffset(offset);
    }

    #endregion

}
