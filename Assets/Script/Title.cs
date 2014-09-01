using UnityEngine;
using System.Collections;

public class Title : MonoBehaviour {

	bool m_login = false;
	// Use this for initialization
	void Start () {
		GooglePlayGames.PlayGamesPlatform.Activate();

		Social.localUser.Authenticate((bool success) => {
			// handle success or failure
			m_login = true;
		});
	}
	
	// Update is called once per frame
	void Update () {
		int touchedCount = TouchMgr.Update();
		
		if(touchedCount > 0)
		{
			//if (m_login == true)
			{
				DestroyObject(gameObject);
				Application.LoadLevel("main");
			}

		}
	}
}
