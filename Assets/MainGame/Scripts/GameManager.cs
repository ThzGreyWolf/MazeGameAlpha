using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    private static GameManager m_instance;

    public bool firstTimeArrival = true;

    public Stopwatch timer = null;
    public bool levelCompleted = false;
    public string levelName = string.Empty;
    public Transform playerTransform = null;

    public int currentHours = 0;
    public int currentMinutes = 0;
    public int currentSeconds = 0;
    public int secondaryLevelNum = 0;
    public List<Cell> mazeCells = null;

    public int controlMode = 0;

    private Text m_timeText = null;
    private LevelMaker m_lm = null;
    private GameObject m_ggScreen = null;

    private int savedHours = 0;
    private int savedMinutes = 0;
    private int savedSeconds = 0;

    void Awake() {
        if (!m_instance) {
            m_instance = this;
        } else {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

	void Start () {
        m_lm = GetComponent<LevelMaker>();
	}

	void Update () {
        if(SceneManager.GetActiveScene().name == "GameScene") {
            GetCurrentLevel();

            if (m_ggScreen == null) {
                m_ggScreen = GameObject.FindGameObjectWithTag("GameOverOverlay");
                m_ggScreen.SetActive(false);
            }

            if(playerTransform == null) {
                playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            }

            if (m_lm.SecondoryLevelNum == 1) {
                if (mazeCells == null) {
                    mazeCells = GameObject.FindGameObjectWithTag("MazeManager").GetComponent<Generator>().GetCells();
                       
                    if(PlayerPrefs.GetInt("Level" + levelName + "MidLevelSaveExists") == 1) { 
                        int[] positions = GetSavedVisibleCellPositions();
                        for(int j = 0; j < mazeCells.Count; j++) {
                            for (int i = 0; i < positions.Length; i += 2) {
                                int x = positions[i];
                                int z = positions[i + 1];

                                if ((int)mazeCells[j].transform.position.x == x && (int)mazeCells[j].transform.position.z == z) {
                                    mazeCells[j].RemoveCover();
                                }

                            }
                        }
                    }
                }

                if (playerTransform != null && mazeCells != null) {
                    int playerPosX = (((int)playerTransform.position.x / 10) * 10);
                    int playerPosZ = (((int)playerTransform.position.z / 10) * 10);

                    for (int i = 0; i < mazeCells.Count; i++) {
                        if (mazeCells[i].transform.position.x == playerPosX && mazeCells[i].transform.position.z == playerPosZ) {
                            mazeCells[i].RemoveCover();
                            for (int j = 0; j < mazeCells[i].openlyConnectedNeighbours.Count; j++) {
                                mazeCells[i].openlyConnectedNeighbours[j].RemoveCover();
                            }
                        }
                    }
                }
            }

            if(m_timeText == null) {
                m_timeText = GameObject.FindGameObjectWithTag("TimeText").GetComponent<Text>();
            } else {
                if(timer == null) {
                    timer = new Stopwatch();
                    timer.Start();

                    savedHours = PlayerPrefs.GetInt("Level" + levelName + "SavedHours");
                    savedMinutes = PlayerPrefs.GetInt("Level" + levelName + "SavedMinuts");
                    savedSeconds = PlayerPrefs.GetInt("Level" + levelName + "SavedSeconds");

                } else {
                    int extraMinutes = 0;
                    int extraHours = 0;

                    currentSeconds = timer.Elapsed.Seconds + savedSeconds;
                    if(currentSeconds >= 60) {
                        currentSeconds = currentSeconds - 60;
                        extraMinutes++;
                    }

                    currentMinutes = timer.Elapsed.Minutes + savedMinutes + extraMinutes;
                    if (currentMinutes >= 60) {
                        currentMinutes = currentMinutes - 60;
                        extraHours++;
                    }

                    currentHours = timer.Elapsed.Hours + savedHours + extraHours;

                    // string hours = timer.Elapsed.Hours.ToString();
                    // if(timer.Elapsed.Hours < 10) { hours = "0" + timer.Elapsed.Hours; }
                    // string minutes = timer.Elapsed.Minutes.ToString();
                    // if (timer.Elapsed.Minutes < 10) { minutes = "0" + timer.Elapsed.Minutes; }
                    // string seconds = timer.Elapsed.Seconds.ToString();
                    // if (timer.Elapsed.Seconds < 10) { seconds = "0" + timer.Elapsed.Seconds; }

                    string hours = currentHours.ToString();
                    if (currentHours < 10) { hours = "0" + currentHours; }
                    string minutes = currentMinutes.ToString();
                    if (currentMinutes < 10) { minutes = "0" + currentMinutes; }
                    string seconds = currentSeconds.ToString();
                    if (currentSeconds < 10) { seconds = "0" + currentSeconds; }

                    m_timeText.text = hours + ":" + minutes + ":" + seconds;
                }
            }
        } else {
            playerTransform = null;
            mazeCells = null;

            m_timeText = null;
            timer = null;
            m_ggScreen = null;

            levelCompleted = false;
        }
    }

    public void GetCurrentLevel() {
        secondaryLevelNum = m_lm.SecondoryLevelNum;
        levelName = m_lm.LevelNum + "" + secondaryLevelNum;
    }

    public void LevelCompleted() {
        GetCurrentLevel();
        timer.Stop();

        levelCompleted = true;
        PlayerPrefs.SetInt("Level" + levelName + "IsCompleted", 1);

        int hours = PlayerPrefs.GetInt("Level" + levelName + "Hours");
        int minutes = PlayerPrefs.GetInt("Level" + levelName + "Minuts");
        int seconds = PlayerPrefs.GetInt("Level" + levelName + "Seconds");

        if(hours > currentHours) {
            hours = currentHours;
            minutes = currentMinutes;
            seconds = currentSeconds;
        } else if(minutes > currentMinutes) {
            minutes = currentMinutes;
            seconds = currentSeconds;
        } else  if(seconds > currentSeconds) {
            seconds = currentSeconds;
        }

        int score = PlayerPrefs.GetInt("Level" + levelName + "Score");

        // TODO: Figure out a new way to get player score this one is just temp
        int newScore = (currentHours * 10000) + (currentMinutes * 100) + (currentSeconds);

        if(score < newScore) {
            score = newScore;
            PlayerPrefs.SetInt("Level" + levelName + "Score", score);
        }

        PlayerPrefs.SetInt("Level" + levelName + "Hours", hours);
        PlayerPrefs.SetInt("Level" + levelName + "Minuts", minutes);
        PlayerPrefs.SetInt("Level" + levelName + "Seconds", seconds);

        // TODO: NOTE: Function below will be called form MazePlayer after the gameOver animation ran
        // ShowGameOverScreen();
    }

    public void ShowGameOverScreen() {
        m_ggScreen.SetActive(true);
        GameObject.FindGameObjectWithTag("ScoreText").GetComponent<Text>().text = PlayerPrefs.GetInt("Level" + levelName + "Score").ToString();
    }

    public int[] GetSavedVisibleCellPositions() {
        string[] data = PlayerPrefs.GetString("Level" + levelName + "SavedVisibleCells").Split('|');
        int[] positions = new int[data.Length];

        for(int i = 0; i < positions.Length; i++) {
            if(!int.TryParse(data[i], out positions[i])) {
                UnityEngine.Debug.Log("int parse failed at data index " + i);
                UnityEngine.Debug.Log(data[i]);
            }
        }

        return positions;
    }

    public string GetSerializedString(List<int> positions) {
        string retVal = string.Empty;

        for(int i = 0; i < positions.Count; i++) {
            if(i == 0) {
                retVal = positions[i].ToString();
            } else {
                retVal += ("|" + positions[i]);
            }
        }

        return retVal;
    }
}