using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour {

    private RaceManager m_raceManager;

    private void Start()
    {
        m_raceManager = FindObjectOfType<RaceManager>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Car"))
        {
            m_raceManager.CarEnteredGoal(collider.gameObject);
        }
    }

}
