using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitGridGUI : MonoBehaviour
{
	public GameObject gridObject;
	List<GameObject> previousGridObjs = new List<GameObject> ();
	public Grid worldGrid;
	public int length = 5;
	bool[,] visitedRowCol;
	int recurisiveCount = 0;
	int nodeSize = 2;
	List<Vector3> debugGridChecker = new List<Vector3> ();
	List<Vector3> debugGridCheckerBadNodes = new List<Vector3> ();

	public GameObject startTestPos;
	public Color testColor = Color.red;

	// Use this for initialization
	void Start()
	{
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		foreach (Vector3 n in debugGridChecker) {
			Gizmos.DrawSphere (n, 0.25f);
		}
		Gizmos.color = Color.yellow;
		foreach (Vector3 n in debugGridCheckerBadNodes) {
			Gizmos.DrawWireSphere (n, 0.25f);
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {
			destroyPreviousUnitGrid ();
			createUnitGrid (startTestPos.transform.position, testColor, length);
		}
	}

	public void createUnitGrid(Vector3 startPos, Color teamColor, int pathLength)
	{
		length = pathLength;
		Vector2 wrld = worldGrid.gridWorldSize;  //Clear the visited flag array
		visitedRowCol = new bool[Mathf.RoundToInt (wrld.x), Mathf.RoundToInt (wrld.y)];
		previousGridObjs.Clear ();
		Node startNode = worldGrid.NodeFromWorldPoint (startPos);
		//crtUGrid (startPos, teamColor, 0);
		StartCoroutine(crtUGridSub(startPos, teamColor, 0));
	}

	IEnumerator crtUGridSub(Vector3 nodePos, Color teamColor, int depth)
	{
		debugGridCheckerBadNodes.Add (nodePos);
		Node node = worldGrid.NodeFromWorldPoint (nodePos);
		if (visitedRowCol [Mathf.RoundToInt (node.gridX), Mathf.RoundToInt (node.gridY)] == false && node.walkable && depth <= length) {

			visitedRowCol [Mathf.RoundToInt (node.gridX), Mathf.RoundToInt (node.gridY)] = true;
			if (depth != 0) {
				previousGridObjs.Add (Instantiate (gridObject, new Vector3 (node.worldPosition.x, node.worldPosition.y + 0.3f, node.worldPosition.z), gridObject.transform.rotation) as GameObject);
				debugGridChecker.Add (nodePos);
			}
			yield return new WaitForSeconds (0.000005f);

			StartCoroutine(crtUGridSub (new Vector3 (node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), teamColor, depth + 1));
			StartCoroutine(crtUGridSub (new Vector3 (node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), teamColor, depth + 1));
			StartCoroutine(crtUGridSub (new Vector3 (node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), teamColor, depth + 1));
			StartCoroutine(crtUGridSub (new Vector3 (node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), teamColor, depth + 1));
		}
	}

	void crtUGrid(Vector3 nodePos, Color teamColor, int depth)
	{
		//debugGridCheckerBadNodes.Add (nodePos);
		Node node = worldGrid.NodeFromWorldPoint (nodePos);
		if (visitedRowCol [Mathf.RoundToInt (node.gridX), Mathf.RoundToInt (node.gridY)] == false && node.walkable && depth <= length) {
			
			visitedRowCol [Mathf.RoundToInt (node.gridX), Mathf.RoundToInt (node.gridY)] = true;
			if (depth != 0) {
				previousGridObjs.Add (Instantiate (gridObject, new Vector3 (node.worldPosition.x, node.worldPosition.y + 0.3f, node.worldPosition.z), gridObject.transform.rotation) as GameObject);
				//debugGridChecker.Add (nodePos);
			}

			crtUGrid (new Vector3 (node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), teamColor, depth + 1);
			crtUGrid (new Vector3 (node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), teamColor, depth + 1);
			crtUGrid (new Vector3 (node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), teamColor, depth + 1);
			crtUGrid (new Vector3 (node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), teamColor, depth + 1);
		}
	}

	public void destroyPreviousUnitGrid()
	{
		debugGridChecker.Clear ();
		debugGridCheckerBadNodes.Clear ();

		foreach (GameObject obj in previousGridObjs) {
			Destroy (obj);
		}
	}
}
