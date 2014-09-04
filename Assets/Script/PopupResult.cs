using UnityEngine;
using System.Collections;

public class PopupResult : MonoBehaviour {

	GameObject m_prefRestartAds = null;

	// Use this for initialization
	void Start () {



	}

	void resume()
	{
		if (GameBlackboard.m_gameState == GameState.PEDING_QUIT)
		{					
			Application.LoadLevel("main");
		}
		else
		{
			GameBlackboard.m_gameState = GameState.RUNNING;
		}
		
		gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update () {
	
		int touchedCount = TouchMgr.Update();
		
		if (TouchMgr.isTouchUp("restart"))
		{
			
			Application.LoadLevel("main");
			gameObject.SetActive(false);
			return;
		}
		
		if (TouchMgr.isTouchUp("resume"))
		{
			resume();
			return;
		}
		
		if (TouchMgr.isTouchUp("leaderBoard"))
		{
			Social.ShowLeaderboardUI();
			return;
		}

		if (TouchMgr.isTouchUp("quit"))
		{
			Application.Quit();
			return;
		}

		if (Input.GetKeyDown(KeyCode.Escape)) 
		{
			resume();
			return;
		}
	}
}
