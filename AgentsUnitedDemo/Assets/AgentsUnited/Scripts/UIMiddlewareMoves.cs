using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Threading;

using System.Linq;

using ASAPToolkit.Unity.Middleware;
using UMA;
using ASAPToolkit.Unity.Characters;

public class UIMiddlewareMoves : MonoBehaviour, IMiddlewareListener {

    private static UIMiddlewareMoves _instance;
    public static UIMiddlewareMoves INSTANCE { get { 
        if (UIMiddlewareMoves._instance == null)
            UIMiddlewareMoves._instance = GameObject.FindObjectOfType<UIMiddlewareMoves>();
        return UIMiddlewareMoves._instance;
    } private set{ UIMiddlewareMoves.INSTANCE = value; }}

    private MiddlewareBase middleware;

    public Transform panelRoot;
    public UIMiddlewareActorPanel actorPanelPrefab;
    public string uiId;

    //stores a mapping for each agentId to the corresponding umadata instance
    private Dictionary<String, UMAData> umaInstances;

    private List<UIMiddlewareActorPanel> actorPanels;
    public UIMiddlewareCustomActionsPanel customActionsPanel;
    
    public UIMiddlewareLogs logPanel;

    void Awake() {
        umaInstances = new Dictionary<String, UMAData>();
        actorPanels = new List<UIMiddlewareActorPanel>();
        middleware = GetComponent<MiddlewareBase>();
        middleware.Register(this);
        
        foreach(UMAAvatarBase uma in FindObjectsOfType<UMAAvatarBase>())
        {
            uma.CharacterCreated.AddListener(OnCreated);
        }
    }

    void OnCreated(UMAData umaData)
    {
        umaInstances.Add(umaData.GetComponentInParent<BasicCharacter>().agentId, umaData);
    }

    void Start() {
        SendInitRequest();
    }

    // Update is called once per frame
    void Update() {
	}

    //on receiving a message, check what topic it came in, and give it to the corresponding handler method

    void ClearUnsetPanels(UIMiddlewareActorPanel[] keep) {
        foreach (UIMiddlewareActorPanel panel in actorPanels) {
            if (!keep.Contains(panel)) Destroy(panel.gameObject);
        }
        actorPanels.Clear();
		foreach (UIMiddlewareActorPanel panel in keep) {
			actorPanels.Add(panel);
		}
    }

    void ClearNoMoveButtons(UIMiddlewareActorPanel[] haveMoves) {
        foreach (UIMiddlewareActorPanel panel in actorPanels) {
            if (!haveMoves.Contains(panel)) panel.SetMoveSet(null);
        }
    }

    void HandleStatus(UIProtocolStatusResponse res) {
        UIMiddlewareActorPanel[] keepState = SetActorsStates(res.actors);
        UIMiddlewareActorPanel[] haveMoves = SetMoveSets(res.moveSets);
        SetCustomActions(res.customActions);
        ClearUnsetPanels(keepState);
        ClearNoMoveButtons(haveMoves);

    }

    void HandleMoveStatus(UIProtocolMoveStatusUpdate res) {
        UIMiddlewareActorPanel ap = actorPanels.FirstOrDefault(p => p.actor.identifier == res.actorIdentifier);
        if (ap != null) ap.SetMoveButtonStatus(res.moveId, res.status);
    }

    void HandleLog(UIProtocolLogMessage res) {
        if (logPanel != null) logPanel.Log(res);
        else Debug.Log("UIEnvironment Log: " + res.logString);
    }

    private void SetCustomActions(UIProtocolCustomAction[] customActions) {
        customActionsPanel.SetCustomActions(customActions, null);
    }

