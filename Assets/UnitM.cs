using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitM : MonoBehaviour
{
	public int ownedBy = 0;
	public int movementRange = 5;
	public bool selected = false;
	public bool movedThisRound = false;
	public bool chargedThisRound = false;
	bool enabledSelectUI = false;
	[SerializeField] GameObject selectionCircle;
	[SerializeField] GameObject targetCircle;
	[SerializeField] Grid myGrid;
	[SerializeField] float yOffset = 0.5f;
	[SerializeField] GameObject confirmButtonObj;
	Button confirmButton;
	[SerializeField] Button action;
	[SerializeField] int chargeDistance = 5;
	[SerializeField] GameObject arrowBody;
	[SerializeField] GameObject arrowHead;
	bool moving = false;
	public bool charging = false;
	float chargMod = 2f;
	List<GameObject> myArrow = new List<GameObject>();
	public LayerMask walkableMask;
	Color myColor;


	void Start()
	{
		myGrid = GameObject.FindObjectOfType<Grid> ();
		confirmButton = confirmButtonObj.GetComponentInChildren<Button> ();
		myColor = gameObject.GetComponentInParent<PlayerInfo> ().playerColor;
	}

	/*void OnCollisionStay(Collision collision) {
		if (charging) {
			if (collision.collider.tag == "Player1" || collision.collider.tag == "Player2") {
				Debug.Log("ChargedThrough");
			}
		}
	}*/


	//Unit Example Variables
	public Vector3 target = new Vector3 (69f, 69f, 69f);
	[SerializeField] float speed = 20;
	Vector3[] path;
	int targetIndex;

	public void SetMovement(Vector3 tar)
	{
		//Before even checking figure out if we can actually stand there
		Node tarNode = myGrid.NodeFromWorldPoint (tar);
		if (tarNode.walkable) {
			target = tarNode.worldPosition;
			targetCircle.transform.position = new Vector3 (target.x, target.y + 0.1f, target.z);
			targetCircle.SetActive (true);
			PathRequestManager.RequestPath (transform.position, target, OnPathFound);
		}
	}

	public void ExecuteAction()
	{
		if (target != new Vector3 (69f, 69f, 69f)) {
			target = new Vector3 (69f, 69f, 69f);
			targetCircle.SetActive (false);
			StopCoroutine ("FollowPath");
			StartCoroutine ("FollowPath");
			selectionCircle.SetActive (false);
			selected = false;
			movedThisRound = true;
		}
	}

	//Unit Example
	public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
	{
		if (newPath.Length <= movementRange) {
			if (pathSuccessful) {
				path = newPath;
				targetIndex = 0;
				ConfirmButton (newPath [newPath.Length - 1]);
			}
		} else {
			target = new Vector3 (69f, 69f, 69f);
			targetCircle.SetActive (false);
			confirmButtonObj.SetActive (false);
			//Debug.Log ("Path excedes movement range, Rng:" + path.Length.ToString());
		}
	}

	void ConfirmButton(Vector3 pos)
	{
		confirmButtonObj.transform.position = new Vector3 (pos.x, pos.y + 1f, pos.z);
		//Set the color of the button
		confirmButtonObj.SetActive (true);
		confirmButton.onClick.RemoveAllListeners ();
		confirmButton.onClick.AddListener (ExecuteAction);
	}

	//Unit Example
	IEnumerator FollowPath()
	{
		moving = true;
		Vector3 currentWaypoint = path [0];
		while (true) {
			if (transform.position == new Vector3 (currentWaypoint.x, currentWaypoint.y + yOffset, currentWaypoint.z)) {
				targetIndex++;
				if (targetIndex >= path.Length) {
					moving = false;
					yield break;
				}
				currentWaypoint = path [targetIndex];
			}

			transform.position = Vector3.MoveTowards (transform.position, new Vector3 (currentWaypoint.x, currentWaypoint.y + yOffset, currentWaypoint.z), speed * Time.deltaTime);
			yield return null;

		}

	}

	void chargeDisplayDirection(Vector3 direction, Vector3 startPos, float Y)
	{
		for (int i = 1; i <= chargeDistance; i++) {
			Quaternion Rot = Quaternion.Euler (new Vector3 (arrowBody.transform.rotation.eulerAngles.x, Y, arrowBody.transform.rotation.eulerAngles.z));  //Alters just the Y rot so the sprites face poperly
			Vector3 newSpace = startPos + (direction * (i + 1));  //Checks spaces + 1 to see if the current space needs an arrow/End

			Node tmpNode = myGrid.NodeFromWorldPoint (newSpace); //Get the node we are checking to see if its walkable
			bool nodeWalkable = tmpNode.walkable;

			bool somethingUnderNode = Physics.Raycast (new Vector3 (newSpace.x, newSpace.y + 0.5f, newSpace.z), Vector3.down, 2f);  //Is there something under this node? #TODO needs walkable mask
			if (!somethingUnderNode)
				nodeWalkable = false;
			
			if (i != chargeDistance && nodeWalkable) {
				myArrow.Add(Instantiate (arrowBody, startPos + (direction * i), Rot) as GameObject);
			} else {
				//if(i != 1 && !somethingUnderNode)  //If this is the first go around we are likely already off so don't build an arrow
				myArrow.Add(Instantiate (arrowHead, startPos + (direction * i), Rot) as GameObject);
				break;
			}
		}
	}

	public void Charge (Vector3 newPos) {
		if (!charging && !chargedThisRound) {
			charging = true;
			StartCoroutine (ChargeRoutine (newPos));
			chargedThisRound = true;
		}
	}

	public void Pushed (Vector3 newPos, Vector3 chargeDir) {
		newPos = newPos + (-chargeDir * 1f);
		//newPos = newPos + (-chargeDir * (1 + 1)); 
		if (!charging) {
			charging = true;
			StartCoroutine (PushedRoutine (newPos));
		}
	}

	IEnumerator PushedRoutine (Vector3 newPos) {
		while (transform.position != new Vector3 (newPos.x, transform.position.y, newPos.z)) {
			if (transform.position == new Vector3 (newPos.x, transform.position.y, newPos.z)) {
				charging = false;
				CheckIfWeFellOff ();
				yield break;
			}
			transform.position = Vector3.MoveTowards (transform.position, new Vector3 (newPos.x, transform.position.y, newPos.z), speed * chargMod * Time.deltaTime);
			yield return null;
		}
		charging = false;
		CheckIfWeFellOff ();
	}

	void CheckIfWeFellOff () {
		Debug.DrawRay (new Vector3 (transform.position.x, transform.position.y + 1, transform.position.z), Vector3.down * 2f, Color.green, 10f);
		if (!Physics.Raycast (new Vector3 (transform.position.x, transform.position.y + 1, transform.position.z), Vector3.down, 2f, walkableMask)) { 
			Destroy (gameObject);
		} else {
			Debug.Log ("Something is under us!");
		}
	}

	IEnumerator ChargeRoutine (Vector3 newPos) {
		RaycastHit hit;
		Vector3 chargDir = transform.position - newPos;
		chargDir = chargDir.normalized;
		while (transform.position != new Vector3 (newPos.x, transform.position.y, newPos.z)) {
			Debug.DrawRay (new Vector3 (transform.position.x, transform.position.y + 0.5f, transform.position.z), -chargDir * 1f);
			if(Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), -chargDir,out hit, 1f)){
				if (hit.collider.tag == "Player1" || hit.collider.tag == "Player2") {
					if (gameObject.tag != hit.collider.tag) {
						hit.collider.GetComponent<UnitM> ().Pushed (newPos, chargDir);
					}
				}
			}

			if (transform.position == new Vector3 (newPos.x, transform.position.y, newPos.z)) {
				charging = false;
				yield break;
			}
			transform.position = Vector3.MoveTowards (transform.position, new Vector3 (newPos.x, transform.position.y, newPos.z), speed * chargMod * Time.deltaTime);
			yield return null;
		}
		charging = false;
		destroyOldChargeArrows ();
	}

	public void displayChargeOptions()
	{
		if (!moving && !chargedThisRound) {
			chargeDisplayDirection (Vector3.forward, transform.position, 0);
			chargeDisplayDirection (Vector3.left, transform.position, -90);
			chargeDisplayDirection (Vector3.right, transform.position, 90);
			chargeDisplayDirection (Vector3.back, transform.position, 180);
		}
	}

	void destroyOldChargeArrows () {
		foreach (GameObject arrSprites in myArrow) {
			Destroy (arrSprites);
		}
	}

	//Unit Example
	public void OnDrawGizmos()
	{
		if (target != new Vector3 (69f, 69f, 69f)) {
			Gizmos.color = Color.red;
			Gizmos.DrawCube (target, Vector3.one);
		}
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube (path [i], new Vector3 (0.25f, 0.25f, 0.25f));

				if (i == targetIndex) {
					Gizmos.DrawLine (transform.position, path [i]);
				} else {
					Gizmos.DrawLine (path [i - 1], path [i]);
				}
			}
		}
	}

	public void selectUnit()
	{
		if (!movedThisRound)
			selected = true;
	}

	public void deselectUnit()
	{
		selected = false;
		destroyOldChargeArrows ();
	}
	
	// Update is called once per frame
	void Update()
	{
		if (selected && !enabledSelectUI && !movedThisRound) {
			selectionCircle.transform.position = new Vector3(transform.position.x, transform.position.y - (yOffset- 0.05f), transform.position.z);
			selectionCircle.SetActive (true);
			enabledSelectUI = true;
			action.gameObject.SetActive (true);
			action.onClick.RemoveAllListeners ();
			action.onClick.AddListener (displayChargeOptions);
			if (!selected && enabledSelectUI) {
				selectionCircle.SetActive (false);
				enabledSelectUI = false;
			}
		}
	}
}
