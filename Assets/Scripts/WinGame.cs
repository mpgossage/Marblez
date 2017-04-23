using UnityEngine;
using System.Collections;

public class WinGame : MonoBehaviour 
{
    public enum Win { ReallyWin, CloseWin, NotWin };
    //public static Win win = Win..CloseWin;
    public static Win win = Win.NotWin;

    public string realWinFile, closeWinFile;
    public AudioClip selectClip;

    string theText;
    float counter = 0;
    OTTextSprite text;

	// Use this for initialization
	void Start () 
    {
        text=GetComponent<OTTextSprite>();
        if (win==Win.NotWin)
        {
            theText="Not yet";
        }
        if (win==Win.CloseWin)
        {
            theText = UFileLoader.ReadAllText(closeWinFile);
        }
        if (win == Win.ReallyWin)
        {
            theText = UFileLoader.ReadAllText(realWinFile);
        }
        theText += "\n\n<Press any key to return>";
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))   // escape to skip to end
        {
            text.text = theText;
            counter = 1000;
        } 
        float textSpeed = 10;    // X char/second
        counter += Time.deltaTime*textSpeed;
        int len = Mathf.RoundToInt(counter);
        if (len <= theText.Length)   // still adding
        {
            text.text = theText.Substring(0, len);
        }
        else
        {
            if (Input.anyKey || Input.GetMouseButton(0))
            {
                if (!Transition.Fading)
                {
                    audio.PlayOneShot(selectClip);
                    // use the Transition class for a scene change
                    Transition.FadeIn(GuiUtils.BlackTexture, 1);
                    // in 1.5 second scene change
                    Invoke("MainMenu", 1.5f);
                }
            }
        }
    }
    void MainMenu()
    {
        Application.LoadLevel("marblezMenu");   // back to menu
    }

	
}
