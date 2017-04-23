using UnityEngine;
using System.Collections;

public class GameGui : MonoBehaviour 
{
    public OTContainer font;

	MarblezControl control;
    OTContainer ballSpriteSheet;

    OTTextSprite textSprite1, textSprite2;
    OTSprite ballSprite;
	
	
	int ballRequirement;
	float timer;
	int nextBallColour;
    bool gameOver = false;	
	
	enum FadeState{In,Show,Out,Complete};
	FadeState fadeState=FadeState.Complete;
	float fadeTimer;

   	// Use this for initialization
	void Start () 
	{
		control=GetComponent<MarblezControl>();
        ballSpriteSheet = control.ballSpriteSheet;
        ballRequirement = MapLoader.BallRequirement;
        timer = MapLoader.TimeLimit;
		nextBallColour=Random.Range(0,6);
        // create the sprites:
        textSprite1 = OT.CreateObject(OTObjectType.TextSprite).GetComponent<OTTextSprite>();
        textSprite1.spriteContainer = font;
        textSprite1.pivot = OTObject.Pivot.Top;
        textSprite1.position = new Vector2(0, -215);
        textSprite1.depth = 100;    // quite far back
        textSprite1.tintColor = Color.white;
        textSprite1.size = Vector2.one;
        textSprite2 = OT.CreateObject(OTObjectType.TextSprite).GetComponent<OTTextSprite>();
        textSprite2.spriteContainer = font;
        textSprite2.pivot = OTObject.Pivot.Top;
        textSprite2.position = new Vector2(0, -240);
        textSprite2.depth = 100;    // quite far back
        ballSprite = OT.CreateObject(OTObjectType.Sprite).GetComponent<OTSprite>();
        ballSprite.spriteContainer = ballSpriteSheet;
        ballSprite.pivot = OTObject.Pivot.TopRight;
        ballSprite.position = new Vector2(310, -215);
        ballSprite.depth = 100;
        ballSprite.size = new Vector2(19, 19);
	}
	
	// Update is called once per frame
	void Update () 
	{
        UpdateStatus();
        UpdateTween();
        if (gameOver)   // dont keep going if level is done
            return;
        // if escape to main menu
		if (Input.GetKeyDown(KeyCode.Escape))
            timer = 0;  // end level
		timer-=Time.deltaTime;
		if (timer<0)
		{
			timer=0;
			FailLevel();
		}
    }
    void UpdateStatus()
    {
        int sec = Mathf.CeilToInt(timer);
        int min = sec / 60;
        sec = sec % 60;
        textSprite1.text = string.Format("Score {0,5}  Time {1}:{2:00}  Left {3,2}  Next  ", GameState.levelScore + GameState.gameScore, min, sec, ballRequirement);
        ballSprite.frameIndex = nextBallColour;
    }
    void UpdateTween()
    {
        // fade timer
        fadeTimer += Time.deltaTime;
        if (fadeState == FadeState.In && fadeTimer > 0.5f)
        {
            fadeState = FadeState.Show;
            fadeTimer = 0;
        }
        if (fadeState == FadeState.Show && fadeTimer > 2)
        {
            fadeState = FadeState.Out;
            fadeTimer = 0;
        }
        if (fadeState == FadeState.Out && fadeTimer > 0.5f)
        {
            fadeState = FadeState.Complete;
            fadeTimer = 0;
        }
        // pos is the position
        // 0 is fully up, 1 is fully down
        float pos = 0;
        if (fadeState == FadeState.In)
            pos = fadeTimer / 0.5f;
        if (fadeState == FadeState.Out)
            pos = 1-(fadeTimer / 0.5f);
        if (fadeState == FadeState.Show)
            pos = 1;
        // set positions accordingly
        Vector2 textUp=new Vector2(-10,-215),textDown=new Vector2(-10,-240),
            ballUp=new Vector2(310,-215),ballDown=new Vector2(310,-240);
        textSprite1.position = Vector2.Lerp(textUp, textDown, pos);
        textSprite2.position = Vector2.Lerp(textDown, textUp, pos);
        ballSprite.position = Vector2.Lerp(ballUp, ballDown, pos);
    }
	public int NextBallColour()
	{
		int val=nextBallColour;
		nextBallColour=Random.Range(0,6);
		return val;
	}
	public void BallHome()
	{
		GameState.levelScore+=100;
		ShowText("Ball home");
		ChangeBallRequirments(-1);	// 1 ball home		
	}
	public void WrongBallHome()
	{
		ShowText("Wrong ball home");
		TimeBonus(-30);	// time penalty
	}
	public void ChangeBallRequirments(int number)
	{
		ballRequirement+=number;
		if (ballRequirement<=0)
		{
			ballRequirement=0;
			CompleteLevel();
		}
	}
	public void TimeBonus(float amount)
	{
		timer+=amount;
		if (timer<0)
		{
			timer=0;
			FailLevel();
		}
	}
	public void ShowText(string txt)
	{
		textSprite2.text=txt;

        if (fadeState==FadeState.In)
        {
            // ignore
        }
        else if (fadeState==FadeState.Show)
        {
            fadeTimer=0;	// reset timer
        }
        else if (fadeState==FadeState.Out)
        {
            fadeState=FadeState.In;
            fadeTimer=0.5f-fadeTimer;	// reverse
        }
        else
        {	// fade it
            fadeState=FadeState.In;
            fadeTimer=0;
        }
	}
	
	void FailLevel()
	{
		if (gameOver) return;
		gameOver=true;
		Debug.Log("Fail Level");
        control.audio.PlayOneShot(control.sounds.timeup);
		control.DisableAllBalls();
		Destroy(control);
        RecordResults();
	}
	void CompleteLevel()
	{
		if (gameOver) return;
		gameOver=true;
		Debug.Log("Complete Level");
        control.audio.PlayOneShot(control.sounds.levelComplete);
        control.DisableAllBalls();
		Destroy(control);
        RecordResults();
	}

    void RecordResults()
    {
        // store time: the ResultBox will look at it
		GameState.levelTime=Mathf.CeilToInt(timer);
	    // trigger the results GUI
        GetComponent<ResultBox>().enabled = true;
    }
	
	/*void GotoMenu()
	{
		Application.LoadLevel("menu");
	}
	void GotoNext()
	{
		//Application.LoadLevel("marblezScene");
		if (GameState.levelNumber<=20)
		{
			GameState.levelNumber++;
			//scene=string.Format("level{0:00}",levelNumber);
		}
		Application.LoadLevel("marblezScene");
	}*/
}
