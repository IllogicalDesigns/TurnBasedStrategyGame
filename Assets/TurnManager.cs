using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
	[SerializeField] UnitM[] player1Units;
	[SerializeField] UnitM[] player1NonActiveUnits;
	[SerializeField] UnitM[] player2Units;
	[SerializeField] UnitM[] player2NonActiveUnits;
	public int playerTurnNumber = 1;
	SelectAndMove mySelectAndMove;

	public enum whosTurn
	{
		player1,
		player2
	}

	public whosTurn currTurn;

	void Start()
	{
		mySelectAndMove = gameObject.GetComponent<SelectAndMove> ();
	}

	int[] checkIfWeHaveMoves()
	{
		int p1UMoves = 0;
		int p2UMoves = 0;
		foreach (UnitM u in player1Units) {
			if (u != null && u.movedThisRound == false)
				p1UMoves++;
		}
		foreach (UnitM u in player2Units) {
			if (u != null && u.movedThisRound == false)
				p2UMoves++;
		}
		return  new int[] { p1UMoves, p2UMoves };
	}

	void unlockAllUnits()
	{
		foreach (UnitM u in player1Units) {
			u.movedThisRound = false;
		}
		foreach (UnitM u in player2Units) {
				u.movedThisRound = false;
		}
	}

	public void passTurn()
	{
		int[] movesLeft = checkIfWeHaveMoves ();

		Debug.Log (movesLeft[0].ToString() + " " + movesLeft[1].ToString());
		if (movesLeft [0] <= 0 && movesLeft [1] <= 0) {
			Debug.Log ("NewRound");
			unlockAllUnits ();
		}

		if (currTurn == whosTurn.player2 && movesLeft [0] > 0) {
			//currTurn = whosTurn.player1;
			//mySelectAndMove.deselectOldUnit ();
			//mySelectAndMove.playerTurn = 0;
			Debug.Log("Player2 is out of moves");
		} else if (currTurn == whosTurn.player1 && movesLeft [1] > 0) {
			//currTurn = whosTurn.player2;
			//mySelectAndMove.deselectOldUnit ();
			//mySelectAndMove.playerTurn = 1;
			Debug.Log("Player1 is out of moves");
		}
	}
}
