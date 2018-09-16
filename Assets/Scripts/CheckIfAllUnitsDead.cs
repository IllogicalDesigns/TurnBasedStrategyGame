using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckIfAllUnitsDead : MonoBehaviour {
    [SerializeField] GameObject[] red;
    [SerializeField] GameObject[] blue;
    [SerializeField] GameObject tcontroller;
    int re = 0;
    int blu = 1;

    public void TurnStart(int whoTurn) {
        int r = 0;
        int b = 0;
        foreach(GameObject g in red) {
            if (g.activeInHierarchy)
                r++;
        }
        foreach (GameObject g in blue) {
            if (g.activeInHierarchy)
                b++;
        }
        if(r == 0)
            tcontroller.SendMessage("LostWho", re);
        if (b == 0)
            tcontroller.SendMessage("LostWho", blu);
    }
}
