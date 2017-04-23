using UnityEngine;
using System.Collections;
using System.Text;

/* Game state holds the current state of play.
 * It works as a singleton & should never be attached to any item
 */
public class GameState :MonoBehaviour
{
#region Statics for data sharing
	
	public static int levelScore=0;
	public static int levelTime=0;	// time left (at end of a level)
	public static int gameScore=0;
	//public static string scene="demo";
	public static int levelNumber=1;	// this level the user is doing/intends to do
#endregion
	
	public class LevelResult
	{
		public int score=0;
		public int stars=0;
	}
	public LevelResult[] Results=new LevelResult[20];
	
	// sets level (1+) results & saves
	public void SetLevelResult(int level,int score,int stars)
	{
		LevelResult lr=Results[level-1]; // -1 to offset for id
		if (lr.score<score)	lr.score=score;
		if (lr.stars<stars)	lr.stars=stars;
		//SaveState();
		SyncWithKong();	// save level details in Kong
	}
	// returns if a given level is available
	public bool IsLevelAvailable(int level)
	{
		if (level<=0)
			return false;	// no tutorial yet
		if (level==1)
			return true;	// level 1 is always avaialable
		LevelResult lr=Results[level-2]; // -2 is -1 for level to index & -1 for previous
		return lr.stars>0;	// if any stars
	}
    // returns total number of stars collected
    public int TotalStars()
    {
        int total = 0;
        for (int i = 0; i < 20; i++)
        {
            total += GameState.Instance.Results[i].stars;
        }
        return total;
    }
	public int BestLevel()
	{
        for (int i = 0; i < 20; i++)
			if (Results[i].stars==0)	// did not score on this level
				return i;
		return 20;	// finished all
	}



	#region Load/Save/Reset Code
	public void LoadState()
	{
		bool loadOK=false;
		if (PlayerPrefs.HasKey("Save1"))
		{
			Debug.Log("Load Game State");
			string str=PlayerPrefs.GetString("Save1");
			string[] a=str.Split(',');
			if (a.Length==40)
			{
				for(int i=0;i<a.Length;i+=2)
				{
					Results[i/2]=new LevelResult();
					Results[i/2].score=int.Parse(a[i]);
					Results[i/2].stars=int.Parse(a[i+1]);
				}
				loadOK=true;
			}
		}
		if (!loadOK)
		{
			Debug.Log("Unable Load Game State");		
			ResetState();
		}
		if (loadOK)
		{
			SyncWithKong();	// should sync at start just in case some data was lost
		}
	}
	public void SaveState()
	{
		Debug.Log("Save Game State");
		StringBuilder builder=new StringBuilder();
		builder.AppendFormat("{0},{1}",Results[0].score,Results[0].stars);
		for(int i=1;i<Results.Length;i++)
		{
			builder.AppendFormat(",{0},{1}",Results[i].score,Results[i].stars);
		}
		string str=builder.ToString();
		Debug.Log("Saving: "+str);
		PlayerPrefs.SetString("Save1",str);
		PlayerPrefs.Save(); // actually saves to disk
		//builder.
	}
	// clears the save, resets all data
	// called by LoadState() if it fails
	public void ResetState()
	{
		Debug.Log("Reset Game State");
		for(int i=0;i<Results.Length;i++)
		{
			Results[i]=new LevelResult();
            Results[i].score = 0;//Random.Range(0,1000);
            Results[i].stars = 0;//Random.Range(0,4);
        }		
	}
	
	public void SyncWithKong()
	{
		// update Kong with details
		Kongregate kong=Kongregate.Instance;
		kong.SubmitStatistic("BestLevel",BestLevel());
		kong.SubmitStatistic("Stars",TotalStars());
		kong.SubmitStatistic("HighScore",gameScore);
	}
	#endregion	
	#region Singleton Code
	private static GameState instance;
	public void Awake()
	{
		//if (instance!=null)
		//	Debug.LogError("Someone created too many Singletons");
		//LoadState();
	}
	public static GameState Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject obj=new GameObject("GameState");
				instance = obj.AddComponent<GameState>();
				GameObject.DontDestroyOnLoad(instance.gameObject);	// don't get destroyed in a level loading
                instance.LoadState();
			}
			return instance;
		}
	}

	public void OnApplicationQuit ()
	{
		SaveState();
		Destroy(instance.gameObject);
		instance = null;
	}
	#endregion	
}
