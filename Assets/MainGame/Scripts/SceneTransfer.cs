using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneTransfer : MonoBehaviour {

    [SerializeField] private Transform m_pauseMenu = null;
    [SerializeField] private float m_pauseMenuHiddenX;
    [SerializeField] private float m_pauseMenuShownX;
    [SerializeField] private float m_transSpeed = 10;

    private GameManager m_gm = null;
    private Transform m_player = null;
    private bool m_menuActive = false;

    void Start() {
        m_gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        m_player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update() {
        if (m_menuActive) {
            m_pauseMenu.localPosition = Vector3.Lerp(m_pauseMenu.localPosition, new Vector3(m_pauseMenuShownX, m_pauseMenu.localPosition.y, m_pauseMenu.localPosition.z), m_transSpeed * Time.deltaTime);
        } else {
            m_pauseMenu.localPosition = Vector3.Lerp(m_pauseMenu.localPosition, new Vector3(m_pauseMenuHiddenX, m_pauseMenu.localPosition.y, m_pauseMenu.localPosition.z), m_transSpeed * Time.deltaTime);
        }
    }

    void OnApplicationQuit() {
        CheckAndSave();
    }

    public void ToMainMenu() {
        CheckAndSave();
        SceneManager.LoadScene("MainScene");
    }

    public void ShowHidePauseMenu() {
        m_menuActive = !m_menuActive;
    }

    private void CheckAndSave() {
        m_gm.GetCurrentLevel();

        if (m_gm.levelCompleted) {
            // TODO: find out if there is anyhting special i need to do when not saveing progress
            PlayerPrefs.SetInt("Level" + m_gm.levelName + "MidLevelSaveExists", 0);
            PlayerPrefs.SetInt("Level" + m_gm.levelName + "MapSeed", -1);
        } else {
            m_gm.timer.Stop();

            PlayerPrefs.SetInt("Level" + m_gm.levelName + "MidLevelSaveExists", 1);

            int playerPosX = ((((int)m_player.transform.position.x / 10) * 10) + 5);
            int playerPosZ = ((((int)m_player.transform.position.z / 10) * 10) + 5);

            PlayerPrefs.SetInt("Level" + m_gm.levelName + "PlayerX", playerPosX);
            PlayerPrefs.SetInt("Level" + m_gm.levelName + "PlayerZ", playerPosZ);

            PlayerPrefs.SetInt("Level" + m_gm.levelName + "SavedHours", m_gm.currentHours);
            PlayerPrefs.SetInt("Level" + m_gm.levelName + "SavedMinuts", m_gm.currentMinutes);
            PlayerPrefs.SetInt("Level" + m_gm.levelName + "SavedSeconds", m_gm.currentSeconds);

            if (m_gm.secondaryLevelNum == 1) {
                // TODO: Save Visible cell x and z values
                List<int> positions = new List<int>();
                for (int i = 0; i < m_gm.mazeCells.Count; i++) {
                    if (m_gm.mazeCells[i].coverRemoved) {
                        positions.Add((int)m_gm.mazeCells[i].transform.position.x);
                        positions.Add((int)m_gm.mazeCells[i].transform.position.z);
                    }
                }
                PlayerPrefs.SetString("Level" + m_gm.levelName + "SavedVisibleCells", m_gm.GetSerializedString(positions));
            }
        }
    }
}
