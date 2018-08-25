using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picker : MonoBehaviour {
    public bool red = false;
    [SerializeField] UnitChess[] UCs;

    // Use this for initialization
    void Start() {

    }

    void CompareAndMove() {
        if (UCs[0].gameObject.activeInHierarchy && UCs[1].gameObject.activeInHierarchy) {
            if (UCs[0].finNodes[0].treeVal > UCs[1].finNodes[0].treeVal) {
                //UCs[0].gameObject.transform.position = UCs[0].firstNodes[0].endNode.worldPosition;
                UCs[0].gameObject.SendMessage("MoveTo", (UCs[0].firstNodes[0].endNode));
            }
            else {
                UCs[1].gameObject.SendMessage("MoveTo", (UCs[1].firstNodes[0].endNode));
                //UCs[1].gameObject.transform.position = UCs[1].firstNodes[0].endNode.worldPosition;
            }
        }else if (UCs[0].gameObject.activeInHierarchy) {
            UCs[0].gameObject.SendMessage("MoveTo", (UCs[0].firstNodes[0].endNode));
            //UCs[0].gameObject.transform.position = UCs[0].firstNodes[0].endNode.worldPosition;
        }
        else if (UCs[1].gameObject.activeInHierarchy) {
            UCs[1].gameObject.SendMessage("MoveTo", (UCs[1].firstNodes[0].endNode));
            //UCs[1].gameObject.transform.position = UCs[1].firstNodes[0].endNode.worldPosition;
        }
        else {
            Debug.Log("We be dead");
        }
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.A) && red) {
            CompareAndMove();
        }
        if (Input.GetKeyDown(KeyCode.S) && !red) {
            CompareAndMove();
        }

    }
}
