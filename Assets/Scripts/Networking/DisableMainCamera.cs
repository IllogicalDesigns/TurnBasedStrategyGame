using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableMainCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject.Find("StartCamera").SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
