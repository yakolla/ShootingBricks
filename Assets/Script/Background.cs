using UnityEngine;
using System.Collections;

public class Background : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject prefab = Resources.LoadAssetAtPath<GameObject>("Assets/Pref/Bricks.prefab");
		Sprite[] spr = Resources.LoadAll<Sprite>("Sprite/Bricks");

		for (int i = 0; i < 5; ++i) 
		{
			GameObject bricks = Instantiate (prefab, new Vector3 (5f+i*1f, 0f, 0f), Quaternion.Euler (0, 180, 0)) as GameObject;

			Sprite myFruit = spr[1];
			bricks.GetComponent<SpriteRenderer>().sprite = myFruit;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
