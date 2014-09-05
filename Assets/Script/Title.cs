using UnityEngine;
using System.Collections;

public class Title : MonoBehaviour {

	bool m_login = false;
	// Use this for initialization
	void Start () {

		GameBlackboard.init();

		GooglePlayGames.PlayGamesPlatform.Activate();
		Social.localUser.Authenticate((bool success) => {
			// handle success or failure
			m_login = true;
		});
	}
	
	// Update is called once per frame
	void Update () {


		if (Input.GetKeyUp(KeyCode.Escape)) 
		{
			GameBlackboard.popupQuit();
			return;
		}

		if (GameBlackboard.m_gameState == GameState.QUIT)
		{
			return;
		}

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
