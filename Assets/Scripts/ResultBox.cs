using UnityEngine;
using System.Collections;

public class ResultBox : MonoBehaviour 
{
	public OTSpriteAtlas font;
	public Texture2D panelTex;
	public Texture2D starTex;
    public AudioClip[] sounds;
	
	OTSprite blankSprite; 
	OTSprite panelSprite;
	OTTextSprite titleText;
	OTSprite[] starSprites;
	OTTextSprite detailText;
    OTTextSprite continueText;

    int stars = 0;  // how many stars awarded
    bool waiting = false;
	
	// Use this for initialization
	void Start () 
	{
        // stop music:
        MusicManager.Instance.FadeOut();
        DetermineResult();
        CreateSprites();
	    // setup the tweens:
        // all must fade in from transparent
        blankSprite.alpha = 0;
		OTTween t=new OTTween(blankSprite,1).Tween("alpha",0.8f); // not fully black
        panelSprite.alpha = 0;
        new OTTween(panelSprite, 1).Tween("alpha", 1);
        titleText.alpha = 0;
        new OTTween(titleText, 1).Tween("alpha", 1);
        detailText.alpha = 0;
        new OTTween(detailText, 1).Tween("alpha", 1);
        // the stars will grow to full size, one at a time
        Vector2 starSize = new Vector2(50, 50);
        starSprites[0].size = starSprites[1].size = starSprites[2].size = Vector2.zero;
        // tween in stars if needed
        if (stars >= 1)
        {
            t = new OTTween(starSprites[0], 1, OTEasing.BounceOut).Tween("size", starSize).Wait(1);
            Invoke("Sound1", 1);
        }
        if (stars>=2)
        {
            t=new OTTween(starSprites[2], 1, OTEasing.BounceOut).Tween("size", starSize).Wait(2);
            Invoke("Sound2", 2);
        }
        if (stars>=3)
        {
            t=new OTTween(starSprites[1], 1, OTEasing.BounceOut).Tween("size", starSize).Wait(3);
            Invoke("Sound3", 3);
        }

        // wait for the animation to stop before acting
        t.onTweenFinish = new OTTween.TweenDelegate(v => { waiting = true; new OTTween(continueText, 0.5f).Tween("alpha", 1); });
    }

    void DetermineResult()
    {
        stars=0;
        if (GameState.levelTime>0) 
        {
            // bonus of 10 score/second left
            GameState.levelScore += GameState.levelTime * 10;
            // level complete: determine star rating
            stars=1;
		    if (GameState.levelTime>=30)
			    stars=2;
		    if (GameState.levelTime>=60)
			    stars=3;
        }
        // update game score for Kong save
        GameState.gameScore += GameState.levelScore;
        // save scores
        GameState.Instance.SetLevelResult(GameState.levelNumber, GameState.levelScore, stars);
    }
    void CreateSprites()
    {
        // blanking BG
        blankSprite = OT.CreateObject(OTObjectType.Sprite).GetComponent<OTSprite>();
        blankSprite.position = Vector2.zero;
        blankSprite.depth = 10;	// slightly behind
        blankSprite.size = new Vector2(Screen.width, Screen.height);
        blankSprite.texture = GuiUtils.Texture; // a blank texture
        blankSprite.tintColor = Color.black;
        blankSprite.transparent = true;
        // create the panel with the texture on
        panelSprite = OT.CreateObject(OTObjectType.Sprite).GetComponent<OTSprite>();
        panelSprite.texture = panelTex;
        panelSprite.size = new Vector2(panelTex.width, panelTex.height);
        panelSprite.position = new Vector2(0, 15); // 0,0 is screen centre, move a little up to be in line with the grid
        panelSprite.depth = 0;
        // the win.lose text:
        titleText = OT.CreateObject(OTObjectType.TextSprite).GetComponent<OTTextSprite>();
        titleText.spriteContainer = font;
        titleText.position = new Vector2(0, 50);
        titleText.size = new Vector2(1.2f, 1.2f);
        titleText.depth = -10;	// in front of panel
        if (stars>0)
            titleText.text = "Level Complete!";
        else
            titleText.text = "Level Failed!";
        titleText.tintColor = Color.black;
        // the stars
        starSprites = new OTSprite[3];
        for (int i = 0; i < 3; i++)
        {
            OTSprite spr = OT.CreateObject(OTObjectType.Sprite).GetComponent<OTSprite>();
            spr.texture = starTex;
            spr.size = new Vector2(50, 50);
            spr.depth = -11;
            starSprites[i] = spr;
        }
        starSprites[0].position = new Vector2(-60, 100);
        starSprites[1].position = new Vector2(0, 110);
        starSprites[2].position = new Vector2(+60, 100);
        // time/score text
        detailText = OT.CreateObject(OTObjectType.TextSprite).GetComponent<OTTextSprite>();
        detailText.spriteContainer = font;
        detailText.position = new Vector2(0, -15);
        detailText.depth = -10;	// in front of panel
        int min = GameState.levelTime / 60;
        int sec = GameState.levelTime % 60;
        detailText.text = string.Format("Time Left:   {0}:{1:00}\n\nLevel Score:{2,5}\n\nTotal Score:{3,5}", min, sec, GameState.levelScore,GameState.levelScore+GameState.gameScore);
        detailText.tintColor = Color.black;
        // press to continue
        continueText = OT.CreateObject(OTObjectType.TextSprite).GetComponent<OTTextSprite>();
        continueText.spriteContainer = font;
        continueText.position = new Vector2(0, -80);
        continueText.depth = -10;	// in front of panel
        continueText.text = "Click to Continue";
        continueText.tintColor = new Color(255,215,0);
        continueText.alpha = 0; // invis
    }

    void Sound1()
    {
        GetComponent<AudioSource>().PlayOneShot(sounds[0]);
    }
    void Sound2()
    {
        GetComponent<AudioSource>().PlayOneShot(sounds[1]);
    }
    void Sound3()
    {
        GetComponent<AudioSource>().PlayOneShot(sounds[2]);
    }

	// Update is called once per frame
	void Update () 
	{
        if (waiting)
        {
            if (Input.anyKey || Input.GetMouseButton(0))
            {
                waiting = false;    // stop repeat call
                // use the Transition class for a scene change
                Transition.FadeIn(GuiUtils.BlackTexture, 1);
                // in 1.5 second scene change
                Invoke("NextScene", 1.5f);
            }
        }
	}
    void NextScene()
    {
        // reset level score
        GameState.levelScore = 0;

        if (stars > 0)  // level passed
        {
            if (GameState.levelNumber <= 20)    // not last level
            {
                GameState.levelNumber++;
                Application.LoadLevel("marblezScene");
            }
            else
            {
                // level 20 finished! I'm impressed
                WinGame.win = WinGame.Win.ReallyWin;
                Application.LoadLevel("winScene");
            }
        }
        else
        {
            // level is lost:

            // special case: level 20 with more that 50 stars (2.5 per level * 19 levels = 47.5)
            if (GameState.levelNumber == 20 && GameState.Instance.TotalStars() >= 50)
            {
                // level 20 failed, but close enough, give them 3 stars for this level
                WinGame.win = WinGame.Win.CloseWin;
                GameState.Instance.SetLevelResult(GameState.levelNumber, GameState.levelScore, 3);
                Application.LoadLevel("winScene");
            }
            else
            {
                // you just lost:
                Application.LoadLevel("marblezMenu");   // back to menu
            }
        }
    }
}
