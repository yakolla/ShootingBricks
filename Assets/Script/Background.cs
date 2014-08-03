using UnityEngine;
using System.Collections;

enum BrickType
{
	Obstacle,
	Normal,
	Bullet,
};

class Brick
{
	public GameObject 	m_object;
	public int			m_col;
	public BrickType	m_type;
	public int			m_overlapCount;
}

public class Background : MonoBehaviour {

	const int MAX_COL = 5;
	const float SHOOT_COOL_TIME = 0.1f;
	const float leftLinePos = 0;
	const float bottomLinePosY = -6f;


	Sprite[] m_sprBricks = null;
	Sprite[] m_sprLineBars = null;
	GameObject[] m_lineButtons = new GameObject[MAX_COL];
	GameObject[] m_lineBars = new GameObject[MAX_COL];
	GameObject m_prefBrick = null;
	ArrayList[] m_listBricks = new ArrayList[MAX_COL];
	ArrayList m_bullets = new ArrayList();
	ArrayList m_throwAwayBricks = new ArrayList();
	float topBricksLinePosY = 0f;
	float scrollDownSpeed = -0.5f;
	float scrollUpSpeed = 8f;
	float shootLastTime = 0;


	int m_score = 0;
	int m_hp = 10;
	// Use this for initialization
	void Start () {

		Screen.SetResolution(Screen.width, Screen.width/2*3, false);

		for(int i  = 0; i < MAX_COL; ++i)
		{
			m_listBricks[i] = new ArrayList();
		}

		m_sprBricks = Resources.LoadAll<Sprite>("Sprite/Bricks");
		m_prefBrick = Resources.Load<GameObject>("Pref/Bricks");
		createLineBars();
		createButtons();
	}

	void createLineBars()
	{
		GameObject pref = Resources.Load<GameObject>("Pref/LineBar");
		m_sprLineBars =  Resources.LoadAll<Sprite>("Sprite/LineBar");
		for(int col = 0; col < MAX_COL; ++col)
		{
			Vector3 pos = new Vector3 (col+leftLinePos, -3f, 0f);		
			
			GameObject obj = Instantiate (pref, pos, Quaternion.Euler (0, 0, 0)) as GameObject;
			
			obj.GetComponent<SpriteRenderer>().sprite = m_sprLineBars[0];
			m_lineBars[col] = obj;
		}
	}

	void createButtons()
	{
		GameObject pref = Resources.Load<GameObject>("Pref/LineButton");
		for(int col = 0; col < MAX_COL; ++col)
		{
			Vector3 pos = new Vector3 (col+leftLinePos, -6.0f, 0f);		
			
			GameObject obj = Instantiate (pref, pos, Quaternion.Euler (0, 0, 0)) as GameObject;
			
			obj.GetComponent<SpriteRenderer>().sprite = m_sprBricks[3];
			m_lineButtons[col] = obj;
		}
	}

	Brick createBrick(int col, BrickType type, int overlapCount)
	{
		Vector3 pos = new Vector3 (col+leftLinePos, 1f, 0f);

		if (type == BrickType.Bullet)
		{
			pos.y = bottomLinePosY;
		}

		GameObject obj = Instantiate (m_prefBrick, pos, Quaternion.Euler (0, 0, 0)) as GameObject;
		
		obj.GetComponent<SpriteRenderer>().sprite = m_sprBricks[(int)type];

		Brick brick = new Brick();
		brick.m_object = obj;
		brick.m_col = col;
		brick.m_type = type;
		brick.m_overlapCount = overlapCount;

		return brick;
	}

	void generateBricksToTopLine()
	{
		for (int i = 0; i < MAX_COL; ++i) 
		{
			BrickType type = (BrickType)Random.Range((int)BrickType.Obstacle, (int)BrickType.Bullet);
			int overlapCount = 0;
			if (type == BrickType.Obstacle)
				overlapCount = Random.Range(0, 2);

			Brick brick = createBrick(i, type, overlapCount);
			if (m_listBricks[i].Count > 0)
			{
				Brick topBrick = (Brick)m_listBricks[i][0];
				Vector3 pos = topBrick.m_object.transform.position;
				pos.y += 1;
				brick.m_object.transform.position = pos;
			}
			m_listBricks[i].Insert(0, brick);
		}
	}

