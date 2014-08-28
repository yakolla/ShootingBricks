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
				GameBlackboard.m_gameState = GameState.RUNNING;
				DestroyObject(gameObject);
				return;
			}

			if (TouchMgr.isTouched("resume"))
			{
				GameBlackboard.m_gameState = GameState.RUNNING;
				DestroyObject(gameObject);
				return;
			}
		}
	}
}
