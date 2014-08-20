using UnityEngine;
using System.Collections;

enum BrickType
{
	Obstacle,
	Bullet,
	Normal,
};

enum LinebarType
{
	Normal,
	Touched,
};

enum BombBrickType
{
	A_TYPE,
	B_TYPE,
	C_TYPE,
};

class Brick
{
	public GameObject 	m_object;
	public int			m_col;
	public BrickType	m_type;
	public int			m_overlapCount;
	public float		m_creationTime;
	public GameObject	m_3dBrickObject;
	public GameObject	m_obstacleObject;
	public GameObject	m_crashEffect;
	public bool			m_enableFeverMode;
	public Brick		Clone()
	{
		Brick dupBrick = new Brick();
		dupBrick.m_col = m_col;
		dupBrick.m_object = MonoBehaviour.Instantiate (m_object, m_object.transform.position, m_object.transform.localRotation) as GameObject;
		dupBrick.m_overlapCount = 0;
		dupBrick.m_type = m_type;
		dupBrick.m_creationTime = m_creationTime;
		dupBrick.m_3dBrickObject = m_3dBrickObject;
		dupBrick.m_obstacleObject = m_obstacleObject;
		dupBrick.m_crashEffect = m_crashEffect;
		dupBrick.m_enableFeverMode = m_enableFeverMode;
		return dupBrick;
	}
}



[RequireComponent(typeof(AudioSource))]
public class Background : MonoBehaviour {

	const int MAX_COL = 5;
	public float ShootCoolTime = 0.1f;
	const float leftLinePos = 0f;
	const float bottomLinePosY = -8f;
	const float topLinePos = 1f;
	const int	MAX_OBSTACLE_COUNT = 3;
	public int DefaultBombBrickType = (int)BombBrickType.A_TYPE;
	public AudioClip	shootingSound = null;
	public AudioClip	hittingBrickSound = null;


	Sprite[] m_sprLineBars = null;
	Sprite[] m_sprLineButtons = null;
	Sprite[] m_sprObstacleNumbers = null;
	Material[] m_metBricks = null;
	GameObject[] m_lineButtons = new GameObject[MAX_COL];
	GameObject[] m_lineBars = new GameObject[MAX_COL];
	GameObject m_prefBrick = null;
	GameObject m_prefCrashEffect = null;

	ArrayList[] m_listBricks = new ArrayList[MAX_COL];
	ArrayList m_bullets = new ArrayList();
	ArrayList m_throwAwayBricks = new ArrayList();
	float topBricksLinePosY = 0f;
	public float scrollDownSpeed = -0.5f;
	public float scrollUpSpeed = 8f;
	public float shootingAccelSpeed = 0.1f;
	float shootLastTime = 0;
	float m_frictionForDownSpeed=0;
	Fever m_fever = null;
	Score	m_score = null;
	int m_hp = 1;
	// Use this for initialization
	void Start () {

		Screen.SetResolution(Screen.width, Screen.width/2*3, true);


		m_score = this.GetComponent<Score>();
		m_fever = this.GetComponent<Fever>();
		for(int i  = 0; i < MAX_COL; ++i)
		{
			m_listBricks[i] = new ArrayList();
		}
		m_metBricks = new Material[6];
		m_metBricks[0] = new Material(Resources.Load<Material>("3dbox/" + "obstacle brick"));
		m_metBricks[1] = new Material(Resources.Load<Material>("3dbox/" + "blue brick"));
		m_metBricks[2] = new Material(Resources.Load<Material>("3dbox/" + "red brick"));
		m_metBricks[3] = new Material(Resources.Load<Material>("3dbox/" + "purple brick"));
		m_metBricks[4] = new Material(Resources.Load<Material>("3dbox/" + "yellow brick"));
		m_metBricks[5] = new Material(Resources.Load<Material>("3dbox/" + "green brick"));


		m_prefBrick = Resources.Load<GameObject>("Pref/Brick");
		m_prefCrashEffect = Resources.Load<GameObject>("Pref/CrashEffect");

		m_sprObstacleNumbers = Resources.LoadAll<Sprite>("Sprite/obstacleBrickNumbers");
		createLineBars();
		createButtons();
	}

