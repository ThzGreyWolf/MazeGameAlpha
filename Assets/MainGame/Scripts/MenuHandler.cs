using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour {

    [SerializeField]
    private GameObject m_mainMenu;
    [SerializeField]
    private GameObject m_levelSelectMenu;
    [SerializeField]
    private GameObject m_optionsMenu;

    private GameManager m_gm = null;

    void Start() {
        m_gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        
        if(!m_gm.firstTimeArrival) {
            ShowLevelSelectMenu();
        } else {
            m_gm.firstTimeArrival = false;
        }

        CloseOptionsMenu();
    }

    public void ShowMainMenu() {
        if(m_levelSelectMenu.activeInHierarchy) {
            m_levelSelectMenu.SetActive(false);
        }

        if(m_optionsMenu.activeInHierarchy) {
            m_optionsMenu.SetActive(false);
        }

        m_mainMenu.SetActive(true);
    }

    public void ShowLevelSelectMenu() {
        if(m_mainMenu.activeInHierarchy) {
            m_mainMenu.SetActive(false);
        }

        m_levelSelectMenu.SetActive(true);
    }

    public void ShowOptionsMenu() {
        m_optionsMenu.SetActive(true);
    }

    public void CloseOptionsMenu() {
        m_optionsMenu.SetActive(false);
    }
}
