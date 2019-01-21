using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{

    // public

    public Button[] m_mapsButtons = new Button[0];
    public string[] m_sceneNames = new string[0];
    public CanvasGroup m_raceModeScreen;
    public Button[] m_raceModesButtons = new Button[0];
    public RaceManager.RaceMode[] m_raceModes = new RaceManager.RaceMode[0];
    public Button m_closeRaceModeScreen;

    // private

    private SceneMaster m_sceneMaster;
    private bool m_isActive;

    private string m_sceneSelected;
    private RaceManager.RaceMode m_raceModeSelected;
    
    void Start()
    {
        m_isActive = true;

        if (m_mapsButtons.Length != m_sceneNames.Length)
        {
            throw new System.Exception("There are different number of buttons and scenes related.");
        }

        for (int i = 0; i < m_mapsButtons.Length; ++i)
        {
            Button button = m_mapsButtons[i];
            m_mapsButtons[i].onClick.AddListener(() => RaceButtonPressed(button));
        }
        
        if (m_raceModesButtons.Length < 1)
        {
            throw new System.Exception("There are no race mode buttons");
        }
        for (int i = 0; i < m_raceModesButtons.Length; ++i)
        {
            Button button = m_raceModesButtons[i];
            m_raceModesButtons[i].onClick.AddListener(() => RaceModeButtonPressed(button));
        }

        m_closeRaceModeScreen.onClick.AddListener(() => CloseRaceModeScreen());
        HideRaceModeScreen();

        m_sceneMaster = FindObjectOfType<SceneMaster>();
        if (m_sceneMaster == null)
        {
            throw new System.Exception("Scene Master not found.");
        }
    }

    private void RaceButtonPressed(Button button)
    {
        if (!m_isActive)
        {
            return;
        }

        for (int i = 0; i < m_mapsButtons.Length; ++i)
        {
            if (m_mapsButtons[i] == button)
            {
                if (m_sceneNames[i].Length == 0)
                {
                    Debug.Log("No scene related to button " + button.name);
                    return;
                }
                m_sceneSelected = m_sceneNames[i];
                ShowRaceModeScreen();
                return;
            }
        }
    }

    private void ShowRaceModeScreen()
    {
        m_raceModeScreen.alpha = 1.0f;
        m_raceModeScreen.blocksRaycasts = true;
    }

    private void HideRaceModeScreen()
    {
        m_raceModeScreen.alpha = 0.0f;
        m_raceModeScreen.blocksRaycasts = false;
    }

    private void RaceModeButtonPressed(Button button)
    {
        if (!m_isActive)
        {
            return;
        }

        for (int i = 0; i < m_raceModesButtons.Length; ++i)
        {
            if (m_raceModesButtons[i] == button)
            {
                m_raceModeSelected = m_raceModes[i];
                StartRace();
                return;
            }
        }
    }

    private void CloseRaceModeScreen()
    {
        HideRaceModeScreen();
    }

    private void StartRace()
    {
        m_isActive = false;
        m_sceneMaster.LoadRaceFromTitleScreen(m_sceneSelected, m_raceModeSelected);
    }

}
