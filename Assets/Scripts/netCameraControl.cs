using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class netCameraControl : MonoBehaviour {
    [SerializeField] private Camera cam;

	// Use this for initialization
	void Start () {
        Camera[] m_Cams = GameObject.FindObjectsOfType<Camera>();
        foreach(Camera c in m_Cams)
        {
            c.gameObject.SetActive(false);// = false;
            //c.gameObject.GetComponent<AudioSource>().enabled = false;
        }
        cam.gameObject.SetActive(true);
        gameObject.name = "PlayerPrefab " + Random.Range(1111, 9999).ToString();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
