using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceAnimation : MonoBehaviour {

    public GameObject m_cameraPoints;
    public float m_cameraSpeed;
    public bool m_loop;
    
    private Camera m_camera;
    private Transform[] m_points;
    private int m_pointCount;
    private int m_prevPoint;
    private int m_nextPoint;
    private float m_positionDistance;

    private float m_minDistance;
    private float m_timePerFragment;
    private float m_time;

    private enum RaceAnimationState
    {
        Ready,
        Playing,
        Finished
    };

    private RaceAnimationState m_raceAnimationState;

    private void Awake()
    {
        m_pointCount = m_cameraPoints.transform.childCount;
        m_points = new Transform[m_pointCount];
        for (int i = 0; i < m_pointCount; ++i)
        {
            m_points[i] = m_cameraPoints.transform.GetChild(i);
        }
        m_nextPoint = 0;
    }

    private void OnDestroy()
    {
        m_points = null;
    }

    public void StartAnimation()
    {
        FindObjectOfType<RaceAnimationDirector>().GetCamera(out m_camera);
        m_prevPoint = 0;
        m_nextPoint = 0;
        NextPoint();
        ChangeState(RaceAnimationState.Playing);
    }

    private void ChangeState(RaceAnimationState raceAnimationState)
    {
        m_raceAnimationState = raceAnimationState;
    }

    private void Update()
    {
        if (m_raceAnimationState == RaceAnimationState.Playing)
        {
            m_time += Time.deltaTime;
            float timePercentage = Mathf.Min(m_time / m_timePerFragment, 1.0f);
            if (timePercentage == 1.0f)
            {
                m_camera.transform.position = m_points[m_nextPoint].position;
                m_camera.transform.rotation = m_points[m_nextPoint].rotation;
                NextPoint();
            }
            else
            {
                m_camera.transform.position = Vector3.Lerp(m_points[m_prevPoint].position, m_points[m_nextPoint].position, timePercentage);
                m_camera.transform.rotation = Quaternion.Lerp(m_points[m_prevPoint].rotation, m_points[m_nextPoint].rotation, timePercentage);
            }
        }
    }

    private void NextPoint()
    {
        m_prevPoint = m_nextPoint;
        m_nextPoint++;
        if (m_nextPoint == m_pointCount)
        {
            if (m_loop)
            {
                m_nextPoint = 0;
            }
            else
            {
                ChangeState(RaceAnimationState.Finished);
                return;
            }
        }

        float distance = Vector3.Distance(m_points[m_prevPoint].position, m_points[m_nextPoint].position);
        m_timePerFragment = distance / m_cameraSpeed;
        m_time = 0.0f;
    }

    public bool HasFinished()
    {
        return m_raceAnimationState == RaceAnimationState.Finished;
    }

}
