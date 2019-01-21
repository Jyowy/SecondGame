using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    private bool m_isPlayerInputBlocked;
    
    private CameraController m_cameraController;
    private CarController m_carController;

    private float k_horizontalMoveThreshold = 0.0625f;
    private float k_verticalMoveThreshold = 0.0625f;
    private float k_breakMoveThreshold = 0.0625f;

    private void Awake()
    {
        m_isPlayerInputBlocked = false;
    }

    public void SetPlayerInputBlocked(bool blockPlayerInput)
    {
        m_isPlayerInputBlocked = blockPlayerInput;
    }

    public void SetCarController(ref CarController carController)
    {
        m_carController = carController;
    }
    
    public void GetCarPosition(out Transform carPosition)
    {
        m_carController.GetCarPosition(out carPosition);
    }

    public void SetCarPosition(ref Transform carPosition)
    {
        m_carController.SetCarPosition(ref carPosition);
    }

    public int GetCameraCount()
    {
        return m_carController.GetCameraCount();
    }

    public void GetCameraTransform(out Transform cameraTransform, int index)
    {
        m_carController.GetCameraTransform(out cameraTransform, index);
    }

    private void Update()
    {
        if (m_isPlayerInputBlocked)
        {
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float _break = Input.GetAxis("Break");
        if (Mathf.Abs(vertical) > k_verticalMoveThreshold)
        {
            m_carController.MoveFrontal(vertical);
        }
        if (Mathf.Abs(horizontal) > k_horizontalMoveThreshold)
        {
            m_carController.MoveLateral(horizontal);
        }
        if (Mathf.Abs(_break) > k_breakMoveThreshold)
        {
            m_carController.Break(_break);
        }
    }

}
