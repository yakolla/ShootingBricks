using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour {

	int m_displayScore = 0;
	int m_score = 0;
	public int m_numberCount = 5;
	public float m_top = 1f;
	public float m_left = 3f;
	public float m_width = 0.3f;
	public float m_z = 1f;
	Sprite[] m_sprNumbers = new Sprite[10];
	GameObject[] m_numbers = null;
	const float COOL_TIME = 0.05f;
	float	m_lastUpdateTime = 0.0f;
	// Use this for initialization
	void Start () {
	
		m_numbers = new GameObject[m_numberCount];
		GameObject pref = Resources.Load<GameObject>("Pref/Number");
		m_sprNumbers =  Resources.LoadAll<Sprite>("Sprite/Numbers");
		for(int col = 0; col < m_numberCount; ++col)
		{
			Vector3 pos = new Vector3 (m_left+(m_numberCount-col)*m_width, m_top, m_z);		
			
			GameObject obj = Instantiate (pref, Vector3.zero, Quaternion.Euler (0, 0, 0)) as GameObject;
			obj.transform.parent = gameObject.transform;
			obj.transform.localPosition = pos;

			obj.GetComponent<SpriteRenderer>().sprite = m_sprNumbers[0];
			m_numbers[col] = obj;
		}

		m_lastUpdateTime = Time.time;
	}

	public void setNumber(int number)
	{
		m_score = number;
	}

	public int getNumber()
	{
		return m_score;
	}
	
	// Update is called once per frame
	void Update () {

		if (Time.time-m_lastUpdateTime >= COOL_TIME)
		{
			if (m_displayScore < m_score)
			{
				m_displayScore += 1;
				int number = m_displayScore;
				int a = 1;
				for(int i = 0; i < m_numberCount; ++i)
				{
					m_numbers[i].GetComponent<SpriteRenderer>().sprite = m_sprNumbers[(number / a) % 10];
					
					a *= 10;
				}
			}

			m_lastUpdateTime = Time.time;
		}


	}
}
