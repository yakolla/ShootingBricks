using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;

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
	public GameObject	m_shootingEffect;
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
		dupBrick.m_shootingEffect = m_shootingEffect;
		return dupBrick;
	}
}

class PopupResultObject
{
	GameObject 	m_obj;
	Score		m_curScore;
	Score		m_highScore;

	public PopupResultObject()
	{
		m_obj = GameObject.Find("/PopupResult");
		m_obj.SetActive(false);
	}

	public void Show()
	{
		m_obj.SetActive(true);
		m_curScore = GameObject.Find("score").GetComponent<Score>();
		m_highScore = GameObject.Find("highScore").GetComponent<Score>();

		m_curScore.setNumber(GameBlackboard.m_curScore);
		m_highScore.setNumber(GameBlackboard.m_highScore);
	}
}


[RequireComponent(typeof(AudioSource))]
public class Background : MonoBehaviour {

	const int MAX_COL = 5;
	public float ShootCoolTime = 0.1f;
	const float leftLinePos = 0f;
	const float bottomLinePosY = -8f;
	const float topLinePos = 1f;
	const int	MAX_OBSTACLE_COUNT = 1;
	public int DefaultBombBrickType = (int)BombBrickType.A_TYPE;
	public AudioClip	shootingSound = null;
	public AudioClip	bombBrickSound = null;
	public AudioClip	hittingBrickSound = null;

	CGoogleAnalytics 	m_ga;
	Sprite[] 			m_sprLineBars = null;
	Sprite[] 			m_sprLineButtons = null;
	Sprite[] 			m_sprObstacleNumbers = null;
	Sprite 				m_sprFeverBgEffect = null;
	Sprite				m_sprDangerBgEffect = null;
	Material[] 			m_metBricks = null;
	GameObject[] 		m_lineButtons = new GameObject[MAX_COL];
	GameObject[] 		m_lineBars = new GameObject[MAX_COL];
	GameObject 			m_prefBrick = null;
	GameObject 			m_prefCrashEffect = null;
	GameObject 			m_prefShootingEffect = null;
	GameObject 			m_prefFeverShootingEffect = null;
	GameObject 			m_prefRestartAds = null;
	GameObject			m_prefFeverBar = null;
	Animator			m_prefDangerEffect = null;
	Animator 			m_prefBackgroundEffect = null;

	ArrayList[] 		m_listBricks = new ArrayList[MAX_COL];
	ArrayList 			m_bullets = new ArrayList();
	ArrayList 			m_throwAwayBricks = new ArrayList();
	float 				topBricksLinePosY = 0f;
	public float 		scrollDownSpeed = -0.5f;
	public float 		scrollUpSpeed = 8f;
	public float 		shootingAccelSpeed = 0.1f;
	float 				m_frictionForDownSpeed=0;
	Fever 				m_fever = null;
	Score				m_score = null;
	PopupResultObject	m_popupResultObject = null;
	int[]				m_feverCountOfShootingBrick = new int[MAX_COL];
	int 				m_hp = 1;

	// Use this for initialization
	void Start () {

		Screen.SetResolution(Screen.width, Screen.width/2*3, true);
		GameBlackboard.init();
		GameBlackboard.m_gameState = GameState.RUNNING;
		// session starts
		m_ga = this.GetComponent<CGoogleAnalytics>();
		m_ga.analytics.TrackSession(true);


		m_score = this.GetComponent<Score>();
		m_fever = this.GetComponent<Fever>();

		m_fever.m_onStartFever += OnStartFever;
		m_fever.m_onEndFever += OnEndFever;

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

		m_prefBackgroundEffect = GameObject.Find("/Bg").GetComponent<Animator>();
		m_prefBrick = Resources.Load<GameObject>("Pref/Brick");
		m_prefCrashEffect = Resources.Load<GameObject>("Pref/CrashEffect");
		m_prefShootingEffect = Resources.Load<GameObject>("Pref/shoot particle");
		m_prefFeverShootingEffect = Resources.Load<GameObject>("Pref/fever shot ef");
		m_prefDangerEffect = GameObject.Find("/DangerEffect").GetComponent<Animator>();
		m_prefRestartAds = Resources.Load<GameObject>("Pref/RestartAds");
		m_prefFeverBar = GameObject.Find("/FeverBar");

		m_popupResultObject = new PopupResultObject();
		m_sprObstacleNumbers = Resources.LoadAll<Sprite>("Sprite/obstacleBrickNumbers");
		m_sprFeverBgEffect = Resources.Load<Sprite>("Sprite/feverBgEffect");
		m_sprDangerBgEffect = Resources.Load<Sprite>("Sprite/danger");
		createLineBars();
		createButtons();
	}

