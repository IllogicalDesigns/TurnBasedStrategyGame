using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour {
    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] LayerMask lM;
    bool pushing = false;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
    void PushCheck(Vector3 goToPos, Vector3 dir) {
        RaycastHit hit;
        Debug.DrawRay(transform.position, -dir * 0.7f, Color.cyan);
        if (Physics.Raycast(transform.position, -dir, out hit, 0.7f, lM)) {
            Vector3 newSpace = goToPos + (-dir * 1);
            hit.collider.SendMessage("PushedTo", new Vector3(newSpace.x, 0, newSpace.z));
            pushing = true;
        }
    }
    IEnumerator Moving(Vector3 goToPos) {
        Vector3 dir = (transform.position - new Vector3(goToPos.x, 1f, goToPos.z)).normalized;
        while (transform.position != goToPos) {
            transform.position = Vector3.SmoothDamp(transform.position, goToPos, ref velocity, smoothTime);
            if (!pushing)
                PushCheck(goToPos, dir);
            yield return null;
        }
    }
    public void PushedTo(Vector3 goTo) {
        Debug.Log(gameObject.name);
        pushing = true;
        StartCoroutine(Moving(goTo));
    }
    public void MoveTo(Node nodeToGoTo) {
        pushing = false;
        StartCoroutine(Moving(nodeToGoTo.worldPosition));
    }
}
