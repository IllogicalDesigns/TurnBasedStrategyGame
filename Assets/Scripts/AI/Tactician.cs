using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tactician : MonoBehaviour {
    public List<AIUnit> allSubordinates = new List<AIUnit>();
    public List<AIUnit> activeSubordinates = new List<AIUnit>();
    public List<valuedMoves> returned = new List<valuedMoves>();
    public int Stupidity = 2; //Must be 1
    public TurnController m_TurnController;
    [SerializeField] bool displayGizmos = true;

    // Use this for initialization
    void Start () {
		
	}

    void OnDrawGizmos()
    {
        if (returned.Count > 0 && displayGizmos)
        {
            foreach (valuedMoves n in returned)
            {
                if(n.value < 1)
                    Gizmos.color = Color.green;
                else if (n.value >= 1 && n.value <= 2)
                    Gizmos.color = Color.yellow;
                else if  (n.value >= 2 && n.value <= 3)
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
            yield return StartCoroutine(ai.FindBestIshPath(Stupidity));
            for(int i = 0; i < ai.nodeWithValues.Count && i < Stupidity; i++)
            {
                returned.Add(new valuedMoves(ai.nodeWithValues[i].value, ai.nodeWithValues[i], ai));
            }
        }
        if (returned.Count > 0)
        {
            returned.Sort();
            Debug.Log("Best choice was " + returned[0].node.moveNode.worldPosition + " Value: " + returned[0].value + " Unit: " + returned[0].unit.name);
            Debug.DrawRay(returned[0].node.moveNode.worldPosition, Vector3.up * 10, Color.green, 5);
        }
        else
            Debug.LogError("The returned list of actions is empty");
    }

    public void takeTurn()
    {
        if (activeSubordinates.Count == 0)
        {
            activeSubordinates.AddRange(allSubordinates);
            foreach(AIUnit a in activeSubordinates)
            {
                a.activateUnit();
            }
        }
        StartCoroutine(turnTaker());
    }

    IEnumerator turnTaker ()
    {
        yield return StartCoroutine(AskUnitsForBestResult());
        Debug.DrawLine(returned[0].unit.gameObject.transform.position, returned[0].node.moveNode.worldPosition, Color.cyan, 10f);
        Debug.DrawLine(returned[0].node.moveNode.worldPosition, returned[0].node.endNode.worldPosition, Color.cyan, 10f);
        returned[0].unit.MoveToNewPos(returned[0].node.moveNode.worldPosition);
        do{
            yield return new WaitForSeconds(0.1f);
        } while (returned[0].unit.moving);
        yield return new WaitForSeconds(1f);
        if (returned[0].node.endNode.worldPosition != null)
            returned[0].unit.m_Action.SendMessage("PerformAction", returned[0].node.endNode.worldPosition);
        returned[0].unit.deactivateUnit();
        activeSubordinates.Remove(returned[0].unit);
        m_TurnController.passTurn();
    }
}
public class valuedMoves : System.IComparable<valuedMoves>
{
    public int value;
    public valuedNode node;
    public AIUnit unit;
    public valuedMoves(int val, valuedNode nde, AIUnit uAi)  //Constructs a val node
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
