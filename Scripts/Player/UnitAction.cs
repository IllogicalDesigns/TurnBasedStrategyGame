using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitAction : MonoBehaviour {
	public TBSUnit unitAttached;
	public PlayerInfo m_Player;
	public Grid m_grid;

	void Start () {
		m_Player = gameObject.GetComponentInParent<PlayerInfo> ();
		m_grid = GameObject.FindObjectOfType<Grid> ();  //This is slow and should be stored in the playerInfo for reference
		unitAttached = gameObject.GetComponent<TBSUnit>();
	}

	//Displays the UI used to inform the player of what an action does, not a full help
	public virtual void DisplayHelperAction () {
	}
	//Cleans the UI after something occurs
	public virtual void CleanHelperAction () {
	}
	//Performs the action
	public virtual void PerformAction () {
	}
    public virtual void influenceHeatMap(Grid m_Grid, Node n)
    {
    }
    public virtual vMove[] EvalFunctForAi(Node nPos, Vector3 tar)
    {
        return null;
    }
}
