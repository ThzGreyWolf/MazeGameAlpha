using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotPoint {
    public int x;
    public int z;
}

public class Generator : MonoBehaviour {
	public Grid aStarGrid;
	public bool instant = false;
	public bool MR = false;

	[SerializeField] private GameObject m_spotPrefab, m_centerPrefab, m_playerPrefab, m_extPrefab, m_coinPrefab;
    [SerializeField] private Transform m_cellsParent = null;
	[SerializeField] private int m_width, m_depth;
	[SerializeField] private int m_centerHalfWidth, m_centerHalfDepth;
    [SerializeField] private LevelMaker m_lm = null;
    [SerializeField] private Material m_litWallMat = null;
    [SerializeField] private Material m_unlitWallMat = null;

    private Transform m_player = null;
    private CameraHandler m_mainCam = null;
	private List<Cell> m_cells = new List<Cell>();
	private List<Cell> m_lastVisitedCells = new List<Cell>();
	private Cell m_currentCell;
	private Vector3 m_mazeCenter = Vector3.zero;
	private int m_cellCount, m_visitedCellCount;
	private int m_startX, m_startZ;
	private int m_endX, m_endZ;
    private int m_numCoinsNeeded, m_numCoinsSpawned;

    private string m_levelName = string.Empty;

    private List<PlotPoint> m_coinPoints = new List<PlotPoint>();

	void Awake() {
        m_player = GameObject.FindGameObjectWithTag("Player").transform;

        m_numCoinsNeeded = 3;
        m_numCoinsSpawned = 0;

        m_mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraHandler>();

        m_lm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<LevelMaker>();
        m_width = m_lm.MazeSize;
        m_depth = m_lm.MazeSize;

        m_cellsParent.position = new Vector3((m_width * 5), 0f, (m_depth * 5));

        m_levelName = m_lm.LevelNum + "" + m_lm.SecondoryLevelNum;

        SetCoinPositions();

        m_mainCam.overviewSize = m_width * 9;
        m_mainCam.overviewPos = new Vector3((m_width * 10) * 0.5f, 50, (m_depth * 10) * 0.5f);

        m_cellCount = 0;
		m_visitedCellCount = 0;

		if (MR) {
			if (m_width < 12) {
				m_width = 12;
				if (m_depth < 24) {
					m_depth = 24;
				} 
			} else if (m_depth < 12) {
				m_depth = 12;
				if (m_width < 24) {
					m_width = 24;
				} 
			}
		}

		m_mazeCenter = new Vector3 ((m_width * 10) / 2, 0.0f, (m_depth * 10) / 2);
		// print(m_mazeCenter);

		aStarGrid.gameObject.transform.position = m_mazeCenter;
		aStarGrid.gridWorldSize = new Vector2 (m_width * 10, m_depth * 10);

		// ======================================
		// Code bellow used to be in start moved it to Awake because i need the maze to be created before the grid and
		// grid to be greated before any units start asking for a path, since units are asking for a path in start atm
		// i need to create all ths in Awake, if the units asks for path later then start MOVE CODE BACK TO START
		// ======================================

		if (MR) {
			Vector3 centerPos = m_mazeCenter;
			if (centerPos.x % 2 != 0) {
				centerPos.x -= 5;
			}
			if (centerPos.z % 2 != 0) {
				centerPos.z -= 5;
			}

			Instantiate (m_centerPrefab, centerPos, Quaternion.identity);

            if (m_playerPrefab != null) {
                GameObject spawnedPlayer = Instantiate(m_playerPrefab, m_mazeCenter, Quaternion.identity);
            } else {
                Debug.LogWarning("No 'm_playerPrefab' defined");
            }
		}

		for(int i = 0; i < m_width; i++) {
			for(int j = 0; j < m_depth; j++) {
				GameObject spawnedCell = Instantiate (m_spotPrefab, new Vector3 ((i * 10), 0f, (j * 10)), Quaternion.identity, m_cellsParent) as GameObject;
				m_cells.Add (spawnedCell.GetComponent<Cell> ());
				m_cells[m_cells.Count - 1].indexX = i;
				m_cells[m_cells.Count - 1].indexZ = j;
                m_cells[m_cells.Count - 1].SetLevelMode(m_lm.SecondoryLevelNum);

                if(m_lm.SecondoryLevelNum == 2) {
                    spawnedCell.GetComponent<Cell>().SetMaterial(m_unlitWallMat);
                } else {
                    spawnedCell.GetComponent<Cell>().SetMaterial(m_litWallMat);
                }

                m_cellCount++;
			}
		}

		for(int i = 0; i < m_cells.Count; i++) { m_cells[i].GetNeighbours(m_cells, i, m_width, m_depth); }

        int mapSeed = -1;
        Debug.Log(m_levelName);
        if (PlayerPrefs.GetInt("Level" + m_levelName + "MidLevelSaveExists") == 1) {
            Debug.Log("Resume Game");

            int savedPlayerX = PlayerPrefs.GetInt("Level" + m_levelName + "PlayerX");
            int savedPlayerZ = PlayerPrefs.GetInt("Level" + m_levelName + "PlayerZ");

            m_player.transform.position = new Vector3(savedPlayerX, 3.5f, savedPlayerZ);
            mapSeed = PlayerPrefs.GetInt("Level" + m_levelName + "MapSeed");

            m_mainCam.SetCameraPos(savedPlayerX, savedPlayerZ);
        } else {
            Debug.Log("New Game");

            mapSeed = (Random.Range(0, 9) * 10000) + (Random.Range(0, 9) * 1000) + (Random.Range(0, 9) * 100) + (Random.Range(0, 9) * 10) + (Random.Range(0, 9));
        }

        Random.InitState(mapSeed);
        PlayerPrefs.SetInt("Level" + m_levelName + "MapSeed", mapSeed);

        m_currentCell = m_cells[Random.Range(0, m_cells.Count)];
		m_currentCell.visited = true; 
		m_visitedCellCount++;

		if(!instant) {
			StartCoroutine(MakeMaze());
		} else {
			while(m_visitedCellCount < (m_cellCount)) {
				List<Neighbour> nonVisitedCells = new List<Neighbour>();
				for(int j = 0; j < m_currentCell.neighbours.Count; j++) {
					if(!m_currentCell.neighbours[j].cell.visited) { 
						nonVisitedCells.Add(m_currentCell.neighbours[j]);
					}
				}

				// ManageCenterCell (m_currentCell);

				if(nonVisitedCells.Count > 0) {
					int randNum = (int)Random.Range(0, nonVisitedCells.Count);

					switch(nonVisitedCells[randNum].pos) {
					case 0:
						m_currentCell.DestroyNorthWall(nonVisitedCells[randNum].cell);
						nonVisitedCells[randNum].cell.DestroySouthWall(m_currentCell);
						break;

					case 1:
						m_currentCell.DestroySouthWall(nonVisitedCells[randNum].cell);
						nonVisitedCells[randNum].cell.DestroyNorthWall(m_currentCell);
						break;

					case 2:
						m_currentCell.DestroyEastWall(nonVisitedCells[randNum].cell);
						nonVisitedCells[randNum].cell.DestroyWestWall(m_currentCell);
						break;

					case 3:
						m_currentCell.DestroyWestWall(nonVisitedCells[randNum].cell);
						nonVisitedCells[randNum].cell.DestroyEastWall(m_currentCell);
						break;
					}

					m_lastVisitedCells.Add(m_currentCell);
					m_currentCell = nonVisitedCells[randNum].cell;

					// ManageCenterCell (m_currentCell);

					m_currentCell.visited = true; 

					m_visitedCellCount++;
				} else {
					m_currentCell = m_lastVisitedCells[m_lastVisitedCells.Count - 1];
					m_lastVisitedCells.RemoveAt(m_lastVisitedCells.Count - 1);
				}
			}
		}

        Random.InitState(System.Environment.TickCount);

        // SpawnExtraction ();
        SpawnFinish();
        SpawnCoins();

        if (m_lm.SecondoryLevelNum == 1) {
            m_cells[m_cells.Count - 1].RemoveCover();
            // for (int i = 0; i < m_cells[m_cells.Count - 1].openlyConnectedNeighbours.Count; i++) {
            //     m_cells[m_cells.Count - 1].openlyConnectedNeighbours[i].RemoveCover();
            // }
        }

        aStarGrid.StartGrid ();
	}

