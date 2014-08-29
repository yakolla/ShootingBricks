using UnityEngine;
using System.Collections;

public class Title : MonoBehaviour {


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		int touchedCount = TouchMgr.Update();
		
		if(touchedCount > 0)
		{
			DestroyObject(gameObject);
			Application.LoadLevel("main");
		}
	}
}
