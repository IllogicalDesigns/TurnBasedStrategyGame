using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitChess : MonoBehaviour {
    public int chargeSqrs = 3;
    [SerializeField] string team = "team0";
    [SerializeField] Grid m_grid;
    [SerializeField] ThinkingCoordinator bigThink;
    public List<vMove> finNodes = new List<vMove>();
    public List<vMove> firstNodes = new List<vMove>();
    int EdgeDanger = 0;
    int posElim = 5;
    int unitInLine = 4;
    float distCoverd = 0.1f;
    [SerializeField] LayerMask lM;
    [SerializeField] Color debugColor;
    bool checking = false;
    [SerializeField] int who = 0;

    // Use this for initialization
    void Start() {
        Debug.DrawRay(transform.position, Vector3.forward * 5, Color.green, 1f);
        Debug.DrawRay(transform.position, Vector3.right * 5, Color.red, 1f);
    }
    private void OnDrawGizmos() {
        Gizmos.color = debugColor;
        foreach (vMove vn in firstNodes) {
            if (vn != null) {
                Gizmos.DrawSphere(vn.endNode.worldPosition, (0.015f + (vn.treeVal * 0.01f)));
            }
        }
        foreach (vMove vn in finNodes) {
            if (vn != null) {
                Gizmos.DrawSphere(vn.endNode.worldPosition, (0.005f + (vn.treeVal * 0.01f)));
            }
        }
    }
    public string TeamNum() {
        return team;
    }
    vMove ChargeMove(Vector3 startXy, Vector3 dir, int dist, vMove previosNode, bool simMove) {
        bool unitInLine = false;
        float val = 0;
        int valUnitLine = 0;
        Vector3 newSpace = startXy + (dir * (dist));  //create a pos at the end spot
        Node endNode = m_grid.NodeFromWorldPoint(newSpace);  // Get node from new pos
        if (!endNode.walkable) {
            //Debug.DrawRay(newSpace, Vector3.up, Color.red, 5f);
            return null;
        }
        else {
            for (int i = 1; i <= dist; i++) {
                //Debug.DrawRay(newSpace, Vector3.up, Color.green, 5f);
                Vector3 testPos = startXy + (dir * (i));
                Node testN = m_grid.NodeFromWorldPoint(testPos);
                if (!testN.walkable) {
                    //Debug.DrawRay(newSpace, dir, Color.yellow, 5f);
                    //Debug.DrawRay(newSpace, Vector3.up, Color.yellow, 5f);
                    return null;
                }
            }
            Debug.DrawRay(newSpace, -dir * dist, debugColor, 5f);
            val += dist * distCoverd;
            val += CheckPushDanger(endNode);  //Check pushablity of endnode  //TODO actually check danger
            valUnitLine += CheckIfUnitInLine(dist, startXy, dir);
            if (valUnitLine > 0)
                unitInLine = true;
            if (CheckIfPossibleElim(newSpace, dir) && unitInLine)
                val += valUnitLine * posElim;
            else
                val += valUnitLine;
            //Distance to 
            //allows for movement in line in dir based on dist
            return new vMove(val, endNode, previosNode);
        }
    }
    bool CheckIfPossibleElim(Vector3 endPos, Vector3 dir) {
        Vector3 newSpace = endPos + (dir * (1));  //create a pos at the end spot
        Node endNode = m_grid.NodeFromWorldPoint(newSpace);
        if (endNode.walkable)
            return false;
        else
            return true;
    }
    int CheckIfUnitInLine(int dist, Vector3 startPos, Vector3 dir) { //TODO Test if proper
        int units = 0;
        for (int i = 0; i < dist; i++) {
            Vector3 newSpace = startPos + (dir * (i));  //create a pos along line
            RaycastHit hit;
            if (Physics.Raycast(newSpace, Vector3.up, out hit, 2f, lM)) {
                Debug.Log("1");
                if (!hit.collider.CompareTag(team)) {
                    units++;
                    Debug.DrawRay(newSpace, Vector3.up, Color.yellow, 5f);
                    Debug.Log("TEST");
                }
            }
        }
        return units;
    }
    int CheckPushDanger(Node testNode) {  //TODO actually check something
        //check n,s,e,w if we see an edge and a square to push from
        return -EdgeDanger * 2;
    }
    void SendEndPosToBigThink() {
        Vector3[] nEndPos = new Vector3[finNodes.Count];
        int i = 0;
        foreach (vMove vm in finNodes) {
            nEndPos[i] = vm.endNode.worldPosition;
            i++;
        }
        bigThink.endNodes(nEndPos, team);
    }
    IEnumerator NSWEChargeCheck(Vector3 startPos, vMove preMove, List<vMove> vMoves, bool simMove) {
        checking = true;
        for (int i = 1; i <= chargeSqrs; i++) {
            vMove newNodeVal = ChargeMove(startPos, Vector3.forward, i, preMove, simMove);
            if (newNodeVal != null)
                vMoves.Add(newNodeVal);
            //yield return null; //Wait frame
        }
        for (int i = 1; i <= chargeSqrs; i++) {
            vMove newNodeVal = ChargeMove(startPos, -Vector3.forward, i, preMove, simMove);
            if (newNodeVal != null)
                vMoves.Add(newNodeVal);
            //yield return null; //Wait frame
        }
        for (int i = 1; i <= chargeSqrs; i++) {
            vMove newNodeVal = ChargeMove(startPos, Vector3.right, i, preMove, simMove);
            if (newNodeVal != null)
                vMoves.Add(newNodeVal);
            //yield return null; //Wait frame
        }
        for (int i = 1; i <= chargeSqrs; i++) {
            vMove newNodeVal = ChargeMove(startPos, -Vector3.right, i, preMove, simMove);
            if (newNodeVal != null)
                vMoves.Add(newNodeVal);
            //yield return null; //Wait frame
        }
        checking = false;
        yield return null; //Wait frame
    }
    IEnumerator FirstLvlChargeCheck() {
        firstNodes.Clear();
        finNodes.Clear();
        bigThink.Thinking(who);
        StartCoroutine(NSWEChargeCheck(transform.position, null, firstNodes, false));
        while (checking) {
            yield return null; //Wait frame
        }
        firstNodes.Sort();
        firstNodes.Reverse();
        finNodes.AddRange(firstNodes);
        Debug.Log("Checked Move Number (0): " + finNodes.Count);
        finNodes.Sort();
        finNodes.Reverse();
        finNodes.RemoveRange(Mathf.RoundToInt(finNodes.Count / 2), Mathf.RoundToInt(finNodes.Count / 2));  //TODO perform pruning based on scores
        SendEndPosToBigThink();
        bigThink.DoneThinking(who);
        yield return null;
    }
    IEnumerator LvlChargeCheck() {
        List<vMove> startNodes = new List<vMove>(finNodes);
        bigThink.Thinking(who);
        finNodes.Clear();
        foreach (vMove vm in startNodes) {
            StartCoroutine(NSWEChargeCheck(vm.endNode.worldPosition, vm, finNodes, true));
            while (checking) {
                yield return null; //Wait frame
            }
        }
        Debug.Log("Checked Move Number (1): " + finNodes.Count);
        firstNodes.Sort();
        firstNodes.Reverse();
        finNodes.Sort();
        finNodes.Reverse();
        finNodes.RemoveRange(Mathf.RoundToInt(finNodes.Count / 2), Mathf.RoundToInt(finNodes.Count / 2));
        //Send in finNodePos to bigThink
        SendEndPosToBigThink();
        bigThink.DoneThinking(who);
        yield return null;
    }
    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.F)) {
            finNodes.Clear();
            StartCoroutine(FirstLvlChargeCheck());
        }
        if (Input.GetKeyDown(KeyCode.G)) {
            StartCoroutine(LvlChargeCheck());  //TODO slurp of information from opponets on possible positions
        }
    }
    public void FirstCheck() {
        finNodes.Clear();
        StartCoroutine(FirstLvlChargeCheck());
    }
    public void Check() {
        StartCoroutine(LvlChargeCheck());
    }
}

public class vMove : System.IComparable<vMove> {
    public float treeVal;
    public float value;
    public Node endNode;
    public vMove prevNode;
    public vMove(float val, Node eNde, vMove preNode)  //Constructs a val node
    {
        value = val;
        endNode = eNde;
        prevNode = preNode;
        treeVal = treeValFill(value);
    }
    public float treeValFill(float soFar) {
        soFar += value;
        if (prevNode != null)
            soFar = prevNode.treeValFill(soFar);
        treeVal = soFar;
        return treeVal;
    }
    public int CompareTo(vMove other)  //Lets us use .sort 
    {
        return treeVal.CompareTo(other.treeVal);
    }
}
