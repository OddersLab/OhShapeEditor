using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

//Store config values
public class DeserializedConfig
{
    public bool   Debug  { get; set; }
    public string Lang   { get; set; }

	public float SensitivityHeadPosition { get; set; }
	public float SensitivityHeadAngle    { get; set; }
	public float SensitivityHandPosition { get; set; }
	public float HandRadiusX             { get; set; }
	public float HandRadiusY             { get; set; }
	public float SensitivityHandAngle    { get; set; }
	public float SensitivityHit          { get; set; }
	public float Success				 { get; set; }
	public float Failure				 { get; set; }
	public float PerfectRadius			 { get; set; }

}

//Store all deserialized datas
public class DeserializedSong
{
	//Song info
	public string Title      { get; set; }
	public string Clip       { get; set; }
    public string Icon       { get; set; }
    public string Source     { get; set; }
    public int    Speed      { get; set; }
	public float  Offset     { get; set; }

	public string Author     { get; set; }
	public string Difficulty { get; set; }
	public float  Preview    { get; set; }
	public string Genere     { get; set; }
    public float AudioTime { get; set; }
    public bool Grid { get; set; }
    public int GridBpm { get; set; }
    public float GridOffset { get; set; }


	public bool ForceDebug { get; set; }

	//Video
	public string Video { get; set; }
	public float  VOffset { get; set; }

	//Song Objects
	public List<SongObjects> Objects { get; set; }

	//Poses
	public List<MainPoses> Poses  { get; set; }
	public float OffsetHorizontal { get; set; }
	public float OffsetVertical   { get; set; }

	//Poses
	public struct MainPoses
	{
		public string Pose { get; set; }

		public float X { get; set; }
		public float Y { get; set; }
		public float Ang { get; set; }
	}

	public class SongObjects
    {
        public string Id	{ get; set; }
        public string Type  { get; set; }

		public struct Vector3D
		{
			public float X { get; set; }
			public float Y { get; set; }
			public float Z { get; set; }
		}

		//Walls
		public Vector3D Head      { get; set; }
        public Vector3D LeftHand  { get; set; }
        public Vector3D RightHand { get; set; }

		//Walls Half
		public Vector3D Position { get; set; }
		public Vector3D Size     { get; set; }
		public float    Life     { get; set; }

	}
	


    //Song Levels
    public List<SongLevels> Levels { get; set; }
    public class SongLevels
    {
        public int Level { get; set; }
        public List<LevelSequence> Sequence { get; set; }
        public struct LevelSequence
        {
            public float  Second   { get; set; }
            public string Obj	   { get; set; }
            public int	  Position { get; set; }
            public int	  Track	   { get; set; }
			public float  Length   { get; set; }
        }
    }
}

//Load a yaml file into a DeserializedSong class and retunr it.
public class YamlImporter
{

	public static DeserializedSong Deserialize(string yamlFileName)
    {
        string textFile = FileManager.ReadYaml(yamlFileName);
        var input = new StringReader(textFile);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(new CamelCaseNamingConvention())
            .IgnoreUnmatchedProperties()
            .Build();
        DeserializedSong deserializeObject = deserializer.Deserialize<DeserializedSong>(input);
        return deserializeObject;
    }


    public static DeserializedConfig DeserializeConfig(string input)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(new CamelCaseNamingConvention())
            .IgnoreUnmatchedProperties()
            .Build();
        DeserializedConfig deserializeObject = deserializer.Deserialize<DeserializedConfig>(input);
        return deserializeObject;
    }

	public static DeserializedSong DeserializeObjects(string input)
	{
		var deserializer = new DeserializerBuilder()
			.WithNamingConvention(new CamelCaseNamingConvention())
			.IgnoreUnmatchedProperties()
			.Build();
		DeserializedSong deserializeObject = deserializer.Deserialize<DeserializedSong>(input);
		return deserializeObject;
	}
}


