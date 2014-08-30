using UnityEngine;
using System.Collections;

public class PopupResult : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		int touchedCount = TouchMgr.Update();
		
		if(touchedCount > 0)
		{
			
			if (TouchMgr.isTouched("restart"))
			{
				Application.LoadLevel("main");
				gameObject.SetActive(false);
				return;
			}

			if (TouchMgr.isTouched("resume"))
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
				return;
			}

			if (TouchMgr.isTouched("leaderBoard"))
			{
				Social.ShowLeaderboardUI();
				return;
			}
		}
	}
}
