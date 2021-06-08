using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeExit : MonoBehaviour {
    
    [SerializeField] private GameObject m_exitPointLight = null;

    public void EnableLight() {
        m_exitPointLight.SetActive(true);
    }
}
