using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCreater : MonoBehaviour {

    public GameObject levelButtonPrefab = null;
    public Transform levelsParent = null;
    public int numLevels = 0;

    [SerializeField]
    private float m_startX = 0;
    [SerializeField]
    private float m_startY = 0;
    [SerializeField]
    private float m_gapX = 0;
    [SerializeField]
    private float m_gapY = 0;
    [SerializeField]
    private float m_numLevelsInRow = 0;

    private int m_levelCounter = 0;
    private bool m_currentCheckDone = true;
    private bool m_newLevelNeeded = true;

	void Start () {
        LevelDetails spawnedButton = null;

        while(PlayerPrefs.HasKey("Level" + m_levelCounter + "" + 0 + "IsActive") && m_currentCheckDone) {
            // Create a level button which already exitsts in playerprefs
            // Check if it had been completed 
            // if it has been completed
            // create it, mark it completed, check its points and time, then state it, and set m_currentCheckDone as true and increment m_levelCounter so the next level can be checked and made
            // if it's not been completed
            // create it as a new level and end the creation sequence

            m_currentCheckDone = false;

            spawnedButton = CreateLevelButton(m_levelCounter).GetComponent<LevelDetails>();
            // spawnedButton.levelNum = m_levelCounter;
            // spawnedButton.secondoryLevelNum = 0;
            // spawnedButton.SetLevelNameAndIndicators(m_levelCounter + "" + 0);
            SetLevelButtonDetails(spawnedButton, m_levelCounter, 0);

            if (PlayerPrefs.GetInt("Level" + m_levelCounter + "" + 0 + "IsCompleted") == 1) {
                // Level is completed
                spawnedButton.completed = true;
                spawnedButton.score = PlayerPrefs.GetInt("Level" + m_levelCounter + "" + 0 + "Score");
                
                LevelDetails spawnedSubLevelButton = CreateLevelButton(m_levelCounter, 1).GetComponent<LevelDetails>();
                // spawnedSubLevelButton.levelNum = m_levelCounter;
                // spawnedSubLevelButton.secondoryLevelNum = 1;
                // spawnedSubLevelButton.SetLevelNameAndIndicators(m_levelCounter + "" + 1);
                SetLevelButtonDetails(spawnedSubLevelButton, m_levelCounter, 1);

                // Creating 
                if (PlayerPrefs.HasKey("Level" + m_levelCounter + "" + 1 + "IsActive")) {
                    if(PlayerPrefs.GetInt("Level" + m_levelCounter + "" + 1 + "IsCompleted") == 1) {
                        spawnedSubLevelButton.completed = true;
                        spawnedSubLevelButton.score = PlayerPrefs.GetInt("Level" + m_levelCounter + "" + 1 + "Score");

                        LevelDetails spawnedSecondSubLevelButton = CreateLevelButton(m_levelCounter, 2).GetComponent<LevelDetails>();
                        // spawnedSecondSubLevelButton.levelNum = m_levelCounter;
                        // spawnedSecondSubLevelButton.secondoryLevelNum = 2;
                        // spawnedSecondSubLevelButton.SetLevelNameAndIndicators(m_levelCounter + "" + 2);
                        SetLevelButtonDetails(spawnedSecondSubLevelButton, m_levelCounter, 2);

                        if (PlayerPrefs.HasKey("Level" + m_levelCounter + "" + 2 + "IsActive")) {
                            if (PlayerPrefs.GetInt("Level" + m_levelCounter + "" + 2 + "IsCompleted") == 1) {
                                spawnedSecondSubLevelButton.completed = true;
                                spawnedSecondSubLevelButton.score = PlayerPrefs.GetInt("Level" + m_levelCounter + "" + 2 + "Score");
                            }
                        } else {
                            CreateNewLevelPlayerPrefValues(m_levelCounter + "" + 2);
                        }
                    }
                } else {
                    CreateNewLevelPlayerPrefValues(m_levelCounter + "" + 1, true);
                }

                m_currentCheckDone = true;
                m_levelCounter++;
            } else {
                // Level is not completed
                spawnedButton.completed = false;
                m_newLevelNeeded = false;
            }
        }

        
        if(m_newLevelNeeded) {
            // Create the new level since whatever level is not active in the player prefs
            spawnedButton = CreateLevelButton(m_levelCounter).GetComponent<LevelDetails>();
            // spawnedButton.levelNum = m_levelCounter;
            // spawnedButton.secondoryLevelNum = 0;
            // spawnedButton.SetLevelNameAndIndicators(m_levelCounter + "" + 0);
            SetLevelButtonDetails(spawnedButton, m_levelCounter, 0);

            CreateNewLevelPlayerPrefValues(m_levelCounter + "" + 0);
        }

        levelsParent.GetComponent<RectTransform>().sizeDelta = new Vector2(1500, (350 + (m_levelCounter * 300)) + 100);

        // for(var i = 0; i < numLevels; i++) {
        //    if(i != 0 && i % 6 == 0) {
        //        m_startY += m_gapY;
        //        m_startX -= m_gapX * 6;
        //    }

        //    GameObject spawnedButton = GameObject.Instantiate(levelButtonPrefab, new Vector3(m_startX, m_startY, 0f), Quaternion.identity, levelsParent) as GameObject;
        //    spawnedButton.transform.localPosition = new Vector3(m_startX, m_startY, 0f);

        //    spawnedButton.GetComponent<LevelDetails>().mazeSize = 5 + i;

        //    m_startX += m_gapX;
        // }
    }

    private void SetLevelButtonDetails(LevelDetails ld, int lc, int sli) {
        ld.levelNum = lc;
        ld.secondoryLevelNum = sli;
        ld.SetLevelNameAndIndicators(lc, sli);
    }

    private void CreateNewLevelPlayerPrefValues(string levelName, bool spcLevel = false) {
        PlayerPrefs.SetInt("Level" + levelName + "IsActive", 1);
        PlayerPrefs.SetInt("Level" + levelName + "IsCompleted", 0);
        PlayerPrefs.SetInt("Level" + levelName + "Score", 0);

        PlayerPrefs.SetInt("Level" + levelName + "Hours", 999);
        PlayerPrefs.SetInt("Level" + levelName + "Minuts", 99);
        PlayerPrefs.SetInt("Level" + levelName + "Seconds", 99);

        PlayerPrefs.SetInt("Level" + levelName + "CoinPositionsSet", 0);

        PlayerPrefs.SetInt("Level" + levelName + "Coin0Collected", 0);
        PlayerPrefs.SetInt("Level" + levelName + "Coin0X", -1);
        PlayerPrefs.SetInt("Level" + levelName + "Coin0Z", -1);

        PlayerPrefs.SetInt("Level" + levelName + "Coin1Collected", 0);
        PlayerPrefs.SetInt("Level" + levelName + "Coin1X", -1);
        PlayerPrefs.SetInt("Level" + levelName + "Coin1Z", -1);

        PlayerPrefs.SetInt("Level" + levelName + "Coin2Collected", 0);
        PlayerPrefs.SetInt("Level" + levelName + "Coin2X", -1);
        PlayerPrefs.SetInt("Level" + levelName + "Coin2Z", -1);

        // Mid Level Loading Saving Prefs
        PlayerPrefs.SetInt("Level" + levelName + "MidLevelSaveExists", 0);

        // Map seed will be a four digit number [(((Random.Range(0,9) * 1000) * (Random.Range(0,9) * 100)) * (Random.Range(0,9) * 10)) * Random.Range(0,9)] should word
        PlayerPrefs.SetInt("Level" + levelName + "MapSeed", -1);

        PlayerPrefs.SetInt("Level" + levelName + "PlayerX", -1);
        PlayerPrefs.SetInt("Level" + levelName + "PlayerZ", -1);

        PlayerPrefs.SetInt("Level" + levelName + "SavedHours", 0);
        PlayerPrefs.SetInt("Level" + levelName + "SavedMinuts", 0);
        PlayerPrefs.SetInt("Level" + levelName + "SavedSeconds", 0);

        if(spcLevel) {
            PlayerPrefs.SetString("Level" + levelName + "SavedVisibleCells", string.Empty);
        }
    }

    private GameObject CreateLevelButton(int levelNum, int subLevelNum = 0) {
        if (levelNum != 0 && subLevelNum == 0) {
            m_startY += m_gapY;
        }

        GameObject spawnedButton = GameObject.Instantiate(levelButtonPrefab, new Vector3(m_startX, m_startY, 0f), Quaternion.identity, levelsParent) as GameObject;

        float spawnX = m_startX;
        if (subLevelNum != 0) { spawnX = m_startX + (m_gapX * subLevelNum); }

        spawnedButton.transform.localPosition = new Vector3(spawnX, m_startY, 0f);
        spawnedButton.GetComponent<LevelDetails>().mazeSize = 5 + levelNum;

        return spawnedButton;
    }
}
