using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectAndMove : MonoBehaviour
{
	Camera myCam;
	public LayerMask myLayers;
	public int playerTurn = 0;
	UnitM lastUnit;
	public bool movedUnitThisTurn = false;
	public bool actionOfUnitThisTurn = false;
	public bool actionPhase = false;


	// Use this for initialization
	void Start()
	{
		myCam = gameObject.GetComponent<Camera> ();
	}

	public void setPlayerTurn(int i)
	{
		playerTurn = i;
	}

	public void turnWasPassed()
	{
		movedUnitThisTurn = false;
		actionOfUnitThisTurn = false;
	}

	void SelectionCast()
	{
		Ray myRay = myCam.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (myRay, out hit, myLayers)) {
			if (hit.collider.tag == "Grid") {
				if (lastUnit != null && !lastUnit.movedThisRound)
					lastUnit.SetMovement (hit.point);
			} else if (hit.collider.tag == "Player1" || hit.collider.tag == "Player2") {
				if (lastUnit != null)
					lastUnit.deselectUnit ();
				SelectPlayerUnit (hit);
			}else if (hit.collider.tag == "Charge"){
				//Debug.Log ("Charge " + hit.transform.position.ToString ());
				lastUnit.Charge (hit.transform.position);
			} else {
				Debug.Log ("No logic for " + hit.collider.name.ToString () + " in Select And Move");
			}
		}
	}

	public void deselectOldUnit()
	{
		
	}

	void SelectPlayerUnit(RaycastHit hit)
	{
		UnitM tmpUnit = hit.collider.gameObject.GetComponent<UnitM> ();
		lastUnit = tmpUnit;
		if (tmpUnit.ownedBy == playerTurn && !tmpUnit.movedThisRound) {
			tmpUnit.selectUnit ();
		} else {
			Debug.Log (tmpUnit.name.ToString ());
			//tmpUnit.displayStats ();
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown (0)) {
			SelectionCast ();
		}
	}
}
