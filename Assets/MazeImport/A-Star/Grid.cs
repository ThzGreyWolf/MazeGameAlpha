using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

	public bool displayGridGizmos;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	public TerrainType[] walkableRegions; 
	public int obsticleProximityPenalty = 50;

	LayerMask walkableMask;
	Dictionary<int, int> walkableReionsDictionary = new Dictionary<int, int>();
	Node[,] grid;

	float nodeDiameter;
	int gridSizeX, gridSizeZ;

	int penMin = int.MaxValue;
	int penMax = int.MinValue;

	// void Start() {
		// nodeDiameter = nodeRadius * 2;
		// gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		// gridSizeZ = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

		// foreach (TerrainType region in walkableRegions) {
		// 	walkableMask.value |= region.terrainMask.value;
		// 	walkableReionsDictionary.Add ((int)Mathf.Log (region.terrainMask.value, 2), region.terrainPenalty);
		// }

		// Moved call to Maze Generator so it gets called after the maze is generated;
		// CreateGrid ();
	// }

	public int MaxSize {
		get {
			return gridSizeX * gridSizeZ;
		}
	}

	public void StartGrid() {
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeZ = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

		foreach (TerrainType region in walkableRegions) {
			walkableMask.value |= region.terrainMask.value;
			walkableReionsDictionary.Add ((int)Mathf.Log (region.terrainMask.value, 2), region.terrainPenalty);
		}

		CreateGrid ();
	}

	 public void CreateGrid() {
		grid = new Node[gridSizeX, gridSizeZ];
		Vector3 worldBottomLeft = transform.position - Vector3.right * (gridWorldSize.x / 2) - Vector3.forward * (gridWorldSize.y / 2);

		for (int x = 0; x < gridSizeX; x++) {
			for (int z = 0; z < gridSizeZ; z++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere (worldPoint, nodeRadius, unwalkableMask));

				int movementPenalty = 0;

				if (walkableRegions.Length != 0) {
					Ray ray = new Ray (worldPoint + Vector3.up * 50, Vector3.down);
					RaycastHit hit;
					if (Physics.Raycast (ray, out hit, 100, walkableMask)) {
						walkableReionsDictionary.TryGetValue (hit.collider.gameObject.layer, out movementPenalty);
					}
				}

				if (!walkable) {
					movementPenalty += obsticleProximityPenalty;
				}

				grid [x, z] = new Node (walkable, worldPoint, x, z, movementPenalty);
			}
		}

		BlurPenaltyMap (3);
	}

	void BlurPenaltyMap(int blurSize) {
		int kernelSize = blurSize * 2 + 1;
		int kernelEntents = (kernelSize - 1) / 2;

		int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeZ];
		int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeZ];

		for (int z = 0; z < gridSizeZ; z++) {
			for (int x = -kernelEntents; x <= kernelEntents; x++) {
				int sampleX = Mathf.Clamp (x, 0, kernelEntents);
				penaltiesHorizontalPass [0, z] += grid [sampleX, z].movementPenality;
			}

			for (int x = 1; x < gridSizeX; x++) {
				int removeIndex = Mathf.Clamp (x - kernelEntents - 1, 0, gridSizeX);
				int addIndex = Mathf.Clamp (x + kernelEntents, 0, gridSizeX - 1);

				penaltiesHorizontalPass [x, z] = penaltiesHorizontalPass [x - 1, z] - grid [removeIndex, z].movementPenality + grid [addIndex, z].movementPenality;
			}
		}

		for (int x = 0; x < gridSizeX; x++) {
			for (int z = -kernelEntents; z <= kernelEntents; z++) {
				int sampleZ = Mathf.Clamp (z, 0, kernelEntents);
				penaltiesVerticalPass [x, 0] += penaltiesHorizontalPass [x, sampleZ]; 
			}

			int bluredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass [x, 0] / (kernelSize * kernelSize)); 
			grid [x, 0].movementPenality = bluredPenalty;

			for (int z = 1; z < gridSizeZ; z++) {
				int removeIndex = Mathf.Clamp (z - kernelEntents - 1, 0, gridSizeZ);
				int addIndex = Mathf.Clamp (z + kernelEntents, 0, gridSizeZ - 1);

				penaltiesVerticalPass [x, z] = penaltiesVerticalPass [x, z - 1] - penaltiesHorizontalPass [x, removeIndex] + penaltiesHorizontalPass [x, addIndex];
				bluredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass [x, z] / (kernelSize * kernelSize));
				grid [x, z].movementPenality = bluredPenalty;

				if (bluredPenalty > penMax) {
					penMax = bluredPenalty;
				}
				if (bluredPenalty < penMin) {
					penMin = bluredPenalty;
				}
			}
		}
	}

	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node> ();

		for (int x = -1; x <= 1; x++) {
			for (int z = -1; z <= 1; z++) {
				if (x == 0 && z == 0) {
					continue; // TODO: CLEAN UP!
				}

				int checkX = node.gridX + x;
				int checkZ = node.gridZ + z;

				if (checkX >= 0 && checkX < gridSizeX && checkZ >= 0 && checkZ < gridSizeZ) {
					neighbours.Add (grid [checkX, checkZ]);
				}

			}
		}

		return neighbours;
	}

	public Node NodeFromWorldPoint(Vector3 worldPos) {
		// float precentX = (worldPos.x + (gridWorldSize.x / 2)) / gridWorldSize.x; 
		// float precentZ = (worldPos.z + (gridWorldSize.y / 2)) / gridWorldSize.y;
		float precentX = worldPos.x / gridWorldSize.x; 
		float precentZ = worldPos.z / gridWorldSize.y;
		precentX = Mathf.Clamp01 (precentX);
		precentZ = Mathf.Clamp01 (precentZ); 

		int x = Mathf.RoundToInt((gridSizeX - 1) * precentX); 
		int z = Mathf.RoundToInt((gridSizeZ - 1) * precentZ);

		return grid [x, z];
	}
		
	void OnDrawGizmos() {
		Gizmos.DrawWireCube (transform.position, new Vector3 (gridWorldSize.x, 1.0f, gridWorldSize.y));

		if (grid != null && displayGridGizmos) {
			foreach (Node n in grid) {
				Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penMin, penMax, n.movementPenality));
				Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;
				Gizmos.DrawCube (n.worldPos, Vector3.one * (nodeDiameter));
			}
		}
	}

	[System.Serializable]
	public class TerrainType {
		public LayerMask  terrainMask;
		public int terrainPenalty;
	}
}
