using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamSelect : MonoBehaviour {
    [SerializeField] LayerMask selectable;
    Camera m_Cam;
    [SerializeField] GameObject currentSelected;

    void Start() {
        m_Cam = gameObject.GetComponent<Camera>();  //Setup the main camera reference for raycasting
    }

    void SelectRay() {
        Ray myRay = m_Cam.ScreenPointToRay(Input.mousePosition);  //Create a ray from the main camera using the mouse pointer
        RaycastHit hit;
        if (Physics.Raycast(myRay, out hit, selectable)) {
            if (hit.collider.CompareTag("Team0")) {
                if (currentSelected)
                    currentSelected.SendMessage("ClearDisplayOptions");
                currentSelected = hit.collider.gameObject;
                hit.collider.gameObject.SendMessage("DisplayOptions");
            }
            else if (hit.collider.CompareTag("Grid") && currentSelected != null) {
                currentSelected.SendMessage("MoveTo",  hit.collider.gameObject.transform.position);
                currentSelected.SendMessage("ClearDisplayOptions");
            }
        }
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetMouseButtonDown(0)) {
            SelectRay();
        }
        if (Input.GetMouseButtonDown(1) && currentSelected != null) {
            currentSelected.SendMessage("ClearDisplayOptions");
        }
    }
}
