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
}

public class Background : MonoBehaviour {

	const int MAX_COL = 5;
	const float SHOOT_COOL_TIME = 0.1f;

	Sprite[] m_sprBricks = null;
	GameObject m_prefBrick = null;
	ArrayList[] m_listBricks = new ArrayList[MAX_COL];
	ArrayList m_bullets = new ArrayList();

	float topLinePosY = 0f;
	float bottomLinePosY = -7f;
	float scrollDownSpeed = -0.5f;
	float scrollUpSpeed = 4f;
	float shootLastTime = 0;

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

	Brick createBrick(int col, BrickType type)
	{
		Vector3 pos = new Vector3 (col, 1f, 0f);

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

		return brick;
	}

	void generateBricksToTopLine()
	{
		for (int i = 0; i < MAX_COL; ++i) 
		{
			Brick brick = createBrick(i, (BrickType)Random.Range((int)BrickType.Obstacle, (int)BrickType.Bullet));
			if (m_listBricks[i].Count > 0)
			{
				Brick topBrick = (Brick)m_listBricks[i][0];

			}
			m_listBricks[i].Insert(0, brick);
		}
	}

	void destroyBrick(int col, int row)
	{
		Brick brick = (Brick)m_listBricks[col][row];
		DestroyObject(brick.m_object);
		m_listBricks[col].RemoveAt(row);
	}


	void scrollDownBricks(Vector3 v)
	{
		for(int col = 0; col < MAX_COL; ++col)
		{
			ArrayList deleted = new ArrayList();
			for(int row = 0; row < m_listBricks[col].Count; ++row)
			{
				Brick brick = (Brick)m_listBricks[col][row];
				brick.m_object.transform.Translate(v);
				
				if (brick.m_object.transform.position.y <= bottomLinePosY)
				{
					deleted.Add(col);
				}
			}
			
			foreach(int r in deleted)
			{
				int lastIndex = m_listBricks[col].Count-1;
				destroyBrick(col, lastIndex);
			}
		}
	}

	void scrollUpBullets(Vector3 v)
	{

		for(int b = 0; b < m_bullets.Count; ++b)
		{
			Brick bullet = (Brick)m_bullets[b];
			bullet.m_object.transform.Translate(v);			

			int lastIndex = m_listBricks[bullet.m_col].Count-1;
			if (lastIndex < 0)
			{
				continue;
			}

			// 총알이 블록을 만났을 때
			Brick brick = (Brick)m_listBricks[bullet.m_col][lastIndex];
			if (brick.m_object.transform.position.y-1 <= bullet.m_object.transform.position.y)
			{
				Vector3 pos = brick.m_object.transform.position;
				pos.y--;
				bullet.m_object.transform.position = pos;

				if (brick.m_type == BrickType.Obstacle)
				{
					destroyBrick(bullet.m_col, lastIndex);
					DestroyObject(bullet.m_object);
				}
				else
				{
					bullet.m_type = BrickType.Normal;
					m_listBricks[bullet.m_col].Add(bullet);
				}


				m_bullets.RemoveAt(b);
			}	
		}
	}

	void scrollUpUnCombinedBricks(Vector3 v)
	{
		for(int col = 0; col < MAX_COL; ++col)
		{
			for(int row = 1; row < m_listBricks[col].Count; ++row)
			{
				Brick brickUpper = (Brick)m_listBricks[col][row-1];
				Brick brick = (Brick)m_listBricks[col][row];
				
				if (brickUpper.m_object.transform.position.y-1 > brick.m_object.transform.position.y)
				{
					brick.m_object.transform.Translate(v);
				}
			}
		}
	}

	void shootBullet(int col)
	{
		Brick bullet = createBrick(col, BrickType.Bullet);
		m_bullets.Add(bullet);
	}

	void delteCompletedLine()
	{
		int row = 0;

		while(true)
		{
			int brickCount = 0;
			for(int col = 0; col < MAX_COL; ++col)
			{
				if (row >= m_listBricks[col].Count)
				{
					return;
				}

				Brick brick = (Brick)m_listBricks[col][row];
				if (brick.m_type != BrickType.Obstacle)
				{
					++brickCount;
				}
			}

			if (brickCount == MAX_COL)
			{
				for(int col = 0; col < MAX_COL; ++col)
				{
					destroyBrick(col, row);
				}
				break;
			}

			++row;
		}
	}
		
	// Update is called once per frame
	void Update () {

		Vector3 scrollDownPos = Vector3.up * scrollDownSpeed * Time.deltaTime;
		Vector3 scrollUpPos = Vector3.up * scrollUpSpeed * Time.deltaTime;

		topLinePosY += scrollDownPos.y;

		if (topLinePosY < -1)
		{
			generateBricksToTopLine();
			topLinePosY = 0;
		}

		scrollDownBricks(scrollDownPos);
		scrollUpBullets(scrollUpPos);
		scrollUpUnCombinedBricks(scrollUpPos);
		delteCompletedLine();

		Vector2 touchPos;
		bool touched = false;
		if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			touched = Input.GetMouseButton(0);
			touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}
		else
		{
			touched = Input.touchCount > 0;
			touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
		}
		
		if(touched)
		{
			if (Time.time-shootLastTime > SHOOT_COOL_TIME)
			{
				bool shootable = true;
				int col = (int)Mathf.Clamp(touchPos.x, 0, MAX_COL-1);
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
