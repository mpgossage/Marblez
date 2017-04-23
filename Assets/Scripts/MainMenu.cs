using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
	string[] levels;
	int selection=0;

	// Use this for initialization
	void Start () 
	{
		Object[] levs=Resources.LoadAll("",typeof(TextAsset));
		Debug.Log(levs+":"+levs.Length);
		levels=new string[levs.Length];
		for(int i=0;i<levs.Length;i++)
		{
			TextAsset ta=(TextAsset)levs[i];
			levels[i]=ta.name;
		}
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	void OnGUI()
	{
		selection=GUILayout.SelectionGrid(selection,levels,5);
		if (GUILayout.Button("Play"))
		{
			//GameGui.scene=levels[selection];
			Application.LoadLevel("marblezScene");
		}
	}
}
