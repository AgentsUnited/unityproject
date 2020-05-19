using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UIToggleAll : MonoBehaviour {

    public Canvas canvasToggle;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.G)) {
            if (canvasToggle != null) canvasToggle.enabled = !canvasToggle.enabled;
        }
	}
}
