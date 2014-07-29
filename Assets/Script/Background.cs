using UnityEngine;
using System.Collections;



public class Background : MonoBehaviour {

	const int MAX_COL = 5;
	enum BrickType
	{
		Obstacle,
		Normal,
		Shoot,
		Max,
	};
	Sprite[] m_sprBricks = null;
	GameObject m_prefBrick = null;
	ArrayList[] m_listBricks = new ArrayList[MAX_COL];
	float topLinePosY = 0f;
	float bottomLinePosY = -10f;
	float scrollSpeed = -0.5f;
	// Use this for initialization
	void Start () {

		Screen.SetResolution(Screen.width, Screen.width/2*3, false);

		for(int i  = 0; i < MAX_COL; ++i)
		{
			m_listBricks[i] = new ArrayList();
		}

		m_sprBricks = Resources.LoadAll<Sprite>("Sprite/Bricks");
		m_prefBrick = Resources.LoadAssetAtPath<GameObject>("Assets/Pref/Bricks.prefab");

		generateBricksToTopLine();
	}

	GameObject createBrick(int col, BrickType type)
	{
		GameObject brick = Instantiate (m_prefBrick, new Vector3 (col, 1f, 0f), Quaternion.Euler (0, 0, 0)) as GameObject;
		
		brick.GetComponent<SpriteRenderer>().sprite = m_sprBricks[(int)type];

		return brick;
	}

	void generateBricksToTopLine()
	{
		for (int i = 0; i < MAX_COL; ++i) 
		{
			GameObject brick = createBrick(i, (BrickType)Random.Range((int)BrickType.Obstacle, (int)BrickType.Max));
			m_listBricks[i].Insert(0, brick);
		}
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 scrollPos = Vector3.up * scrollSpeed * Time.deltaTime;
		topLinePosY += scrollPos.y;

		if (topLinePosY < -1)
		{
			generateBricksToTopLine();
			topLinePosY = 0;
		}

		for(int col = 0; col < MAX_COL; ++col)
		{
			ArrayList deleted = new ArrayList();
			for(int row = 0; row < m_listBricks[col].Count; ++row)
			{
				GameObject brick = (GameObject)m_listBricks[col][row];
				brick.transform.Translate(scrollPos);

				if (brick.transform.position.y <= bottomLinePosY)
				{
					deleted.Add(row);
				}
			}

			foreach(int r in deleted)
			{
				DestroyObject((GameObject)m_listBricks[col][r]);
				m_listBricks[col].RemoveAt(r);
			}
		}

	}
}
