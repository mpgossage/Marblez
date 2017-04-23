using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MapLoader 
{
    public static int[,] Tiles { get { return tiles; } }
    public static int MaxBalls { get { return maxBall; } }
    public static int BallRequirement { get { return ballReq; } }
    public static int TimeLimit { get { return timeLimit; } }

    static int[,] tiles = new int[10, 7];
    static int maxBall,ballReq, timeLimit;

    public static void LoadMap(string filename)
    {
        //Debug.Log(UFileLoader.ReadAllText(filename));
        List<string[]> csv = UFileLoader.LoadCsvFile(filename);
        // error check:
        //Debug.Log(string.Format("CSV is {0} x {1} (or so)", csv.Count, csv[0].Length));
        // not bother to error check
        for (int y = 0; y < 7; y++)
        {
            //Debug.Log("Y=" + y + " " + csv[y]);
            for (int x = 0; x < 10; x++)
            {
                // string=>int
                // -1 is because mappy saves indexes 1+ rather than 0+
                tiles[x, y] = int.Parse(csv[y][x]);
            }
        }

        // default values
        timeLimit = 3 * 60;
        ballReq = 5;
        maxBall = 2;

        // load all the level information
        // we could cache this, but not bothering
        csv = UFileLoader.LoadCsvFile("allLevels");
        // try to lookup the name
        //Debug.Log("looking for a record for:" + filename);
        for (int i = 0; i < csv.Count; i++)
        {
            if (filename == csv[i][0].Trim()) // trim & check
            {
                //Debug.Log("found it");
                maxBall = int.Parse(csv[i][1]);
                ballReq = int.Parse(csv[i][2]);
                timeLimit = int.Parse(csv[i][3]);
            }
        }
    }

}
