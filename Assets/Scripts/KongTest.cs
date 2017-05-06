using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KongTest : MonoBehaviour
{
    // for my game:
    // http://www.kongregate.com/games/MyName/MyGame/statistics
    // I have the following statistics (int, max value)
    string[] stats = { "BestLevel", "HighScore", "Stars" };
    string[] vals= { "0", "0", "0"};

	// Original Unity GUI, WHY? Because its so simple to build test code with it
	void OnGUI()
    {
        var kong = Kongregate.Instance;
        GUILayout.Label("IsKongregateReady: " + kong.IsKongregateReady);

        GUILayout.Label("User id: " + kong.UserId);
        GUILayout.Label("User name: " + kong.Username);

        for(int i=0;i<stats.Length;i++)
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(stats[i]);
            vals[i] = GUILayout.TextField(vals[i]);
            if (GUILayout.Button("Update"))
            {
                int val = int.Parse(vals[i]);
                Debug.LogFormat("SubmitStatistic {0} {1}", stats[i], val);
                kong.SubmitStatistic(stats[i], val);
            }
            GUILayout.EndHorizontal();
        }

    }
}
