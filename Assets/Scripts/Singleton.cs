using UnityEngine;
using System.Collections;

public class Singleton : MonoBehaviour 
{
	private static GameObject instance=null;
	
	public static GameObject Instance
	{
		get
		{
			if (instance==null)
			{
				instance=new GameObject();
				instance.AddComponent(typeof(Singleton));
				Component.DontDestroyOnLoad(instance);
			}
			return instance;
		}
	}
}
