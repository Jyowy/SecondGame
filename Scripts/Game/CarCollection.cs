using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCollection : MonoBehaviour {

    public GameObject[] m_carPrefabs;

    public void GetCarPrefab(out GameObject carPrefab, int index)
    {
        int id = Mathf.Clamp(index, 0, m_carPrefabs.Length - 1);
        carPrefab = m_carPrefabs[id];
    }

    public int GetCarCount()
    {
        return m_carPrefabs.Length;
    }

}
