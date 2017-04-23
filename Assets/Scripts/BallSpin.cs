using UnityEngine;
using System.Collections;

public class BallSpin : MonoBehaviour 
{
	public Vector3 velocity=Vector3.one;
	
	Vector3 inital;

	// Use this for initialization
	void Start () 
	{
		inital=transform.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// sin^3 gives a nice -1..0..+1 sweep, but remains close to 0 for quite a while
		// sin() moved through zero too fast
		Vector3 rot=inital+velocity*Mathf.Pow(Mathf.Sin(Time.time),3);
		transform.eulerAngles=rot;
	}
}
