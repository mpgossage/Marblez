using UnityEngine;
using System.Collections;
using Gossage.System;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class ResultBox : MonoBehaviour 
{
    [SerializeField]
    private CanvasGroup resultBox;

    public AudioClip[] sounds;
	
    int stars = 0;  // how many stars awarded
    bool pressAnyKey = false; // if its time for the press any key
	
	// Use this for initialization
	void Start () 
	{
        // stop music:
        MusicManager.Instance.FadeOut();
        DetermineResult();
        // fade entire UI
        resultBox.gameObject.SetActive(true);
        CanvasFader fader = resultBox.GetComponent<CanvasFader>();
        fader.SetOut();
        fader.FadeIn(1);
        // grab text & set it
        Text titleText, detailText;
        UiUtils.GetGameObjectComponent(resultBox.gameObject, "BlankPanel/BgImage/TitleText", out titleText);
        titleText.text = (stars > 0) ? "Level Complete!" : "Level Failed!";
        int min = GameState.levelTime / 60;
        int sec = GameState.levelTime % 60;
        UiUtils.GetGameObjectComponent(resultBox.gameObject, "BlankPanel/BgImage/ScoreText", out detailText);
        detailText.text = string.Format("Time Left:   {0}:{1:00}\n\nLevel Score:{2,5}\n\nTotal Score:{3,5}", min, sec, GameState.levelScore, GameState.levelScore + GameState.gameScore);
        // grab the stars
        Image star1, star2, star3;
        UiUtils.GetGameObjectComponent(resultBox.gameObject, "BlankPanel/BgImage/Star1", out star1);
        UiUtils.GetGameObjectComponent(resultBox.gameObject, "BlankPanel/BgImage/Star2", out star2);
        UiUtils.GetGameObjectComponent(resultBox.gameObject, "BlankPanel/BgImage/Star3", out star3);
        // set size to zero
        Vector3 starSize = star1.transform.localScale;
        star1.transform.localScale = star2.transform.localScale = star3.transform.localScale = Vector3.zero;
        // tween on the stars (this is either awesome tween usage, or a real headache)
        if (stars >= 1)
        {
            star1.transform.DOScale(starSize, 1.0f).SetDelay(1.0f).SetEase(Ease.OutElastic)
                .OnStart(() => GetComponent<AudioSource>().PlayOneShot(sounds[0])).Play();
        }
        if (stars >= 2)
        {
            star2.transform.DOScale(starSize, 1.0f).SetDelay(2.0f).SetEase(Ease.OutElastic)
                .OnStart(() => GetComponent<AudioSource>().PlayOneShot(sounds[1])).Play();
        }
        if (stars >= 3)
        {
            star3.transform.DOScale(starSize, 1.0f).SetDelay(3.0f).SetEase(Ease.OutElastic)
                .OnStart(() => GetComponent<AudioSource>().PlayOneShot(sounds[2])).Play();
        }
        // press any key:
        Text txtPressAnyKey;
        if (UiUtils.GetGameObjectComponent(resultBox.gameObject, "BlankPanel/BgImage/PressAnyKeyText", out txtPressAnyKey))
        {
            Color c = txtPressAnyKey.color;
            c.a = 0; // trans
            txtPressAnyKey.color = c;
            txtPressAnyKey.DOFade(1.0f, 0.5f).SetDelay(4.0f).
                OnStart(() => pressAnyKey = true).Play();
        }
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
	// Update is called once per frame
	void Update () 
	{
        if (pressAnyKey)
        {
            if (Input.anyKey || Input.GetMouseButton(0))
            {
                pressAnyKey = false;    // stop repeat call
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
                SceneManager.LoadScene("marblezScene");
            }
            else
            {
                // level 20 finished! I'm impressed
                WinGame.win = WinGame.Win.ReallyWin;
                SceneManager.LoadScene("winScene");
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
                SceneManager.LoadScene("winScene");
            }
            else
            {
                // you just lost:
                SceneManager.LoadScene("marblezMenu");   // back to menu
            }
        }
    }
}
