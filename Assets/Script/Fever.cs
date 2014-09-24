using UnityEngine;
using System.Collections;

public class Fever : MonoBehaviour {

	float			m_chargeGuage=0f;
	public float	m_chargeUpValue=10f;
	public float	m_chargeDownValue=3f;
	public float	m_chargeDownTime=1f;
	public float	m_feverDurationTime=5f;
	float			m_feverStartTime=0f;
	bool			m_feverMode=false;
	float			m_lastChargeUpTime=0f;
	public delegate void OnStartFever();
	public delegate void OnEndFever();
	public OnStartFever m_onStartFever;
	public OnEndFever m_onEndFever;

	// Use this for initialization
	void Start () {
		m_lastChargeUpTime = Time.time;
	}

	public void chargeUp(float alpha)
	{
		if (m_feverMode == true)
			return;

		m_lastChargeUpTime = Time.time;
		m_chargeGuage = Mathf.Min(100, m_chargeGuage+m_chargeUpValue+alpha);
		if (m_chargeGuage == 100)
		{
			startFeverMode();
		}

	}

	public float getChargeValue()
	{
		return m_chargeGuage;
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
		m_onStartFever();
	}

	public void endFeverMode()
	{
		m_feverMode = false;
		m_chargeGuage = 0;
		m_onEndFever();
	}

	// Update is called once per frame
	void Update () {



		if (m_feverMode == true)
		{

			if (Time.time-m_feverStartTime >= m_feverDurationTime)
			{
				endFeverMode();
			}
			else
			{
				m_chargeGuage = (m_feverDurationTime-(Time.time-m_feverStartTime))/m_feverDurationTime*100f;
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