	void createLineBars()
	{
		GameObject pref = Resources.Load<GameObject>("Pref/LineBar");
		m_sprLineBars =  Resources.LoadAll<Sprite>("Sprite/lineBar");
		for(int col = 0; col < MAX_COL; ++col)
		{
			Vector3 pos = new Vector3 (col+leftLinePos, -3f, 0f);		
			
			GameObject obj = Instantiate (pref, pos, Quaternion.Euler (0, 0, 0)) as GameObject;
			
			obj.GetComponent<SpriteRenderer>().sprite = m_sprLineBars[(int)LinebarType.Normal];
			m_lineBars[col] = obj;
		}
	}

	void createButtons()
	{
		GameObject pref = Resources.Load<GameObject>("Pref/LineButton");
		m_sprLineButtons =  Resources.LoadAll<Sprite>("Sprite/lineButton");
		for(int col = 0; col < MAX_COL; ++col)
		{
			Vector3 pos = new Vector3 (col+leftLinePos, bottomLinePosY+0.5f, -5f);		
			
			GameObject obj = Instantiate (pref, pos, Quaternion.Euler (0, 0, 0)) as GameObject;

			obj.GetComponent<SpriteRenderer>().sprite = m_sprLineButtons[0];
			m_lineButtons[col] = obj;
		}
	}

	Brick createBrick(int col, BrickType type, int overlapCount)
	{
		Vector3 pos = new Vector3 (col+leftLinePos, topLinePos, 0f);

		if (type == BrickType.Bullet)
		{
			pos.y = bottomLinePosY;
		}

		GameObject obj = Instantiate (m_prefBrick, pos, Quaternion.Euler (0, 0, 0)) as GameObject;

		Brick brick = new Brick();
		brick.m_object = obj;
		brick.m_col = col;
		brick.m_type = type;
		brick.m_overlapCount = overlapCount;
		brick.m_creationTime = Time.time;
		brick.m_3dBrickObject = obj.transform.FindChild("3DBrick").gameObject;
		brick.m_3dBrickObject.GetComponent<MeshRenderer> ().material = m_metBricks[(int)type];

		switch(type)
		{
		case BrickType.Normal:
		{
			brick.m_3dBrickObject.GetComponent<MeshRenderer> ().material = m_metBricks[Random.Range(2,5)];
		}break;
		case BrickType.Obstacle:
		{
			if (brick.m_overlapCount == 0)
			{
				brick.m_object.SetActive(false);
			}
			else
			{
				brick.m_obstacleObject = brick.m_3dBrickObject.transform.FindChild("ObstacleNumber").gameObject;
				brick.m_obstacleObject.GetComponent<SpriteRenderer>().sprite = m_sprObstacleNumbers[brick.m_overlapCount-1];
			}
		}break;
		case BrickType.Bullet:
		{
			brick.m_enableFeverMode = m_fever.isFeverMode();
			if (brick.m_enableFeverMode == true)
			{
				brick.m_3dBrickObject.GetComponent<MeshRenderer> ().material = m_metBricks[5];
			}
		}break;
		}		

		return brick;
	}

