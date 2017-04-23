using UnityEngine;
using System.Collections;

public class Transition : MonoBehaviour 
{
	#region Public accessors
	public static void FadeIn(Texture2D tex, float show)
	{
		Transition t=Instance;
		t.texture=tex;
		t.fadeState=FadeState.In;
		t.timer=0;
		t.showDuration=show;
	}
    public static bool Fading { get { return Instance.fadeState != FadeState.Complete; } }
	#endregion

	enum FadeState{In,Show,Out,Complete};
	FadeState fadeState=FadeState.Complete;
	float timer;
	float showDuration=1;
	
	Texture2D texture;

	// Update is called once per frame
	void Update() 
	{
		if (fadeState!=FadeState.Complete)
		{
			timer+=Time.deltaTime;
			if (fadeState==FadeState.In && timer>=1)
			{
				timer=0;
				fadeState=FadeState.Show;
			}
			if (fadeState==FadeState.Show && timer>=showDuration)
			{
				timer=0;
				fadeState=FadeState.Out;
			}
			if (fadeState==FadeState.Out && timer>=1)
			{
				timer=0;
				fadeState=FadeState.Complete;
			}
		}
	
	}
	
	void OnGUI()
	{
		if (fadeState==FadeState.Complete) return;
		GUI.depth=-1000;	// VERY VERY TOP
		/*float x=0;
		if (fadeState==FadeState.In)
		{
			x=Screen.width*(1-timer);
		}
		else if (fadeState==FadeState.Out)
			x=Screen.width*(-timer);
        GUI.DrawTexture(new Rect(x, 0, Screen.width, Screen.height), texture, ScaleMode.StretchToFill);*/
        if (fadeState == FadeState.In)
            GUI.color = new Color(1, 1, 1, timer);
        else if (fadeState == FadeState.Out)
            GUI.color = new Color(1, 1, 1, 1 - timer);
    	GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture, ScaleMode.StretchToFill);
        GUI.color = Color.white;
    }
	
	
	#region Singleton Code
	private static Transition instance;

	public static Transition Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameObject("Transition").AddComponent<Transition>();
				GameObject.DontDestroyOnLoad(instance);	// don't get destroyed in a level loading
			}
			return instance;
		}
	}

	public void OnApplicationQuit ()
	{
		instance = null;
	}
	#endregion	
	
}
