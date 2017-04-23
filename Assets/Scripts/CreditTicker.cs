using UnityEngine;
using System.Collections;

public class CreditTicker : MonoBehaviour 
{
    public string creditFile = "";
	public string text="TODO: Some Credits, etc, etc, etc, TODO: Some Credits, etc, etc, etc, TODO: Some Credits, etc, etc, etc,";
	public float textMotion=100;
	
	Vector2 textSize; // size of the overall text
	Rect mainRect;
	
	public OTTextSprite textSprite;
	
	// Use this for initialization
	void Start () 
	{
        if (creditFile.Length>0)
        {
            string s=UFileLoader.ReadAllText(creditFile);
            textSprite.text = s.Replace("\n", "     "); // replace a \n with a gap
        }
        else
		    textSprite.text=text;
        Vector2 pos = textSprite.position;
        pos.x = Screen.width / 2;	// width/2 because OT is 0,0 for screen centre
        textSprite.position = pos;
    }
	void Update()
	{
		Vector2 pos=textSprite.position;
		pos.x-=textMotion*Time.deltaTime;
		if (pos.x<-(textSprite.worldRect.width+Screen.width/2))
			pos.x=Screen.width/2;	// width/2 because OT is 0,0 for screen centre
		textSprite.position=pos;
		//Debug.Log(textSprite.worldRect);
	}
	/*void Update()
	{
		mainRect.x-=textMotion*Time.deltaTime;
		mainRect.width=textSize.x;
		if (mainRect.x<-textSize.x)
			mainRect.x=Screen.width;
	}
	
	// Update is called once per frame
	void OnGUI() 
	{
		GUISkin oldskin=GUI.skin;
		GUI.skin=skin;
		//Color oldCol=GUI.color;
		//Color col=GUI.color;
		//col.a=1-RpgDialog.Fade;	// inverse of the rpgs's fade
		//GUI.color=col;
		
        GUI.Label(mainRect, text, skin.label);	
		//GUI.color=oldCol;		
		GUI.skin=oldskin;
	}*/
}
