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

	//Evaluates gridPos to see if they are within the grid
	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node> (); //Create List of nodes, can this be an array
		int nX = node.gridX;
		int nY = node.gridY;

		if((nX + 1)   <= gridSizeX)
			neighbours.Add (grid [nX + 1, nY]);
		if((nX - 1)   <= gridSizeX)
			neighbours.Add (grid [nX - 1, nY]);
		if((nY + 1)   <= gridSizeY)
			neighbours.Add (grid [nX, nY + 1]);
		if((nY - 1)   <= gridSizeY)
			neighbours.Add (grid [nX, nY - 1]);
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