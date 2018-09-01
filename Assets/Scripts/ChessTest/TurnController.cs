using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnController : MonoBehaviour {
    public int currTeamTurn = 0;
    [SerializeField] Picker[] teamPickers;
    [SerializeField] ThinkingCoordinator bigThink;
    [SerializeField] GameObject playerGui;
    [SerializeField] GameObject aiGui;
    [SerializeField] GameObject wonGui;
    private void Start() {
        PassTurn("Team1");
        StartCoroutine(AITeam());
    }

    void lost() {
        int loss = bigThink.CheckForLoss();
        if(loss == 0) {
            playerGui.SetActive(false);
            aiGui.SetActive(false);
            wonGui.SetActive(true);
            wonGui.gameObject.SendMessage("WhoWon", "Red");
        }
        if(loss == 1) {
            playerGui.SetActive(false);
            aiGui.SetActive(false);
            wonGui.SetActive(true);
            wonGui.gameObject.SendMessage("WhoWon", "Blue");
        }
    }

    public void PassTurn (string team) {
        playerGui.SetActive(false);
        aiGui.SetActive(false);
        lost();
        if (team == "Team0") {
            currTeamTurn = 1;
            aiGui.SetActive(true);
        }
        else {
            playerGui.SetActive(true);
            currTeamTurn = 0;
        }

        if(currTeamTurn == 1) {
            StartCoroutine(AITeam());
        }
    }

    IEnumerator AITeam() {
        bigThink.gameObject.SendMessage("Check");
        while (bigThink.thinking) {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1f);
        teamPickers[currTeamTurn].MoveAI();
        yield return null;
    }
}
