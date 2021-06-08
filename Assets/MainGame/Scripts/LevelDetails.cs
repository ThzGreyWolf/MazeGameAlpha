using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class LevelDetails : MonoBehaviour {

    // This script will hold the information needed for the level
    // at start the LevelCreater will get info from xml and generate the levels

    public bool completed = false;
    public int levelNum = 0;
    public int secondoryLevelNum = 0;
    public int mazeSize = 0;
    public int score = 0;

    [SerializeField]
    private GameObject[] m_indicators;
    [SerializeField]
    private Text m_levelNameText;

    public void SelectLevel() {
        LevelMaker lm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<LevelMaker>();
        lm.MazeSize = mazeSize;
        lm.LevelNum = levelNum;
        lm.SecondoryLevelNum = secondoryLevelNum;

        SceneManager.LoadScene("GameScene");
    }

    public void SetLevelNameAndIndicators(int levelNum, int secLevelNum) {
        m_levelNameText.text = (levelNum+1) + "-" + secLevelNum;
        SetIndicators(levelNum + "" + secLevelNum);
    }

    private void SetIndicators(string levelName) {
        int numCoins = 3;
        for (int i = 0; i < numCoins; i++) {
            if (PlayerPrefs.GetInt("Level" + levelName + "Coin" + i + "Collected") == 1) {
                m_indicators[i].SetActive(true);
            }
        }
    }
}
