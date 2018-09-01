using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlUI : MonoBehaviour {
    [SerializeField] Grid m_grid;
    [SerializeField] GameObject gridPoint;
    [SerializeField] int chargeDist;
    [SerializeField] List<GameObject> gridUis = new List<GameObject>();

    // Use this for initialization
    void Start() {
        chargeDist = gameObject.GetComponent<UnitChess>().chargeSqrs;
    }
    void PlaceGridHelper(Vector3 startXy, Vector3 dir, int dist) {
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
        if(Place)
            gridUis.Add(Instantiate(gridPoint, endNode.worldPosition, gridPoint.transform.rotation) as GameObject);
    }
    public void DisplayOptions() {
        for (int i = 1; i <= chargeDist; i++) {
            PlaceGridHelper(transform.position, Vector3.forward, i);
            PlaceGridHelper(transform.position, Vector3.right, i);
            PlaceGridHelper(transform.position, -Vector3.forward, i);
            PlaceGridHelper(transform.position, -Vector3.right, i);
        }
    }
    public void ClearDisplayOptions() {
        foreach (GameObject g in gridUis) {
            Destroy(g);
        }
    }
    // Update is called once per frame
    void Update() {

    }
    //Vector3 newSpace = goToPos + (-dir * 1);
}
