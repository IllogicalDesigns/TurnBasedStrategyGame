using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeAction : UnitAction
{
    public GameObject arrowBody;
    public GameObject arrowHead;
    public Vector3 tarPos;
    float yOffset = 0.4f;
    int targetIndex;
    public float chargeDistange = 5;
    List<GameObject> currCharArrows = new List<GameObject>();
    int chrgDistance = 5;
    int enemyInt = 0;
    public int ownerInt = 0;
    AIUnit uAi;
    List<Node> nodeForEval = new List<Node>();

    [SerializeField] float heatWeight = 1f;
    [SerializeField] float knockOffWeight = 2f;


    void OnDrawGizmos()
    {
        foreach (Node n in nodeForEval)
            Gizmos.DrawSphere(n.worldPosition, 0.05f);
    }

    void start()
    {
        arrowBody = m_Player.arrowBody;
        arrowHead = m_Player.arrowHead;
        if (unitAttached != null)
            ownerInt = unitAttached.whoOwnsMe();
        else
            ownerInt = gameObject.GetComponent<AIUnit>().whoOwnsMe();
    }

    public override void DisplayHelperAction()
    {
        destroyOldChargeArrows();
        displayChargeOptions();
    }

    public override void CleanHelperAction()
    {
        destroyOldChargeArrows();
    }
    public override void influenceHeatMap(Grid m_Grid, Node n)
    {
        Vector3 startPos = n.worldPosition;
        Vector3 dir = Vector3.forward;
        Vector3 newSpace = startPos;
        Node tmpNode = null;
        for (int i = 1; i < chargeDistange; i++)
        {
            newSpace = startPos + (dir * (i));
            tmpNode = m_grid.NodeFromWorldPoint(newSpace);
            if (tmpNode.walkable && !tmpNode.occupied)
                tmpNode.addAddThreatLvl(1);
        }
        dir = Vector3.back;
        for (int i = 1; i < chargeDistange; i++)
        {
            newSpace = startPos + (dir * (i));
            tmpNode = m_grid.NodeFromWorldPoint(newSpace);
            if (tmpNode.walkable && !tmpNode.occupied)
                tmpNode.addAddThreatLvl(1);
        }
        dir = Vector3.left;
        for (int i = 1; i < chargeDistange; i++)
        {
            newSpace = startPos + (dir * (i));
            tmpNode = m_grid.NodeFromWorldPoint(newSpace);
            if (tmpNode.walkable && !tmpNode.occupied)
                tmpNode.addAddThreatLvl(1);
        }
        dir = Vector3.right;
        for (int i = 1; i < chargeDistange; i++)
        {
            newSpace = startPos + (dir * (i));
            tmpNode = m_grid.NodeFromWorldPoint(newSpace);
            if (tmpNode.walkable && !tmpNode.occupied)
                tmpNode.addAddThreatLvl(1);
        }
    }

    public void Charge(Vector3 newPos)
    {
        moveToChargeLocation(newPos);
    }

    public void PerformAction(Vector3 newPos)
    {
        moveToChargeLocation(newPos);
    }


    void displayChargeOptions()
    {
        m_Player.setTarCross(Vector3.one, false);
        destroyOldChargeArrows();
        if (!unitAttached.moving && !unitAttached.chargedDuringAction)
        {
            chargeDisplayDirection(Vector3.forward, transform.position, 0);   //TODO convert to new vec3 instead of vec.for, optimization
            chargeDisplayDirection(Vector3.left, transform.position, -90);
            chargeDisplayDirection(Vector3.right, transform.position, 90);
            chargeDisplayDirection(Vector3.back, transform.position, 180);
        }
    }

    void chargeDisplayDirection(Vector3 direction, Vector3 startPos, float Y)
    {
        for (int i = 1; i <= unitAttached.chargeDistance; i++)
        {
            Quaternion Rot = Quaternion.Euler(new Vector3(arrowBody.transform.rotation.eulerAngles.x, Y, arrowBody.transform.rotation.eulerAngles.z));  //Alters just the Y rot so the sprites face poperly
            Vector3 newSpace = startPos + (direction * (i + 1));  //Checks spaces + 1 to see if the current space needs an arrow/End

            Node tmpNode = m_grid.NodeFromWorldPoint(newSpace); //Get the node we are checking to see if its walkable
            bool nodeWalkable = tmpNode.walkable;

            bool somethingUnderNode = Physics.Raycast(new Vector3(newSpace.x, newSpace.y + 0.5f, newSpace.z), Vector3.down, 2f);  //Is there something under this node? #TODO needs walkable mask
            if (!somethingUnderNode)
                nodeWalkable = false;

            if (i != unitAttached.chargeDistance && nodeWalkable)
            {
                //currCharArrows.Add (Network.Instantiate (arrowBody, startPos + (direction * i), Rot, 0) as GameObject);
                currCharArrows.Add(Instantiate(arrowBody, startPos + (direction * i), Rot) as GameObject);
            }
            else
            {
                //if(i != 1 && !somethingUnderNode)  //If this is the first go around we are likely already off so don't build an arrow
                //currCharArrows.Add (Network.Instantiate (arrowHead, startPos + (direction * i), Rot, 0) as GameObject);
                currCharArrows.Add(Instantiate(arrowHead, startPos + (direction * i), Rot) as GameObject);
                break;
            }
        }
    }

    void moveToChargeLocation(Vector3 newPos)
    {
        newPos = m_grid.NodeFromWorldPoint(newPos).worldPosition;
        destroyOldChargeArrows();
        if (unitAttached != null)
            unitAttached.chargedDuringAction = true;
        m_Player.activatedUnit = unitAttached;
        m_Player.setChargeButton(unitAttached, false);
        m_Player.setTarCircle(Vector3.one, false);
        Vector3[] tmpArray = new Vector3[] { newPos };
        StartCoroutine(moveThroughPath(tmpArray));
        StartCoroutine(chargePusher(newPos));
    }

    public override vMove[] EvalFunctForAi(Node nPos, Vector3 tar)
    {
        //nodeForEval.Clear();
        //Debug.DrawRay(nPos.worldPosition, Vector3.forward * chrgDistance, Color.green, 10f);
        //Debug.DrawRay(nPos.worldPosition, Vector3.back * chrgDistance, Color.green, 10f);
        //Debug.DrawRay(nPos.worldPosition, Vector3.right * chrgDistance, Color.green, 10f);
        //Debug.DrawRay(nPos.worldPosition, Vector3.left * chrgDistance, Color.green, 10f);
        //Judge Each direction
        vMove[] finalNodes = new vMove[4];
        if (uAi == null)
            uAi = GetComponent<AIUnit>();
        finalNodes[0] = chargeDirEval(nPos, Vector3.forward);
            //finalNodes[0].value += Mathf.RoundToInt(Vector3.Distance(finalNodes[0].endNode.worldPosition, tar));
        finalNodes[1] = chargeDirEval(nPos, Vector3.back);
            //finalNodes[1].value += Mathf.RoundToInt(Vector3.Distance(finalNodes[1].endNode.worldPosition, tar));
        finalNodes[2] = chargeDirEval(nPos, Vector3.left);
            //finalNodes[2].value += Mathf.RoundToInt(Vector3.Distance(finalNodes[2].endNode.worldPosition, tar));
        finalNodes[3] = chargeDirEval(nPos, Vector3.right);
            //finalNodes[3].value += Mathf.RoundToInt(Vector3.Distance(finalNodes[3].endNode.worldPosition, tar));
        //Sort by values
        nodeForEval.Add(finalNodes[0].endNode);
        nodeForEval.Add(finalNodes[1].endNode);
        nodeForEval.Add(finalNodes[2].endNode);
        nodeForEval.Add(finalNodes[3].endNode);
        float temp = 0;
        for (int write = 0; write < finalNodes.Length; write++)
        {
            for (int sort = 0; sort < finalNodes.Length - 1; sort++)
            {
                if (finalNodes[sort].value > finalNodes[sort + 1].value)
                {
                    temp = finalNodes[sort + 1].value;
                    finalNodes[sort + 1].value = finalNodes[sort].value;
                    finalNodes[sort].value = temp;
                }
            }
        }
        return finalNodes;
    }

    public static float Sigmoid(float value)
    {
        return 1.0f / (1.0f + Mathf.Exp((-value)));
    }

    vMove chargeDirEval(Node startPos, Vector3 dir)  //TODO convert to also check for enemies and push potential
    {
        Debug.DrawRay(new Vector3(startPos.worldPosition.x, startPos.worldPosition.y + 0.5f, startPos.worldPosition.z), dir * chargeDistange, Color.black, 5);
        Vector3 newSpace = new Vector3(0f, 0f, 0f);
        Node tmpNode = m_grid.NodeFromWorldPoint(newSpace);
        Vector3 strtPos = startPos.worldPosition;
        bool ReachesAnEdge = false;
        bool ReachesOccupied = false;
        bool KnockEnemyOff = false;
        RaycastHit hit;
        //search till our movement range is exceded or we find an edge
        for (int i = 1; i < chrgDistance; i++)
        {
            newSpace = strtPos + (dir * (i));
            tmpNode = m_grid.NodeFromWorldPoint(newSpace);
            if (tmpNode.occupied)
                ReachesOccupied = true;
            if (!tmpNode.walkable)
            {
                newSpace = strtPos + (dir * (i - 1));
                tmpNode = m_grid.NodeFromWorldPoint(newSpace);
                ReachesAnEdge = true;
                break;
            }
        }
        //If we reached an occupied see who it is
        //Debug.DrawRay(new Vector3(startPos.worldPosition.x, startPos.worldPosition.y + 0.45f, startPos.worldPosition.z), dir * chargeDistange, Color.yellow, 5);
        if (ReachesOccupied && Physics.Raycast(new Vector3(startPos.worldPosition.x, startPos.worldPosition.y + 0.5f, startPos.worldPosition.z), dir, out hit))
        {
            if (hit.collider.tag == "Player")  //TODO remove these hard coded values
            {
                if (hit.collider.GetComponent<TBSUnit>().whoOwnsMe() != ownerInt && ReachesAnEdge)
                {
                    Debug.DrawRay(new Vector3(startPos.worldPosition.x, startPos.worldPosition.y + 0.75f, startPos.worldPosition.z), dir * chargeDistange, Color.blue, 5);
                    //Debug.Log("Reached occupied and hit : " + hit.collider.name);
                    //return new valuedNode(tmpNode.threatLvl - 30, tmpNode, startPos);
                    KnockEnemyOff = true;
                }
            }
            else if (hit.collider.tag == "aiPlayer")
            {
                if (hit.collider.GetComponent<AIUnit>().whoOwnsMe() != ownerInt && ReachesAnEdge)
                {
                    Debug.DrawRay(new Vector3(startPos.worldPosition.x, startPos.worldPosition.y + 0.5f, startPos.worldPosition.z), dir * chargeDistange, Color.red, 5);
                    //return new valuedNode(tmpNode.threatLvl + 9999, tmpNode, startPos);
                }
            }
        }
        if (tmpNode.walkable)  //removed unocuppied flag
        {
            //float chargeVal = Sigmoid(tmpNode.threatLvl + heatWeight);
            float chargeVal = tmpNode.threatLvl * heatWeight;
            if (KnockEnemyOff)
            {
                Debug.Log("ThreatWeight: " + chargeVal + " AttackWeight: -" + (1f * knockOffWeight) + " Name:"+ gameObject.name + " KNOCKED OFF " + tmpNode.worldPosition);
                //chargeVal = Sigmoid(0.5f + knockOffWeight + chargeVal);
                chargeVal = chargeVal - (1 * knockOffWeight);
            }
            else
                Debug.Log("ThreatWeight: " + chargeVal + " AttackWeight: 0");
            return new vMove(chargeVal, tmpNode, startPos);
        }
        return new vMove(999999, tmpNode, startPos);
    }

    IEnumerator moveThroughPath(Vector3[] _path)
    {
        //previosUnWalkableNode.walkable = true;
        //previosUnWalkableNode = m_grid.NodeFromWorldPoint (_path [_path.Length-1]);
        //previosUnWalkableNode.walkable = false;
        if (unitAttached != null)
            unitAttached.moving = true;
        Vector3 currentWaypoint = _path[0];
        while (true)
        {
            if (transform.position == new Vector3(currentWaypoint.x, currentWaypoint.y + yOffset, currentWaypoint.z))
            {
                targetIndex++;
                if (targetIndex >= _path.Length)
                {
                    if (unitAttached != null)
                        unitAttached.moving = false;
                    m_Player.setTarCross(Vector3.one, false);
                    if (unitAttached != null)
                        unitAttached.CheckIfWeFellOff();
                    SendMessage("updateOccupiedNode");
                    //updateOccupiedNode
                    yield break;
                }
                currentWaypoint = _path[targetIndex];
            }
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(currentWaypoint.x, currentWaypoint.y + yOffset, currentWaypoint.z), chargeDistange * Time.deltaTime);
            yield return null;
        }
    }

    public void destroyOldChargeArrows()
    {
        foreach (GameObject arrSprites in currCharArrows)
        {
            //Network.Destroy (arrSprites);
            Destroy(arrSprites);
        }
    }

    IEnumerator chargePusher(Vector3 newPos)
    {
        RaycastHit hit;
        Vector3 chargDir = transform.position - newPos;
        if (unitAttached != null)
        {
            unitAttached.moving = true;
            while (unitAttached.moving)
            {
                Debug.DrawRay(transform.position, -chargDir.normalized * 10f, Color.blue);
                if (Physics.Raycast(transform.position, -chargDir.normalized, out hit, 1f))
                {
                    if (hit.collider.tag == "Player")
                    {
                        TBSUnit tmpUnit = hit.collider.GetComponent<TBSUnit>();
                        if (tmpUnit.whoOwnsMe() != ownerInt)
                        {
                            tmpUnit.Pushed(newPos, chargDir);
                        }
                    }
                    else if (hit.collider.tag == "aiPlayer")
                    {
                        AIUnit tmpAIUnit = hit.collider.GetComponent<AIUnit>();
                        if (tmpAIUnit.whoOwnsMe() != ownerInt)
                        {
                            tmpAIUnit.Pushed(newPos, chargDir);
                        }
                    }
                    else if (hit.collider.tag == "Pushable")
                    {
                        hit.collider.GetComponent<Pushable>().Pushed(newPos, chargDir);
                    }
                }
                yield return null;  //Check next frame but wait until then
            }
            yield return null;
        }
        else
        {
            AIUnit unitTmp = gameObject.GetComponent<AIUnit>();
            unitTmp.moving = true;
            while (unitTmp.moving)
            {
                Debug.DrawRay(transform.position, -chargDir.normalized * 1f, Color.black);
                if (Physics.Raycast(transform.position, -chargDir.normalized, out hit, 1f))
                {
                    //Debug.Log(hit.collider.name);
                    if (hit.collider.tag == "Player")
                    {
                        TBSUnit tmpUnit = hit.collider.GetComponent<TBSUnit>();
                        if (tmpUnit.whoOwnsMe() != ownerInt)
                        {
                            hit.collider.GetComponent<TBSUnit>().Pushed(newPos, chargDir);
                        }
                    }
                    else if (hit.collider.tag == "Pushable")
                    {
                        hit.collider.GetComponent<Pushable>().Pushed(newPos, chargDir);
                    }
                }
                yield return null; //Check next frame but wait until then
            }
            yield return null;
        }
    }
}

