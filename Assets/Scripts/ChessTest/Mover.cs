using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour {
    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] LayerMask lM;
    bool pushing = false;
    [SerializeField] string team = "Team0";
    [SerializeField] Grid m_grid;

    Vector3 goToPos;
    Vector3 dir;
    bool mov = false;
    bool pushed = false;

    [SerializeField] GameObject turnController;

    // Use this for initialization
    void Start() {

    }
    void PushCheck(Vector3 goToPos, Vector3 dir) {
        RaycastHit hit;
        Vector3 testPos = transform.position + (-dir.normalized * 1f);
        Debug.DrawRay(new Vector3(testPos.x, -0.5f, testPos.z), Vector3.up * 1, Color.cyan);
        if (Physics.Raycast(new Vector3(testPos.x, -0.5f, testPos.z), Vector3.up, out hit, 1f, lM)) {
            if (!hit.collider.CompareTag(team)) {
                Vector3 newSpace = goToPos + (-dir * 1.4f);
                newSpace = m_grid.NodeFromWorldPoint(newSpace).worldPosition;
                hit.collider.SendMessage("PushedTo", new Vector3(newSpace.x, 0, newSpace.z));
                pushing = true;
            }
        }
    }
    IEnumerator Moving(Vector3 _goToPos) {
        if (mov)
            transform.position = goToPos;
        dir = (transform.position - new Vector3(_goToPos.x, 1f, _goToPos.z)).normalized;
        goToPos = _goToPos;
        mov = true;
        while (transform.position != goToPos) {
            transform.position = Vector3.SmoothDamp(transform.position, goToPos, ref velocity, smoothTime);
            if (!pushing)
                PushCheck(goToPos, dir);
            yield return new WaitForSecondsRealtime(0.0016f);
            if (Vector3.Distance(transform.position, goToPos) < 0.1f) {
                transform.position = goToPos;
            }
        }
        if (!pushed)
            turnController.SendMessage("PassTurn", team);
        Node newNode = m_grid.NodeFromWorldPoint(transform.position);
        if (!newNode.walkable)
            gameObject.SetActive(false);
        yield return null;
    }

    private void Update() {

    }

    public void PushedTo(Vector3 goTo) {
        Debug.DrawRay(goTo, Vector3.forward, Color.blue, 5f);
        Debug.DrawRay(goTo, Vector3.right, Color.blue, 5f);
        Debug.Log("Pushing " + gameObject.name + " to " + goTo);
        pushed = true;
        StartCoroutine(Moving(goTo));
    }
    public void MoveTo(Node nodeToGoTo) {
        Debug.DrawRay(nodeToGoTo.worldPosition, Vector3.forward, Color.cyan, 5f);
        Debug.DrawRay(nodeToGoTo.worldPosition, Vector3.right, Color.cyan);
        pushing = false;
        pushed = false;
        StartCoroutine(Moving(nodeToGoTo.worldPosition));
    }
    public void MoveTo(Vector3 posToGoTo) {
        //if(turnController.currTeamTurn )
        Debug.DrawRay(posToGoTo, Vector3.forward, Color.cyan, 5f);
        Debug.DrawRay(posToGoTo, Vector3.right, Color.cyan, 5f);
        pushing = false;
        pushed = false;
        StartCoroutine(Moving(posToGoTo));
    }
}
