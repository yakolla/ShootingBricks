//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.18444
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;

public static class TouchMgr
{
	static int MAX_COL = 5;
	static Vector2[] touchPos = new Vector2[MAX_COL];


	static public Vector2 GetTouchedPos(int i)
	{
		return touchPos[i];
	}

	static public int Update()
	{
		
		int touchedCount = 0;
		switch (Application.platform)
		{
		case RuntimePlatform.WindowsEditor:
		case RuntimePlatform.WindowsPlayer:
			touchedCount = Input.GetMouseButton(0) == true ? 1 : 0;
			touchPos[0] = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			break;
		default:
		{
			touchedCount = Mathf.Min(MAX_COL, Input.touchCount);
			Touch[] myTouches = Input.touches;
			for(int i = 0; i < touchedCount; i++)
			{
				touchPos[i] = Camera.main.ScreenToWorldPoint(myTouches[i].position);
			}
		}break;
		}

		return touchedCount;
	}

	static public GameObject isTouched(string tag)
	{
		RaycastHit2D hit = Physics2D.Raycast(touchPos[0], -Vector2.up);
		
		if(hit.collider != null) 
		{
			if (hit.collider.tag == tag)
			{
				return hit.collider.gameObject;
			}			
		}

		return null;
	}
}