    public void OnMessage(MSG msg) {
        UIProtocolResponse res = JsonUtility.FromJson<UIProtocolResponse>(msg.data);
        //Debug.LogError(msg.data);
        switch (res.cmd) {
            case "status":
                HandleStatus(JsonUtility.FromJson<UIProtocolStatusResponse>(msg.data));
                break;
            case "move_status":
                HandleMoveStatus(JsonUtility.FromJson<UIProtocolMoveStatusUpdate>(msg.data));
                break;
            case "log":
                HandleLog(JsonUtility.FromJson<UIProtocolLogMessage>(msg.data));
                break;
        }
    }

    public void SendInitRequest() {
        UIProtocolInitRequest req = new UIProtocolInitRequest(this.uiId);
        middleware.Send(JsonUtility.ToJson(req));
    }

    public void OnSetControlled(UIProtocolActor actor, bool doControl) {
        List<string> controlledActors = new List<string>();
        foreach (UIMiddlewareActorPanel p in actorPanels) {
            if (p.actor.identifier == actor.identifier) {
                if (doControl) controlledActors.Add(p.actor.identifier);
            } else if (p.actor.controlledBy.Contains(this.uiId)) {
               controlledActors.Add(p.actor.identifier); 
            }
        }
        UIProtocolSetWaitsRequest req = new UIProtocolSetWaitsRequest(this.uiId, controlledActors.ToArray());
        middleware.Send(JsonUtility.ToJson(req));
    }

    public void OnMoveButtonClicked(Move m) {
        UIProtocolMoveSelectedRequest req = new UIProtocolMoveSelectedRequest(this.uiId, m);
        middleware.Send(JsonUtility.ToJson(req));
    }

    public void OnCustomActionClicked(UIProtocolCustomAction action, UIProtocolActor actor) {
        UIProtocolCustomActionRequest req = new UIProtocolCustomActionRequest(this.uiId, action, actor, "");
        middleware.Send(JsonUtility.ToJson(req));
    }

    private UIMiddlewareActorPanel GetActorPanelById(string actorIdentifier) {
        UIMiddlewareActorPanel panel = actorPanels.FirstOrDefault(p => p.actor.identifier == actorIdentifier);
        if (panel == null) {
            panel = GameObject.Instantiate(actorPanelPrefab.gameObject).GetComponent<UIMiddlewareActorPanel>();
            panel.transform.name = actorIdentifier;
			panel.transform.SetParent(panelRoot);
			panel.transform.localPosition = Vector3.zero;
			panel.transform.localRotation = Quaternion.identity;
            panel.transform.localScale = Vector3.one;
            actorPanels.Add(panel);
        }
        return panel;
    }

    private UIMiddlewareActorPanel[] SetActorsStates(UIMiddlewareMoves.UIProtocolActor[] actors) {
        List<UIMiddlewareActorPanel> keep = new List<UIMiddlewareActorPanel>();
        List<String> visibleActors = new List<String>();
        foreach (UIProtocolActor actor in actors) {
            visibleActors.Add(actor.identifier);
            UIMiddlewareActorPanel panel = GetActorPanelById(actor.identifier);
            panel.SetActorState(actor);
            keep.Add(panel);
        }
        SetActorVisibility(visibleActors);
        return keep.ToArray();
    }

    private void SetActorVisibility(List<String> visibleActors)
    {
        foreach(KeyValuePair<String, UMAData> uma in umaInstances)
        {
            if (visibleActors.Contains(uma.Key))
            {
                uma.Value.Show();
            } else
            {
                uma.Value.Hide();
            }
        }
    }

    private UIMiddlewareActorPanel[] SetMoveSets(UIMiddlewareMoves.MoveSet[] moveSets) {
        List<UIMiddlewareActorPanel> keep = new List<UIMiddlewareActorPanel>();
        foreach (UIMiddlewareMoves.MoveSet moveSet in moveSets) {
            UIMiddlewareActorPanel panel = GetActorPanelById(moveSet.actorIdentifier);
            panel.SetMoveSet(moveSet);
            keep.Add(panel);
        }
        return keep.ToArray();
    }

