using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionCursor : MonoBehaviour {
	[SerializeField] LayerMask selectable;  //What can we select, grid, units, special
	public TBSUnit currentlySelectedUnit;  //Reference to send commands to

	Camera m_Cam;  //Main Cam Reference

	// Use this for initialization
	void Start () {
		m_Cam = gameObject.GetComponent<Camera> ();  //Setup the main camera reference for raycasting
	}

	void UnitSelection(RaycastHit hit){
		currentlySelectedUnit = hit.collider.gameObject.GetComponent<TBSUnit>();  //Set our current unit to the attempted selection

		if (currentlySelectedUnit.SelectUnit ()) {
			Debug.Log ("Select this unit " + hit.collider.name.ToString ());  
		}
		else {
			Debug.Log ("Unable to Select this unit " + hit.collider.name.ToString ());
			currentlySelectedUnit = null;
		}
	}


	void SelectionCast()
	{
		Ray myRay = m_Cam.ScreenPointToRay (Input.mousePosition);  //Create a ray from the main camera using the mouse pointer
		RaycastHit hit;

		if (Physics.Raycast (myRay, out hit, selectable)) {
			Debug.DrawLine(transform.position, hit.point, Color.blue, 10f);
			if (hit.collider.tag == "Grid") {
				if (currentlySelectedUnit != null && currentlySelectedUnit.movementLeft > 0)
					currentlySelectedUnit.SetMovement (hit.point);
			} else if (hit.collider.tag == "Player") {
				UnitSelection(hit);  //Attempt to select the unit, or select it
			}else if (hit.collider.tag == "Charge"){
				if (currentlySelectedUnit != null && !currentlySelectedUnit.chargedDuringAction)
					currentlySelectedUnit.moveToChargeLocation (hit.collider.gameObject.transform.position);
			} else {
				Debug.Log ("No logic for " + hit.collider.name.ToString () + " in Select And Move");
			}
		}
	}

	void Update()
	{
		if (Input.GetMouseButtonDown (0)) {
			SelectionCast ();
		}
	}
}
