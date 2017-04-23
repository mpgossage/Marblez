using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour {
	public Vector2 border=new Vector2(5,5);
	public Vector2 buttonSize=new Vector2(64,128);
	
	public OTSpriteSheet buttonSheet;
	public OTSpriteAtlas fontSheet;
    public AudioClip rolloverSound, selectSound;
	OTSprite[] buttons;
	OTSprite[] buttonNums;
    OTSprite[] buttonScore;
//	int highlightedButton=-1;	// the current highlighted button
    int rollover = -1;
	
	Color DISABLED=new Color(0.1f,0.1f,0.1f,1.0f);
	Color DIM=new Color(0.7f,0.7f,0.7f,1.0f);
	Color NORMAL=new Color(1,1,1,1);
	// Use this for initialization
	void Start () 
	{
		MakeLevelButtons();
	}
	
	void MakeLevelButtons()
	{
		buttons=new OTSprite[21];
		buttonNums=new OTSprite[21];
        buttonScore = new OTSprite[21];
		const int SIZE_X=80,SIZE_Y=100,BORDER=10,OFFSET_Y=100;
		for(int i=0;i<21;i++)
		{
			OTSprite spr=OT.CreateObject(OTObjectType.Sprite).GetComponent<OTSprite>();
			//spr.pivot=OTObject.Pivot.TopLeft;
			spr.spriteContainer=buttonSheet;
			spr.frameIndex=0;//i%4;//Random.Range(0,4);
			int x=i%7,y=i/7;
			Vector2 pos=Vector2.zero;
			pos.x=SIZE_X/2+x*(SIZE_X+BORDER)+BORDER;
			pos.y=SIZE_Y/2+y*(SIZE_Y+BORDER)+BORDER+OFFSET_Y;
			spr.transform.position=UnityToOT(pos);
			spr.size=new Vector2(SIZE_X,SIZE_Y);
			spr.transparent=true;
			// mouse events
			spr.registerInput=true;
			//spr.onMouseEnterOT=OTMouseEnter;
			//spr.onMouseExitOT=OTMouseExit;
			spr.depth=1000;	// back
			buttons[i]=spr;
			// now level numbers:
			OTTextSprite tspr=OT.CreateObject(OTObjectType.TextSprite).GetComponent<OTTextSprite>();
			tspr.spriteContainer=fontSheet;
			tspr.text=i.ToString();
			if (i==0)
			{
				tspr.text="Tutor";
				tspr.size=tspr.size*0.9f;
                tspr.visible = false;
                spr.visible = false;
			}
			tspr.transform.position=UnityToOT(pos); // pos is the same
			tspr.transparent=true;
			tspr.depth=800;	// fairly back
			//tspr.size=new Vector2(SIZE_X,SIZE_Y);
			buttonNums[i]=tspr;
            // now level score: (if its available & not the tutorial)
            if (i>0)
            {
                int score = GameState.Instance.Results[i - 1].score;
                if (GameState.Instance.IsLevelAvailable(i) && score>0)
                {
                    tspr = OT.CreateObject(OTObjectType.TextSprite).GetComponent<OTTextSprite>();
                    tspr.spriteContainer = fontSheet;
                    tspr.text = GameState.Instance.Results[i-1].score.ToString();
                    tspr.transform.position = UnityToOT(pos)+new Vector2(0,-35);
                    tspr.transparent = true;
                    tspr.depth = 800;	// fairly back
                    tspr.size = tspr.size * 0.8f;   // smaller
                    buttonScore[i] = tspr;
                }
            }
		}
		// read the results & set the data
		for(int i=0;i<20;i++)
		{
			int stars=GameState.Instance.Results[i].stars;
			buttons[i+1].frameIndex=stars; // note: its i+1 because of the tutorial button			
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		GameState gs=GameState.Instance;
		// check for mouseover
        int newrollover = -1;
		for(int i=0;i<buttons.Length;i++)
		{
			Color col=DIM;
            if (gs.IsLevelAvailable(i))	// check if level available (note its 1+ for levels)
            {
                if (OT.Over(buttons[i]))
                {
                    col = NORMAL;
                    newrollover = i;
                }
            }
            else
				col=DISABLED;
			buttonNums[i].tintColor=buttons[i].tintColor=col;
            if (buttonScore[i] != null) // tint those sprites which exist
                buttonScore[i].tintColor = col; 
		}
        // the rollover SFX
        if (newrollover != rollover && newrollover != -1)
            GetComponent<AudioSource>().PlayOneShot(rolloverSound);
        rollover = newrollover; // update
		// check for clicks:
		for(int i=0;i<buttons.Length;i++)
		{
			if (!gs.IsLevelAvailable(i))	// check if level available (note its 1+ for levels)
				continue;
			if (OT.Clicked(buttons[i].gameObject,0))	// left click
			{
                GetComponent<AudioSource>().PlayOneShot(selectSound);
                PlayLevel(i);
			}
		}
	}
    
    void PlayLevel(int level)
    {
        GameState.gameScore = GameState.levelScore = 0;
        GameState.levelNumber = level;
        // use the Transition class for a scene change
        Transition.FadeIn(GuiUtils.BlackTexture, 1);
        // in 1.5 second scene change
        Invoke("PlayTheLevel", 1.5f);
        // stop music in 1 second
        MusicManager.Instance.FadeOut();        
    }
    void PlayTheLevel()
    {
        Application.LoadLevel("marblezScene");
    }

	public static Vector2 UnityToOT(Vector2 pos)
	{
		return new Vector2(pos.x-320,240-pos.y);
	}
}
