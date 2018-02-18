using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeAction : UnitAction {
	GameObject arrowBody;
	GameObject arrowHead;
	float yOffset = 0.4f;
	int targetIndex;
	float speed = 5;
	List<GameObject> currCharArrows = new List<GameObject> ();

	void start (){
		arrowBody = m_Player.arrowBody;
		arrowHead = m_Player.arrowHead;
	}

	public override void DisplayHelperAction () {
	}

	public override void CleanHelperAction () {
	}

	public override void PerformAction () {
	}

	void displayChargeOptions()
	{
		m_Player.setTarCross (Vector3.one, false);
		destroyOldChargeArrows ();
		if (!unitAttached.moving && !unitAttached.chargedDuringAction) {
			chargeDisplayDirection (Vector3.forward, transform.position, 0);
			chargeDisplayDirection (Vector3.left, transform.position, -90);
			chargeDisplayDirection (Vector3.right, transform.position, 90);
			chargeDisplayDirection (Vector3.back, transform.position, 180);
		}
	}

	void chargeDisplayDirection(Vector3 direction, Vector3 startPos, float Y)
	{
		for (int i = 1; i <= unitAttached.chargeDistance; i++) {
			Quaternion Rot = Quaternion.Euler (new Vector3 (arrowBody.transform.rotation.eulerAngles.x, Y, arrowBody.transform.rotation.eulerAngles.z));  //Alters just the Y rot so the sprites face poperly
			Vector3 newSpace = startPos + (direction * (i + 1));  //Checks spaces + 1 to see if the current space needs an arrow/End

			Node tmpNode = m_grid.NodeFromWorldPoint (newSpace); //Get the node we are checking to see if its walkable
			bool nodeWalkable = tmpNode.walkable;

			bool somethingUnderNode = Physics.Raycast (new Vector3 (newSpace.x, newSpace.y + 0.5f, newSpace.z), Vector3.down, 2f);  //Is there something under this node? #TODO needs walkable mask
			if (!somethingUnderNode)
				nodeWalkable = false;

			if (i != unitAttached.chargeDistance && nodeWalkable) {
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

	void moveToChargeLocation(Vector3 newPos)
	{
		newPos = m_grid.NodeFromWorldPoint (newPos).worldPosition;
		destroyOldChargeArrows ();
		unitAttached.chargedDuringAction = true;
		m_Player.activatedUnit = unitAttached;
		m_Player.setChargeButton (unitAttached, false);
		m_Player.setTarCircle (Vector3.one, false);
		Vector3[] tmpArray = new Vector3[] { newPos };
		StartCoroutine (moveThroughPath (tmpArray));
		StartCoroutine (chargePusher (newPos));
	}

	IEnumerator moveThroughPath(Vector3[] _path)
	{
		//previosUnWalkableNode.walkable = true;
		//previosUnWalkableNode = m_grid.NodeFromWorldPoint (_path [_path.Length-1]);
		//previosUnWalkableNode.walkable = false;

		unitAttached.moving = true;
		Vector3 currentWaypoint = _path [0];
		while (true) {
			if (transform.position == new Vector3 (currentWaypoint.x, currentWaypoint.y + yOffset, currentWaypoint.z)) {
				targetIndex++;
				if (targetIndex >= _path.Length) {
					unitAttached.moving = false;
					m_Player.setTarCross (Vector3.one, false); 
					unitAttached.CheckIfWeFellOff ();
					yield break;
				}
				currentWaypoint = _path [targetIndex];
			}
			transform.position = Vector3.MoveTowards (transform.position, new Vector3 (currentWaypoint.x, currentWaypoint.y + yOffset, currentWaypoint.z), speed * Time.deltaTime);
			yield return null;
		}

	}

	public void destroyOldChargeArrows()
	{
		foreach (GameObject arrSprites in currCharArrows) {
			//Network.Destroy (arrSprites);
			Destroy (arrSprites);
		}
	}

	IEnumerator chargePusher(Vector3 newPos)
	{
		RaycastHit hit;
		Vector3 chargDir = transform.position - newPos;
		while (unitAttached.moving) {
			Debug.DrawRay (transform.position, -chargDir.normalized * 1f, Color.blue);
			if (Physics.Raycast (transform.position, -chargDir.normalized, out hit, 1f)) {
				if (hit.collider.tag == "Player") {
					TBSUnit tmpUnit = hit.collider.GetComponent<TBSUnit> ();
					if (tmpUnit.whoOwnsMe () != unitAttached.whoOwnsMe ()) {
						hit.collider.GetComponent<TBSUnit> ().Pushed (newPos, chargDir);
					}
				} else if (hit.collider.tag == "Pushable") {
					hit.collider.GetComponent<Pushable> ().Pushed (newPos, chargDir);
				}
			}
			yield return null;
		}
	}
}
