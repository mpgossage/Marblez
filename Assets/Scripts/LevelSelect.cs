using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Gossage.System;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour {
    public AudioClip rolloverSound, selectSound;
    [SerializeField]
    private GameObject buttonObject, buttonPanel;
    [SerializeField]
    private Sprite[] buttonSprites;

	// Use this for initialization
	void Start () 
	{
		MakeLevelButtons();
	}

    private void MakeLevelButtons()
    {
        // cannot disable the object, it messes up auto layout, so turning off image & button
        buttonObject.GetComponent<Image>().enabled = false;
        buttonObject.GetComponent<Button>().enabled = false;

        Text txtLevel, txtScore;
        for (int i = 0; i < 20; i++) // clone buttons for level 1 to 20
        {
            GameObject btnObj = Instantiate(buttonObject);
            btnObj.transform.SetParent(buttonPanel.transform);
            bool isUnlocked = GameState.Instance.IsLevelAvailable(i + 1);
            if (!isUnlocked) continue;

            Button btn = btnObj.GetComponent<Button>();
            Image img = btnObj.GetComponent<Image>();
            btn.enabled = true;
            img.enabled = true;
            int levelNum = i + 1;   // levels count 1+

            // set stars
            int stars = GameState.Instance.Results[i].stars;
            img.sprite = buttonSprites[stars];
            // find/set the text
            if (UiUtils.GetGameObjectComponent(btnObj, "LevelText", out txtLevel))
            {
                txtLevel.text = levelNum.ToString();
            }
            int score = GameState.Instance.Results[i].score;
            if (score > 0)
            {
                if (UiUtils.GetGameObjectComponent(btnObj, "LevelScore", out txtScore))
                {
                    txtScore.text = score.ToString();
                }
            }
            // callback: (using local variable to avoid issue)
            UiUtils.AddDelegateToButton(btn, () => PlayLevel(levelNum));

            MouseOverDetection mouseOver = btn.GetComponent<MouseOverDetection>();
            mouseOver.OnMouseEnterAction = () => { this.GetComponent<AudioSource>().PlayOneShot(rolloverSound); };

        }
    }
    
    void PlayLevel(int level)
    {
        Debug.Log("PlayLevel " + level);
        GetComponent<AudioSource>().PlayOneShot(selectSound);
        GameState.gameScore = GameState.levelScore = 0;
        GameState.levelNumber = level;
        // disable ALL buttons, to stop multi select/multi-click
        foreach (var b in buttonPanel.GetComponentsInChildren<Button>())
        {
            b.enabled = false;
        }
        foreach (var c in buttonPanel.GetComponentsInChildren<MouseOverDetection>())
        {
            c.enabled = false;
        }

        // use the Transition class for a scene change
        Transition.FadeIn(GuiUtils.BlackTexture, 1);
        // in 1.5 second scene change
        Invoke("PlayTheLevel", 1.5f);
        // stop music in 1 second
        MusicManager.Instance.FadeOut();        
    }
    void PlayTheLevel()
    {
        SceneManager.LoadScene("marblezScene");
    }
}
