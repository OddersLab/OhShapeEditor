using System.Text;

public class WallEdit
{
    public string Id { get; set; }
    public WallsUtils.Walltype WallType { get; set;  } // TODO Delete set when upgrade to C# 6+

    protected virtual void UpdateId() { }
    public virtual void UpdateId(string id) {
        Id = id.ToUpper();
    }

    protected virtual bool CheckId(string id) { return false; }

    public WallEdit(WallsUtils.Walltype wallType, string id)
    {
        Id = id;
        WallType = wallType;
    }

    public string ModifyStringCharacter(char character, int index)
    {
        StringBuilder str = new StringBuilder(Id);
        str[index] = character;
        return str.ToString();
    }
}
