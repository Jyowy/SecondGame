using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenDisplayer : MonoBehaviour {

    public float m_loadingBarLoopTime;
    public Slider m_loadinBar;
    
    private float m_time;

    private void Awake()
    {
        m_time = 0.0f;
    }

    private void Update()
    {
        m_time += Time.deltaTime;
        if (m_time > m_loadingBarLoopTime)
        {
            m_time -= m_loadingBarLoopTime;
        }

        float scrollBarRatio = m_time / m_loadingBarLoopTime;
        m_loadinBar.value = scrollBarRatio;
    }

}
