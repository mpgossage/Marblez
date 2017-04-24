using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class CreditTicker : MonoBehaviour 
{
    public string creditFile = "";
	public string text="TODO: Some Credits, etc, etc, etc, TODO: Some Credits, etc, etc, etc, TODO: Some Credits, etc, etc, etc,";
	public float textMotion=100;
	
    private Text textSprite;
    private RectTransform textRect;
    private Vector3 startPos;
	
	// Use this for initialization
	void Start () 
	{
        textSprite = GetComponent<Text>();
        textRect = GetComponent<RectTransform>();

        if (creditFile.Length > 0)
        {
            string s = UFileLoader.ReadAllText(creditFile);
            textSprite.text = s.Replace("\n", "     "); // replace a \n with a gap
        }
        else
        {
            textSprite.text = text;
        }

        startPos = textRect.position;
        startPos.x += Screen.width;
        textRect.position = startPos;
    }
    void Update()
	{
        textRect.Translate(-textMotion * Time.deltaTime, 0, 0);
        if (textRect.position.x<-textSprite.preferredWidth)
        {
            textRect.position = startPos;
        }
	}

}
