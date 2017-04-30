using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameGui : MonoBehaviour 
{
    [SerializeField]
    private Text txtScore, txtMessage;
    [SerializeField]
    private Image sprNextBall;
    [SerializeField]
    private Sprite[] ballSprites;

	MarblezControl control;

    private Vector3 messageTopPos, messageBottomPos;
	
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
        ballRequirement = MapLoader.BallRequirement;
        timer = MapLoader.TimeLimit;
		nextBallColour=Random.Range(0,6);
        // store message positions for later tweens
        messageTopPos = txtScore.transform.position;
        messageBottomPos = txtMessage.transform.position;
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
        txtScore.text = string.Format("Score {0,5}  Time {1}:{2:00}  Left {3,2}  Next  ", GameState.levelScore + GameState.gameScore, min, sec, ballRequirement);
        sprNextBall.sprite = ballSprites[nextBallColour];
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
        txtScore.transform.position = Vector3.Lerp(messageTopPos, messageBottomPos, pos);
        txtMessage.transform.position = Vector3.Lerp(messageBottomPos, messageTopPos, pos);
        // ball is attached to the text, so nothing to do for that
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
        txtMessage.text = txt;
		//textSprite2.text=txt;

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
        control.GetComponent<AudioSource>().PlayOneShot(control.Sounds.timeup);
		control.DisableAllBalls();
		Destroy(control);
        RecordResults();
	}
	void CompleteLevel()
	{
		if (gameOver) return;
		gameOver=true;
		Debug.Log("Complete Level");
        control.GetComponent<AudioSource>().PlayOneShot(control.Sounds.levelComplete);
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
