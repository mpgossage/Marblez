using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour 
{
	float transitionTime=0,transitionDuration=0;
	AudioSource source,source2;
	
	// Update is called once per frame
	void Update () 
	{
		if (transitionTime>=transitionDuration)
			return;	// no work
		//if (source2.clip==null)
		//	return;	// no work
		transitionTime+=Time.deltaTime;
		float proportion=Mathf.Clamp01(transitionTime/transitionDuration);
		source2.volume=proportion;
		source.volume=1-proportion;
        if (transitionTime >= transitionDuration)	// transition complete
		{
            Debug.Log("Music Manager sound transition");
			// stop old source:
            if (source.isPlaying)
			    source.Stop();
			// swap sources:
			AudioSource temp=source;
			source=source2;
			source2=temp;
		}	
	}
	public void StartMusic(AudioClip clip)
	{
		FadeMusic(clip,5.0f);
	}
    public void FadeOut()
    {
        FadeMusic(null, 1.0f);
    }
	
	public void FadeMusic(AudioClip clip, float duration)
	{
        Debug.Log("FadeMusic " + clip + " " + duration);
		if (source.clip==clip) return;  // same clip: no change
		/*if (source.clip==null)
		{
            Debug.Log("Staring new music");
			source.clip=clip;
			source.volume=1;
			source.Play();
			source.loop=true;
        }
		else*/
		{
            //Debug.Log("fading music");
			source2.clip=clip;
			source2.volume=0;
            if (source2.clip!=null)
			    source2.Play();
			source2.loop=true;
            transitionDuration = duration;
            transitionTime = 0;	//reset timer
            if (source.clip == null)  // no clip
                transitionTime = duration-0.01f;  // straight to swap
		}
	}


	
	#region Singleton Code
	private static MusicManager instance;

	public static MusicManager Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject obj=new GameObject("MusicManager");
				instance = obj.AddComponent<MusicManager>();
				instance.source=obj.AddComponent<AudioSource>();
				instance.source2=obj.AddComponent<AudioSource>();
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
