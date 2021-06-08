using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsHandler : MonoBehaviour {

    [SerializeField]
    private GameObject m_audioOptions;
    [SerializeField]
    private GameObject m_controlsOptions;

    [SerializeField]
    private Toggle[] m_controlToggles;

    private GameManager m_gm = null;

    void Start() {
        m_gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        if(PlayerPrefs.HasKey("SavedControlOption")) {
            switch(PlayerPrefs.GetInt("SavedControlOption")) {
                case 0:
                    ToggleTouch();
                    break;

                case 1:
                    ToggleTilt();
                    break;

                case 2:
                    ToggleRotate();
                    break;
            }
        } else {
            ToggleTouch();
        }

        SwitchOptions(true, false);
        ToggleCheckAndDisable(0); // TODO: The passed in index will be the saved data from playerPrefs
    }

    public void ShowAudioOptions() {
        SwitchOptions(true, false);
    }

    public void ShowContolsOptions() {
        SwitchOptions(false, true);
    }

    public void ToggleTouch() {
        int index = 0;
        ToggleCheckAndDisable(index);
    }

    public void ToggleTilt() {
        int index = 1;
        ToggleCheckAndDisable(index);
    }

    public void ToggleRotate() {
        int index = 2;
        ToggleCheckAndDisable(index);
    }

    private void ToggleCheckAndDisable(int index) {
        if (m_controlToggles[index].isOn) {
            PlayerPrefs.SetInt("SavedControlOption", index);
            m_gm.controlMode = index;
            for (int i = 0; i < m_controlToggles.Length; i++) {
                if (i != index) {
                    m_controlToggles[i].isOn = false;
                }
            }

            m_gm.controlMode = index;
        }
    }

    private void SwitchOptions(bool audio, bool controls) {
        m_audioOptions.SetActive(audio);
        m_controlsOptions.SetActive(controls);
    }
}