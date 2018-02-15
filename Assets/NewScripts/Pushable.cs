using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pushable : MonoBehaviour {
	public LayerMask walkableMask;
	float yOffset = 0.5f;
	bool moving = false;
	float speed = 2f;
	int targetIndex;


	public void Pushed(Vector3 newPos, Vector3 chargeDir)
	{
		newPos = newPos + (-chargeDir.normalized * 1f);
		if (!moving) {
			Vector3[] tmpArray = new Vector3[] { newPos };
			StartCoroutine (moveThroughPath (tmpArray));
			StartCoroutine (chargePusher(newPos));
		}
	}


	IEnumerator moveThroughPath(Vector3[] _path)
	{
		moving = true;
		Vector3 currentWaypoint = _path [0];
		while (true) {
			if (transform.position == new Vector3 (currentWaypoint.x, currentWaypoint.y + yOffset, currentWaypoint.z)) {
				targetIndex++;
				if (targetIndex >= _path.Length) {
					moving = false; 
					CheckIfWeFellOff ();
					yield break;
				}
				currentWaypoint = _path [targetIndex];
			}
			transform.position = Vector3.MoveTowards (transform.position, new Vector3 (currentWaypoint.x, currentWaypoint.y + yOffset, currentWaypoint.z), speed * Time.deltaTime);
			yield return null;
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
					//if (tmpUnit.whoOwnsMe () != whoOwnsMe ()) {  //No one can own me
						hit.collider.GetComponent<TBSUnit> ().Pushed (newPos, chargDir);
					//}
				} else if(hit.collider.tag == "Pushable"){
					hit.collider.GetComponent<Pushable> ().Pushed (newPos, chargDir);
				}
			}
			yield return null;
		}
	}
	void CheckIfWeFellOff()
	{
		Debug.DrawRay (new Vector3 (transform.position.x, transform.position.y + 1, transform.position.z), Vector3.down * 2f, Color.green, 10f);
		if (!Physics.Raycast (new Vector3 (transform.position.x, transform.position.y + 1, transform.position.z), Vector3.down, 2f, walkableMask)) { 
			gameObject.SetActive (false);
		} else {
			Debug.Log ("Something is under us!");
		}
	}
}
