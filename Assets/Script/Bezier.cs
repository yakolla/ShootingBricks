using UnityEngine;

public class Bezier {

	Vector3 m_start;
	Vector3 m_end;
	Vector3 m_handle1;
	Vector3 m_handle2;
	GameObject m_obj;
	float c = 0;

	public Bezier(GameObject obj, Vector3 end, Vector3 handle1, Vector3 handle2)
	{
		m_obj = obj;
		m_start = obj.transform.position;
		m_end = end;
		m_handle1 = handle1;
		m_handle2 = handle2;
	}

	public void Destroy()
	{
		MonoBehaviour.DestroyObject(m_obj);
	}
	
	Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float u = 1 - t;
		float tt = t*t;
		float uu = u*u;
		float uuu = uu * u;
		float ttt = tt * t;
		
		Vector3 p = uuu * p0; //first term
		p += 3 * uu * t * p1; //second term
		p += 3 * u * tt * p2; //third term
		p += ttt * p3; //fourth term
		
		return p;
	}

	public bool Update()
	{
		if (c < 1) //we defined 100 steps to draw the curve
		{
			c += 0.07f; //100 steps to draw each bezier curve
			m_obj.transform.position = CalculateBezierPoint(c, m_start, m_handle1, m_handle2, m_end);
			return true;
		}

		return false;
	}

}
