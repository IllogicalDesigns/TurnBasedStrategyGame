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
    public Grid m_grid;  //Reference to the grid, called it myGrid before, sorry swamp
	public bool moving = false;  //Our we currently moving
	List<GameObject> currCharArrows = new List<GameObject> ();  //TODO move to charge class
	public LayerMask walkableMask;  //Used for checking to see if spaces are valid
	public int chargeDistance = 5;  //How far can we charge  //TODO move to charge class
	GameObject arrowBody;
	GameObject arrowHead;
	UnitAction performableAction;

	public Node previosUnWalkableNode;

	GameObject gridObject;
	public int nodeSize = 1; //Used for movement grid
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
		performableAction = gameObject.GetComponent<UnitAction> ();
		m_Player = gameObject.GetComponentInParent<PlayerInfo> ();
		m_grid = GameObject.FindObjectOfType<Grid> ();  //This is slow and should be stored in the playerInfo for reference
		arrowBody = m_Player.arrowBody;
		arrowHead = m_Player.arrowHead;
		gridObject = m_Player.gridObject;
		previosUnWalkableNode = m_grid.NodeFromWorldPoint (transform.position);
		Node startPos = m_grid.NodeFromWorldPoint(transform.position);
		transform.position = new Vector3 (startPos.worldPosition.x, startPos.worldPosition.y + yOffset, startPos.worldPosition.z);
		previosUnWalkableNode.occupied = true;
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
        Debug.Log("Set Movement has been caled");
        if(performableAction != null)
		    performableAction.CleanHelperAction();
		//Before even checking figure out if we can actually stand there
		Node tarNode = m_grid.NodeFromWorldPoint (tar);
		//Debug.Log(tar.ToString() + " " + tarNode.worldPosition.ToString());
		if (tarNode.walkable) {
			//m_TarX = tarNode.worldPosition;
			//targetCircle.transform.position = new Vector3 (target.x, target.y + 0.1f, target.z);
			//m_TarX.SetActive (true);
			previosUnWalkableNode.occupied = false;
			PathRequestManager.RequestPath (transform.position, tar, OnPathFound);
			m_Player.setConfirmButton (this, true);
		}
	}

	public void MoveUnitToConfirmedMovement()
	{
		destroyPreviousUnitGrid ();
		//destroyOldChargeArrows ();
		performableAction.CleanHelperAction();
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
		previosUnWalkableNode.occupied = true;
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

    //Works through the movent range and Action threat range and adds to threat values (only when active)
    public void influenceHeatMap()
    {
        
        //Touch all nodes we can walk to 
        //if (m_Player.activeUnits.Contains(this))
        if(gameObject.active)
            StartCoroutine(influenceAllReachableNodes(transform.position));
    }

    IEnumerator influenceAllReachableNodes(Vector3 startPos)
    { 
        visitedRowCol = new bool[Mathf.RoundToInt(m_grid.gridWorldSize.x), Mathf.RoundToInt(m_grid.gridWorldSize.y)];
        Node startNode = m_grid.NodeFromWorldPoint(startPos);
        influenceAllReachableNodesSub(startPos, 0);
        yield return null;
    }

    void influenceAllReachableNodesSub(Vector3 nodePos, int depth)
    {
        Node node = m_grid.NodeFromWorldPoint(nodePos);
        if (visitedRowCol[Mathf.RoundToInt(node.gridX), Mathf.RoundToInt(node.gridY)] == false && node.walkable && depth <= movementLeft)
        {
            visitedRowCol[Mathf.RoundToInt(node.gridX), Mathf.RoundToInt(node.gridY)] = true;
            if (depth != 0 && !node.occupied)
            {
                //node.addAddThreatLvl(1);
                performableAction.influenceHeatMap(m_grid, node);
                influenceAllReachableNodesSub(new Vector3(node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                influenceAllReachableNodesSub(new Vector3(node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                influenceAllReachableNodesSub(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), depth + 1);
                influenceAllReachableNodesSub(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), depth + 1);
            }
            else if (depth == 0)
            {
                performableAction.influenceHeatMap(m_grid, node);
                influenceAllReachableNodesSub(new Vector3(node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                influenceAllReachableNodesSub(new Vector3(node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                influenceAllReachableNodesSub(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), depth + 1);
                influenceAllReachableNodesSub(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), depth + 1);
            }
        }
    }

    //Can this be handles in the charge action class? from the pushing end?
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
		previosUnWalkableNode.occupied = false;
		previosUnWalkableNode = m_grid.NodeFromWorldPoint (_path [_path.Length-1]);
		previosUnWalkableNode.occupied = true;
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

    public void updateOccupiedNode()
    {
        if (previosUnWalkableNode != null)
            previosUnWalkableNode.occupied = false;
        previosUnWalkableNode = m_grid.NodeFromWorldPoint(transform.position);
        previosUnWalkableNode.occupied = true;
    }

    public void displayActionGUIHelpers()
	{
		performableAction.DisplayHelperAction();
	}
		
	public void moveToChargeLocation(Vector3 newPos)
	{
		performableAction.PerformAction();
	}
		
	public void destroyOldChargeArrows()
	{
		performableAction.CleanHelperAction();
		/*
		foreach (GameObject arrSprites in currCharArrows) {
			//Network.Destroy (arrSprites);
			Destroy (arrSprites);
		}*/
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

	void createUnitGrid(Vector3 startPos, int pathLength)
	{
		visitedRowCol = new bool[Mathf.RoundToInt (m_grid.gridWorldSize.x), Mathf.RoundToInt (m_grid.gridWorldSize.y)];
		previousGridObjs.Clear ();
		Node startNode = m_grid.NodeFromWorldPoint (startPos);
		StartCoroutine(crtUGridSub(startPos, 0));
	}

	IEnumerator crtUGridSub(Vector3 nodePos, int depth)
	{
		Node node = m_grid.NodeFromWorldPoint (nodePos);
		if (visitedRowCol [Mathf.RoundToInt (node.gridX), Mathf.RoundToInt (node.gridY)] == false && node.walkable && depth <= movementLeft) {  //TODO node occupied
            if (!node.occupied && depth != 0)  //Prevents grid at start and the otherside of obstactacle even if we can't path to it
            {
                visitedRowCol[Mathf.RoundToInt(node.gridX), Mathf.RoundToInt(node.gridY)] = true;
                previousGridObjs.Add(Instantiate(gridObject, new Vector3(node.worldPosition.x, node.worldPosition.y + 0.15f, node.worldPosition.z), gridObject.transform.rotation) as GameObject);
                yield return new WaitForSeconds(0.000005f);
                StartCoroutine(crtUGridSub(new Vector3(node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1));
                StartCoroutine(crtUGridSub(new Vector3(node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1));
                StartCoroutine(crtUGridSub(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), depth + 1));
                StartCoroutine(crtUGridSub(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), depth + 1));
            }
            else if (depth == 0)  //If start we need to start these in each direction
            {
                StartCoroutine(crtUGridSub(new Vector3(node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1));
                StartCoroutine(crtUGridSub(new Vector3(node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1));
                StartCoroutine(crtUGridSub(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), depth + 1));
                StartCoroutine(crtUGridSub(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), depth + 1));
            }

		}
	}
	void destroyPreviousUnitGrid()
	{
		foreach (GameObject obj in previousGridObjs) {
			Destroy (obj);
		}
	}
}
