using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASAPToolkit.Unity.Middleware;

[RequireComponent(typeof(MiddlewareBase))]
public class FlipperDebug : MonoBehaviour, IMiddlewareListener {

    private MiddlewareBase middleware;

    public void OnMessage(MSG msg) {
        Debug.Log("FlipperDebug got message: " + msg.data);
    }

    void Start () {
        middleware = GetComponent<MiddlewareBase>();
        middleware.Register(this);
	}
	
	void Update () {
	}

    private string _moveID = "Introduction";
    private string _charID = "COUCH_M_1";
    private string _addresseeID = "ALL";
    void OnGUI() {

        _moveID = GUI.TextField(new Rect(5, 5, 120, 22), _moveID);
        _charID = GUI.TextField(new Rect(135, 5, 120, 22), _charID);
        _addresseeID = GUI.TextField(new Rect(135, 37, 120, 22), _addresseeID);
        if (GUI.Button(new Rect(265, 5, 120, 22), "Simulate Intent")) {
            SendSimulateMove(_moveID, _charID, _addresseeID);
        }

        if (GUI.Button(new Rect(265, 37, 80, 22), "Reset")) {
            SendReset(_charID);
        }
    }

    void SendReset(string charId) {
        ASAPToolkit.Unity.BML.BMLRequests bmlr = FindObjectOfType<ASAPToolkit.Unity.BML.BMLRequests>();
        if (bmlr == null) return;
        bmlr.SendBML("<bml composition=\"REPLACE\" characterId=\""+charId+"\" id=\"reset0\" xmlns=\"http://www.bml-initiative.org/bml/bml-1.0\" ></bml>");
    }

    void SendSimulateMove(string moveId, string charId, string addressee) {
        IntentRequest intentRequest = new IntentRequest();
        intentRequest.engine = "ASAP";
        intentRequest.bmlTopic = "ASAP";
        intentRequest.charId = charId;
        intentRequest.moveId = moveId;
        intentRequest.intent = new IntentDescription {
            parameters = new IntentParameters {
                addressee = addressee,
                text = "",
                minimalMoveCompletionId = "moveEnd"
            }
        };

        middleware.Send(JsonUtility.ToJson(new SimulateIntentCMD(intentRequest)));
    }

    [System.Serializable]
    public class FlipperDebugMessage : System.Object {
        public string msgType;
        public FlipperDebugMessage(string msgType) {
            this.msgType = msgType;
        }
    }

    [System.Serializable]
    public class ResetBMLCMD : FlipperDebugMessage {
        public string charId;

        public ResetBMLCMD(string charId) : base("resetbml") {
            this.charId = charId;
        }
    }

    [System.Serializable]
    public class SimulateIntentCMD : FlipperDebugMessage {
        public IntentRequest intentRequest;

        public SimulateIntentCMD(IntentRequest intentRequest) : base("simulateintent") {
            this.intentRequest = intentRequest;
        }
    }

    [System.Serializable]
    public class IntentRequest {
        public string engine;
        public string bmlTopic;
        public string charId;
        public string moveId;
        public IntentDescription intent;
    }


    [System.Serializable]
    public class IntentDescription {
        public IntentParameters parameters;
    }

    [System.Serializable]
    public class IntentParameters {
        public string minimalMoveCompletionId;
        public string text;
        public string addressee;
    }
    /*

        {
		    "engine":"ASAP",
		    "bmlTopic":"ASAP",
		    "charId":"COUCH_M_1",
		    "moveId":"move25", 
		    "intent": {             
		        "parameters": {
		            "minimalMoveCompletionId":"move25",
		            "text":"Hello, this is a test",      
		            "addressee":"COUCH_M_2"
		        }
		    }
		}
        */
}
