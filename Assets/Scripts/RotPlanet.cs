﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotPlanet : MonoBehaviour {
    float speed = 1f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(transform.up * Time.deltaTime * speed, Space.World);
    }
}
