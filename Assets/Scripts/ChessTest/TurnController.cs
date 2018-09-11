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
    [SerializeField] CamSelect cSelect;
    [SerializeField] GameObject[] notfiyTurnStart;
    public bool AI = false;

    private void Start() {
        PassTurn("Team1");
        StartCoroutine(AITeam());
    }

    public void LostWho(int loss) {
        if (loss == 1) {
            playerGui.SetActive(false);
            aiGui.SetActive(false);
            wonGui.SetActive(true);
            wonGui.gameObject.SendMessage("WhoWon", "Red");
        }
        else if (loss == 0) {
            playerGui.SetActive(false);
            aiGui.SetActive(false);
            wonGui.SetActive(true);
            wonGui.gameObject.SendMessage("WhoWon", "Blue");
        }
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
            cSelect.SetTeamSelect("Team1");
        }
        else {
            playerGui.SetActive(true);
            currTeamTurn = 0;
            cSelect.SetTeamSelect("Team0");
        }

        if(currTeamTurn == 1 && AI) {
            StartCoroutine(AITeam());
        }
        StartTurn();
    }

    public void StartTurn() {
        lost(); //TODO fix if broken
        foreach (GameObject g in notfiyTurnStart) {
            g.SendMessage("TurnStart", currTeamTurn);
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
