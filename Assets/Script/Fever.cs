using UnityEngine;
using System.Collections;

public class Fever : MonoBehaviour {

	float	m_chargeGuage=0f;
	public float	m_chargeUpValue=10f;
	public float	m_chargeDownValue=3f;
	public float	m_chargeDownTime=1f;
	public float	m_feverDurationTime=5f;
	float			m_feverStartTime=0f;
	bool			m_feverMode=false;
	float			m_lastChargeUpTime=0f;

	// Use this for initialization
	void Start () {
	
		m_lastChargeUpTime = Time.time;
	}

	public void chargeUp()
	{
		if (m_feverMode == true)
			return;

		m_lastChargeUpTime = Time.time;
		m_chargeGuage = Mathf.Min(100, m_chargeGuage+m_chargeUpValue);
		if (m_chargeGuage == 100)
		{
			startFeverMode();
		}

	}

	public bool isFeverMode()
	{
		return m_feverMode;
	}

	void chargeDown()
	{
		m_lastChargeUpTime = Time.time;
		m_chargeGuage = Mathf.Max(0, m_chargeGuage-m_chargeDownValue);
	}

	void startFeverMode()
	{
		m_feverMode = true;
		m_feverStartTime = Time.time;
	}

	void endFeverMode()
	{
		m_feverMode = false;
		m_chargeGuage = 0;
	}

	void OnGUI()
	{
		GUI.TextArea(new Rect(0, 20, 100, 20), "m_fever: " + m_chargeGuage);
	}

	// Update is called once per frame
	void Update () {



		if (m_feverMode == true)
		{
			if (Time.time-m_feverStartTime >= m_feverDurationTime)
			{
				endFeverMode();
			}
		}
		else
		{
			if (Time.time-m_lastChargeUpTime >= m_chargeDownTime)
			{
				chargeDown();
			}
		}

	}
}