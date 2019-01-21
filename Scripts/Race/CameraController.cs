using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    
    public float m_cameraMoveSpeed = 10000.0f;
    public float m_cameraRotationSpeed = 10000.0f;
    public float m_minDistance = 0.0f;
    public float m_minAngle = 0.0f;

    private Transform m_target;
    private Vector3 m_newCameraPosition = Vector3.zero;
    private Quaternion m_newCameraRotation = Quaternion.identity;
    private CarController m_carController;
    private Camera m_camera;
    private int m_cameraIndex;
    private int m_cameraCount;
    private PlayerController m_playerController;
    private Transform m_newCameraTransform;
    private Transform m_targetCameraTransform;
    private Transform m_playerTransform;

    private void Awake()
    {
        m_cameraIndex = 0;
        m_cameraCount = 0;
        m_camera = GetComponent<Camera>();
    }

    public void SetCamera(ref Camera camera)
    {
        m_camera = camera;
    }

    public void SetTarget(ref Transform target)
    {
        m_target = target;
    }

    private void FixedUpdate()
    {
        if (m_target != null)
        {
            // Position
            float distance = Vector3.Distance(m_camera.transform.position, m_target.transform.position);
            if (distance > m_minDistance)
            {
                float distanceThisFrame = m_cameraMoveSpeed * Time.fixedDeltaTime;
                float distancePercentage = Mathf.Min(distanceThisFrame / distance, 1.0f);
                m_newCameraPosition = Vector3.Lerp(m_camera.transform.position, m_target.transform.position, distancePercentage);
                
                m_camera.transform.position = m_newCameraPosition;
            }

            // Rotation
            float degrees = Quaternion.Angle(m_camera.transform.rotation, m_target.transform.rotation);
            if (degrees > m_minAngle)
            {
                float degreesThisFrame = m_cameraRotationSpeed * Time.fixedDeltaTime;
                float degreesPercentage = Mathf.Min(degreesThisFrame / degrees, 1.0f);
                m_newCameraRotation = Quaternion.Lerp(m_camera.transform.rotation, m_target.transform.rotation, degreesPercentage);

                m_newCameraRotation.x = 0.1f;
                m_newCameraRotation.y = 0.0f;
                m_camera.transform.rotation = m_newCameraRotation;
            }
        }
    }

    public void NextCamera()
    {
        m_cameraIndex = Mathf.Min(m_cameraIndex + 1, m_cameraCount - 1);
    }

    public void PrevCamera()
    {
        m_cameraIndex = Mathf.Max(m_cameraIndex - 1, 0);
    }

    public void GetCamera(out Camera camera)
    {
        camera = m_camera;
    }

}
