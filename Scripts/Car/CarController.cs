using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {

    // public
    
    public GameObject m_cameraPoints;

    public float m_forwardSpeed = 300.0f;
    public float m_forwardAcceleration = 1.5f;
    public float m_lateralSpeed = 50.0f;
    public float m_lateralAcceleration = 0.75f;
    public float m_backwardSpeed = 100.0f;
    public float m_backwardAcceleration = 1.0f;
    public float m_decelerateRatio = 0.5f;
    public float m_breakDecelerateRatio = 1.0f;
    public float m_carCollisionTime = 0.5f;
    public float m_carCollisionSpeed = 100.0f;

    public float m_distanceFromFloor = 2.0f;
    public float m_verticalSpeed = 10.0f;
    public float m_rotationSpeed = 10.0f;

    public enum CarState
    {
        Waiting,
        BeforeStart,
        Race,
        RaceFinished
    };

    public enum CarAction
    {
        Nothing,
        Forward,
        Backward,
        Right,
        Left,
        Break
    };

    // private
    
    private int m_id;
    private CarState m_carState;

    private Rigidbody m_rigidbody;
    private Transform[] m_cameraTransforms;

    private float m_frontalInput;
    private float m_lateralInput;
    private float m_breakInput;

    private float m_currentFrontalSpeed;
    private float m_currentBackwardSpeed;
    private float m_currentRightSpeed;
    private float m_currentLeftSpeed;
    
    private float m_carCollisionTimer;
    private float m_currentCarCollisionSpeed;

    private float m_speed;
    private Vector3 m_nextPosition;
    private Quaternion m_nextRotation;

    private int m_cameraIndex;
    private int m_cameraCount;

    private float m_raceTime;

    // Sensors

    private enum Sensors
    {
        CENTER = 0,
        FRONT_RIGHT,
        FRONT_LEFT,
        BACK_RIGHT,
        BACK_LEFT,
        SIZE
    };
    
    private Vector3[] m_sensorPositions;
    private float[] m_sensorDistances;
    private Vector3 m_carSize;

    private int m_scenaryLayer;
    private Ray m_sensorRay;
    private float m_maxDistanceSensorRay;

    //

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        if (m_cameraPoints != null)
        {
            m_cameraCount = m_cameraPoints.transform.childCount;
            if (m_cameraCount > 0)
            {
                int cameraPointIdx = 0;
                m_cameraTransforms = new Transform[m_cameraCount];
                foreach (Transform transform in m_cameraPoints.transform)
                {
                    m_cameraTransforms[cameraPointIdx++] = transform;
                }
            }
            else
            {
                Debug.Log("Camera Points is empty in car '" + name + "'");
            }
        }
        else
        {
            Debug.Log("Camera Points not found in car '" + name + "'");
        }

        m_currentFrontalSpeed = 0.0f;
        m_currentBackwardSpeed = 0.0f;
        m_currentRightSpeed = 0.0f;
        m_currentLeftSpeed = 0.0f;
        m_currentCarCollisionSpeed = 0.0f;
        m_speed = 0.0f;
        m_raceTime = 0.0f;
        m_carCollisionTimer = 0.0f;

        m_carSize = GetComponent<BoxCollider>().size;
        m_carSize.Scale(m_rigidbody.transform.localScale);
        m_sensorPositions = new Vector3[(int)Sensors.SIZE];
        m_sensorPositions[(int)Sensors.CENTER] = new Vector3(0.0f, -m_carSize.y * 0.45f, 0.0f);
        m_sensorPositions[(int)Sensors.FRONT_RIGHT] = new Vector3(m_carSize.x * 0.45f, -m_carSize.y * 0.45f, m_carSize.z * 0.45f);
        m_sensorPositions[(int)Sensors.FRONT_LEFT] = new Vector3(-m_carSize.x * 0.45f, -m_carSize.y * 0.45f, m_carSize.z * 0.45f);
        m_sensorPositions[(int)Sensors.BACK_RIGHT] = new Vector3(m_carSize.x * 0.45f, -m_carSize.y * 0.45f, -m_carSize.z * 0.45f);
        m_sensorPositions[(int)Sensors.BACK_LEFT] = new Vector3(-m_carSize.x * 0.45f, -m_carSize.y * 0.45f, -m_carSize.z * 0.45f);

        for (int i = 0; i < (int)Sensors.SIZE; ++i)
        {
            Debug.Log("Sensor " + (Sensors)i + " origin: " + m_sensorPositions[i]);
        }
        m_sensorDistances = new float[(int)Sensors.SIZE];
        m_sensorRay = new Ray();
        m_maxDistanceSensorRay = m_distanceFromFloor * 10.0f;
        m_scenaryLayer = LayerMask.NameToLayer("Scenary");
        Debug.Log("ScenaryLayer = " + m_scenaryLayer);

        ChangeState(CarState.Waiting);
    }

    public void ChangeState(CarState carState)
    {
        m_carState = carState;

        switch (carState)
        {
            case CarState.Waiting:
                break;
            case CarState.BeforeStart:
                break;
            case CarState.Race:
                break;
            case CarState.RaceFinished:
                break;
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < m_cameraTransforms.Length; ++i)
        {
            m_cameraTransforms[i] = null;
        }
        m_cameraTransforms = null;
    }

    public void SetId(int id)
    {
        m_id = id;
    }

    public int GetId()
    {
        return m_id;
    }

    public void SetNextCarAction(CarAction carAction)
    {
        switch(carAction)
        {
            case CarAction.Nothing:
                break;
            case CarAction.Forward:
                MoveFrontal(1.0f);
                break;
            case CarAction.Backward:
                MoveFrontal(-1.0f);
                break;
            case CarAction.Right:
                MoveLateral(1.0f);
                break;
            case CarAction.Left:
                MoveLateral(-1.0f);
                break;
            case CarAction.Break:
                Break(1.0f);
                break;
        }
    }

    public void MoveFrontal(float frontal)
    {
        m_frontalInput = frontal;
    }

    public void MoveLateral(float lateral)
    {
        m_lateralInput = lateral;
    }

    public void Break(float _break)
    {
        m_breakInput = _break;
    }

    public void GetRigidbody(out Rigidbody rigidbody)
    {
        rigidbody = m_rigidbody;
    }

    public void GetCarPosition(out Transform position)
    {
        position = m_rigidbody.transform;
    }

    public void SetCarPosition(ref Transform position)
    {
        m_rigidbody.position = position.position;
        m_rigidbody.rotation = position.rotation;
    }

    public int GetCameraCount()
    {
        return m_cameraCount;
    }

    public void GetCameraTransform(out Transform cameraTransform, int index)
    {
        int id = Mathf.Clamp(index, 0, m_cameraCount - 1);
        cameraTransform = m_cameraTransforms[id];
    }

    public void GetCameraPoint(out Transform cameraPoint)
    {
        cameraPoint = m_cameraTransforms[m_cameraIndex];
    }

    public bool HasFinished()
    {
        return m_carState == CarState.RaceFinished;
    }
        
    private void FixedUpdate()
    {
        UpdateCarHeightAndRotation();
        if (m_carState == CarState.Race)
        {
            UpdateCarSpeed();
        }
    }

    private void UpdateCarHeightAndRotation()
    {
        RaycastHit hitInfo;
        m_sensorRay.direction = -m_rigidbody.transform.up;
        bool hit = false;

        for (int i = 0; i < (int) Sensors.SIZE; ++i)
        {
            m_sensorRay.origin = m_rigidbody.transform.position;
            m_sensorRay.origin += m_rigidbody.transform.right * m_sensorPositions[i].x +
                m_rigidbody.transform.up * m_sensorPositions[i].y +
                m_rigidbody.transform.forward * m_sensorPositions[i].z;
            hit = Physics.Raycast(m_sensorRay, out hitInfo, m_maxDistanceSensorRay);//, m_scenaryLayer);
            m_sensorDistances[i] = hit ? hitInfo.distance : m_maxDistanceSensorRay;
            Debug.DrawLine(m_sensorRay.origin, m_sensorRay.origin + m_sensorRay.direction * m_maxDistanceSensorRay, Color.blue);
        }
        
        // Update Height
        float distanceFromFloor = Mathf.Min(m_sensorDistances);
        float heightDiff = distanceFromFloor - m_distanceFromFloor;
        if (Mathf.Abs(heightDiff) > 0.05f)
        {
            Vector3 correction = m_rigidbody.transform.up * Mathf.Min(m_verticalSpeed * Time.fixedDeltaTime, Mathf.Abs(heightDiff));
            if (distanceFromFloor > m_distanceFromFloor)
            {
                m_nextPosition = m_rigidbody.position - correction;
            }
            else
            {
                m_nextPosition = m_rigidbody.position + correction;
            }
            m_rigidbody.position = m_nextPosition;
        }

        // Update Rotation
        m_nextRotation = m_rigidbody.transform.rotation;

        float rightHeight = Mathf.Min(m_sensorDistances[(int)Sensors.FRONT_RIGHT], m_sensorDistances[(int)Sensors.BACK_RIGHT]);
        float leftHeight = Mathf.Min(m_sensorDistances[(int)Sensors.FRONT_LEFT], m_sensorDistances[(int)Sensors.BACK_LEFT]);
        
        float lateralDiff = leftHeight - rightHeight;
        if (Mathf.Abs(lateralDiff) > 0.05f)
        {
            float zRad = lateralDiff / Mathf.Sqrt(lateralDiff * lateralDiff + m_carSize.x * 0.5f);
            float zDeg = Mathf.Asin(zRad) * Mathf.Rad2Deg * 0.75f;
            //float correction = zDeg * Mathf.Min(m_rotationSpeed * Time.fixedDeltaTime, 1.0f);
            if (Mathf.Abs(zDeg) > 1.0f)
            {
                float correction = Mathf.Min(m_rotationSpeed * Time.fixedDeltaTime, zDeg);
                m_nextRotation *= Quaternion.Euler(0.0f, 0.0f, correction);
            }
        }

        float frontHeight = Mathf.Min(m_sensorDistances[(int)Sensors.FRONT_RIGHT], m_sensorDistances[(int)Sensors.FRONT_LEFT]);
        float backHeight = Mathf.Min(m_sensorDistances[(int)Sensors.BACK_RIGHT], m_sensorDistances[(int)Sensors.BACK_LEFT]);

        float frontalDiff = frontHeight - backHeight;
        if (Mathf.Abs(frontalDiff) > 0.05f)
        {
            float xRad = frontalDiff / Mathf.Sqrt(frontalDiff * frontalDiff + m_carSize.z * 0.5f);
            float xDeg = Mathf.Asin(xRad) * Mathf.Rad2Deg;
            //float correction = xDeg * Mathf.Min(m_rotationSpeed * Time.fixedDeltaTime, 1.0f);
            float correction = Mathf.Min(m_rotationSpeed * Time.fixedDeltaTime, xDeg);
            m_nextRotation *= Quaternion.Euler(correction, 0.0f, 0.0f);
        }

        //m_rigidbody.rotation = m_nextRotation;
        if (m_rigidbody.rotation != m_nextRotation)
        {
            m_nextRotation.y = 0.0f;
            m_rigidbody.MoveRotation(m_nextRotation);
        }

        Debug.Log(m_rigidbody.rotation);
    }

    private void UpdateCarSpeed()
    {
        m_nextPosition = m_rigidbody.position;

        // Set Position

        // Frontal

        if (m_breakInput > 0.0f)
        {
            m_currentFrontalSpeed = Mathf.Max(m_currentFrontalSpeed - Time.fixedDeltaTime / (m_forwardAcceleration * m_breakDecelerateRatio), 0.0f);
        }
        else if (m_frontalInput > 0.0f)
        {
            m_currentFrontalSpeed = Mathf.Min(m_currentFrontalSpeed + Time.fixedDeltaTime / m_forwardAcceleration, 1.0f);
        }
        else
        {
            //m_currentFrontalSpeed = Mathf.Max(m_currentFrontalSpeed - Time.fixedDeltaTime / (m_forwardAcceleration * m_decelerateRatio), 0.0f);
        }

        // Backward

        if (m_breakInput > 0.0f)
        {
            m_currentBackwardSpeed = Mathf.Max(m_currentBackwardSpeed - Time.fixedDeltaTime / (m_backwardAcceleration * m_breakDecelerateRatio), 0.0f);
        }
        else if (m_frontalInput < 0.0f)
        {
            m_currentBackwardSpeed = Mathf.Min(m_currentBackwardSpeed + Time.fixedDeltaTime / m_backwardAcceleration, 1.0f);
        }
        else
        {
            m_currentBackwardSpeed = Mathf.Max(m_currentBackwardSpeed - Time.fixedDeltaTime / (m_backwardAcceleration * m_decelerateRatio), 0.0f);
        }

        // Right

        if (m_carCollisionTimer > 0.0f)
        {
            m_carCollisionTimer -= Time.fixedDeltaTime;
            if (m_carCollisionTimer <= 0.0f)
            {
                m_carCollisionTimer = 0.0f;
                m_currentCarCollisionSpeed = 0.0f;
            }
        }
        else
        {
            if (m_breakInput > 0.0f)
            {
                m_currentRightSpeed = Mathf.Max(m_currentRightSpeed - Time.fixedDeltaTime / (m_lateralAcceleration * m_breakDecelerateRatio), 0.0f);
            }
            else if (m_lateralInput > 0.0f)
            {
                m_currentRightSpeed = Mathf.Min(m_currentRightSpeed + Time.fixedDeltaTime / m_lateralAcceleration, 1.0f);
            }
            else
            {
                m_currentRightSpeed = Mathf.Max(m_currentRightSpeed - Time.fixedDeltaTime / (m_lateralAcceleration * m_decelerateRatio), 0.0f);
            }

            // Left

            if (m_breakInput > 0.0f)
            {
                m_currentLeftSpeed = Mathf.Max(m_currentLeftSpeed - Time.fixedDeltaTime / (m_lateralAcceleration * m_breakDecelerateRatio), 0.0f);
            }
            else if (m_lateralInput < 0.0f)
            {
                m_currentLeftSpeed = Mathf.Min(m_currentLeftSpeed + Time.fixedDeltaTime / m_lateralAcceleration, 1.0f);
            }
            else
            {
                m_currentLeftSpeed = Mathf.Max(m_currentLeftSpeed - Time.fixedDeltaTime / (m_lateralAcceleration * m_decelerateRatio), 0.0f);
            }
        }

        //

        float frontalSpeed = (m_currentFrontalSpeed * m_forwardSpeed - m_currentBackwardSpeed * m_backwardSpeed) * Time.fixedDeltaTime;
        float lateralSpeed = 0.0f;
        if (m_currentCarCollisionSpeed != 0.0f)
        {
            lateralSpeed = m_currentCarCollisionSpeed * Time.fixedDeltaTime;
        }
        else
        {
            lateralSpeed = (m_currentRightSpeed - m_currentLeftSpeed) * m_lateralSpeed * Time.fixedDeltaTime;
        }

        m_nextPosition += m_rigidbody.transform.forward * frontalSpeed + m_rigidbody.transform.right * lateralSpeed;
        
        m_frontalInput = 0.0f;
        m_lateralInput = 0.0f;
        m_breakInput = 0.0f;

        m_speed = frontalSpeed;
        m_rigidbody.position = m_nextPosition;
        //m_rigidbody.MovePosition(m_nextPosition);
    }

    private void Update()
    {
        if (m_carState == CarState.Race)
        {
            m_raceTime += Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Car"))
        {
            GameObject car = collider.gameObject;
            OnCarCollision(ref car);
        }
    }

    private void OnCarCollision(ref GameObject car)
    {
        Vector3 cross = Vector3.Cross(transform.forward, car.transform.position - transform.position);
        m_carCollisionTimer = m_carCollisionTime;
        if ((transform.up.y >= 0.0f && cross.y < 0.0f)
            || (transform.up.y < 0.0f && cross.x < 0.0f))
        {
            m_currentCarCollisionSpeed = m_carCollisionSpeed;
            //Debug.Log(name + " is in the right");
        }
        else
        {
            m_currentCarCollisionSpeed = -m_carCollisionSpeed;
            //Debug.Log(name + " is in the left");
        }
    }
    
    public void NextCamera()
    {
        m_cameraIndex = Mathf.Min(m_cameraIndex + 1, m_cameraCount - 1);
    }

    public void PrevCamera()
    {
        m_cameraIndex = Mathf.Max(0, m_cameraIndex - 1);
    }

    public float GetSpeed()
    {
        return m_speed;
    }

    public float GetRaceTime()
    {
        return m_raceTime;
    }

}