	private IEnumerator MakeMaze() {
		while(m_visitedCellCount < (m_cellCount - 1)) {
			List<Neighbour> nonVisitedCells = new List<Neighbour>();
			for(int i = 0; i < m_currentCell.neighbours.Count; i++) {
				if(!m_currentCell.neighbours[i].cell.visited) {
					nonVisitedCells.Add(m_currentCell.neighbours[i]);
				}
			}

			if(nonVisitedCells.Count > 0) {
				int randNum = (int)Random.Range(0, nonVisitedCells.Count);

				switch(nonVisitedCells[randNum].pos) {
				case 0:
					m_currentCell.DestroyNorthWall(nonVisitedCells[randNum].cell);
					nonVisitedCells[randNum].cell.DestroySouthWall(m_currentCell);
					break;

				case 1:
					m_currentCell.DestroySouthWall(nonVisitedCells[randNum].cell);
					nonVisitedCells[randNum].cell.DestroyNorthWall(m_currentCell);
					break;

				case 2:
					m_currentCell.DestroyEastWall(nonVisitedCells[randNum].cell);
					nonVisitedCells[randNum].cell.DestroyWestWall(m_currentCell);
					break;

				case 3:
					m_currentCell.DestroyWestWall(nonVisitedCells[randNum].cell);
					nonVisitedCells[randNum].cell.DestroyEastWall(m_currentCell);
					break;
				}

				m_lastVisitedCells.Add(m_currentCell);
				m_currentCell = nonVisitedCells[randNum].cell;
				m_currentCell.visited = true;
				m_visitedCellCount++;
			} else {
				m_currentCell = m_lastVisitedCells[m_lastVisitedCells.Count - 1];
				m_lastVisitedCells.RemoveAt(m_lastVisitedCells.Count - 1);
			}

			yield return new WaitForSeconds(0.5f);
		}
	}

