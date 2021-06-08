using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour {

    public int coinIndex = -1;
    public string levelName = string.Empty;

    [SerializeField]
    private GameObject m_coinPointLight;

	public void Collect() {
        // TODO: More fancy collection
    
        gameObject.SetActive(false);
        // PlayerPrefs.SetInt("Level" + levelName + "Coin" + coinIndex + "Collected", 1);
    }

    public void EnableLight() {
        m_coinPointLight.SetActive(true);
    }
}
