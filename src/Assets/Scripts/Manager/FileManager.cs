using System.IO;
using UnityEngine;
using System.Windows.Forms;
using System.Runtime.InteropServices;

//Create, load and save files.
public static class FileManager
{

    #region static references

    //Variables
    public static string Path; // const "Songs\\"
    public const string ymlExtension = ".yml";
    public const string videoExtension = ".mp4";
    public const string audioExtension = ".ogg";

    public static string CurrentFilename = "";
	public static string CurrentFilenameExtension = "";
	public static string CurrentAudioClipFilename = "";
	public static string CurrentAudioClipFilenameExtension = "";
	public static string CurrentVideoFilename = "";
	public static string CurrentVideoFilenameExtension = "";



    #endregion

    #region Create Save YAMLs 

    //New file
    public static bool CreateFile(string name)
    {
		
		if (!Directory.Exists(Path))
            Directory.CreateDirectory(Path);

        CurrentFilename = name;

        if (CurrentFilename.Length < 1)
        {
			DialogsWindowsManager.Instance.InfoMessage("Filename not valid.");
			return false;
        }

        var file = Path + CurrentFilename + ymlExtension;

        if (File.Exists(file))
        {
			DialogsWindowsManager.Instance.InfoMessage(name + "\nalready exists.");
			return false;
        }

        string songYaml =
         "%YAML 1.1\n" +
         "---\n" +
         "title: {0}\n" +
         "clip: {1}\n" +
         "speed: {2}\n" +
         "difficulty: easy\n" +
         "preview: 0\n";


        var sr = File.CreateText(file);
        sr.WriteLine(songYaml, name, "Load Clip", 20);
        sr.Close();
        return true;
    }

    public static void SaveYaml(string song)
    {
        //Backup
        var file = Path + CurrentFilename + ymlExtension;

        var streamWriter = File.CreateText(Path + CurrentFilename + ymlExtension);
        streamWriter.Write(song);
        streamWriter.Close();
    }

    public static string ReadYaml(string yamlFileName = "")
    {
        yamlFileName = yamlFileName == "" ? Path + CurrentFilename + ymlExtension : yamlFileName;
        return File.ReadAllText(yamlFileName);
    }

    public static void SaveYamlWithBackup(string song)
    {
        //Backup
        var file = Path + CurrentFilename + ymlExtension;
        var backInt = 0;


        var fileBackup = string.Format("{0}({1}).backup", file, backInt);

        while (File.Exists(fileBackup))
        {
            fileBackup = string.Format("{0}({1}).backup", file, backInt++);
        }
        var bk = false;
        try
        {
            //Do backup moving file (renaming)
            File.Move(file, fileBackup);
            bk = true;
        }
        catch
        {
        }
        var sr = File.CreateText(Path + CurrentFilename + ymlExtension);
        sr.WriteLine(song);
        sr.Close();

        if (bk)
        {
            var msg = string.Format("Saved in:\n{0}\nOld one is backuped in:\n {1}", file, fileBackup);
            DialogsWindowsManager.Instance.InfoMessage(msg);
        }

    }

    #endregion

    #region Read Create Directories

    //Read all .yml in directory
    public static string[] ReadDirectoryYML()
    {
        return ReadDirectoryExtension(ymlExtension);
    }

    // Read all .mp4 in directory
    public static string[] ReadDirectoryMP4()
    {
        return ReadDirectoryExtension(videoExtension);
    }

    private static string[] ReadDirectoryExtension(string extension)
    {
        DirectoryInfo dir = new DirectoryInfo(Path);
        FileInfo[] info = dir.GetFiles("*" + extension);
        string[] fileList = new string[info.Length];
        var i = 0;
        foreach (FileInfo f in info)
        {
            fileList[i++] = f.Name;
        }
        return fileList;
    }

    //Create folders in user PC, where we can read (or save in our editor) customs song.
    public static void CreateFolders()
    {
        //New path.
        var folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/OhShape/Songs";

        //Check if directory exist
        if (!Directory.Exists(folder))
        {
            Debug.LogError(folder + " no exist.");

            //Make paths variables.
            var ohShapeFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/OhShape";
            var ohShapeSongsFolder = ohShapeFolder + "/Songs";

            //In ohShapeFolder not exist, create it.
            if (!Directory.Exists(ohShapeFolder))
            {
                Debug.LogFormat("Creating {0} folder", ohShapeFolder);
                Directory.CreateDirectory(ohShapeFolder);
            }

            //In ohShapeSongsFolder not exist, create it.
            if (!Directory.Exists(ohShapeSongsFolder))
            {
                Debug.LogFormat("Creating {0} folder", ohShapeSongsFolder);
                Directory.CreateDirectory(ohShapeSongsFolder);
            }

            //If don't exist yet, some wrong think happened. :(
            if (!Directory.Exists(ohShapeFolder) || !Directory.Exists(ohShapeSongsFolder))
            {
                Debug.LogError("Error creating folders");
                return;
            }
        }

        //Set Path if both folders has been created.
        SetPath(folder);
    }