	private void SpawnExtraction() {
		int x = 0;
		int z = 0;

		switch ((int)Random.Range(0, 4)) {
			case 0:
				x = 0;
				z = (int)Random.Range(0, m_depth);
				break;

			case 1:
				x = (int)Random.Range(0, m_width);
				z = 0;
				break;

			case 2:
				x = (int)Random.Range(0, m_width);
				z = m_depth - 1;
				break;

			case 3:
				x = m_width -1;
				z = (int)Random.Range(0, m_depth);
				break;
		}

        if (m_extPrefab != null) {
            Instantiate(m_extPrefab, new Vector3(x * 10, 0.0f, z * 10), Quaternion.identity);
        } else {
            Debug.LogWarning("No 'm_exitPerfab' defined");
        }
    }

    private void SpawnFinish() {
        if (m_extPrefab != null) {
            MazeExit exit = Instantiate(m_extPrefab, new Vector3((m_width-0.5f) * 10, 0.5f, (m_depth-0.5f) * 10), Quaternion.identity, m_cellsParent).GetComponent<MazeExit>();
            if (m_lm.SecondoryLevelNum == 2) {
                exit.EnableLight();
            }
        } else {
            Debug.LogWarning("No 'm_exitPerfab' defined");
        }
    }

	// private void ManageCenterCell(Cell cell) {
	//	if(MR) {
	//		if(	cell.indexX >= ((m_width / 2) - m_centerHalfWidth) && 
	//			cell.indexX <= ((m_width / 2) + m_centerHalfWidth) - 1 && 
	//			cell.indexZ >= ((m_depth / 2) - m_centerHalfDepth) && 
	//			cell.indexZ <= ((m_depth / 2) + m_centerHalfDepth) - 1) 
	//		{
	//			cell.DestroyNorthWall();
	//			cell.DestroySouthWall();
	//			cell.DestroyEastWall();
	//			cell.DestroyWestWall();
	//		}
	//	}
	// }

    private void SetCoinPositions() {
        for (int i = 0; i < m_numCoinsNeeded; i++) {
            PlotPoint point = new PlotPoint();

            if (PlayerPrefs.GetInt("Level" + m_levelName + "CoinPositionsSet") == 0) {
                do {
                    point.x = Random.Range(0, m_width);
                    if (point.x == 0) {
                        point.z = Random.Range(1, m_depth);
                    } else {
                        point.z = Random.Range(0, m_depth);
                    }
                } while ((i == 1 && point.x == m_coinPoints[0].x && point.z == m_coinPoints[0].z) ||
                         (i == 2 && point.x == m_coinPoints[0].x && point.z == m_coinPoints[0].z && point.x == m_coinPoints[1].x && point.z == m_coinPoints[1].z));

                PlayerPrefs.SetInt("Level" + m_levelName + "Coin" + i + "X", point.x);
                PlayerPrefs.SetInt("Level" + m_levelName + "Coin" + i + "Z", point.z);
            } else {
                point.x = PlayerPrefs.GetInt("Level" + m_levelName + "Coin" + i + "X");
                point.z = PlayerPrefs.GetInt("Level" + m_levelName + "Coin" + i + "Z");
            }

            m_coinPoints.Add(point);
        }

        if (PlayerPrefs.GetInt("Level" + m_levelName + "CoinPositionsSet") == 0) {
            PlayerPrefs.SetInt("Level" + m_levelName + "CoinPositionsSet", 1);
        }
    }

    private void SpawnCoins() {
        for(int i = 0; i < m_numCoinsNeeded; i++) {
            if (PlayerPrefs.GetInt("Level" + m_levelName + "Coin" + i + "Collected") == 0) {
                Coin coin = Instantiate(m_coinPrefab, new Vector3((m_coinPoints[i].x * 10) + 5f, 5f, (m_coinPoints[i].z * 10) + 5f), Quaternion.identity, m_cellsParent).GetComponent<Coin>();
                coin.coinIndex = i;
                coin.levelName = m_levelName;

                if(m_lm.SecondoryLevelNum == 2) {
                    coin.EnableLight();
                }


                if (m_lm.SecondoryLevelNum == 1) {
                    for (int j = 0; j < m_cells.Count; j++) {
                        if ((int)m_cells[j].transform.position.x == (m_coinPoints[i].x * 10) && (int)m_cells[j].transform.position.z == (m_coinPoints[i].z * 10)) {
                            m_cells[j].RemoveCover();
                            // for (int k = 0; k < m_cells[j].openlyConnectedNeighbours.Count; k++) {
                            //     m_cells[j].openlyConnectedNeighbours[k].RemoveCover();
                            // }
                        }
                    }
                }
            }

            m_numCoinsSpawned++;
        }
    }

    public List<Cell> GetCells() {
        return m_cells;
    }
}
