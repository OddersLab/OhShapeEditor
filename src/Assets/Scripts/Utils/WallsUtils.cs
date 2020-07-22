using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class WallsUtils
{
    public enum Walltype { NONE, WP, WA, WH, WC, CN, EV }

    public static List<string> WallsPoseIds;
    public static List<string> WallsCustomsIds;
    public static List<string> WallsHitsIds;
    public static List<string> WallsAvoidIds;

    static WallsUtils()
    {
        PopulateWallsIds();
    }


    public static bool CheckWallId(Walltype wallType, string id)
    {
        bool validWall = false;

        switch (wallType)
        {
            case Walltype.WP:
                validWall = WallsPoseIds.Contains(id);
                break;
            case Walltype.WC:
                validWall = WallsCustomsIds.Contains(id);
                break;
            case Walltype.WH:
                validWall = WallsHitsIds.Contains(id);
                break;
        }

        return validWall;
    }

	public static Walltype GetWallType(string id)
	{
		Regex regex = new Regex("(WP|WA|WH|WC|CN)\\..*");

		if (!regex.IsMatch (id)) {
			return Walltype.NONE;
		}
	
		return (Walltype)Enum.Parse(typeof(Walltype), id.Substring(0, 2));
	}

    public static string FlipWallObject(string id)
    {
        string[] splitedName = id.ToUpper().Split('.');
        string flippedID = "";

        flippedID = id;  // sets default (original panel) ... for cases WC, EV 

        try
        {
            WallsUtils.Walltype wallType = (WallsUtils.Walltype)Enum.Parse(typeof(WallsUtils.Walltype), splitedName[0]);

            string wallId = splitedName[1];
            switch (wallType)
            {
                case WallsUtils.Walltype.WP:
                    wallId = flipObject(wallId);

                    char[] wallIdChars = wallId.ToCharArray();

                    char temporalChar = wallIdChars[1];
                    wallIdChars[1] = wallIdChars[2];
                    wallIdChars[2] = temporalChar;

                    wallId = new string(wallIdChars);
                    flippedID = "WP." + wallId;

                    break;

                case WallsUtils.Walltype.WA:
                case WallsUtils.Walltype.WH:
                    flippedID = flipObject(id);

                    break;

                case WallsUtils.Walltype.CN:

                    int invertX = -(Int32.Parse(splitedName[1]));

                    flippedID = splitedName[0] + "." + invertX.ToString() + "." + splitedName[2];
                    break;

                case WallsUtils.Walltype.WC:
                case WallsUtils.Walltype.EV:
                    break;
            }
        }
        catch (Exception e)
        {
            Log.AddLine("Bad Wall Id -> " + id + " Error : " + e.StackTrace);
            Debug.Log("try catch caught");
        }

        // Method assumes the mirror of a valid panel should be valid, but if one wants to recheck
        // validity, this would validate and conditionally return to default shape.

        // if (!WallsUtils.CheckWallId(wallType, strPanel)) wallObject.WallObjectId = savedPanelCode;

        return flippedID;
    }

    private static string flipObject(string id)
    {
        id = id.Replace('R', '*');
        id = id.Replace('L', '$');

        id = id.Replace('*', 'L');
        id = id.Replace('$', 'R');

        return id;
    }

    public static bool CheckCoinWall(string x, string y)
    {
        int xMin = -10;
        int xMax = 10;
        int yMin = 0;
        int yMax = 13;

        int xPos = int.TryParse(x, out xPos) ? xPos : xMax + 1;
        int yPos = int.TryParse(y, out yPos) ? yPos : yMax + 1;

        return (xPos >= xMin) && (xPos <= xMax) && (yPos >= yMin) && (yPos <= yMax);
    }

    public static bool CheckAvoidWall(string id, string time)
    {
        float lenght = float.TryParse(time, out lenght) ? lenght : 0;
        return WallsAvoidIds.Contains(id) && lenght >= 2 && lenght <= 1000;
    }

    public static Sprite getWallSprite(string id) // TODO CamelCase
    {
        WallsUtils.Walltype wallType = WallsUtils.GetWallType(id);

        switch (wallType)
        {
            case WallsUtils.Walltype.WP:
                Sprite wallSprite;
                wallSprite = id.Length != 9 ? null : Resources.Load<Sprite>("PhotoWalls/" + id.Substring(4));

                return wallSprite;
            case WallsUtils.Walltype.WA:
                string[] splittedId = id.Split('.');
                if (splittedId.Length <= 1) return null;
                return Resources.Load<Sprite>("assets/wall/dodge/" + splittedId[1]);

            default:
                return null;
        }
    }

    // https://stackoverflow.com/questions/32571057/generate-all-combinations-from-multiple-n-lists
    private static void PopulateWallsIds()
    {
        string[] offSetTorso = { "C", "R", "L" };
        string[] handPose = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B" };
        string[] height = { "U", "D" };
        string[] legs = { "L", "C", "R" };
        string[] hitHands = { "L", "R", "B" };
        string[] avoidWalls = { "U","L", "R", "RI", "LI" };

        Dictionary<int, List<string>> wallsPoseCombinations = new Dictionary<int, List<string>>();
        wallsPoseCombinations.Add(1, new List<string>(offSetTorso));
        wallsPoseCombinations.Add(2, new List<string>(handPose));
        wallsPoseCombinations.Add(3, new List<string>(handPose));
        wallsPoseCombinations.Add(4, new List<string>(offSetTorso));
        wallsPoseCombinations.Add(5, new List<string>(height));
        wallsPoseCombinations.Add(6, new List<string>(legs));

        Dictionary<int, List<string>> wallsHitCombinations = new Dictionary<int, List<string>>();
        wallsHitCombinations.Add(1, new List<string>(offSetTorso));
        wallsHitCombinations.Add(2, new List<string>(hitHands));
        wallsHitCombinations.Add(3, new List<string>(height));

        WallsPoseIds = GetCombos(wallsPoseCombinations);
        WallsHitsIds = GetCombos(wallsHitCombinations);
        WallsAvoidIds = new List<string>(avoidWalls);
        WallsCustomsIds = new List<string>();

        // Special Walls
        WallsPoseIds.Add("28RDO");
        WallsPoseIds.Add("82LDO");

        CleanInvalidWalls(ref WallsPoseIds);
    }

    private static void CleanInvalidWalls(ref List<string> wallsIds)
    {
        wallsIds = wallsIds.Where(wall =>
            ((wall[1] != '1' && wall[2] != '1') || wall[4] != 'D')
            && ((wall[1] != '6' && wall[2] != '6') || wall[1] == wall[2])
            && !((wall[4] == 'R' || wall[4] == 'L') && wall[3] == 'D')
        ).ToList();
    }

    private static List<string> GetCombos(IEnumerable<KeyValuePair<int, List<string>>> remainingTags)
    {
        if (remainingTags.Count() == 1)
        {
            return remainingTags.First().Value;
        }
        else
        {
            var current = remainingTags.First();
            List<string> outputs = new List<string>();
            List<string> combos = GetCombos(remainingTags.Where(tag => tag.Key != current.Key));

            foreach (var tagPart in current.Value)
            {
                foreach (var combo in combos)
                {
                    outputs.Add(tagPart + combo);
                }
            }

            return outputs;
        }
    }
}