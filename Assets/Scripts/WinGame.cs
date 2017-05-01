using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Text))]
public class WinGame : MonoBehaviour 
{
    public enum Win { ReallyWin, CloseWin, NotWin };
    public static Win win = Win.NotWin;

    public string realWinFile, closeWinFile;
    public AudioClip selectClip;

    string theText;
    float counter = 0;
    Text text;

	// Use this for initialization
	void Start () 
    {
        text=GetComponent<Text>();
        SelectText(win);
    }
    void SelectText(Win result)
    {
        if (result == Win.NotWin)
        {
            theText = "Not yet";
        }
        if (result == Win.CloseWin)
        {
            theText = UFileLoader.ReadAllText(closeWinFile);
        }
        if (result == Win.ReallyWin)
        {
            theText = UFileLoader.ReadAllText(realWinFile);
        }
        theText += "\n\n<Press any key to return>";
    }
    void Update()
    {
#if UNITY_EDITOR    // debug
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SelectText(Win.CloseWin);
            counter = 0;
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            SelectText(Win.ReallyWin);
            counter = 0;
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            SelectText(Win.NotWin);
            counter = 0;
        }
#endif
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
                    GetComponent<AudioSource>().PlayOneShot(selectClip);
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
        SceneManager.LoadScene("marblezMenu");   // back to menu
    }

	
}