    //Read all .ogg in directory ( audio clips )
    public static string[] ReadDirectoryOgg()
    {
        DirectoryInfo dir = new DirectoryInfo(Path);
        FileInfo[] info = dir.GetFiles("*" + ".ogg");
        string[] fileList = new string[info.Length];
        var i = 0;
        foreach (FileInfo f in info)
        {
            fileList[i++] = System.IO.Path.GetFileName(f.Name);
        }
        return fileList;
    }

    #endregion

    #region Utils

    public static void SetFilename(string filename)
    {
		var extension = FileManager.GetFileNameExtension(filename);

		CurrentFilename = FileManager.GetFileNameWithoutExtension(filename);
		if (extension.Length > 0) {
			CurrentFilenameExtension = extension;
		}
    }

	public static string GetFileNameWithoutExtension(string name)
	{
		return System.IO.Path.GetFileNameWithoutExtension(name);
	}
	
	public static string GetFileNameExtension(string name)
	{
		return System.IO.Path.GetExtension(name);
	}

    private static void SetPath(string newPath)
    {
        if (!newPath.EndsWith("/"))
            Path = (newPath + "\\").Replace("\\", "/");

        Path = Path.Replace("\\", "/");
    }

    #endregion

    #region System File Explorer

    //We need copy System.Windows.Forms.dll from Unity_Location\Editor\Data\Mono\lib\mono\2.0 to a Plugins folder.
    //Wraper
    [DllImport("user32.dll")]
    private static extern void OpenFileDialog();
    [DllImport("user32.dll")]
    private static extern void SaveFileDialog();

    //Open File Dialog
    //[UnityEditor.MenuItem("OhShape/Test OpenFileDialog")]
	public static bool OpenDialog(string filter)
    {
        var state = false;

        OpenFileDialog openDialog = new OpenFileDialog();

        //Open in this directory
        openDialog.InitialDirectory = Path;
        openDialog.RestoreDirectory = true;
        //Extension filters
        openDialog.Filter = filter;

        //Open Dialog and store result.
        DialogResult result = openDialog.ShowDialog();

        //If result is OK
        if (result == DialogResult.OK)
        {
			var filePath = openDialog.FileName;
			SetPath(System.IO.Path.GetDirectoryName(filePath));
			var fileName = System.IO.Path.GetFileName(filePath);

			// TODO fix this
			if (filter.Contains(".ogg")) {
				FileManager.CurrentAudioClipFilename = fileName;
			} else if (filter.Contains(".mp4;*.MOV")) {
				FileManager.CurrentVideoFilename = fileName;
			} else {
				FileManager.SetFilename(filePath);
			}

            //Get the path of specified file
            Debug.Log("File Path:" + filePath);

            state = true;
        }

        return state;
    }

    //Save File Dialog.
    //[UnityEditor.MenuItem("OhShape/Test SaveFileDialog")]
    public static bool SaveDialog()
    {
        var state = false;

        SaveFileDialog saveDialog = new SaveFileDialog();

        // or overwrite the file if it does exist.
        saveDialog.CreatePrompt = true;//Create the file if it doesn't exist 
        saveDialog.OverwritePrompt = true;

        // Set the file name to SongName, set the type filter
        // to yml files, and set the initial directory to the 
        // MyDocuments folder.
        saveDialog.FileName = CurrentFilename;
        // DefaultExt is only used when "All files" is selected from 
        // the filter box and no extension is specified by the user.
        saveDialog.DefaultExt = "yml";
        saveDialog.Filter =
            "Text files (*.yml)|*.yml";
        saveDialog.InitialDirectory = Path;

        // Call ShowDialog and check for a return value of DialogResult.OK,
        // which indicates that the file was saved. 
        DialogResult result = saveDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            var filePath = saveDialog.FileName;

            SetPath(System.IO.Path.GetDirectoryName(filePath));
			FileManager.SetFilename(filePath);

            state = true;
        }
        return state;
    }

    #endregion
}

