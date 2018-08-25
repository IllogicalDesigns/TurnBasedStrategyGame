using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//odd behavior at 3 search depth, correct at 2 depth
public class ThinkingCoordinator : MonoBehaviour {
    public bool[] thinkings;
    public float currDepth = 0;
    public float searchDepth = 3;
    bool thinking = false;
    bool AnalyzingState = false;
    [SerializeField] Vector3[] endNodesRed;
    [SerializeField] Vector3[] endNodesBlue;
    public List<GameObject> bluePosCurrAnal = new List<GameObject>();
    public List<GameObject> redPosCurrAnal = new List<GameObject>();
    [SerializeField] string blueTag;
    [SerializeField] string redTag;
    [SerializeField] UnitChess[] chesses;
    [SerializeField] GameObject dummyCollider;

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
    void IsAnyoneThinking() {
        thinking = false;
        for (int x = 0; x < thinkings.Length; x++) {
            if (thinkings[x])
                thinking = true;
        }
    }
    void ClearDummyCollider() {
        foreach (GameObject b in bluePosCurrAnal) {
            Destroy(b);
        }
        foreach (GameObject r in redPosCurrAnal) {
            Destroy(r);
        }
    }
    void PlaceDummyColliders() {
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
    }
    IEnumerator Checks() {
        currDepth = 0;
        foreach (UnitChess uc in chesses) {
            if (uc.gameObject.activeInHierarchy) {
                uc.FirstCheck();
                yield return null;
            }
        }
        while (thinking) {
            yield return null;
        }

        /*while (true) {
                if (Input.GetKeyDown(KeyCode.N))
                    break;
                yield return null;
            }*/

        currDepth++;
        while (currDepth < searchDepth) {
            ClearDummyCollider();
            PlaceDummyColliders();
            foreach (UnitChess uc in chesses) {
                if (uc.gameObject.activeInHierarchy) {
                    uc.Check();
                    yield return null;
                }
            }
            while (thinking) {
                yield return null;
            }

            /*while (true) {
                if (Input.GetKeyDown(KeyCode.N))
                    break;
                yield return null;
            }*/
            currDepth++;
        }
        ClearDummyCollider();
        yield return null;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(Checks());
        }
    }
}
