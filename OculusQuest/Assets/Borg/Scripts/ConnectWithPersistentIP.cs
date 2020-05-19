using ASAPToolkit.Unity.Middleware;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectWithPersistentIP : MonoBehaviour {

    public static string ipPropertyName = "asapIP";

    public Button connectButton1;
    public Button connectButton2;
    public string sceneName1;
    public string sceneName2;
    public InputField ipInput;

	void Start() {
        DontDestroyOnLoad(this.gameObject);
        //SceneManager.sceneLoaded += OnSceneLoaded;
        ipInput.onValueChanged.RemoveAllListeners();
        ipInput.onValueChanged.AddListener(OnEdit);
        ipInput.onEndEdit.RemoveAllListeners();
        ipInput.onEndEdit.AddListener(OnEdit);
        ipInput.text = PlayerPrefs.GetString(ipPropertyName, "192.168.1.47");
        connectButton1.onClick.RemoveAllListeners();
        connectButton1.onClick.AddListener(OnConnect1);
        connectButton2.onClick.RemoveAllListeners();
        connectButton2.onClick.AddListener(OnConnect2);
    }

    public void OnEdit(string val) {
        PlayerPrefs.SetString(ipPropertyName, val);
        Debug.Log("Saved: " + val);
    }

    public void OnConnect1()
    {
        ipInput.onValueChanged.RemoveAllListeners();
        ipInput.onEndEdit.RemoveAllListeners();
        connectButton1.onClick.RemoveAllListeners();
        connectButton1 = null;
        ipInput = null;
        //SceneManager.LoadScene(sceneName1);
    }
    public void OnConnect2()
    {
        ipInput.onValueChanged.RemoveAllListeners();
        ipInput.onEndEdit.RemoveAllListeners();
        connectButton2.onClick.RemoveAllListeners();
        connectButton2 = null;
        ipInput = null;
        //SceneManager.LoadScene(sceneName2);
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Debug.Log("In scene: " + scene.name);
        string ip = PlayerPrefs.GetString(ipPropertyName);
        string amqURI = "tcp://"+ip+":61616";

        UIMiddlewareMoves uimm = FindObjectOfType<UIMiddlewareMoves>();
        if (uimm != null) {
            AMQMiddleware amqm = uimm.GetComponent<AMQMiddleware>();
            if (amqm != null) amqm.URI = amqURI;
            else Debug.Log("No AMQMiddleware!");
        } else   Debug.Log("No UIMiddlewareMoves!");

        ASAPToolkit.Unity.ASAPToolkitManager atm = FindObjectOfType<ASAPToolkit.Unity.ASAPToolkitManager>();
        if (atm != null) {
            UDPMultiClientMiddleware udp = atm.GetComponent<UDPMultiClientMiddleware>();
            if (udp != null) udp._remoteIP = ip;
            else Debug.Log("No UDPMiddleware on ASAPToolkitManager");
        } else   Debug.Log("No ASAPToolkitManager!");

        ASAPToolkit.Unity.Environment.AudioStreamingReceiver aar = FindObjectOfType<ASAPToolkit.Unity.Environment.AudioStreamingReceiver>();
        if (atm != null) {
            UDPMultiClientMiddleware udp = aar.GetComponent<UDPMultiClientMiddleware>();
            if (udp != null) udp._remoteIP = ip;
            else Debug.Log("No UDPMiddleware on AudioStreamingReceiver");
        } else Debug.Log("No AudioStreamingReceiver!");
    }
}