	void destroyBrick(int col, int row, bool destoryObject)
	{
		Brick brick = (Brick)m_listBricks[col][row];
		if (destoryObject)
			DestroyObject(brick.m_object);
		m_listBricks[col].RemoveAt(row);
	}


	void scrollDownBricks(Vector3 v)
	{
		ArrayList deleted = new ArrayList();
		for(int col = 0; col < MAX_COL; ++col)
		{

			for(int row = 0; row < m_listBricks[col].Count; ++row)
			{
				Brick brick = (Brick)m_listBricks[col][row];
				if (brick.m_type == BrickType.Obstacle)
				{
					if (brick.m_overlapCount == 0)
					{
						brick.m_object.SetActive(false);
					}
				}
				brick.m_object.transform.Translate(v);
				if (brick.m_object.transform.position.y <= bottomLinePosY)
				{
					deleted.Add(col);
				}
			}
		}
		foreach(int col in deleted)
		{
			int lastIndex = m_listBricks[col].Count-1;
			destroyBrick(col, lastIndex, true);
			m_hp -= 1;
		}
	}

	void scrollUpBullets(Vector3 v)
	{
		for(int b = 0; b < m_bullets.Count; ++b)
		{
			Brick bullet = (Brick)m_bullets[b];
			bullet.m_object.transform.Translate(v);

			BrickType upperType = BrickType.Normal;
			Vector3 upperPos = new Vector3(bullet.m_col, 1, 0);
			int bottomIndex = 0;
			if (0 < m_listBricks[bullet.m_col].Count)
			{
				bottomIndex = m_listBricks[bullet.m_col].Count-1;
				Brick brick = (Brick)m_listBricks[bullet.m_col][bottomIndex];
				upperPos = brick.m_object.transform.position;
				upperType = brick.m_type;
			}

			// 총알이 블록을 만났을 때

			bool collition = upperPos.y-1 <= bullet.m_object.transform.position.y;
			if (collition == true)
			{
				bool removeBullet = true;
				Vector3 pos = upperPos;
				pos.y--;
				bullet.m_object.transform.position = pos;



				if (upperType == BrickType.Obstacle)
				{
					Brick upperBrick = (Brick)m_listBricks[bullet.m_col][bottomIndex];
					if (upperBrick.m_overlapCount == 0)
					{
						destroyBrick(bullet.m_col, bottomIndex, true);
						removeBullet = false;
					}
					else
					{
						upperBrick.m_overlapCount--;
						if (upperBrick.m_overlapCount == 0)
						{
							upperBrick.m_object.SetActive(false);
						}
						DestroyObject(bullet.m_object);
					}

				}
				else
				{
					m_listBricks[bullet.m_col].Add(bullet);
				}

				if (removeBullet == true)
				{
					m_bullets.RemoveAt(b);
				}

			}	
		}
	}

	void scrollUpUncombinedBricks(Vector3 v)
	{
		for(int col = 0; col < MAX_COL; ++col)
		{
			for(int row = 0; row < m_listBricks[col].Count; ++row)
			{
				Vector3 upperPos = new Vector3(col+leftLinePos, 1, 0);
				BrickType upperType = BrickType.Normal;
				int upperOverlapCount = 0;
				if (row > 0)
				{
					Brick upperBrick = (Brick)m_listBricks[col][row-1];
					upperPos = upperBrick.m_object.transform.position;
					upperType = upperBrick.m_type;
					upperOverlapCount = upperBrick.m_overlapCount;
				}

				Brick brick = (Brick)m_listBricks[col][row];
				
				if (upperPos.y-1 > brick.m_object.transform.position.y)
				{
					brick.m_object.transform.Translate(v);

					if (upperPos.y-1 < brick.m_object.transform.position.y)
					{
						Vector3 pos = upperPos;
						pos.y-=1;
						brick.m_object.transform.position = pos;
					}
				}
			}
		}

	}

	void shootBullet(int col)
	{
		Brick bullet = createBrick(col, BrickType.Bullet, 0);
		m_bullets.Add(bullet);
	}

