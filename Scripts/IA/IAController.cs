using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAController : MonoBehaviour {

    // public

    public enum IALevel
    {
        Basic = 0,
        //Average,
        //Hard,
        //OMG,
        Size
    };

    // private

    private enum IAFlags
    {
        BestPath = 1 << 0,
        CarsAwareness = 1 << 1,
        Turbo = 1 << 2,
        AttackObjects = 1 << 3,
        DefenseObjects = 1 << 4,
        SpeedObjects = 1 << 5
    };

    private enum IAState
    {
        BeforeRace,
        OnRace,
        OnPause,
        AfterRace
    };
    
    private CarController m_carController;
    private CarController.CarAction m_nextCarAction;

    private bool m_isPlaying;

    private IALevel m_IALevel;
    private IAFlags m_IAFlags;
    private IAState m_IAState;

    private void Awake()
    {
        m_carController = GetComponentInParent<CarController>();
        ChangeIAState(IAState.BeforeRace);
    }

    public void SetCarController(ref CarController carController)
    {
        //m_carController = carController;
    }

    public void SetIALevel(IALevel iaLevel)
    {
        m_IALevel = iaLevel;
    }

    public void StartRace()
    {
        ChangeIAState(IAState.OnRace);
    }

    public void ResumeRace()
    {
        ChangeIAState(IAState.OnRace);
    }
    
    public void PauseRace()
    {
        ChangeIAState(IAState.OnPause);
    }

    public void EndRace()
    {
        ChangeIAState(IAState.AfterRace);
    }

    private void FixedUpdate()
    {
        ChooseNextAction();
        m_carController.SetNextCarAction(m_nextCarAction);
    }

    private void ChangeIAState(IAState iaState)
    {
        m_IAState = iaState;
    }

    private void Update()
    {
    }

    private void ChooseNextAction()
    {
        m_nextCarAction = CarController.CarAction.Nothing;

        if (m_IAState == IAState.OnRace
            || m_IAState == IAState.AfterRace)
        {
            m_nextCarAction = CarController.CarAction.Forward;
            

        }
    }

}
