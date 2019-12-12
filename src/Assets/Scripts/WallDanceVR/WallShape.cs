using System.Text.RegularExpressions;

public class WallShape : WallEdit
{
    private char _wallPosition;
    private char _bodyPosition;
    private char _height;
    private char _leftHand;
    private char _rightHand;
    private char _legs;

    protected override void UpdateId()
    {
        Id = string.Format("WP.{0}{1}{2}{3}{4}{5}", _wallPosition, _leftHand, _rightHand, _bodyPosition, _height, _legs);
    }

    protected override bool CheckId(string id)
    {
        Regex regex = new Regex("WP\\.[RCL][0-B][0-B][RCL][UD][LCR]");
        return regex.IsMatch(id);
    }

    private void updateBodyParts()
    {
        _wallPosition = Id[3];
        _leftHand = Id[4];
        _rightHand = Id[5];
        _bodyPosition = Id[6];
        _height = Id[7];
        _legs = Id[8];
    }

    public WallShape() : base(WallsUtils.Walltype.WP, "WP.C00CUC0")
    {
        _wallPosition = 'C';
        _leftHand = '0';
        _rightHand = '0';
        _bodyPosition = 'C';
        _height = 'U';
        _legs = 'C';
        UpdateId();
    }

    public WallShape(string id) : base(WallsUtils.Walltype.WP, id)
    {
        Id = id;
        updateBodyParts();
    }

    public override void UpdateId(string id)
    {
        if (!CheckId(id)) return;
        Id = id;
        updateBodyParts();
    }

    public float getPosition()
    {
        switch (Id[3])
        {
            case 'L':
                return -1f;
            case 'C':
                return  0f;
            case 'R':
                return  1f;
            default:
                return 0f;
        }
    }

    public static int GetPositionOfCharToChange(string buttonName)
    {
        if (buttonName.Contains("ShapePosition"))
        {
            return 3;
        }
        else if (buttonName.Contains("LeftHand"))
        {
            return 4;
        }
        else if (buttonName.Contains("RightHand"))
        {
            return 5;
        }
        else if (buttonName.Contains("BodyPosition"))
        {
            return 6;
        }
        else if (buttonName.Contains("HeightPosition"))
        {
            return 7;
        }
        else if (buttonName.Contains("Legs"))
        {
            return 8;
        }

        return -1;
    }
}
