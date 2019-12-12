using System.Collections.Generic;
using System.Text.RegularExpressions;

public class WallHit : WallEdit
{
    private WallsUtils.Walltype _wallType = WallsUtils.Walltype.WH;
    private char _bodyPosition;
    private char _hands;
    private char _height;

    protected override void UpdateId()
    {
        // Id = string.Format($"WH.{_bodyPosition}{_hands}{_height}");
        Id = string.Format("{0}.{1}{2}{3}", _wallType, _bodyPosition, _hands, _height);
    }

    protected override bool CheckId(string id)
    {
        Regex regex = new Regex(_wallType + "\\.[RCL][RBL][UD]");
        return regex.IsMatch(id);
    }

    private void updateHitSettings()
    {
        _bodyPosition = Id[3];
        _hands = Id[4];
        _height = Id[5];
    }

    public WallHit() : base(WallsUtils.Walltype.WH, WallsUtils.Walltype.WH + ".CBU")
    {
        _bodyPosition = 'C';
        _hands = 'B';
        _height = 'U';
        UpdateId();
    }

    public WallHit(string id) : base(WallsUtils.Walltype.WH, id)
    {
        Id = id;
        updateHitSettings();
    }

    public override void UpdateId(string id)
    {
        if (!CheckId(id)) return;
        Id = id;
        updateHitSettings();
    }

    public static int GetPositionOfCharToChange(string buttonName) {
        if (buttonName.Contains("WallPosition"))
        {
            return 3;
        }
        else if (buttonName.Contains("Hand"))
        {
            return 4;
        }
        else if (buttonName.Contains("HeightPosition"))
        {
            return 5;
        }

        return -1;
    }

    public static List<int> GetValidHits(string id) // TODO set in hit class
    {
        List<int> validHits;
        switch (id[3])
        {
            case 'L':
                validHits = new List<int>() { 0, 1, 6, 7 };
                break;

            case 'C':
                validHits = new List<int>() { 2, 3, 8, 9 };
                break;

            case 'R':
                validHits = new List<int>() { 4, 5, 10, 11 };
                break;

            default:
                validHits = new List<int>() { };
                break;
        }

        validHits.RemoveAll(x => (id[4] == 'L' && x % 2 == 1) || (id[4] == 'R' && x % 2 == 0));
        validHits.RemoveAll(x => (id[5] == 'U' && x >= 6) || (id[5] == 'D' && x < 6));

        return validHits;
    }
}
