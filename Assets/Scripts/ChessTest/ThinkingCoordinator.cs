using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//odd behavior at 3 search depth, correct at 2 depth
public class ThinkingCoordinator : MonoBehaviour {
    public bool[] thinkings;
    public float currDepth = 0;
    public float searchDepth = 3;
    public bool thinking = false;
    bool AnalyzingState = false;
    [SerializeField] Vector3[] endNodesRed;
    [SerializeField] Vector3[] endNodesBlue;
    [SerializeField] vMove[] vMovesRed;
    [SerializeField] vMove[] vMovesBlue;
    public List<GameObject> bluePosCurrAnal = new List<GameObject>();
    public List<GameObject> redPosCurrAnal = new List<GameObject>();
    [SerializeField] string blueTag;
    [SerializeField] string redTag;
    [SerializeField] UnitChess[] chesses;
    [SerializeField] GameObject dummyCollider;

    [SerializeField] Picker[] groups;

    /*private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        if(BestNode != null)
       Gizmos.DrawSphere(BestNode.endNode.worldPosition, (0.015f + (BestNode.treeVal * 0.01f)));
    }*/

    public void DoneThinking(int who) {
        thinkings[who] = false;
        IsAnyoneThinking();
    }
    public void Thinking(int who) {
        thinkings[who] = true;
        thinking = true;
    }
    public void endNodes(Vector3[] eNodesPos, string blue) {
        if (blue == blueTag)
            endNodesBlue = eNodesPos;
        else
            endNodesRed = eNodesPos;
    }
    public bool IsAnyoneThinking() {
        bool notThinking = false;
        for (int x = 0; x < thinkings.Length; x++) {
            if (thinkings[x]) {
                notThinking = true;
                break;
            }
        }
        return notThinking;
    }
    void ClearDummyCollider() {
        foreach (GameObject b in bluePosCurrAnal) {
            Destroy(b);
        }
        foreach (GameObject r in redPosCurrAnal) {
            Destroy(r);
        }
    }
    void PlaceDummyColliders(UnitChess[] ucs) { //TODO fully deprecate and move to unit chess to better handle testing of being pushed
        /*foreach (UnitChess uc in ucs) {
            uc.gameObject.GetComponent<BoxCollider>().enabled = false;
        }
        foreach (Vector3 v in endNodesRed) {
            GameObject newAdd = Instantiate(dummyCollider, v, dummyCollider.transform.rotation) as GameObject;
            newAdd.tag = redTag;
            redPosCurrAnal.Add(newAdd);
        }
        foreach (Vector3 v in endNodesBlue) {
            GameObject newAdd = Instantiate(dummyCollider, v, dummyCollider.transform.rotation) as GameObject;
            newAdd.tag = blueTag;
            bluePosCurrAnal.Add(newAdd);
        }
        foreach (UnitChess uc in ucs) {
            uc.gameObject.GetComponent<BoxCollider>().enabled = true;
        }*/
    }
    IEnumerator Checks() {//TODO add a loop that works through the list once from a given postion and check for whos turn it is and start there.
        currDepth = 0;
        thinking = true;
        foreach (UnitChess uc in chesses) {
            if (uc.gameObject.activeInHierarchy) {
                uc.FirstCheck();
                yield return null;
            }
        }
        while (IsAnyoneThinking()) {
            yield return null;
        }
        currDepth++;
        while (currDepth < searchDepth) {
            thinking = true;
            ClearDummyCollider();
            PlaceDummyColliders(chesses);
            foreach (UnitChess uc in chesses) {
                if (uc.gameObject.activeInHierarchy) {
                    uc.Check();
                    yield return null;
                }
            }
            while (IsAnyoneThinking()) {
                yield return null;
            }
            currDepth++;
        }
        ClearDummyCollider();
        yield return null;
    }

    IEnumerator GroupedChecks() {
        currDepth = 0;
        thinking = true;
        foreach (Picker p in groups) {
            foreach(UnitChess uc in p.UCs) {
                if (uc.gameObject.activeInHierarchy) {
                    uc.FirstCheck();
                    yield return null;
                    //yield return new WaitForSecondsRealtime(0.25f);
                }
            }
            while (IsAnyoneThinking()) {
                yield return null;
            }
        }
        while (IsAnyoneThinking()) {
            yield return null;
        }

        currDepth++;
        while (currDepth < searchDepth) {
            //yield return new WaitForSecondsRealtime(2f);
            foreach (Picker p in groups) {
                foreach (UnitChess uc in p.UCs) {
                    if (uc.gameObject.activeInHierarchy) {
                        uc.Check();
                        yield return null;
                        //yield return new WaitForSecondsRealtime(0.25f);
                    }
                }
                while (IsAnyoneThinking()) {
                    yield return null;
                }
            }
            currDepth++;
        }
        thinking = false;
    }

    public void Check() {
        StartCoroutine(GroupedChecks());
    }
    public int CheckForLoss() {
        int red = 0;
        int blue = 0;
        foreach(UnitChess uc in chesses) {
            if (uc.TeamNum() == "Team0" && uc.gameObject.activeInHierarchy)
                blue++;
            if (uc.TeamNum() == "Team1" && uc.gameObject.activeInHierarchy)
                red++;
        }
        if (red == 0)
            return 1;
        if (blue == 0)
            return 0;
        return -1;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Check();
        }
    }
}
