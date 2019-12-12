using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class WallDodge : WallEdit
{
    private string _wallPosition;
    private int _duration;

    public int Duration
    {
        get
        {
            return _duration;
        }

        set
        {
            _duration = value;
            UpdateId();
        }
    }

    public string WallPosition
    {
        get
        {
            return _wallPosition;
        }

        set
        {
            _wallPosition = value;
            UpdateId();
        }
    }

    protected override void UpdateId()
    {
        // Id = string.Format($"WP.{_wallPosition}.{_duration}");
        Id = string.Format("WA.{0}.{1}", _wallPosition, _duration);
    }

    protected override bool CheckId(string id)
    {
        Regex regex = new Regex("WA\\.(R|RI|U|LI|L)\\.\\d+");
        return regex.IsMatch(id);
    }

    private void updateDodgeSettings()
    {
        string[] idSplitted = Id.Split('.');
        _wallPosition = idSplitted[1];
        _duration = Int32.Parse(idSplitted[2]);
    }

    public WallDodge() : base(WallsUtils.Walltype.WA, "WA.U.10")
    {
        _wallPosition = "U";
        _duration = 10;
        UpdateId();
    }

    public WallDodge(string id) : base(WallsUtils.Walltype.WA, id)
    {
        Id = id;
        updateDodgeSettings();
    }

    public override void UpdateId(string id)
    {
        if (!CheckId(id)) return;
        Id = id;
        updateDodgeSettings();
    }
}
