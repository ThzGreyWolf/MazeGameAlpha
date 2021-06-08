using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazePlayer : MonoBehaviour {

    public float speed = 1;
    public int[] collectedCoins = new int[3];

    [SerializeField] private GameObject m_playerPointLight = null;
    [SerializeField] private float m_animSpeed = 5f;
    [SerializeField] private Transform m_cellsParent = null;
    [SerializeField] private float m_rotationMargin = 10f;

    private Rigidbody m_rb = null;
    private GameManager m_gm = null;
    private Vector3 m_finishPos = Vector3.zero;
    private bool m_animDone = false;

    private float m_targetRotationY = 0;
    private float m_currentRotationY = 0;
    private bool m_rotating = false;
    private bool m_rotatingClockwise = false;

	void Start () {
        // PlayerPrefs.DeleteAll();

        m_rb = GetComponent<Rigidbody>();
        m_gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        m_gm.GetCurrentLevel();

        if(m_gm.controlMode == 2) {
            transform.SetParent(m_cellsParent);
        } else {
            m_rb.useGravity = false;
        }

        
        if(m_gm.secondaryLevelNum != 2) {
            m_playerPointLight.SetActive(false);
        }

        for(int i = 0; i < collectedCoins.Length; i++) {
            collectedCoins[i] = 0;
            if(PlayerPrefs.GetInt("Level" + m_gm.levelName + "Coin" + i + "Collected") == 1) {
                collectedCoins[i] = 1;
            }
        }

        // m_rb.AddForce(new Vector3(0f, 0f, -0.1f) * speed);
    }

    void FixedUpdate () {
        if (!m_gm.levelCompleted) {
            // float x = Input.GetAxis("Horizontal");
            // float z = Input.GetAxis("Vertical");
            // m_rb.AddForce(new Vector3(x, 0f, z) * speed);

            float x = 0f;
            float z = 0f;

            // TODO: Mobile controls (currently its for testing on editor)
                // Touch control for touch
                // Tilt Controls
                // Swipe for rotate

            switch(m_gm.controlMode) {
                case 0: // Touch
                    if(Input.GetMouseButton(0)) {
                        Vector2 mousePos = Input.mousePosition;
                        Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);

                        x = Mathf.Clamp(mousePos.x - playerScreenPos.x, -1, 1);
                        z = Mathf.Clamp(mousePos.y - playerScreenPos.y, -1, 1);

                        m_rb.AddForce(new Vector3(x, 0f, z) * speed);
                    }
                    break;

                case 1: // Tilt
                    break;

                case 2: // Rotate with buttons - maybe i should name it as gravity controls
                    if (!m_rotating) {
                        if (Input.GetKeyDown(KeyCode.A)) {
                            RotateCounterClockwise();
                            m_rotating = true;
                        } else if (Input.GetKeyDown(KeyCode.D)) {
                            RotateClockwise();
                            m_rotating = true;
                        }
                    }

                    if(m_rotating) {
                        // Debug.Log((int)m_cellsParent.eulerAngles.y + "   ==?   " + m_targetRotationY);
                        //if((int)m_cellsParent.eulerAngles.y != m_targetRotationY) {
                        //    if(m_rotatingClockwise) {
                        //        m_cellsParent.transform.eulerAngles = new Vector3(0f, (int)m_cellsParent.transform.eulerAngles.y + m_rotationMargin, 0f);
                        //    } else {
                        //        m_cellsParent.transform.eulerAngles = new Vector3(0f, (int)m_cellsParent.transform.eulerAngles.y - m_rotationMargin, 0f);
                        //    }
                        //} else {
                        //    m_rotating = false;
                        //}

                        if (m_currentRotationY > m_targetRotationY) {
                            m_currentRotationY -= m_rotationMargin;
                            m_cellsParent.transform.eulerAngles = new Vector3(0f, m_currentRotationY, 0f);
                            if(m_currentRotationY <= m_targetRotationY) {
                                m_cellsParent.transform.eulerAngles = new Vector3(0f, m_targetRotationY, 0f);
                                m_rotating = false;
                            }
                        } else if(m_currentRotationY < m_targetRotationY) {
                            m_currentRotationY += m_rotationMargin;
                            m_cellsParent.transform.eulerAngles = new Vector3(0f, m_currentRotationY, 0f);
                            if (m_currentRotationY >= m_targetRotationY) {
                                m_cellsParent.transform.eulerAngles = new Vector3(0f, m_targetRotationY, 0f);
                                m_rotating = false;
                            }
                        }
                    }
                    break;
            }
        } else {
            //TODO: Complete what happens to player after the exit reached
            if(!m_animDone) {
                if (transform.localScale.x > 0.2f) {
                    transform.position = Vector3.Lerp(transform.position, m_finishPos, m_animSpeed * Time.deltaTime);
                    transform.localScale = Vector2.Lerp(transform.localScale, Vector3.zero, (m_animSpeed*0.5f) * Time.deltaTime);
                } else {
                    m_gm.ShowGameOverScreen();
                    m_rb.isKinematic = true;

                    for (int i = 0; i < collectedCoins.Length; i++) {
                        PlayerPrefs.SetInt("Level" + m_gm.levelName + "Coin" + i + "Collected", collectedCoins[i]);
                    }

                    m_animDone = true;
                }
            }
        }
	}

    void OnTriggerEnter(Collider other) {
        if(other.tag == "Exit") {
            m_gm.LevelCompleted();
            m_finishPos = other.transform.position;
        } else if(other.tag == "Coin") {
            Coin coin = other.GetComponent<Coin>();
            coin.Collect();

            switch(coin.coinIndex) {
                case 0:
                    collectedCoins[0] = 1;
                    break;

                case 1:
                    collectedCoins[1] = 1;
                    break;

                case 2:
                    collectedCoins[2] = 1;
                    break;
            }
        }
    }

    private void RotateClockwise() {
        m_rotatingClockwise = true;
        m_targetRotationY += 90f;
    }

    private void RotateCounterClockwise() {
        m_rotatingClockwise = false;
        m_targetRotationY -= 90f;
    }
}
