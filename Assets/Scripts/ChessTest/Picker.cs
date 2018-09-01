using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picker : MonoBehaviour {
    public bool AI = false;
    public bool ourTurn = false;
    public UnitChess[] UCs;

    public void OurTurn() {
        ourTurn = true;
    }
    public void NotOurTurn() {
        ourTurn = false;
    }
    public void MoveAI() {
        if (AI)
            CompareAndMove();
    }
    void CompareAndMove() {  //TODO fix God awful node weight compare
        if (UCs[0].gameObject.activeInHierarchy && UCs[1].gameObject.activeInHierarchy && UCs[2].gameObject.activeInHierarchy) {
            if (UCs[0].finNodes[0].treeVal > UCs[1].finNodes[0].treeVal && UCs[0].finNodes[0].treeVal > UCs[2].finNodes[0].treeVal) {
                //UCs[0].gameObject.transform.position = UCs[0].firstNodes[0].endNode.worldPosition;
                UCs[0].gameObject.SendMessage("MoveTo", (UCs[0].firstNodes[0].endNode));
            }
            else if (UCs[1].finNodes[0].treeVal > UCs[0].finNodes[0].treeVal && UCs[1].finNodes[0].treeVal > UCs[2].finNodes[0].treeVal) {
                UCs[1].gameObject.SendMessage("MoveTo", (UCs[1].firstNodes[0].endNode));
                //UCs[1].gameObject.transform.position = UCs[1].firstNodes[0].endNode.worldPosition;
            }
            else {
                UCs[2].gameObject.SendMessage("MoveTo", (UCs[2].firstNodes[0].endNode));
                //UCs[1].gameObject.transform.position = UCs[1].firstNodes[0].endNode.worldPosition;
            }
        }
        else if (UCs[0].gameObject.activeInHierarchy) {
            UCs[0].gameObject.SendMessage("MoveTo", (UCs[0].firstNodes[0].endNode));
            //UCs[0].gameObject.transform.position = UCs[0].firstNodes[0].endNode.worldPosition;
        }
        else if (UCs[1].gameObject.activeInHierarchy) {
            UCs[1].gameObject.SendMessage("MoveTo", (UCs[1].firstNodes[0].endNode));
            //UCs[1].gameObject.transform.position = UCs[1].firstNodes[0].endNode.worldPosition;
        }
        else if (UCs[2].gameObject.activeInHierarchy) {
            UCs[2].gameObject.SendMessage("MoveTo", (UCs[2].firstNodes[0].endNode));
            //UCs[1].gameObject.transform.position = UCs[1].firstNodes[0].endNode.worldPosition;
        }
        else {
            Debug.Log("We be dead");
        }
    }

    private void Update() {
        if (ourTurn && Input.GetKeyDown(KeyCode.A))
            MoveAI();
    }
}
