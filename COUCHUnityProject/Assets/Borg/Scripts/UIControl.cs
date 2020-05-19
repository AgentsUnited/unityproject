using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Threading;

using ASAPToolkit.Unity.Middleware;

public class UIControl : MonoBehaviour, IMiddlewareListener {

    private MiddlewareBase middleware;


    public string curRole = "";
    public int curDialogueID;

    public int maxButtons;
    public Button buttonPrefab;
    private Button[] buttons;

    private Text[] texts;
    private bool[] active_btn;
    private bool[] interactive_btn;
    private bool showingButtons = false;
    private int numberOfMoves = 0;

    public UserMoveMsg userMoveMsg = new UserMoveMsg();
    //data classes for the received json message that holds the user moves, the message looks like this:
    /*
         {
          "dialogueID":78,
          "roleMoves":[
            { "role": "Patient", "moves":[
              {
                "reply":{
                  "p":"$p"
                },
                "opener":"Hey there, Hank!",
                "moveID":"PatientIntro"
              },
              {
                "reply":{
                  "p":"$p"
                },
                "opener":"Ehm, hi...",
                "moveID":"PatientUnsure"
              },
              {
                "reply":{
                  "p":"$p"
                },
                "opener":"",
                "moveID":"PatientSilence"
              }
            ]
          }
        }    
     */
    [Serializable]
    public class UserMoveMsg
    {
        public int dialogueID;
        public RoleMoves[] roleMoves;
    }
    [Serializable]
    public class RoleMoves {
        public string role;
        public Move[] moves;
    }
    [Serializable]
    public class Move
    {
        public Reply reply;
        public string opener;
        public string moveID;
    }
    [Serializable]
    public class Reply
    {
        public String p;
    }

    void Start () {

        texts = new Text[maxButtons];
        active_btn = new bool[maxButtons];
        interactive_btn = new bool[maxButtons];
        buttons = new Button[maxButtons];
        
        for (int i = 0; i < buttons.Length; i++) {
            int btnIdx = i;
            if (i == 0) {
                buttons[i] = buttonPrefab;
            } else {
                buttons[i] = Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            }
            buttons[i].onClick.AddListener(delegate { OnClicked(btnIdx); });
            texts[i] = buttons[i].GetComponentInChildren<Text>();
        }

        middleware = GetComponent<MiddlewareBase>();
        middleware.Register(this);
    }

    // Update is called once per frame
    void Update() {
        if (userMoveMsg == null || userMoveMsg.roleMoves.Length == 0) return; 
        curDialogueID = userMoveMsg.dialogueID;
        curRole = userMoveMsg.roleMoves[0].role;
        numberOfMoves = userMoveMsg.roleMoves[0].moves.Length;
            
        for (int i = 0; i < buttons.Length; i++) {
            if (i < numberOfMoves) {
                texts[i].text = userMoveMsg.roleMoves[0].moves[i].opener;
                active_btn[i] = true;
                interactive_btn[i] = true;
            } else {
                active_btn[i] = false;
                interactive_btn[i] = false;
            }
        }

        for (int i = 0; i < buttons.Length; i++) {
            buttons[i].gameObject.SetActive(showingButtons && active_btn[i]);
            buttons[i].interactable = interactive_btn[i];
        }
	}

    //on receiving a message, check what topic it came in, and give it to the corresponding handler method

    public void OnMessage(MSG msg) {
        if (msg.src.Equals("topic://COUCH/UI/UI_Control")) {
            UI_Control(msg.data);
        }

        if (msg.src.Equals("topic://COUCH/UI/moves_to_buttons")) {
            UserMoveMsgHandler(msg.data);
        }
    }

    //when we get a ui control message, we check if we need to do something, if so, call the changebutton method
    void UI_Control(String msg)
    {
        if (msg.Equals("true")) {
            showingButtons = true;
        } else {
            showingButtons = false;
        }       
    }   

    //we try to unpack the msg into our move-datastructure and store dialogID etc for later use
    void UserMoveMsgHandler(String msg) { 
        try  {
            userMoveMsg = JsonUtility.FromJson<UserMoveMsg>(msg);
        } catch (Exception e) {
            Debug.LogError("Creating a JSON from the received message failed. "+e);
            throw;
        }

    }

    //we try to match the button that was clicked to the moves we have, if we have a match we get the moveID from that move, create the return json msg, and send this
    void OnClicked(int buttonIdx) {
        if (userMoveMsg == null || buttonIdx >= userMoveMsg.roleMoves[0].moves.Length)
        {
            Debug.LogError("This button is not in sync with the state of the userMoveMsg object");
        }

        string moveID = userMoveMsg.roleMoves[0].moves[buttonIdx].moveID.ToString();
        //Debug.Log("Usermove opener = " + userMoveMsg.moves.Patient[buttonIdx].opener.ToString() + ", and content of clicked button = " + contentClickedBtn + " match! Found the corresponding moveID = " + moveID);
        string returnJsonMsg = buildResponseJson(curDialogueID, curRole, moveID);
        middleware.Send(returnJsonMsg);
    }


    public String buildResponseJson(int dialogueID, string speaker, string moveID) {
        String json = "{\"cmd\":\"interaction\",\"params\":{\"dialogueID\":" + dialogueID.ToString() + ",\"speaker\":\""+ speaker + "\",\"interactionID\":\""+ moveID +"\",\"reply\":{}}}";
        return json;
    }
}

