using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetPlaceGrid : NetworkBehaviour {
    [SerializeField] Grid m_grid;
    [SerializeField] GameObject gridPoint;
    [SerializeField] int chargeDist;
    [SerializeField] List<GameObject> gridUis = new List<GameObject>();

    // Use this for initialization
    void Start() {
        chargeDist = this.gameObject.GetComponent<UnitChess>().chargeSqrs;
    }

    [Command]
    void CmdPlaceGridHelper(Vector3 startXy, Vector3 dir, int dist) {
        bool Place = true;
        Vector3 newSpace = startXy + (dir * (dist));  //create a pos at the end spot
        Node endNode = m_grid.NodeFromWorldPoint(newSpace);  // Get node from new pos
        for (int i = 1; i <= dist; i++) {
            Vector3 testPos = startXy + (dir * (i));
            Node testN = m_grid.NodeFromWorldPoint(testPos);
            if (!testN.walkable) {
                Place = false;
                break;
            }
        }
        if (Place) {
            gridUis.Add(NetworkIdentity.Instantiate(gridPoint, endNode.worldPosition, gridPoint.transform.rotation) as GameObject);
        }
    }
    public void DisplayOptions() {
        for (int i = 1; i <= chargeDist; i++) {
            CmdPlaceGridHelper(this.transform.position, Vector3.forward, i);
            CmdPlaceGridHelper(this.transform.position, Vector3.right, i);
            CmdPlaceGridHelper(this.transform.position, -Vector3.forward, i);
            CmdPlaceGridHelper(this.transform.position, -Vector3.right, i);
        }
    }

    [Command]
    public void CmdClearDisplayOptions() {
        foreach (GameObject g in gridUis) {
            Object.Destroy(g);
        }
    }
}
