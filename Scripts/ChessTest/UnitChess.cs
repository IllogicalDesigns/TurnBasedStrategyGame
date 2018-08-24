using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitChess : MonoBehaviour
{
    public int chargeSqrs = 3;
    [SerializeField] int team = 0;
    [SerializeField] Grid m_grid;
    //valMove
    public List<vMove> nodeWithValues = new List<vMove>();

    int EdgeDanger = 2;
    int posElim = 1;
    int unitInLine = 1;
    [SerializeField] LayerMask lM;

    // Use this for initialization
    void Start()
    {
        Debug.DrawRay(transform.position, Vector3.forward * 5, Color.green, 1f);
        Debug.DrawRay(transform.position, Vector3.right * 5, Color.red, 1f);
    }

    private void OnDrawGizmos()
    {
        foreach (vMove vn in nodeWithValues)
        {
            Gizmos.color = Color.green;
            if (vn != null)
            {
                Gizmos.DrawSphere(vn.endNode.worldPosition, (1f + (vn.value * 0.1f)));
                Debug.Log("!");
            }
        }
    }

    public int TeamNum()
    {
        return team;
    }
    vMove ChargeMove(Vector3 startXy, Vector3 dir, int dist, vMove previosNode)
    {
        int val = 0;
        Vector3 newSpace = startXy + (dir * (dist));  //create a pos at the end spot
        Node endNode = m_grid.NodeFromWorldPoint(newSpace);  // Get node from new pos
        if (!endNode.walkable)
        {
            Debug.Log("X");
            Debug.DrawRay(newSpace, Vector3.up, Color.red, 5f);
            return null;
        }
        else
        {
            Debug.DrawRay(newSpace, -dir * dist, Color.green, 5f);
            val += CheckPushDanger(endNode);  //Check pushablity of endnode  //TODO actually check danger
            val += CheckIfUnitInLine(newSpace, new Vector3(startXy.x, 0, startXy.y));  //TODO finish function
            if (CheckIfPossibleElim(newSpace, dir))
                val += posElim;
            //allows for movement in line in dir based on dist
            return new vMove(val, endNode, previosNode);
        }
    }
    bool CheckIfPossibleElim(Vector3 endPos, Vector3 dir)
    {
        Vector3 newSpace = endPos + (dir * (1));  //create a pos at the end spot
        Node endNode = m_grid.NodeFromWorldPoint(newSpace);
        if (endNode.walkable)
            return false;
        else
            return true;
    }
    int CheckIfUnitInLine(Vector3 endPos, Vector3 startPos)  //TODO catch all units and reject teamates?
    {
        //Check what units maybe in line
        RaycastHit hit;
        if (Physics.Linecast(endPos, startPos, out hit, lM))
            return unitInLine;
        else
            return 0;
    }
    int CheckPushDanger(Node testNode)
    {
        //check n,s,e,w if we see an edge and a square to push from
        return -EdgeDanger * 2;
    }

    IEnumerator CheckChargeMoves()
    {
        for (int i = 1; i <= chargeSqrs; i++)
        {
            vMove newNodeVal = ChargeMove(transform.position, Vector3.forward, i, null);
            if(newNodeVal != null)
                nodeWithValues.Add(newNodeVal);
            yield return null;
        }
        for (int i = 1; i <= chargeSqrs; i++)
        {
            vMove newNodeVal = ChargeMove(transform.position, -Vector3.forward, i, null);
            if (newNodeVal != null)
                nodeWithValues.Add(newNodeVal);
            yield return null;
        }
        for (int i = 1; i <= chargeSqrs; i++)
        {
            vMove newNodeVal = ChargeMove(transform.position, Vector3.right, i, null);
            if (newNodeVal != null)
                nodeWithValues.Add(newNodeVal);
            yield return null;
        }
        for (int i = 1; i <= chargeSqrs; i++)
        {
            vMove newNodeVal = ChargeMove(transform.position, -Vector3.right, i, null);
            if (newNodeVal != null)
                nodeWithValues.Add(newNodeVal);
            yield return null;
        }
        Debug.Log("Checked Move Number: " + nodeWithValues.Count);
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            nodeWithValues.Clear();
            StartCoroutine(CheckChargeMoves());
        }
    }
}

public class vMove : System.IComparable<vMove>
{
    public float valueN;
    public Node endNodeN;
    public vMove prevNode;
    public vMove(float val, Node eNde, vMove preNode)  //Constructs a val node
    {
        valueN = val;
        endNodeN = eNde;
        prevNode = preNode;
    }
    public int CompareTo(vMove other)  //Lets us use .sort 
    {
        return value.CompareTo(other.value);
    }
}
