using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Neighbour {
	public Cell cell;
	public int pos; // 0 == north // 1 == south // 2 == east // 3 == west //
}

public class Cell : MonoBehaviour {
	public bool visited = false;
    public bool coverRemoved = false;

    public int indexX, indexZ, myLevelMode;
	public List<Neighbour> neighbours = new List<Neighbour>();
	public List<Cell> openlyConnectedNeighbours = new List<Cell>();

    [SerializeField] private GameObject m_northWall, m_southWall, m_westWall, m_eastWall, m_celingCover;
    [SerializeField] private GameObject m_piller00, m_piller01, m_piller02, m_piller03;

    // Get my neighbours
    public void GetNeighbours(List<Cell> cells, int myIndex, int cellsWidth, int cellsDepth) {
		Neighbour tmp;

		if(transform.position.x != 0) {
			tmp = new Neighbour();
			tmp.cell = cells[myIndex - cellsDepth];
			tmp.pos = 3;

			neighbours.Add(tmp);
		}

		if(transform.position.z != 0) {
			tmp = new Neighbour();
			tmp.cell = cells[myIndex - 1];
			tmp.pos = 1;

			neighbours.Add(tmp);
		}

		if(transform.position.x != ((cellsWidth - 1) * 10)) {
			tmp = new Neighbour();
			tmp.cell = cells[myIndex + cellsDepth];
			tmp.pos = 2;

			neighbours.Add(tmp);
		}

		if(transform.position.z != ((cellsDepth - 1) * 10)) {
			tmp = new Neighbour();
			tmp.cell = cells[myIndex + 1];
			tmp.pos = 0;

			neighbours.Add(tmp);
		}
	}

	// Wall Destroiers
	public void DestroyNorthWall(Cell connectedCell) {
        m_northWall.SetActive(false);
        openlyConnectedNeighbours.Add(connectedCell);
    }

	public void DestroySouthWall(Cell connectedCell) {
        m_southWall.SetActive(false);
        openlyConnectedNeighbours.Add(connectedCell);
    }

	public void DestroyEastWall(Cell connectedCell) {
        m_eastWall.SetActive(false);
        openlyConnectedNeighbours.Add(connectedCell);
    }

	public void DestroyWestWall(Cell connectedCell) {
        m_westWall.SetActive(false);
        openlyConnectedNeighbours.Add(connectedCell);
    }

    public void SetLevelMode(int levelMode) {
        myLevelMode = levelMode;

        switch(myLevelMode) {
            case 1: // Light up mode, the others don't need any thing done on the cell's side
                if (!coverRemoved) {
                    m_celingCover.SetActive(true);
                }
                break;

            default:
                m_celingCover.SetActive(false);
                break;
        }
    }

    public void RemoveCover() {
        if (!coverRemoved) {
            m_celingCover.SetActive(false);
            coverRemoved = true;
        }
    }

    public void SetMaterial(Material newMat) {
        m_northWall.GetComponent<Renderer>().material = newMat;
        m_southWall.GetComponent<Renderer>().material = newMat;
        m_westWall.GetComponent<Renderer>().material = newMat;
        m_eastWall.GetComponent<Renderer>().material = newMat;

        m_piller00.GetComponent<Renderer>().material = newMat;
        m_piller01.GetComponent<Renderer>().material = newMat;
        m_piller02.GetComponent<Renderer>().material = newMat;
        m_piller03.GetComponent<Renderer>().material = newMat;
    }
}