	void generateBricksToTopLine()
	{
		int normalBricks = 0;
		BrickType[] types = new BrickType[MAX_COL];
		do
		{
			normalBricks = 0;
			for (int i = 0; i < MAX_COL; ++i) 
			{
				BrickType type = BrickType.Normal;
				int rand = Random.Range(0, 1000);
				if (400+Mathf.Min(m_score.getNumber()/300, 700) < rand)
				{
					++normalBricks;
				}
				else
				{
					type = BrickType.Obstacle;
				}
				
				types[i] = type;
			}
		}while(normalBricks == MAX_COL);

		for (int i = 0; i < MAX_COL; ++i) 
		{
			BrickType type = types[i];

			int overlapCount = 0;
			if (type == BrickType.Obstacle)
				overlapCount = Random.Range(0, Mathf.Min (MAX_OBSTACLE_COUNT+1, m_score.getNumber()/100));

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

	void playCrashEffect (Brick bullet)
	{
		if (bullet.m_crashEffect == null) {
			bullet.m_crashEffect = Instantiate (m_prefCrashEffect, Vector3.zero, Quaternion.Euler (0, 0, 0)) as GameObject;
			bullet.m_crashEffect.transform.parent = bullet.m_3dBrickObject.transform;
			bullet.m_crashEffect.transform.localPosition = new Vector3(0f, 0f, -1f);
		}
		Animator ani = bullet.m_crashEffect.GetComponent<Animator> ();
		ani.Play ("crash");
	}

	void scrollUpBullets(Vector3 v)
	{
		for(int b = 0; b < m_bullets.Count; ++b)
		{
			Vector3 cv = new Vector3(v.x, v.y, v.z);
			Brick bullet = (Brick)m_bullets[b];
			float elapsedTime = Time.time-bullet.m_creationTime;
			cv.y += shootingAccelSpeed * elapsedTime;
			bullet.m_object.transform.Translate(cv);

			BrickType upperType = BrickType.Normal;
			Vector3 upperPos = new Vector3(bullet.m_col, 1, 0);
			int lastIndex = 0;
			if (0 < m_listBricks[bullet.m_col].Count)
			{
				lastIndex = m_listBricks[bullet.m_col].Count-1;
				Brick brick = (Brick)m_listBricks[bullet.m_col][lastIndex];
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

				bool bombAble = upperType == BrickType.Obstacle;
				if (bullet.m_enableFeverMode)
				{
					bombAble = lastIndex > 0;
				}

				if (bombAble)
				{
					Brick upperBrick = (Brick)m_listBricks[bullet.m_col][lastIndex];

					if (upperBrick.m_overlapCount == 0)
					{
						if (bullet.m_enableFeverMode)
						{
							Brick dupBrick = upperBrick.Clone();
							bombBrick((BombBrickType)DefaultBombBrickType, bullet.m_col, dupBrick);
						}
						destroyBrick(bullet.m_col, lastIndex, true);
						removeBullet = false;
					}
					else
					{
						Brick dupBrick = upperBrick.Clone();
						bombBrick((BombBrickType)DefaultBombBrickType, bullet.m_col, dupBrick);

						upperBrick.m_overlapCount--;
						if (upperBrick.m_overlapCount == 0)
						{
							upperBrick.m_object.SetActive(false);
						}
						else
						{
							upperBrick.m_obstacleObject.GetComponent<SpriteRenderer>().sprite = m_sprObstacleNumbers[upperBrick.m_overlapCount-1];
						}

						DestroyObject(bullet.m_object);
					}

				}
				else
				{
					playCrashEffect(bullet);



					m_listBricks[bullet.m_col].Add(bullet);
					bullet.m_creationTime = Time.time;

					playBounceEffect(bullet.m_col);

				}

				if (removeBullet == true)
				{
					m_bullets.RemoveAt(b);
				}

			}	
		}
	}

	void playBounceEffect(int col)
	{
		for(int r = m_listBricks[col].Count-1; r >= 0; --r)
		{
			Brick brick = (Brick)m_listBricks[col][r];
			if (brick.m_type == BrickType.Obstacle && brick.m_overlapCount == 0)
				break;
			
			Animator ani = brick.m_3dBrickObject.GetComponent<Animator>();
			ani.SetTrigger("Bounce");

		}
	}

	void scrollUpUncombinedBricks(Vector3 v)
	{
		for(int col = 0; col < MAX_COL; ++col)
		{
			for(int row = 0; row < m_listBricks[col].Count; ++row)
			{
				Vector3 upperPos = new Vector3(col+leftLinePos, topLinePos+1, 0);
				if (row > 0)
				{
					Brick upperBrick = (Brick)m_listBricks[col][row-1];
					upperPos = upperBrick.m_object.transform.position;

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


		audio.PlayOneShot(shootingSound);
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
				brick.m_creationTime = Time.time;
				m_bullets.Add(brick);
				destroyBrick(col, row, false);
			}
		}
	}

	void bombBrick(BombBrickType type, int lastShootCol, Brick brick)
	{
		int col = brick.m_col;
		switch(type)
		{
		case BombBrickType.A_TYPE:
		{
			
			int x = col-MAX_COL/2;
			brick.m_object.rigidbody2D.AddForce(new Vector2(x*50.0f, 50.0f));
			brick.m_object.rigidbody2D.AddTorque(30f);


		}break;
		case BombBrickType.B_TYPE:
		{
			int x = col-MAX_COL/2;
			int y = (col+lastShootCol)%(MAX_COL);
			
			brick.m_object.rigidbody2D.mass = 10;
			brick.m_object.rigidbody2D.AddForce(new Vector2(x*50.0f*Random.Range(1, 5), y*y*100.0f));
			brick.m_object.rigidbody2D.AddTorque(100f);

		}break;
		case BombBrickType.C_TYPE:
		{
			int x = col-MAX_COL/2;
			int y = (col+lastShootCol)%(MAX_COL);

			brick.m_object.rigidbody.AddForce(new Vector3(x*50.0f*Random.Range(1, 5), y*y*100.0f, y*y*-100.0f));
			brick.m_object.rigidbody.AddTorque(new Vector3(100f*Random.Range(-5, 5), 100f*Random.Range(1, 5), 100f*Random.Range(1, 5)));
			brick.m_object.rigidbody.useGravity = true;
		}break;
		}


		m_throwAwayBricks.Add(brick);

		audio.PlayOneShot(hittingBrickSound);
	}
	
	void delteCompletedLine(BombBrickType type)
	{
		int compLine = getCompletedLine();
		if (compLine == -1)
			return;

		int lastShootCol = Random.Range(0, MAX_COL);
		for(int col = 0; col < MAX_COL; ++col)
		{
			Brick brick = (Brick)m_listBricks[col][compLine];
			bombBrick(type, lastShootCol, brick);
			destroyBrick(col, compLine, false);
		}

		m_score.setNumber(m_score.getNumber() + MAX_COL);
		m_frictionForDownSpeed=getScrollDownSpeed()/-2f;
		m_fever.chargeUp();
		changeBricksOfAfterCompletedLineToBullets(compLine);
	}
	
	void destoryThrowAwayBricks()
	{
		while(m_throwAwayBricks.Count > 0)
		{
			Brick brick = (Brick)m_throwAwayBricks[m_throwAwayBricks.Count-1];
			if (brick.m_object.transform.position.y > bottomLinePosY-2)
			{
				break;
			}

			DestroyObject(brick.m_object);
			m_throwAwayBricks.RemoveAt(m_throwAwayBricks.Count-1);
		}
	}

	float getScrollDownSpeed()
	{
		return scrollDownSpeed-(Mathf.Log(1.1f, m_score.getNumber()));
	}
		


	// Update is called once per frame

	void Update () {

		Vector3 scrollDownPos = Vector3.up * Mathf.Min(0, getScrollDownSpeed()+m_frictionForDownSpeed) * Time.deltaTime;
		Vector3 scrollUpPos = Vector3.up * scrollUpSpeed * Time.deltaTime;

		topBricksLinePosY += scrollDownPos.y;

		if (topBricksLinePosY < (topLinePos-1))
		{
			generateBricksToTopLine();
			topBricksLinePosY = topLinePos;
		}

		scrollDownBricks(scrollDownPos);
		scrollUpBullets(scrollUpPos);
		scrollUpUncombinedBricks(scrollUpPos);
		delteCompletedLine((BombBrickType)DefaultBombBrickType);
		destoryThrowAwayBricks();

		if (m_frictionForDownSpeed > 0)
		{
			m_frictionForDownSpeed -= m_frictionForDownSpeed / 10f;
			m_frictionForDownSpeed = Mathf.Max (0, m_frictionForDownSpeed);
		}

		if (m_hp <= 0)
		{
			GoogleAnalytics.instance.LogScreen("Score " + m_score.getNumber()/100*100);
			Application.LoadLevel("main");
			return;
		}


		Vector2[] touchPos = new Vector2[MAX_COL];
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
			touchedCount = Input.touchCount;
			Touch[] myTouches = Input.touches;
			for(int i = 0; i < Input.touchCount; i++)
			{
				touchPos[i] = Camera.main.ScreenToWorldPoint(myTouches[i].position);
			}
		}break;
		}

		
		if(touchedCount > 0)
		{
			if (Time.time-shootLastTime > ShootCoolTime)
			{
				for(int i = 0; i < touchedCount; i++)
				{
					bool shootable = true;
					int col = (int)Mathf.Clamp(touchPos[i].x-leftLinePos+0.5F, 0, MAX_COL-1);
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
						
						m_lineBars[col].GetComponent<Animator>().SetTrigger("Touch");
						m_lineButtons[col].GetComponent<Animator>().SetTrigger("Touch");
						shootBullet(col);
						
						shootLastTime = Time.time;
					}
				}

			}
		}
	}
}
