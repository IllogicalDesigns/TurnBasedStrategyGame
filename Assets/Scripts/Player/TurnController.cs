using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnController : MonoBehaviour {
	public int playerTurnNumber = 1;
	public PlayerInfo[] myPlayers;

	public void passTurn () {
		playerTurnNumber++;
		if (playerTurnNumber > 2)
			playerTurnNumber = 1;
		
		foreach (PlayerInfo p in myPlayers) {
			p.PassTurn ();
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
