using System;
using UnityEngine;

public class Coin : WallEdit
{
    private static int MinX = -10;
    private static int MaxX = 10;
    private static int MinY = 0;
    private static int MaxY = 13;

    private int _x;
    private int _y;
    private Vector2 pos;

    public int X
    {
        get
        {
            return _x;
        }

        set
        {
            _x = value;
            UpdateId();
        }
    }

    public int Y
    {
        get
        {
            return _y;
        }

        set
        {
            _y = value;
            UpdateId();
        }
    }

    protected override void UpdateId()
    {
        // Id = string.Format($"WP.{_wallPosition}.{_duration}");
        Id = string.Format("CN.{0}.{1}", _x, _y);
    }

    private bool checkId(string id)
    {
        string[] idSplitted = id.Split('.');
        if (idSplitted.Length != 3) return false;
        int checkX;
        int checkY;
        if (!Int32.TryParse(idSplitted[1], out checkX)) return false;
        if (!Int32.TryParse(idSplitted[2], out checkY)) return false;

        return true;
    }

    private void updateCoinPosition()
    {
        string[] idSplitted = Id.Split('.');
        _x = Int32.Parse(idSplitted[1]);
        _y = Int32.Parse(idSplitted[2]);
    }

    public Coin() : base(WallsUtils.Walltype.CN, "CN.0.5")
    {
        _x = 0;
        _y = 5;
        UpdateId();
    }

    public Coin(string id) : base(WallsUtils.Walltype.CN, id)
    {
        if (!checkId(id)) return;
        Id = id;
        updateCoinPosition();
    }

    public override void UpdateId(string id)
    {
        if (!checkId(id)) return;
        Id = id;
        updateCoinPosition();
    }


    #region Coin Math Calculus
    private static int getCoinCoordinate1D(float coin1D, float size, int max, int min)
    {
        return (int)((Mathf.Abs(min) + Mathf.Abs(max)) * coin1D / size + ((float)(min + max) / 2));
    }

    private static float getWorldPoint1D(float point1D, float containerSize, int min, int max)
    {
        return containerSize * (2 * point1D - (min + max)) / (2 * (Mathf.Abs(min) + Mathf.Abs(max)));
    }

    private static float getPositionInsideElement1D(float position, float limit)
    {
        if (Mathf.Abs(position) >= limit)
        {
            return position < 0 ? -limit : limit;
        }
        return position;
    }

    public static Vector2 GetCoinPosition(Vector2 worldPosition, Rect container, Rect coin)
    {
        float x = getPositionInsideElement1D(worldPosition.x, container.width / 2 - coin.width / 2);
        float y = getPositionInsideElement1D(worldPosition.y, container.height / 2 - coin.height / 2);
        return new Vector2(x, y);
    }

    public static Vector2 GetCoinInWorldPosition(Vector2 coinPosition, Rect container)
    {
        float x = getWorldPoint1D(coinPosition.x, container.width, MinX, MaxX);
        float y = getWorldPoint1D(coinPosition.y, container.height, MinY, MaxY);
        return new Vector2(x, y);
    }

    public static Vector2 GetCoinCoordinates(Vector2 coinPositionRelativeToBackground, Vector2 containerSize)
    {
        // int x = (int)((Mathf.Abs(MinX) + Mathf.Abs(MaxX)) * coinPositionRelativeToBackground.x / container.width + ((float)(MinX + MaxX) / 2));
        // int y = (int)((Mathf.Abs(MinY) + Mathf.Abs(MaxY)) * coinPositionRelativeToBackground.y / container.height + ((float)(MinY + MaxY) / 2));
        int x = getCoinCoordinate1D(coinPositionRelativeToBackground.x, containerSize.x, MinX, MaxX);
        int y = getCoinCoordinate1D(coinPositionRelativeToBackground.y, containerSize.y, MinY, MaxY);
        return new Vector2(x, y);
    }
    #endregion
}
