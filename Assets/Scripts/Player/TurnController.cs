using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurnController : MonoBehaviour {
	public int playerTurnNumber = 1;
	public PlayerInfo[] myPlayers;
    public UnityEvent[] playerTurns;
    [SerializeField] CalculateHeatMap calHeatMap;

    public void passTurn()
    {
        StartCoroutine(turnCoRoutine());
    }

    IEnumerator turnCoRoutine () {
        calHeatMap.recalAddThreatLvl();
        playerTurns[playerTurnNumber - 1].Invoke();
        playerTurnNumber++;
        yield return new WaitForSecondsRealtime(2f);
        if (playerTurnNumber > myPlayers.Length)
            playerTurnNumber = 1;
        foreach (PlayerInfo p in myPlayers)
        {
            p.PassTurn();
        }
        yield return null;
    }

// Use this for initialization
void Start () {
        calHeatMap.recalAddThreatLvl();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
