using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceAnimationDirector : MonoBehaviour {

    public Camera m_cameraPrefab;
    public RaceAnimation m_beforeStartAnimation;
    public RaceAnimation m_afterEndAnimation;

    private Camera m_camera;

    private void Awake()
    {
        GameObject race = GameObject.FindGameObjectWithTag("Race");
        m_camera = Instantiate(m_cameraPrefab, race.transform);
    }

    private void OnDestroy()
    {
        m_camera = null;
    }

    public void GetCamera(out Camera camera)
    {
        camera = m_camera;
    }

    public void StartBeforeStartAnimation()
    {
        m_camera.enabled = true;
        m_beforeStartAnimation.StartAnimation();
    }

    public void StartAfterEndAnimation()
    {
        m_camera.enabled = true;
        m_afterEndAnimation.StartAnimation();
    }

    public bool HasBeforeStartAnimationFinished()
    {
        return m_beforeStartAnimation.HasFinished();
    }

    public bool HasAfterEndAnimationFinished()
    {
        return m_afterEndAnimation.HasFinished();
    }

    public void DisableCamera()
    {
        m_camera.enabled = false;
    }

}
