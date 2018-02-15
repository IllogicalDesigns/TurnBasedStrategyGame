using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{

	public bool displayGridGizmos;
	public LayerMask unwalkableMask;
	public LayerMask walkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	Node[,] grid;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	void Awake()
	{
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt (gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt (gridWorldSize.y / nodeDiameter);
		CreateGrid ();
	}

	public int MaxSize {
		get {
			return gridSizeX * gridSizeY;
		}
	}

	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++) {
			for (int y = 0; y < gridSizeY; y++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = false;  // default to false
				if (Physics.Raycast (new Vector3 (worldPoint.x, worldPoint.y + 1, worldPoint.z), Vector3.down, 2f, walkableMask)) {  //see if we can walk on something
					walkable = true;
					walkable = !(Physics.CheckSphere (worldPoint, nodeRadius, unwalkableMask));  //If we can walk can we enter that space
				} else {
					walkable = false;
				}
				grid [x, y] = new Node (walkable, worldPoint, x, y);
			}
		}
	}

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node> ();

		for (int y = -1; y <= 1; y++) {
			if (y == 0)
				continue;
			int checkY = node.gridY + y;
			int checkX = node.gridX;
			if (checkY >= 0 && checkY < gridSizeX) {
				neighbours.Add (grid [checkX, checkY]);
			}
		}

		for (int x = -1; x <= 1; x++) {
			if (x == 0)
				continue;
			int checkY = node.gridY;
			int checkX = node.gridX + x;
			if (checkX >= 0 && checkX < gridSizeX) {
				neighbours.Add (grid [checkX, checkY]);
			}
		}

		return neighbours;
	}


	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01 (percentX);
		percentY = Mathf.Clamp01 (percentY);

		int x = Mathf.RoundToInt ((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt ((gridSizeY - 1) * percentY);
		return grid [x, y];
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawWireCube (transform.position, new Vector3 (gridWorldSize.x, 1, gridWorldSize.y));
		if (grid != null && displayGridGizmos) {
			foreach (Node n in grid) {
				Gizmos.color = (n.walkable) ? Color.white : Color.red;
				Gizmos.DrawWireCube (n.worldPosition, Vector3.one * (nodeDiameter));
			}
		}
	}
}