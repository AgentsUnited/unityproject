using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UIToggleAll : MonoBehaviour {

    public Button toggleButton;
    public RectTransform canvasToggle;

	// Use this for initialization
	void Start () {
        if (toggleButton != null) toggleButton.onClick.AddListener(Toggle);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.G)) {
            Toggle();
        }
	}

    public void Toggle() {
        if (canvasToggle == null) return;
        bool val = canvasToggle.gameObject.activeSelf;
        canvasToggle.gameObject.SetActive(!val);
    }
}
