using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUnit : MonoBehaviour
{
    public int movement = 2;
    public int movementMax = 2;
    bool[,] visitedRowCol;
    public Grid m_grid;
    int nodeSize = 1; //Used for movement grid
    List<Node> nodeForEval = new List<Node>();
    //public Dictionary<Node, int> nodeVals = new Dictionary<Node, int>();
    public List<vMove> nodeWithValues = new List<vMove>();
    public int bestChoice = 0;  //Index of the nodeval
    int pathLegth = 1;
    bool justREturnedPath = false;
    public UnitAction m_Action;
    int actionMod = 1;
    public LayerMask walkableMask;
    Node previosUnWalkableNode;
    public bool moving = false;

    Material activeMat;
    Material deactiveMat;
    PlayerInfo m_Player;

    int maxThreatReach = 5;

    public Transform target;

    float yOffset = 0.5f;
    int targetIndex;
    float speed = 5;

    [SerializeField] float stayWeight = 2f;
    [SerializeField] float actWeight = 1f;

    // Use this for initialization
    void Start()
    {
        updateOccupiedNode();
        m_Player = gameObject.GetComponentInParent<PlayerInfo>();
        activeMat = m_Player.activeColor;
        deactiveMat = m_Player.deactiveColor;
    }

    void OnDrawGizmos()
    {
        foreach(Node n in nodeForEval)
            Gizmos.DrawWireSphere(n.worldPosition, 0.05f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int whoOwnsMe()
    {
        return m_Player.ownerNumber;
    }

    public void deactivateUnit()
    {
        gameObject.GetComponent<Renderer>().material = deactiveMat;
        movement = 0;
    }

    public void activateUnit()
    {
        gameObject.GetComponent<Renderer>().material = activeMat;
        movement = movementMax;
    }

    public void updateOccupiedNode()
    {
        if (previosUnWalkableNode != null)
            previosUnWalkableNode.occupied = false;
        previosUnWalkableNode = m_grid.NodeFromWorldPoint(transform.position);
        previosUnWalkableNode.occupied = true;
    }

    public static float Sigmoid(float value)
    {
        return 1.0f / (1.0f + Mathf.Exp((-value)));
    }

    /*int highestValue(Dictionary<Node, int> m_Dict)
    {
        int highest = -1;
        int k = 0;
        int hIndex = 0;
        foreach (int i in m_Dict.Values)
        {
            if (i > highest)
            {
                highest = i;
                hIndex = k;
            }
            k++;
        }
        return hIndex;  //returns an index for the highest index
    }*/

    public IEnumerator FindBestIshPath(int rndness) //Deprecated
    {
        Debug.Log(gameObject.name + " FindingBest");
        if (previosUnWalkableNode != null)
            previosUnWalkableNode.occupied = false;
        nodeWithValues.Clear();

        //Find all accessable nodes
        yield return StartCoroutine(GatherReachableNodes(transform.position, movement));

        Debug.Log(gameObject.name + "  NodeChecking " + nodeForEval.Count);
        //Foreach node get the node's worth
        foreach (Node n in nodeForEval)
        {
            //Debug.Log("Found Nodes " + gameObject.name);
            //yield return StartCoroutine(locationEval(n));
        }
        Debug.Log(gameObject.name + "  NodeVals " + nodeWithValues.Count);
        //bestChoice = highestValue(nodeVals);
        nodeWithValues.Sort();  //Will this actually sort by values
                                //yield return 
                                //Debug.Log("Best choice was " + nodeWithValues[0].node.worldPosition + " : " + nodeWithValues[0].value);
                                //Debug.DrawRay(nodeWithValues[0].node.worldPosition, Vector3.up * 10, Color.green, 5);
    }

    public IEnumerator NewBestishPath(int ignoredRnd)
    {
        //Return to a proper state
        movement = movementMax;
        //Clear old bestish path stuff
        nodeWithValues.Clear();
        if (previosUnWalkableNode != null)
            previosUnWalkableNode.occupied = false;
        //Find all accessable nodes
        //yield return StartCoroutine(GatherReachableNodes(transform.position, movement));

        nodeForEval.Clear();
        visitedRowCol = new bool[Mathf.RoundToInt(m_grid.gridWorldSize.x), Mathf.RoundToInt(m_grid.gridWorldSize.y)];
        reachableNodes2(transform.position, 0);
        Debug.Log("# of eval nodes " + nodeForEval.Count);

        // remove nodes for eval, remove gather reachable, create a reachable with single gather and return function

        //float stay = Sigmoid(m_grid.NodeFromWorldPoint(transform.position).threatLvl * stayWeight);  //Eval original node for possible leaving dangers  TODO balence
        float stay = m_grid.NodeFromWorldPoint(transform.position).threatLvl * stayWeight;
        float master = 99999f;

        //Eval each node for danger if left and value if moved
        for (int i = 0; i < nodeForEval.Count; i++)
        {
            if (nodeForEval[i].walkable && !nodeForEval[i].occupied)
            {
                justREturnedPath = false; //UnsureWhat this fixes
                yield return null;
                updateOccupiedNode();

                //Debug.Log("walkable And !occupied");
                //yield return StartCoroutine(locationEval(nodeForEval[i])); //unsure what this does

                vMove[] tmp = m_Action.EvalFunctForAi(nodeForEval[i], target.position);  //Eval the action
                for(int k = 0; k <tmp.Length; k++)
                {
                    //float Action = Sigmoid(tmp[k].value + actWeight);
                    float Action = tmp[k].value * actWeight;
                    master = Action + stay;
                    //master = Sigmoid(Action + -stay);
                    tmp[k].value = master;
                    Debug.Log(gameObject.name + " Charge: " + Action + " Stay: " + stay + "Master Weight: " + master + " " + tmp[k].endNode.worldPosition);
                    nodeWithValues.Add(tmp[k]);
                }
               /* float Action = Sigmoid(tmp[0].value + actWeight);  
                master = Sigmoid(Action + -stay);
                tmp[0].value = master;
                Debug.Log(gameObject.name + " Charge: " + Action + " Stay: " + stay + "Master Weight: " + master);
                nodeWithValues.Add(tmp[0]);*/
            }
        }
        nodeWithValues.Sort();
        Debug.Log(gameObject.name + " BEST move " + nodeWithValues[0].value + " loc:" + nodeWithValues[0].endNode.worldPosition);
        
        yield return null;
    }

    int distToUnwalkable(Node startPos, Vector3 dir)
    {
        Vector3 strtPos = startPos.worldPosition;
        //search till our movement range is exceded or we find an edge
        for (int i = 1; i < maxThreatReach; i++)
        {
            Vector3 newSpace = strtPos + (dir * (i));
            Node tmpNode = m_grid.NodeFromWorldPoint(newSpace);
            if (!tmpNode.walkable)
                return i;
        }
        return maxThreatReach;
    }

    IEnumerator GatherReachableNodes(Vector3 startPos, int pathLength)
    {
        nodeForEval.Clear();
        visitedRowCol = new bool[Mathf.RoundToInt(m_grid.gridWorldSize.x), Mathf.RoundToInt(m_grid.gridWorldSize.y)];
        Node startNode = m_grid.NodeFromWorldPoint(startPos);
        yield return StartCoroutine(reachableNodes(startPos, 0));
        reachableNodes(startPos, 0);
    }

    IEnumerator moveThroughPath(Vector3[] _path)
    {
        targetIndex = 0;
        moving = true;
        Vector3 currentWaypoint = _path[0];
        while (true)
        {
            if (transform.position == new Vector3(currentWaypoint.x, currentWaypoint.y + yOffset, currentWaypoint.z))
            {
                targetIndex++;
                if (targetIndex >= _path.Length)
                {
                    updateOccupiedNode();
                    CheckIfWeFellOff();
                    moving = false;
                    yield break;
                }
                currentWaypoint = _path[targetIndex];
            }
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(currentWaypoint.x, currentWaypoint.y + yOffset, currentWaypoint.z), speed * Time.deltaTime);
            yield return null;
        }
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        movement -= (newPath.Length - 1);
        StartCoroutine(moveThroughPath(newPath));
    }

    //Also strikes old node as walkable and new node as occupied/Unwalkable
    public void CheckIfWeFellOff()
    {
        //Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), Vector3.down * 2f, Color.green, 10f);
        if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), Vector3.down, 2f, walkableMask))
        {
            //m_Player.RemoveDeadUnit(this);
            gameObject.SetActive(false);
        }
        else
        {
            //Debug.Log("Something is under us!");
        }
    }

    public void MoveToNewPos(Vector3 nPos)
    {
        if (previosUnWalkableNode != null)
            previosUnWalkableNode.occupied = false;
        PathRequestManager.RequestPath(transform.position, nPos, OnPathFound);
    }

    //TODO Prevent race conditons in this later
    IEnumerator reachableNodes(Vector3 nodePos, int depth)
    {
        Node node = m_grid.NodeFromWorldPoint(nodePos);
        if (visitedRowCol[Mathf.RoundToInt(node.gridX), Mathf.RoundToInt(node.gridY)] == false && node.walkable && depth <= movement)
        {
            visitedRowCol[Mathf.RoundToInt(node.gridX), Mathf.RoundToInt(node.gridY)] = true;
            if (depth != 0 && !node.occupied)
            {
                nodeForEval.Add(node);
                reachableNodes(new Vector3(node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                reachableNodes(new Vector3(node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                reachableNodes(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), depth + 1);
                reachableNodes(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), depth + 1);
                yield return null;
            }
            else if (depth == 0)
            {
                reachableNodes(new Vector3(node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                reachableNodes(new Vector3(node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                reachableNodes(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), depth + 1);
                reachableNodes(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), depth + 1);
                yield return null;
            }
        }
        yield return null;
    }

    void reachableNodes2(Vector3 nodePos, int depth)
    {
        //Debug.DrawRay(nodePos, Vector3.up * 2f, Color.blue, 2.5f);
        Node node = m_grid.NodeFromWorldPoint(nodePos);
        if (visitedRowCol[Mathf.RoundToInt(node.gridX), Mathf.RoundToInt(node.gridY)] == false && node.walkable && depth <= movement)
        {
            visitedRowCol[Mathf.RoundToInt(node.gridX), Mathf.RoundToInt(node.gridY)] = true;
            if (depth != 0 && !node.occupied)
            {
                nodeForEval.Add(node);
                reachableNodes2(new Vector3(node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                reachableNodes2(new Vector3(node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                reachableNodes2(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), depth + 1);
                reachableNodes2(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), depth + 1);
            }
            else if (depth == 0)
            {
                reachableNodes2(new Vector3(node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                reachableNodes2(new Vector3(node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                reachableNodes2(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), depth + 1);
                reachableNodes2(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), depth + 1);
            }
        }
    }

    //Can this be handles in the charge action class? from the pushing end?
    public void Pushed(Vector3 newPos, Vector3 chargeDir)
    {
        newPos = newPos + (-chargeDir.normalized * 1f);
        //if (!moving)
        //{
        Vector3[] tmpArray = new Vector3[] { new Vector3(newPos.x, newPos.y + (yOffset / 2), newPos.z) };
        StartCoroutine(moveThroughPath(tmpArray));
        //}
    }
}

public class vMove1 : System.IComparable<vMove1>
{
    public float value;
    public Node endNode;
    public Node moveNode;
    public vMove1(float val, Node eNde, Node mNode)  //Constructs a val node
    {
        value = val;
        endNode = eNde;
        moveNode = mNode;
    }
    public int CompareTo(vMove other)  //Lets us use .sort 
    {
        return value.CompareTo(other.value);
    }
}
