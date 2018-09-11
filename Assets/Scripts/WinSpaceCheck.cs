using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinSpaceCheck : MonoBehaviour {
    public int myTeam = 0;
    public string teamWinIf = "Team0";
    [SerializeField] LayerMask lM;
    [SerializeField] TurnController tcontroller;

    public void TurnStart(int whoTurn) {
        RaycastHit hit;
        if (whoTurn == myTeam) {
            if (Physics.Raycast(transform.position, Vector3.up, out hit, 1f, lM)) {
                if (!hit.collider.CompareTag(teamWinIf))
                    tcontroller.SendMessage("LostWho", myTeam);
            }
        }
    }
}
