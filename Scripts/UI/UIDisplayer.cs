using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDisplayer : MonoBehaviour {
    
    public Text m_startTimerText;
    public Color m_starTimerNormalColor;
    public float m_startTimerLastSeconds;
    public Color m_startTimerLastSecondsColor;
    public string m_startTimerGoString;
    public Color m_startTimerGoColor;

    public Text m_speedMeterText;
    public float m_highSpeed;
    public Color m_normalSpeedColor;
    public Color m_highSpeedColor;

    public Text m_raceTimerText;
    public Color m_normalRaceTimerColor;
    public Color m_secondRaceTimerColor;
    
    private RaceManager m_raceManager;
    private CarController m_mainCar;

    private const float k_unitySpeedToKm = 0.55f * 100.0f;

    public enum UIState
    {
        Empty,
        StartTime,
        LastSeconds,
        Go,
        Race,
        Finishing,
        Finished
    };

    private UIState m_uistate;

    private void Awake()
    {
        ChangeState(UIState.Empty);
    }

    private void Start()
    {
        m_raceManager = FindObjectOfType<RaceManager>();
        m_raceManager.GetMainCarController(out m_mainCar);

        m_startTimerText.color = m_starTimerNormalColor;
        m_speedMeterText.color = m_normalSpeedColor;
    }

    public void ChangeState(UIState uistate)
    {
        m_uistate = uistate;

        switch (uistate)
        {
            case UIState.Empty:
                m_startTimerText.enabled = false;
                m_speedMeterText.enabled = false;
                m_raceTimerText.enabled = false;
                break;
            case UIState.StartTime:
                m_startTimerText.enabled = true;
                break;
            case UIState.LastSeconds:
                m_startTimerText.color = m_startTimerLastSecondsColor;
                break;
            case UIState.Go:
                m_startTimerText.text = m_startTimerGoString;
                m_startTimerText.color = m_startTimerGoColor;
                break;
            case UIState.Race:
                m_startTimerText.enabled = false;
                m_speedMeterText.enabled = true;
                m_raceTimerText.enabled = true;
                break;
            case UIState.Finished:
                m_startTimerText.enabled = false;
                m_speedMeterText.enabled = false;
                m_raceTimerText.enabled = false;
                break;
        }
    }

    void Update()
    {
        UpdateStartTimer();
        UpdateSpeedMeter();
        UpdateRaceTimer();
    }

    private void UpdateStartTimer()
    {
        if (m_uistate == UIState.StartTime || m_uistate == UIState.LastSeconds)
        {
            float startTimer = m_raceManager.GetStartTimer() + 0.5f; // Delay
            if (startTimer < 1.0f)
            {
                ChangeState(UIState.Go);
            }
            else
            {
                m_startTimerText.text = Mathf.Floor(startTimer).ToString("F0");
            }

            if (m_uistate == UIState.StartTime && startTimer < m_startTimerLastSeconds)
            {
                ChangeState(UIState.LastSeconds);
            }
        }
    }

    private void UpdateSpeedMeter()
    {
        float speed = m_mainCar.GetSpeed();
        m_speedMeterText.text = (speed * k_unitySpeedToKm).ToString("F0") + " km/h";
        if (speed < m_highSpeed)
        {
            m_speedMeterText.color = m_normalSpeedColor;
        }
        else
        {
            m_speedMeterText.color = m_highSpeedColor;
        }

    }
    
    private void UpdateRaceTimer()
    {
        float raceTimer = m_mainCar.GetRaceTime();
        Utils.TimeToString(raceTimer, ref m_raceTimerText, 0.4f);
    }

}
