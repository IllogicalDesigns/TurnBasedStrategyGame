using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tactician : MonoBehaviour
{
    public List<AIUnit> allSubordinates = new List<AIUnit>();
    public List<AIUnit> activeSubordinates = new List<AIUnit>();
    public List<valuedMoves> returned = new List<valuedMoves>();
    public int Stupidity = 2; //Must be 1
    public TurnController m_TurnController;
    [SerializeField] bool displayGizmos = true;

    [SerializeField] GameObject debugFlag;
    bool debugFlags = true;

    // Use this for initialization
    void Start()
    {

    }

    void OnDrawGizmos()
    {
        if (returned.Count > 0 && displayGizmos)
        {
            foreach (valuedMoves n in returned)
            {
                if (n.value < 0.25f)
                    Gizmos.color = Color.green;
                else if (n.value >= 0.26f && n.value <= 0.5f)
                    Gizmos.color = Color.yellow;
                else if (n.value >= 0.51f && n.value <= 0.75f)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.black;
                Gizmos.DrawSphere(n.node.endNode.worldPosition, 0.15f);
            }
        }
    }

    //Starts a threaded mess to calculate the best-ish result for all AI Units
    public IEnumerator AskUnitsForBestResult()
    {
        returned.Clear();
        foreach (AIUnit ai in activeSubordinates)
        {
            //Ask for them to eval all actions
            if (ai.gameObject.activeInHierarchy) {
                yield return StartCoroutine(ai.NewBestishPath(Stupidity));
            }

            for (int i = 0; i < ai.nodeWithValues.Count && i < Stupidity; i++)
            {
                returned.Add(new valuedMoves(ai.nodeWithValues[i].value, ai.nodeWithValues[i], ai));
            }
        }
        if (returned.Count > 0)
        {
            returned.Sort();
            returned.Reverse();
            //Debug.Log("Best choice was " + returned[0].node.moveNode.worldPosition + " Value: " + returned[0].value + " Unit: " + returned[0].unit.name);
            //Debug.DrawRay(returned[0].node.moveNode.worldPosition, Vector3.up * 10, Color.green, 5);
            int i = 0;
            foreach (valuedMoves vM in returned)
            {
                i++;
                // Debug.Log("name:" + vM.unit.gameObject.name + " value:" + vM.value + " Node:" + vM.node.moveNode.worldPosition +" :" +
                // vM.node.endNode.worldPosition + " ");
                if (debugFlags && vM == returned[0])
                {
                    GameObject tmpFlag = Instantiate(debugFlag, vM.node.endNode.worldPosition, debugFlag.transform.rotation) as GameObject;
                    tmpFlag.GetComponent<NodeFlagHelpers>().SetWeightText(i.ToString() + " : " + vM.value.ToString(), Color.green);
                }
                else { 
                    GameObject tmpFlag = Instantiate(debugFlag, vM.node.endNode.worldPosition, debugFlag.transform.rotation) as GameObject;
                    tmpFlag.GetComponent<NodeFlagHelpers>().SetWeightText(i.ToString() + " : " + vM.value.ToString(), Color.red);
                }
            }
        }
        else
            Debug.LogError("The returned list of actions is empty");
    }

    public void takeTurn()
    {
        if (activeSubordinates.Count == 0)
        {
            activeSubordinates.AddRange(allSubordinates);
            foreach (AIUnit a in activeSubordinates)
            {
                if (a.gameObject.activeInHierarchy)
                    a.activateUnit();
            }
        }
        StartCoroutine(turnTaker());
    }

    IEnumerator turnTaker()
    {
        yield return StartCoroutine(AskUnitsForBestResult());
        Debug.DrawLine(returned[0].unit.gameObject.transform.position, returned[0].node.moveNode.worldPosition, Color.cyan, 10f);
        Debug.DrawLine(returned[0].node.moveNode.worldPosition, returned[0].node.endNode.worldPosition, Color.cyan, 10f);
        returned[0].unit.MoveToNewPos(returned[0].node.moveNode.worldPosition);
        do
        {
            yield return new WaitForSeconds(0.1f);
        } while (returned[0].unit.moving);//&& !(Vector3.Distance(returned[0].unit.transform.position, returned[0].node.moveNode.worldPosition) < 0.1f));
        yield return new WaitForSeconds(1f);
        //returned[0].unit.transform.position = returned[0].node.moveNode.worldPosition;
        if (returned[0].node.endNode.worldPosition != null)
            returned[0].unit.m_Action.SendMessage("PerformAction", returned[0].node.endNode.worldPosition);
        //returned[0].unit.deactivateUnit();
        //activeSubordinates.Remove(returned[0].unit);
        m_TurnController.passTurn();
    }
}
public class valuedMoves : System.IComparable<valuedMoves>
{
    public float value;
    public valuedNode node;
    public AIUnit unit;
    public valuedMoves(float val, valuedNode nde, AIUnit uAi)  //Constructs a val node
    {
        value = val;
        node = nde;
        unit = uAi;
    }
    public int CompareTo(valuedMoves other)  //Lets us use .sort 
    {
        return value.CompareTo(other.value);
    }
}
