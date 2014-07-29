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

	Sprite[] m_sprBricks = null;
	GameObject m_prefBrick = null;
	ArrayList[] m_listBricks = new ArrayList[MAX_COL];
	ArrayList m_bullets = new ArrayList();
	float topLinePosY = 0f;
	float bottomLinePosY = -10f;
	float scrollDownSpeed = -0.5f;
	float scrollUpSpeed = 1f;
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
			m_listBricks[i].Insert(0, brick);
		}
	}

	void scrollDown(Vector3 v)
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
				Brick brick = (Brick)m_listBricks[col][lastIndex];
				DestroyObject(brick.m_object);
				m_listBricks[col].RemoveAt(lastIndex);
			}
		}
	}

	void scrollUp(Vector3 v)
	{

		for(int b = 0; b < m_bullets.Count; ++b)
		{
			Brick bullet = (Brick)m_bullets[b];
			bullet.m_object.transform.Translate(v);			

			for(int col = 0; col < MAX_COL; ++col)
			{
				int lastIndex = m_listBricks[col].Count-1;
				if (lastIndex < 0)
				{
					continue;
				}

				if (bullet.m_col != col)
					continue;

				
				Brick brick = (Brick)m_listBricks[col][lastIndex];
				if (brick.m_object.transform.position.y-1 <= bullet.m_object.transform.position.y)
				{
					Vector3 pos = brick.m_object.transform.position;
					pos.y--;
					bullet.m_object.transform.position = pos;
					m_listBricks[col].Add(bullet);
					m_bullets.RemoveAt(b);
					break;
				}	
			}

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

		scrollDown(scrollDownPos);
		scrollUp(scrollUpPos);


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
			int col = (int)touchPos.x;
			Brick bullet = createBrick(col, BrickType.Bullet);
			m_bullets.Add(bullet);

		}
		else
		{

		}
	}
}
