using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetCam : MonoBehaviour {
    [SerializeField] LayerMask selectable;
    Camera m_Cam;
    [SerializeField] GameObject currentSelected;
    string teamSelect = "Team0";

    void Start() {
        m_Cam = gameObject.GetComponent<Camera>();  //Setup the main camera reference for raycasting
    }

    public void SetTeamSelect(string team) {
        teamSelect = team;
    }
    public string GetTeamSelect(string team) {
        return teamSelect;
    }


    void SelectRay() {
        Ray myRay = m_Cam.ScreenPointToRay(Input.mousePosition);  //Create a ray from the main camera using the mouse pointer
        RaycastHit hit;
        if (Physics.Raycast(myRay, out hit, selectable)) {
            if (hit.collider.CompareTag(teamSelect)) {
                if (currentSelected) {
                    currentSelected.SendMessage("ClearDisplayOptions");
                }
                currentSelected = hit.collider.gameObject;
                hit.collider.gameObject.SendMessage("DisplayOptions");
            }
            else if (hit.collider.CompareTag("Grid") && currentSelected != null) {
                currentSelected.SendMessage("MoveTo",  hit.collider.gameObject.transform.position);
                currentSelected.SendMessage("ClearDisplayOptions");
            }
        }
    }


    void Update() {

        if (Input.GetMouseButtonDown(0)) {
            SelectRay();
        }
        if (Input.GetMouseButtonDown(1) && currentSelected != null) {
            currentSelected.SendMessage("ClearDisplayOptions");
        }
    }
}
