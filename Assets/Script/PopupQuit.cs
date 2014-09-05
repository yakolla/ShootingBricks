using UnityEngine;
using System.Collections;

public class PopupQuit : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{

	}

	// Update is called once per frame
	void Update ()
	{
		int touchedCount = TouchMgr.Update();
		
		if (TouchMgr.isTouchUp("quit-yes"))
		{
			Application.Quit();
			return;
		}
		
		if (TouchMgr.isTouchUp("quit-no"))
		{
			GameBlackboard.m_gameState = GameState.RUNNING;
			gameObject.SetActive(false);
			return;
		}
	}
}

