using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour {

    public enum RaceMode
    {
        Solo,
        SoloIA,
        TimeAttack,
        VS1,
        VSN
    };

    public enum RaceState
    {
        Loading,
        BeforeStartAnimation,
        StartTime,
        Race,
        Finishing,
        RaceFinished
    };

    public bool m_debugModeOn = false;
    public RaceMode m_debugRaceMode = RaceMode.Solo;
    
    public float m_timeBeforeStart = 5.0f;
    public float m_timeAfterFirstArrival = 5.0f;
    public RaceAnimationDirector m_raceAnimationDirector;
    public CameraController m_raceCamera;

    private SceneMaster m_sceneMaster;
    private RaceMode m_raceMode;
    private RaceState m_raceState;
    private PlayerController m_playerController;
    private IAController[] m_IACarController;
    private GameObject[] m_cars;
    private CarController[] m_carControllers;
    private Transform[] m_initialPositions;
    private GameObject m_race;
    private int m_carTarget = 0;
    private int m_carCount;
    private int m_mainCarId;
    private int[] m_ranking;
    private int m_nextRank;

    public GameObject m_uiPrefab;
    private UIDisplayer m_uiDisplayer;

    private float m_startTime;
    private float m_finishTime;

    private void Awake()
    {
        m_carCount = 1;
        m_mainCarId = 0;
        m_sceneMaster = FindObjectOfType<SceneMaster>();
        m_raceCamera = GameObject.FindGameObjectWithTag("RaceCamera").GetComponent<CameraController>();
        if (m_raceCamera == null)
        {
            Debug.Log("RaceCamera not found");
        }
        m_race = GameObject.FindGameObjectWithTag("Race");

        ChangeState(RaceState.Loading);

        if (m_debugModeOn)
        {
            SetUpRaceManager(RaceMode.Solo);
        }
    }

    private void OnDestroy()
    {
        RestoreMasterCamera();
    }

    public void ChangeState(RaceState raceState)
    {
        m_raceState = raceState;

        switch (raceState)
        {
            case RaceState.Loading:
                break;
            case RaceState.BeforeStartAnimation:
                for (int i = 0; i < m_carCount; ++i)
                {
                    m_carControllers[i].ChangeState(CarController.CarState.BeforeStart);
                }
                m_raceAnimationDirector.StartBeforeStartAnimation();
                break;
            case RaceState.StartTime:
                m_raceAnimationDirector.DisableCamera();
                m_startTime = m_timeBeforeStart;
                m_uiDisplayer.ChangeState(UIDisplayer.UIState.StartTime);
                break;
            case RaceState.Race:
                for (int i = 0; i < m_carCount; ++i)
                {
                    m_carControllers[i].ChangeState(CarController.CarState.Race);
                }
                if (m_IACarController != null)
                {
                    foreach (IAController iaController in m_IACarController)
                    {
                        iaController.StartRace();
                    }
                }
                m_uiDisplayer.ChangeState(UIDisplayer.UIState.Race);
                break;
            case RaceState.Finishing:
                m_finishTime = m_timeAfterFirstArrival;
                for (int i = 0; i < m_carCount; ++i)
                {
                    m_carControllers[i].ChangeState(CarController.CarState.Race);
                }
                m_uiDisplayer.ChangeState(UIDisplayer.UIState.Finishing);
                break;
            case RaceState.RaceFinished:
                m_sceneMaster.LoadResultsScreenInRace();
                for (int i = 0; i < m_carCount; ++i)
                {
                    if (!m_carControllers[i].HasFinished())
                    {
                        m_carControllers[i].ChangeState(CarController.CarState.RaceFinished);
                    }
                }
                if (m_IACarController != null)
                {
                    foreach (IAController iaController in m_IACarController)
                    {
                        iaController.EndRace();
                    }
                }
                m_uiDisplayer.ChangeState(UIDisplayer.UIState.Finished);
                m_raceAnimationDirector.StartAfterEndAnimation();
                break;
        }
    }

    public void SetUpRaceManager(RaceMode raceMode)
    {
        m_raceMode = raceMode;

        switch (raceMode)
        {
            case RaceMode.Solo:
            case RaceMode.SoloIA:
            case RaceMode.TimeAttack:
                m_carCount = 1;
                break;
            case RaceMode.VS1:
                m_carCount = 2;
                break;
            case RaceMode.VSN:
                m_carCount = 4;
                break;
        }

        SetUpInitialPositions();
        SetUpCars();
        SetUpPlayers();
        SetUpCamera();
        SetRaceCamera();

        m_uiDisplayer = Instantiate(m_uiPrefab, m_race.transform).GetComponent<UIDisplayer>();
    }

    private void SetUpInitialPositions()
    {
        m_initialPositions = new Transform[m_carCount];
        
        GameObject positionContainer = GameObject.FindGameObjectWithTag("InitialPositions");
        int positionIdx = 0;
        for (int i = 0; i < m_carCount; ++i)
        {
            m_initialPositions[positionIdx++] = positionContainer.transform.GetChild(i);
        }
    }

    private void ClearInitialPositions()
    {
        for (int i = 0; i < m_initialPositions.Length; ++i)
        {
            m_initialPositions[i] = null;
        }
        m_initialPositions = null;
    }

    private void SetUpCars()
    {
        CarCollection carCollection = FindObjectOfType<CarCollection>();
        if (carCollection == null)
        {
            Debug.Log("CarCollection not found");
        }
        m_cars = new GameObject[m_carCount];
        m_carControllers = new CarController[m_carCount];
        m_ranking = new int[m_carCount];
        for (int i = 0; i < m_carCount; ++i)
        {
            GameObject carPrefab;
            carCollection.GetCarPrefab(out carPrefab, 0);
            m_cars[i] = Instantiate(carPrefab, m_race.transform);
            m_cars[i].name = "Car" + i;
            m_cars[i].transform.position = m_initialPositions[i].position;
            m_cars[i].transform.rotation = m_initialPositions[i].rotation;
            m_carControllers[i] = m_cars[i].GetComponent<CarController>();
            m_carControllers[i].SetId(i);
            m_ranking[i] = -1;
        }
        m_nextRank = 0;
    }

    private void ClearCars()
    {
        for (int i = 0; i < m_carCount; ++i)
        {
            m_carControllers[i] = null;
            DestroyImmediate(m_cars[i]);
            m_cars[i] = null;
        }
        m_cars = null;
        m_carControllers = null;
        m_ranking = null;
    }

    private void SetUpPlayers()
    {
        int nCarsCreated = 0;

        if (m_raceMode != RaceMode.SoloIA)
        {
            m_playerController = gameObject.AddComponent<PlayerController>();
            nCarsCreated++;
        }

        if (nCarsCreated < m_carCount)
        {
            int IACount = m_carCount - nCarsCreated;
            m_IACarController = new IAController[IACount];
            for (int i = 0; i < IACount; ++i)
            {
                // SetUp IAControllers
                //m_IACarController[i] = new IAController();
                m_carControllers[i + nCarsCreated].gameObject.AddComponent<IAController>();
                m_IACarController[i] = m_carControllers[i + nCarsCreated].GetComponent<IAController>();
                //m_IACarController[i].SetCarController(ref m_carControllers[i]);
                m_IACarController[i].SetIALevel(IAController.IALevel.Basic);
            }
        }

        switch (m_raceMode)
        {
            case RaceMode.Solo:
            case RaceMode.TimeAttack:
                m_playerController.SetCarController(ref m_carControllers[m_mainCarId]);
                break;
            case RaceMode.SoloIA:
                // IAController
                break;
            case RaceMode.VS1:
                m_playerController.SetCarController(ref m_carControllers[m_mainCarId]);
                // IAController
                break;
            case RaceMode.VSN:
                m_playerController.SetCarController(ref m_carControllers[m_mainCarId]);
                // IAControllers
                break;
        }
    }

    private void SetUpCamera()
    {
        Transform cameraTarget;
        m_carControllers[m_carTarget].GetCameraPoint(out cameraTarget);
        m_raceCamera.SetTarget(ref cameraTarget);
    }

    private void SetUpScenary()
    {

    }

    private void SetPlayerController(ref PlayerController playerController)
    {
        m_playerController = playerController;
    }

    private void SetRaceCamera()
    {
        m_raceCamera.enabled = true;
        if (m_debugModeOn)
        {
            return;
        }
        m_sceneMaster.EnableMasterCamera(false);
    }

    private void RestoreMasterCamera()
    {
        if (m_raceCamera != null)
        {
            m_raceCamera.enabled = false;
        }
        if (m_debugModeOn)
        {
            return;
        }
        m_sceneMaster.EnableMasterCamera(true);
    }

    private void StartRace()
    {
        ChangeState(RaceState.Race);
    }

    private void Exit()
    {
        ClearInitialPositions();
        ClearCars();
        m_sceneMaster.UnloadRaceToTitleScreen();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Exit();
        }

        switch (m_raceState)
        {
            case RaceState.BeforeStartAnimation:
                if (m_raceAnimationDirector.HasBeforeStartAnimationFinished())
                {
                    ChangeState(RaceState.StartTime);
                }
                break;
            case RaceState.StartTime:
                m_startTime = Mathf.Max(m_startTime - Time.deltaTime, 0.0f);
                if (m_startTime == 0.0f)
                {
                    ChangeState(RaceState.Race);
                }
                break;
            case RaceState.Finishing:
                m_finishTime = Mathf.Max(m_finishTime - Time.deltaTime, 0.0f);
                if (m_finishTime == 0.0f)
                {
                    ChangeState(RaceState.RaceFinished);
                }
                break;
        }
    }

    public void CarEnteredGoal(GameObject car)
    {
        int carId = car.GetComponent<CarController>().GetId();
        for (int i = 0; i < m_nextRank; ++i)
        {
            // Check multiple collider detection
            if (m_ranking[i] == carId)
            {
                return;
            }
        }

        if (m_raceState == RaceState.Race)
        {
            if (m_carCount == 1 || carId == m_mainCarId)
            {
                ChangeState(RaceState.RaceFinished);
            }
            else
            {
                ChangeState(RaceState.Finishing);
                if (m_IACarController.Length > 0)
                {
                    m_IACarController[carId - 1].EndRace();
                }
            }
            m_ranking[m_nextRank++] = carId;
        }
        else if (m_raceState == RaceState.Finishing)
        {
            m_ranking[m_nextRank++] = carId;
            if (m_nextRank < m_carCount)
            {
                ChangeState(RaceState.RaceFinished);
            }
        }
    }

    public void GetMainCarController(out CarController carController)
    {
        carController = m_carControllers[m_mainCarId];
    }

    public float GetStartTimer()
    {
        return m_startTime;
    }
    
    public void GetRaceCamera(out Camera raceCamera)
    {
        m_raceCamera.GetCamera(out raceCamera);
    }

    public RaceMode GetRaceMode()
    {
        return m_raceMode;
    }

    public int GetCarCount()
    {
        return m_carCount;
    }

    public int GetMainCarPosition()
    {
        int position = -1;

        if (m_carControllers[m_mainCarId].HasFinished())
        {
            for (int i = 0; i < m_carCount; ++i)
            {
                if (m_ranking[i] == m_mainCarId)
                {
                    position = i + 1;
                    break;
                }
            }
        }

        return position;
    }

    public float GetMainCarTime()
    {
        return m_carControllers[m_mainCarId].GetRaceTime();
    }

}