	void OnStartFever()
	{		
		m_prefBackgroundEffect.SetTrigger("Fever");
		m_prefDangerEffect.GetComponent<SpriteRenderer>().sprite = m_sprFeverBgEffect;
		m_prefDangerEffect.SetTrigger("DangerOn");
		for(int col = 0; col < MAX_COL; ++col)
		{
			m_feverCountOfShootingBrick[col] += 1;
			m_lineButtons[col].GetComponent<Animator>().SetTrigger("Fever");
		}

	}

	void OnEndFever()
	{		
		m_prefBackgroundEffect.SetTrigger("Normal");
		m_prefDangerEffect.GetComponent<SpriteRenderer>().sprite = m_sprDangerBgEffect;
		m_prefDangerEffect.SetTrigger("DangerOff");
		for(int col = 0; col < MAX_COL; ++col)
		{
			m_lineButtons[col].GetComponent<Animator>().SetTrigger("Normal");
		}
	}

	void createLineBars()
	{
		GameObject pref = Resources.Load<GameObject>("Pref/LineBar");
		m_sprLineBars =  Resources.LoadAll<Sprite>("Sprite/lineBar");
		for(int col = 0; col < MAX_COL; ++col)
		{
			Vector3 pos = new Vector3 (col+leftLinePos, -3f, 5f);		
			
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

	Brick createBrick(int col, BrickType type, int overlapCount, bool fever)
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
			brick.m_enableFeverMode = fever;
			if (brick.m_enableFeverMode == true)
			{
				brick.m_3dBrickObject.GetComponent<MeshRenderer> ().material = m_metBricks[5];
			}

			if (fever == true)
			{
				brick.m_shootingEffect = Instantiate (m_prefFeverShootingEffect, Vector3.zero, Quaternion.Euler (0, 0, 0)) as GameObject;
			}
			else
			{
				brick.m_shootingEffect = Instantiate (m_prefShootingEffect, Vector3.zero, Quaternion.Euler (0, 0, 0)) as GameObject;
			}

			brick.m_shootingEffect.transform.parent = brick.m_3dBrickObject.transform;
			brick.m_shootingEffect.transform.localPosition = new Vector3(0f, -1.0f, -1f);
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
				if (400+Mathf.Min(m_score.getNumber()/300, 400) < rand)
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

			Brick brick = createBrick(i, type, overlapCount, false);
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


	bool scrollDownBricks(Vector3 v)
	{
		bool danger = false;
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

				if (brick.m_object.transform.position.y <= bottomLinePosY+3)
				{
					danger = true;
				}
			}
		}
		foreach(int col in deleted)
		{
			int lastIndex = m_listBricks[col].Count-1;

			destroyBrick(col, lastIndex, true);

			m_hp -= 1;

		}

		return danger;
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

				if (bullet.m_shootingEffect != null)
				{
					ParticleSystem particle = bullet.m_shootingEffect.GetComponent<ParticleSystem>();
					if (particle != null)
					{
						particle.emissionRate = 0;
					}
					else
					{
						bullet.m_shootingEffect.SetActive(false);
					}
				}

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

						if (bullet.m_enableFeverMode)
						{
							removeBullet = false;
						}
						else
						{
							DestroyObject(bullet.m_object);
						}
					}

				}
				else
				{
					playCrashEffect(bullet);

					m_listBricks[bullet.m_col].Add(bullet);
					bullet.m_creationTime = Time.time;

					playBounceEffect(bullet.m_col);
					audio.PlayOneShot(hittingBrickSound);
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

	void shootBullet(int col, bool fever)
	{
		Brick bullet = createBrick(col, BrickType.Bullet, 0, fever);
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

		m_score.setNumber(m_score.getNumber() + 1);
		m_throwAwayBricks.Add(brick);

		audio.PlayOneShot(bombBrickSound);
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

		m_frictionForDownSpeed=getScrollDownSpeed()/-1f;
		m_fever.chargeUp();
		changeBricksOfAfterCompletedLineToBullets(compLine);
	}
	
	void destoryThrowAwayBricks()
	{
		while(m_throwAwayBricks.Count > 0)
		{
			Brick brick = (Brick)m_throwAwayBricks[m_throwAwayBricks.Count-1];
			if (brick.m_object.transform.position.y > bottomLinePosY-1)
			{
				break;
			}

			DestroyObject(brick.m_object);
			m_throwAwayBricks.RemoveAt(m_throwAwayBricks.Count-1);
		}
	}

	float getScrollDownSpeed()
	{
		int score = m_score.getNumber();
		if (score == 0)
			return scrollDownSpeed;

		float speedScore = Mathf.Log(score, 0.001f);
		return scrollDownSpeed+speedScore;
	}
		

	void popupResultBoard()
	{
		GameBlackboard.m_gameState = GameState.PAUSE;
		m_popupResultObject.Show();

	}
	// Update is called once per frame

	void Update () {

		if (GameBlackboard.m_gameState != GameState.RUNNING)
			return;

		Vector3 scrollDownPos = Vector3.up * Mathf.Min(0, getScrollDownSpeed()+m_frictionForDownSpeed) * Time.deltaTime;
		Vector3 scrollUpPos = Vector3.up * scrollUpSpeed * Time.deltaTime;

		topBricksLinePosY += scrollDownPos.y;

		if (topBricksLinePosY < (topLinePos-1))
		{
			generateBricksToTopLine();
			topBricksLinePosY = topLinePos;
		}

		bool danger = scrollDownBricks(scrollDownPos);
		scrollUpBullets(scrollUpPos);
		scrollUpUncombinedBricks(scrollUpPos);
		delteCompletedLine((BombBrickType)DefaultBombBrickType);
		destoryThrowAwayBricks();

		if (m_fever.isFeverMode() == false)
		{
			if (danger == true)
			{
				//m_prefBackgroundEffect.SetTrigger("Danger");
				m_prefDangerEffect.SetTrigger("DangerOn");
			}
			else
			{
				m_prefBackgroundEffect.SetTrigger("Normal");
				m_prefDangerEffect.SetTrigger("DangerOff");
			}
		}
		else
		{
			m_prefBackgroundEffect.SetTrigger("Fever");
			m_prefDangerEffect.SetTrigger("DangerOn");
		}

		Vector3 scale = m_prefFeverBar.transform.localScale;
		scale.y = m_fever.getChargeValue()/100f;
		//m_prefFeverBar.transform.localScale = scale;

		SpriteRenderer renderer=m_prefFeverBar.GetComponentInChildren<SpriteRenderer>();
		renderer.material.SetTextureOffset ("_MainTex", new Vector2(0f,1-scale.y));

		if (m_frictionForDownSpeed > 0)
		{
			m_frictionForDownSpeed -= Time.deltaTime;
			m_frictionForDownSpeed = Mathf.Max (0, m_frictionForDownSpeed);
		}

		if (m_hp <= 0)
		{
			Instantiate (m_prefRestartAds, m_prefRestartAds.transform.position, Quaternion.Euler (0, 0, 0));
			GameBlackboard.updateScore(m_score.getNumber());
			popupResultBoard();		

			m_ga.analytics.TrackAppview("Score " + m_score.getNumber());

			Social.ReportScore(m_score.getNumber(), "CgkIjZHLmpcVEAIQBg", (bool success) => {
			});

			GameBlackboard.m_gameState = GameState.GAME_OVER;	
			return;
		}

		int touchedCount = TouchMgr.Update();

		if (TouchMgr.isTouchUp("pause"))
		{
			popupResultBoard();
			return;
		}

		if (Input.GetKeyUp(KeyCode.Escape)) 
		{
			GameBlackboard.popupQuit();
			return;
		}

		if(touchedCount > 0)
		{
			//if (TouchMgr.GetTouchedPos(i).y < bottomLinePosY/2)
			{
				for(int i = 0; i < touchedCount; i++)
				{
					bool shootable = true;
					int col = (int)Mathf.Clamp(TouchMgr.GetTouchedPos(i).x-leftLinePos+0.5F, 0, MAX_COL-1);
					foreach(Brick bullet in m_bullets)
					{
						if (bullet.m_col == col)
						{
							if (bullet.m_object.transform.position.y-2.5 < bottomLinePosY)
							{
								shootable = false;
								break;
							}
						}
					}

					if (TouchMgr.GetTouchedPos(i).y > (bottomLinePosY-topLinePos)/3)
					{
						shootable = false;
					}
					
					if (shootable == true)
					{
						
						m_lineBars[col].GetComponent<Animator>().SetTrigger("Touch");
						m_lineButtons[col].GetComponent<Animator>().SetTrigger("Touch");

						//shootBullet(col, m_feverCountOfShootingBrick[col]>0);
						shootBullet(col, m_fever.isFeverMode());
						m_feverCountOfShootingBrick[col] = Mathf.Max(0, m_feverCountOfShootingBrick[col]-1);

						if (m_fever.isFeverMode())
						{
							int remainFeverCount = 0;
							for (int f = 0; f < MAX_COL; ++f)
							{
								remainFeverCount += m_feverCountOfShootingBrick[f];
							}
							
							if (remainFeverCount == 0)
							{
								//m_fever.endFeverMode();
							}
						}

					}
				}

			}
		}
	}
}
