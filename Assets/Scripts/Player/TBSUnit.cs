using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TBSUnit : MonoBehaviour
{
	PlayerInfo m_Player;  //player Info holds references to player's GUI stuff

	float circleOffset = 0.4f;
	float yOffset = 0.5f;  // Offset for the player so they aren't embeded in the ground, should be updated when model root is changed
	int speed = 2;  //How fast you move

	public int movementMax = 5;  //The max move speed, I don't think this does anything
	public int movementLeft = 5;  //How much spaces you can move
	public bool chargedDuringAction = false;  //Did we charge durning this turn

	Vector3[] path;  //Internal storage of path for after path is found
	int targetIndex;  
	Grid m_grid;  //Reference to the grid, called it myGrid before, sorry swamp
	public bool moving = false;  //Our we currently moving
	List<GameObject> currCharArrows = new List<GameObject> ();  //TODO move to charge class
	public LayerMask walkableMask;  //Used for checking to see if spaces are valid
	public int chargeDistance = 5;  //How far can we charge  //TODO move to charge class
	GameObject arrowBody;
	GameObject arrowHead;


	public Node previosUnWalkableNode;

	GameObject gridObject;
	int nodeSize = 1; //Used for movement grid
	bool[,] visitedRowCol;
	List<GameObject> previousGridObjs = new List<GameObject> ();

	//Draws the pathing output when a path is not null
	public void OnDrawGizmos() {
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i ++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], new Vector3(0.1f, 0.1f, 0.1f));

				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i-1],path[i]);
				}
			}
		}
	}

	// Use this for initialization
	void Start()
	{
		m_Player = gameObject.GetComponentInParent<PlayerInfo> ();
		m_grid = GameObject.FindObjectOfType<Grid> ();  //This is slow and should be stored in the playerInfo for reference
		arrowBody = m_Player.arrowBody;
		arrowHead = m_Player.arrowHead;
		gridObject = m_Player.gridObject;
		previosUnWalkableNode = m_grid.NodeFromWorldPoint (transform.position);
		Node startPos = m_grid.NodeFromWorldPoint(transform.position);
		transform.position = new Vector3 (startPos.worldPosition.x, startPos.worldPosition.y + yOffset, startPos.worldPosition.z);
		//previosUnWalkableNode.walkable = false;
	}

	void UnitWasSelected()
	{
		m_Player.setTarCircle (new Vector3 (transform.position.x, transform.position.y - circleOffset, transform.position.z), true);
		if (!chargedDuringAction)
			m_Player.setChargeButton (this, true);
		m_Player.updateMoveLeftSlider (movementLeft);
		createUnitGrid (transform.position, movementLeft);
		//Setup highlighted grid nodes
	}

	public void deselectedUnit()
	{
		m_Player.setTarCircle (new Vector3 (transform.position.x, transform.position.y - circleOffset, transform.position.z), false);
		destroyPreviousUnitGrid();
	}

	//Accepts a command to display a new possible position, TODO return a failed and deal with it
	public void SetMovement(Vector3 tar)
	{
		destroyOldChargeArrows ();
		//Before even checking figure out if we can actually stand there
		Node tarNode = m_grid.NodeFromWorldPoint (tar);
		//Debug.Log(tar.ToString() + " " + tarNode.worldPosition.ToString());
		if (tarNode.walkable) {
			//m_TarX = tarNode.worldPosition;
			//targetCircle.transform.position = new Vector3 (target.x, target.y + 0.1f, target.z);
			//m_TarX.SetActive (true);
			//previosUnWalkableNode.walkable = true;
			PathRequestManager.RequestPath (transform.position, tar, OnPathFound);
			m_Player.setConfirmButton (this, true);
		}
	}

	public void MoveUnitToConfirmedMovement()
	{
		destroyPreviousUnitGrid ();
		destroyOldChargeArrows ();
		m_Player.activatedUnit = this;
		m_Player.setConfirmButton (this, false);
		m_Player.setTarCircle (Vector3.one, false);
		movementLeft -= (path.Length - 1);
		m_Player.updateMoveLeftSlider (movementLeft);
		StartCoroutine (moveThroughPath (path));
	}

	//Unit Example
	public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
	{
		int pathLen = newPath.Length - 1;
		//previosUnWalkableNode.walkable = false;
		if (pathLen <= movementLeft) {
			if (pathSuccessful) {
				Debug.Log ("Path Cost " + (pathLen));
				path = newPath;

				targetIndex = 0;
				int i = pathLen;
				Vector3 pos = newPath [i];

				m_Player.updateMoveLeftSlider (movementLeft - pathLen);
				m_Player.setTarCross (new Vector3 (pos.x, pos.y + circleOffset / 2, pos.z), true);
			} else {
				Debug.Log ("Path Failed; Cost: " + pathLen);
			}
		}
	}

	IEnumerator chargePusher(Vector3 newPos)
	{
		RaycastHit hit;
		Vector3 chargDir = transform.position - newPos;
		while (moving) {
			Debug.DrawRay (transform.position, -chargDir.normalized * 1f, Color.blue);
			if (Physics.Raycast (transform.position, -chargDir.normalized, out hit, 1f)) {
				if (hit.collider.tag == "Player") {
					TBSUnit tmpUnit = hit.collider.GetComponent<TBSUnit> ();
					if (tmpUnit.whoOwnsMe () != whoOwnsMe ()) {
						hit.collider.GetComponent<TBSUnit> ().Pushed (newPos, chargDir);
					}
				} else if (hit.collider.tag == "Pushable") {
					hit.collider.GetComponent<Pushable> ().Pushed (newPos, chargDir);
				}
			}
			yield return null;
		}
	}

	public void Pushed(Vector3 newPos, Vector3 chargeDir)
	{
		newPos = newPos + (-chargeDir.normalized * 1f);
		if (!moving) {
			Vector3[] tmpArray = new Vector3[] { new Vector3 (newPos.x, newPos.y + (yOffset / 2), newPos.z) };
			StartCoroutine (moveThroughPath (tmpArray));
		}
	}


	IEnumerator moveThroughPath(Vector3[] _path)
	{
		previosUnWalkableNode.walkable = true;
		previosUnWalkableNode = m_grid.NodeFromWorldPoint (_path [_path.Length-1]);
		previosUnWalkableNode.walkable = false;

		moving = true;
		Vector3 currentWaypoint = _path [0];
		while (true) {
			if (transform.position == new Vector3 (currentWaypoint.x, currentWaypoint.y + yOffset, currentWaypoint.z)) {
				targetIndex++;
				if (targetIndex >= _path.Length) {
					moving = false;
					m_Player.setTarCross (Vector3.one, false); 
					CheckIfWeFellOff ();
					yield break;
				}
				currentWaypoint = _path [targetIndex];
			}
			transform.position = Vector3.MoveTowards (transform.position, new Vector3 (currentWaypoint.x, currentWaypoint.y + yOffset, currentWaypoint.z), speed * Time.deltaTime);
			yield return null;
		}

	}

	public void attemptToSelect()
	{
		SelectUnit ();
	}


	//Something can request to select the unit if successful we move into setting up the visuals
	public bool SelectUnit()
	{
		bool areWeActive = m_Player.activeUnits.Contains (this);  //Our we an active unit for our player
		areWeActive = m_Player.isItOurTurn ();

		if (areWeActive && m_Player.activatedUnit != null && m_Player.activatedUnit == this)   //Has our player performed an action with a unit this turn
				areWeActive = true;
		else if (areWeActive && m_Player.activatedUnit != null)
			areWeActive = false;

		m_Player.setConfirmButton (this, false);
		m_Player.setChargeButton (this, false);
		m_Player.setTarCross (Vector3.one, false);

		if (areWeActive) {
			UnitWasSelected ();
			return true;
		} else {
			//Failed to select because we are not active
			Debug.Log ("Unit was not active");
			return false;
		}
	}

	public void displayChargeOptions()
	{
		m_Player.setTarCross (Vector3.one, false);
		destroyOldChargeArrows ();
		if (!moving && !chargedDuringAction) {
			chargeDisplayDirection (Vector3.forward, transform.position, 0);
			chargeDisplayDirection (Vector3.left, transform.position, -90);
			chargeDisplayDirection (Vector3.right, transform.position, 90);
			chargeDisplayDirection (Vector3.back, transform.position, 180);
		}
	}

	public void moveToChargeLocation(Vector3 newPos)
	{
		newPos = m_grid.NodeFromWorldPoint (newPos).worldPosition;
		destroyOldChargeArrows ();
		chargedDuringAction = true;
		m_Player.activatedUnit = this;
		m_Player.setChargeButton (this, false);
		m_Player.setTarCircle (Vector3.one, false);
		Vector3[] tmpArray = new Vector3[] { newPos };
		StartCoroutine (moveThroughPath (tmpArray));
		StartCoroutine (chargePusher (newPos));
	}

	public void destroyOldChargeArrows()
	{
		foreach (GameObject arrSprites in currCharArrows) {
			//Network.Destroy (arrSprites);
			Destroy (arrSprites);
		}
	}

	//Also strikes old node as walkable and new node as occupied/Unwalkable
	public void CheckIfWeFellOff()
	{
		Debug.DrawRay (new Vector3 (transform.position.x, transform.position.y + 1, transform.position.z), Vector3.down * 2f, Color.green, 10f);
		if (!Physics.Raycast (new Vector3 (transform.position.x, transform.position.y + 1, transform.position.z), Vector3.down, 2f, walkableMask)) { 
			m_Player.RemoveDeadUnit (this);
			gameObject.SetActive (false);
		} else {
			Debug.Log ("Something is under us!");
		}
	}

	public int whoOwnsMe()
	{
		return m_Player.ownerNumber;
	}

	void chargeDisplayDirection(Vector3 direction, Vector3 startPos, float Y)
	{
		for (int i = 1; i <= chargeDistance; i++) {
			Quaternion Rot = Quaternion.Euler (new Vector3 (arrowBody.transform.rotation.eulerAngles.x, Y, arrowBody.transform.rotation.eulerAngles.z));  //Alters just the Y rot so the sprites face poperly
			Vector3 newSpace = startPos + (direction * (i + 1));  //Checks spaces + 1 to see if the current space needs an arrow/End

			Node tmpNode = m_grid.NodeFromWorldPoint (newSpace); //Get the node we are checking to see if its walkable
			bool nodeWalkable = tmpNode.walkable;

			bool somethingUnderNode = Physics.Raycast (new Vector3 (newSpace.x, newSpace.y + 0.5f, newSpace.z), Vector3.down, 2f);  //Is there something under this node? #TODO needs walkable mask
			if (!somethingUnderNode)
				nodeWalkable = false;

			if (i != chargeDistance && nodeWalkable) {
				//currCharArrows.Add (Network.Instantiate (arrowBody, startPos + (direction * i), Rot, 0) as GameObject);
				currCharArrows.Add (Instantiate (arrowBody, startPos + (direction * i), Rot) as GameObject);
			} else {
				//if(i != 1 && !somethingUnderNode)  //If this is the first go around we are likely already off so don't build an arrow
				//currCharArrows.Add (Network.Instantiate (arrowHead, startPos + (direction * i), Rot, 0) as GameObject);
				currCharArrows.Add (Instantiate (arrowHead, startPos + (direction * i), Rot) as GameObject);
				break;
			}
		}
	}
	void createUnitGrid(Vector3 startPos, int pathLength)
	{
		visitedRowCol = new bool[Mathf.RoundToInt (m_grid.gridWorldSize.x), Mathf.RoundToInt (m_grid.gridWorldSize.y)];
		previousGridObjs.Clear ();
		Node startNode = m_grid.NodeFromWorldPoint (startPos);
		//previosUnWalkableNode.walkable = true;
		StartCoroutine(crtUGridSub(startPos, 0));
		//previosUnWalkableNode.walkable = false;
	}

	IEnumerator crtUGridSub(Vector3 nodePos, int depth)
	{
		Node node = m_grid.NodeFromWorldPoint (nodePos);
		if (visitedRowCol [Mathf.RoundToInt (node.gridX), Mathf.RoundToInt (node.gridY)] == false && node.walkable && depth <= movementLeft) {

			visitedRowCol [Mathf.RoundToInt (node.gridX), Mathf.RoundToInt (node.gridY)] = true;
			if (depth != 0) {
				previousGridObjs.Add (Instantiate (gridObject, new Vector3 (node.worldPosition.x, node.worldPosition.y + 0.15f, node.worldPosition.z), gridObject.transform.rotation) as GameObject);
			}
			yield return new WaitForSeconds (0.000005f);

			StartCoroutine(crtUGridSub (new Vector3 (node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1));
			StartCoroutine(crtUGridSub (new Vector3 (node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1));
			StartCoroutine(crtUGridSub (new Vector3 (node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), depth + 1));
			StartCoroutine(crtUGridSub (new Vector3 (node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), depth + 1));
		}
	}
	void destroyPreviousUnitGrid()
	{
		foreach (GameObject obj in previousGridObjs) {
			Destroy (obj);
		}
	}
}
