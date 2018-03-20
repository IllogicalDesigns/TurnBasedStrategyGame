using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class NetSelection : NetworkBehaviour {
    [SerializeField] LayerMask selectable;  //What can we select, grid, units, special
    public TBSUnit currentlySelectedUnit;  //Reference to send commands to

    Camera m_Cam;  //Main Cam Reference

    // Use this for initialization
    void Start()
    {
        m_Cam = gameObject.GetComponent<Camera>();  //Setup the main camera reference for raycasting
    }

    void UnitSelection(RaycastHit hit)
    {
        currentlySelectedUnit = hit.collider.gameObject.GetComponent<TBSUnit>();  //Set our current unit to the attempted selection

        if (currentlySelectedUnit.SelectUnit())
        {
            Debug.Log("Select this unit " + hit.collider.name.ToString());
        }
        else
        {
            Debug.Log("Unable to Select this unit " + hit.collider.name.ToString());
            currentlySelectedUnit = null;
        }
    }


    public void SelectionCast()
    {
        Ray myRay = m_Cam.ScreenPointToRay(Input.mousePosition);  //Create a ray from the main camera using the mouse pointer
        RaycastHit hit;

        if (Physics.Raycast(myRay, out hit, selectable))
        {
            Debug.DrawLine(transform.position, hit.point, Color.blue, 10f);
            if (hit.collider.tag == "Grid")
            {
                CmdsetMovementOnUnit(hit.point); //Command to server to set the movement on a unit
            }
            else if (hit.collider.tag == "Player")
            {
                if (currentlySelectedUnit != null)
                    currentlySelectedUnit.deselectedUnit();
                UnitSelection(hit);  //Attempt to select the unit, or select it
            }
            else if (hit.collider.tag == "Charge")
            {
                if (currentlySelectedUnit != null && !currentlySelectedUnit.chargedDuringAction)
                    currentlySelectedUnit.SendMessage("Charge", hit.transform.position);
            }
            else
            {
                Debug.Log("No logic for " + hit.collider.name.ToString() + " in Select And Move");
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectionCast();
        }
    }

    [Command]
    void CmdsetMovementOnUnit (Vector3 pos)
    {
        if (currentlySelectedUnit != null && currentlySelectedUnit.movementLeft > 0)
            currentlySelectedUnit.SetMovement(pos);  //Network this TODO
    }
}
