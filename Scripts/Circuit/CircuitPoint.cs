using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitPoint : MonoBehaviour
{

    public float m_width = 10.0f;
    
    public void GetPoints(out Vector3 leftPoint, out Vector3 rightPoint)
    {
        leftPoint = transform.position - transform.right * m_width * 0.5f;
        rightPoint = transform.position + transform.right * m_width * 0.5f;
    }

}