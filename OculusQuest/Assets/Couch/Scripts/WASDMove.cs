using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASDMove : MonoBehaviour {
    float speed = 5f;
    Camera c;
	// Use this for initialization
	void Start () {
        c = GetComponent<Camera>();

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            c.transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            c.transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey("w"))
        {
            c.transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
        }
        if (Input.GetKey("s"))
        {
            c.transform.Translate(new Vector3(0,0,-speed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            c.transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            c.transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
        }
    }
}
