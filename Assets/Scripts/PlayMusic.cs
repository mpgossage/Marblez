using UnityEngine;
using System.Collections;

public class PlayMusic : MonoBehaviour {
	
	public AudioClip music;
    public AudioClip[] choices;
	// Use this for initialization
	void Start () 
    {
        if (music!=null)
		    MusicManager.Instance.StartMusic(music);
        if (choices != null && choices.Length>0)
        {
            int idx=Random.Range(0, choices.Length - 1);    // -1 because its inclusive
            MusicManager.Instance.FadeMusic(choices[idx], 2);
        }
		Destroy(this);	
	}
}
