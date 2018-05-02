using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUnit : MonoBehaviour {
    public int movement = 2;
    public int movementMax = 2;
    bool[,] visitedRowCol;
    public Grid m_grid;
    int nodeSize = 1; //Used for movement grid
    List<Node> nodeForEval = new List<Node>();
    //public Dictionary<Node, int> nodeVals = new Dictionary<Node, int>();
    public List<valuedNode> nodeWithValues = new List<valuedNode>();
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

    // Use this for initialization
    void Start () {
        updateOccupiedNode();
        m_Player = gameObject.GetComponentInParent<PlayerInfo>();
        activeMat = m_Player.activeColor;
        deactiveMat = m_Player.deactiveColor;
    }

    // Update is called once per frame
    void Update () {
		
	}

    public int whoOwnsMe()
    {
        return m_Player.ownerNumber;
    }

    public void deactivateUnit ()
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
        if(previosUnWalkableNode != null)
            previosUnWalkableNode.occupied = false;
        previosUnWalkableNode = m_grid.NodeFromWorldPoint(transform.position);
        previosUnWalkableNode.occupied = true;
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

    public IEnumerator FindBestIshPath(int rndness)
    {
        Debug.Log(gameObject.name + " FindingBest");
        if (previosUnWalkableNode != null)
            previosUnWalkableNode.occupied = false;
        nodeWithValues.Clear();

        //Find all accessable nodes
        yield return StartCoroutine(EvalUnitNodes(transform.position, movement));

        Debug.Log(gameObject.name + "  NodeChecking " + nodeForEval.Count);
        //Foreach node get the node's worth
        foreach (Node n in nodeForEval)
        {
            //Debug.Log("Found Nodes " + gameObject.name);
            yield return StartCoroutine(nodeEval(n));
        }
        Debug.Log(gameObject.name + "  NodeVals " + nodeWithValues.Count);
        //bestChoice = highestValue(nodeVals);
        nodeWithValues.Sort();  //Will this actually sort by values
        //yield return 
       //Debug.Log("Best choice was " + nodeWithValues[0].node.worldPosition + " : " + nodeWithValues[0].value);
       //Debug.DrawRay(nodeWithValues[0].node.worldPosition, Vector3.up * 10, Color.green, 5);
    }

    int distToUnwalkable(Node startPos, Vector3 dir)
    {
        Vector3 strtPos = startPos.worldPosition;
        //search till our movement range is exceded or we find an edge
        for (int i = 1; i < maxThreatReach; i ++){
            Vector3 newSpace = strtPos + (dir * (i));
            Node tmpNode = m_grid.NodeFromWorldPoint(newSpace);
            if (!tmpNode.walkable)
                return i;
        }
        return maxThreatReach;
    }

    void locationEval(Node nPos)
    {
        justREturnedPath = false;
        //int totalValue = 0; //Lower is better
        //totalValue += Mathf.RoundToInt((10 * Vector3.Distance(transform.position, target.position)));  //Replace with Astar?
        //Evaluate our action funtion from this position and subtract this
        nodeWithValues.AddRange(m_Action.EvalFunctForAi(nPos, target.position)); 
    }

    IEnumerator nodeEval(Node n)
    {
        if (n.walkable && !n.occupied) { 
            locationEval(n);
            //Determine Unit action enpoints val  Utility vs Vulnarability
            //Unit action endpoints distance to edge and enemies
            //Sub unit action utility Tar distance + Tar elimination

            yield return null;
            //If no utility is found then get distance to tar & enemies & edge

            updateOccupiedNode();
        }
        else
        {
            nodeForEval.Remove(n);
        }
    }

    IEnumerator EvalUnitNodes(Vector3 startPos, int pathLength)
    {
        nodeForEval.Clear();
        visitedRowCol = new bool[Mathf.RoundToInt(m_grid.gridWorldSize.x), Mathf.RoundToInt(m_grid.gridWorldSize.y)];
        Node startNode = m_grid.NodeFromWorldPoint(startPos);
        yield return  StartCoroutine(crtUGridSub(startPos, 0));
        crtUGridSub(startPos, 0);
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
    IEnumerator crtUGridSub(Vector3 nodePos, int depth)
    {
        Node node = m_grid.NodeFromWorldPoint(nodePos);
        if (visitedRowCol[Mathf.RoundToInt(node.gridX), Mathf.RoundToInt(node.gridY)] == false && node.walkable && depth <= movement)
        {
            visitedRowCol[Mathf.RoundToInt(node.gridX), Mathf.RoundToInt(node.gridY)] = true;
            if (depth != 0 && !node.occupied)
            {
                nodeForEval.Add(node);
                crtUGridSub(new Vector3(node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                crtUGridSub(new Vector3(node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                crtUGridSub(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), depth + 1);
                crtUGridSub(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), depth + 1);
                yield return null;
            }else if (depth == 0)
            {
                crtUGridSub(new Vector3(node.worldPosition.x + nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                crtUGridSub(new Vector3(node.worldPosition.x - nodeSize, node.worldPosition.y, node.worldPosition.z), depth + 1);
                crtUGridSub(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z + nodeSize), depth + 1);
                crtUGridSub(new Vector3(node.worldPosition.x, node.worldPosition.y, node.worldPosition.z - nodeSize), depth + 1);
                yield return null;
            }
        }
        yield return null;
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

public class valuedNode : System.IComparable<valuedNode>
{
    public int value;
    public Node endNode;
    public Node moveNode;
    public valuedNode (int val, Node eNde, Node mNode)  //Constructs a val node
    {
        value = val;
        endNode = eNde;
        moveNode = mNode;
    }
    public int CompareTo(valuedNode other)  //Lets us use .sort 
    {
        return value.CompareTo(other.value);
    }
}
