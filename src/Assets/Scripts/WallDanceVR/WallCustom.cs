using System.Text.RegularExpressions;

public class WallCustom : WallEdit
{
    private string _code;

    protected override void UpdateId()
    {
        Id = string.Format("WC.{0}", _code);
    }

    protected override bool CheckId(string id)
    {
        Regex regex = new Regex("WC\\.+");
        return regex.IsMatch(id);
    }

    private void updateCustomSettings()
    {
        string[] idSplitted = Id.Split('.');
        _code = idSplitted[1];
    }

    public WallCustom() : base(WallsUtils.Walltype.WC, "WA.GANG")
    {
        _code = "GANG";
        UpdateId();
    }

    public WallCustom(string id) : base(WallsUtils.Walltype.WC, id)
    {
        Id = id;
        updateCustomSettings();
    }

    public override void UpdateId(string id)
    {
        if (!CheckId(id)) return;
        Id = id;
        updateCustomSettings();
    }
}
