using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScreenDisplayer : MonoBehaviour {

    public Button m_mainMenuButton;
    public Text m_raceModeText;
    public Text m_mainCarResultText;

    private SceneMaster m_sceneMaster;
    private RaceManager m_raceManager;

    private void Awake()
    {
        m_sceneMaster = FindObjectOfType<SceneMaster>();
        m_raceManager = FindObjectOfType<RaceManager>();
        m_mainMenuButton.onClick.AddListener(MainMenuButtonPressed);
    }

    private void Start()
    {
        SetUpResultsScreen();
    }

    private void SetUpResultsScreen()
    {
        SetUpRaceMode();
        SetUpMainCarResult();
        SetUpRanking();
    }

    private void SetUpRaceMode()
    {
        RaceManager.RaceMode raceMode = m_raceManager.GetRaceMode();
        switch (raceMode)
        {
            case RaceManager.RaceMode.Solo:
                m_raceModeText.text = "SOLO";
                break;
            case RaceManager.RaceMode.SoloIA:
                m_raceModeText.text = "SOLO IA";
                break;
            case RaceManager.RaceMode.TimeAttack:
                m_raceModeText.text = "TIME ATTACK";
                break;
            case RaceManager.RaceMode.VS1:
                m_raceModeText.text = "1 VS 1";
                break;
            case RaceManager.RaceMode.VSN:
                m_raceModeText.text = m_raceManager.GetCarCount() + " CARS RACE";
                break;
        }
    }

    private void SetUpMainCarResult()
    {
        RaceManager.RaceMode raceMode = m_raceManager.GetRaceMode();
        switch (raceMode)
        {
            case RaceManager.RaceMode.Solo:
            case RaceManager.RaceMode.TimeAttack:
            case RaceManager.RaceMode.SoloIA:
                float raceTime = m_raceManager.GetMainCarTime();
                Utils.TimeToString(raceTime, ref m_mainCarResultText, 0.6f);
                break;
            case RaceManager.RaceMode.VS1:
            case RaceManager.RaceMode.VSN:
                int position = m_raceManager.GetMainCarPosition();
                m_mainCarResultText.text = "Nº " + position;
                if (position == 1)
                {
                    m_mainCarResultText.text += "!";
                }
                break;
        }
    }

    private void SetUpRanking()
    {

    }

    private void MainMenuButtonPressed()
    {
        m_sceneMaster.UnloadRaceToTitleScreen();
    }

}