	int getCompletedLine()
	{
		int row = 0;
		
		while(true)
		{
			int brickCount = 0;
			for(int col = 0; col < MAX_COL; ++col)
			{
				if (row >= m_listBricks[col].Count)
				{
					return -1;
				}
				
				Brick brick = (Brick)m_listBricks[col][row];
				if (brick.m_type != BrickType.Obstacle)
				{
					if (brick.m_overlapCount == 0)
						++brickCount;
				}
			}
			
			if (brickCount == MAX_COL)
			{
				return row;
			}
			
			++row;
		}

		return -1;
	}

	void changeBricksOfAfterCompletedLineToBullets(int compLine)
	{
		for(int col = 0; col < MAX_COL; ++col)
		{
			while(compLine < m_listBricks[col].Count)
			{
				int row = m_listBricks[col].Count-1;
				Brick brick = (Brick)m_listBricks[col][row];
				m_bullets.Add(brick);
				destroyBrick(col, row, false);
			}
		}
	}
	
	void delteCompletedLine()
	{
		int compLine = getCompletedLine();
		if (compLine == -1)
			return;

		for(int col = 0; col < MAX_COL; ++col)
		{
			Brick brick = (Brick)m_listBricks[col][compLine];
			brick.m_object.AddComponent<Rigidbody2D>();
			int x = col-MAX_COL/2;
			brick.m_object.rigidbody2D.AddForce(new Vector2(x*50.0f, 1.5f));
			brick.m_object.rigidbody2D.AddTorque(100f);
			m_throwAwayBricks.Add(brick);
			destroyBrick(col, compLine, false);
		}
		m_score += MAX_COL;
		m_hp++;

		changeBricksOfAfterCompletedLineToBullets(compLine);
	}
	
	void destoryThrowAwayBricks()
	{
		while(m_throwAwayBricks.Count > 0)
		{
			Brick brick = (Brick)m_throwAwayBricks[m_throwAwayBricks.Count-1];
			if (brick.m_object.transform.position.y > bottomLinePosY)
			{
				break;
			}

			DestroyObject(brick.m_object);
			m_throwAwayBricks.RemoveAt(m_throwAwayBricks.Count-1);
		}
	}
		
	void OnGUI()
	{
		GUI.TextArea(new Rect(0, 0, 100, 20), "sc: " + m_score);
		GUI.TextArea(new Rect(0, 20, 100, 20), "hp: " + m_hp);
	}

	// Update is called once per frame

	void Update () {

		Vector3 scrollDownPos = Vector3.up * scrollDownSpeed * Time.deltaTime;
		Vector3 scrollUpPos = Vector3.up * scrollUpSpeed * Time.deltaTime;

		topBricksLinePosY += scrollDownPos.y;

		if (topBricksLinePosY < -1)
		{
			generateBricksToTopLine();
			topBricksLinePosY = 0;
		}

		scrollDownBricks(scrollDownPos);
		scrollUpBullets(scrollUpPos);
		scrollUpUncombinedBricks(scrollUpPos);
		delteCompletedLine();
		destoryThrowAwayBricks();

		if (m_hp <= 0)
		{
			Application.LoadLevel("main");
		}


		Vector2 touchPos;
		bool touched = false;
		switch (Application.platform)
		{
		case RuntimePlatform.WindowsEditor:
		case RuntimePlatform.WindowsPlayer:
			touched = Input.GetMouseButton(0);
			touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			break;
		default:
			touched = Input.touchCount > 0;
			touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
			break;
		}

		
		if(touched)
		{
			Debug.Log(touchPos);
			if (Time.time-shootLastTime > SHOOT_COOL_TIME)
			{
				bool shootable = true;
				int col = (int)Mathf.Clamp(touchPos.x-leftLinePos+0.5F, 0, MAX_COL-1);
				foreach(Brick bullet in m_bullets)
				{
					if (bullet.m_col == col)
					{
						if (bullet.m_object.transform.position.y-1 < bottomLinePosY)
						{
							shootable = false;
							break;
						}
					}
				}

				if (shootable == true)
				{
					shootBullet(col);
					shootLastTime = Time.time;
				}
			}
		}
	}
}