    [Serializable]
    public class UIProtocolResponse {
        public string cmd;
    }


    // PROTOCOL = Data to UI Middleware

    [Serializable]
    public class UIProtocolRequest {
        public string uiId;
        public string cmd;

    }

    [Serializable]
    class UIProtocolInitRequest : UIProtocolRequest {

        public UIProtocolInitRequest(string uiId) {
            this.cmd = "init_request";
            this.uiId = uiId;
        }
    }

    [Serializable]
    class UIProtocolSetWaitsRequest : UIProtocolRequest {
        public string[] actors;
        public UIProtocolSetWaitsRequest(string uiId, string[] actors) {
            this.cmd = "set_waits";
            this.uiId = uiId;
            this.actors = actors;
        }
    }

    [Serializable]
    class UIProtocolMoveSelectedRequest : UIProtocolRequest {
        public string moveID;
        public string actorIdentifier;
        public bool skipPlanner;
        public string userInput;

        public UIProtocolMoveSelectedRequest(string uiId, Move m) {
            this.cmd = "move_selected";
            this.uiId = uiId;
            this.moveID = m.moveID;
            this.actorIdentifier = m.actorIdentifier;
            this.skipPlanner = false; // TODO
            this.userInput = m.userInput;
        }
    }

    [Serializable]
    public class UIProtocolCustomActionRequest : UIProtocolRequest {

        public string command;
        public string actorIdentifier;
        public string parameter;

        public UIProtocolCustomActionRequest(string uiId, UIProtocolCustomAction ca, UIProtocolActor actor, string parameter) {
            this.cmd = "action_selected";
            this.command = ca.command;
            this.parameter = parameter;
            this.uiId = uiId;
            if (actor != null)
                this.actorIdentifier = actor.identifier;
            else
                this.actorIdentifier = null;
        }
    }

    [Serializable]
    class UIProtocolPassRequest : UIProtocolRequest {
        public string reason;
        
        public UIProtocolPassRequest(string uiId, string reason) {
            this.cmd = "pass";
            this.uiId = uiId;
            this.reason = reason; // TODO: enum
        }
    }



    // PROTOCOL = Data from UI Middleware
     
    [Serializable]
    public class UIProtocolStatusResponse : UIProtocolResponse {

        public UIProtocolActor[] actors;
        public MoveSet[] moveSets;
        public UIProtocolCustomAction[] customActions;

        public UIProtocolStatusResponse(UIProtocolActor[] actors, MoveSet[] moveSets, UIProtocolCustomAction[] customActions) {
            this.actors = actors;
            this.moveSets = moveSets;
            this.cmd = "status";
            this.customActions = customActions;
        }

    }

    [Serializable]
    public class UIProtocolCustomAction {
        public string command;
        public string description;
        // TODO: action type / parameters such as trigger, bool, text, number...
    }

    [Serializable]
    public class UIProtocolActor {
        public string identifier;
        public string name;
        public string moveSelector;
        public string movePlanner;
        public string activeMove;
        public string fallbackMove;
        public string[] controlledBy;
        public UIProtocolCustomAction[] customActions;
    }

    [Serializable]
    public class UIProtocolMoveStatusUpdate : UIProtocolResponse {
        public string actorIdentifier;
        public string moveId;
        public string status;
    }

    [Serializable]
    public class MoveSet {
        public int dialogueID;
        public string actorIdentifier;
        public Move[] moves;
	
    } 
    
    [Serializable]
    public class Move {
        public MoveReply reply;
        public string opener;
        public string moveID;

        public bool requestUserInput;
        public string userInput;
        public string storeInSKBVariable;
        
        //public MoveType moveType;
        
        public int dialogueID;
        public string actorIdentifier;
    }

    [Serializable]
    public class MoveReply {
        public int target;
        public string targetIdentifier;
    }

    public class UIProtocolLogMessage : UIProtocolResponse {

        public string logString;

    }
}