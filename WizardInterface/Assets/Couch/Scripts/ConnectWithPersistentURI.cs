using ASAPToolkit.Unity.Middleware;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectWithPersistentURI : MonoBehaviour {

    public static string amqPropertyName = "amqURI";

    public Button connectButton;
    public string sceneName;
    public InputField uriInput;

	void Start() {
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        uriInput.onValueChanged.RemoveAllListeners();
        uriInput.onValueChanged.AddListener(OnEdit);
        uriInput.onEndEdit.RemoveAllListeners();
        uriInput.onEndEdit.AddListener(OnEdit);
        uriInput.text = PlayerPrefs.GetString(amqPropertyName, "tcp://192.168.1.47:61616");
        connectButton.onClick.RemoveAllListeners();
        connectButton.onClick.AddListener(OnConnect);
    }

    public void OnEdit(string val) {
        PlayerPrefs.SetString(amqPropertyName, val);
        Debug.Log("Saved: " + val);
    }

    public void OnConnect() {
        uriInput.onValueChanged.RemoveAllListeners();
        uriInput.onEndEdit.RemoveAllListeners();
        connectButton.onClick.RemoveAllListeners();
        connectButton = null;
        uriInput = null;
        SceneManager.LoadScene(sceneName);
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Debug.Log("In scene: " + scene.name);
        UIMiddlewareMoves uimm = FindObjectOfType<UIMiddlewareMoves>();
        if (uimm != null) {
            AMQMiddleware amqm = uimm.GetComponent<AMQMiddleware>();
            if (amqm != null) amqm.URI = PlayerPrefs.GetString(amqPropertyName);
            else Debug.Log("No AMQMiddleware!");
        } else   Debug.Log("No UIMiddlewareMoves!");
    }
}
