using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Threading;

#if !UNITY_EDITOR && UNITY_METRO
#else
using Apache.NMS;
using Apache.NMS.Util;
using Apache.NMS.Stomp;
#endif

public class AMQUIControl : MonoBehaviour {
	#if !UNITY_EDITOR && UNITY_METRO
	// TODO: UWP implementation of STOMP
	#else
	public string topicRead;
	public string topicWrite;

	public string address;
	public int port;
	public string user;
	public string pass;
	protected static ITextMessage message = null;
	protected static AutoResetEvent semaphore = new AutoResetEvent (false);
	GameObject PrefabButton;
	public RectTransform ParentPanel;


	public GameObject web_browser;
	public GameObject forward_button;
	public GameObject back_button;
	public GameObject global_AMQ_settings;
    
    public bool[] active_btn;
    public bool[] interactive_btn;
    
    //couple of public variables for showing buttons and what should go on them
    public bool showingButtons = false;
    public int numberOfMoves = 0; 
    public int curDialogueID;

    public UserMoveMsg userMoveMsg = new UserMoveMsg();    
    //data classes for the received json message that holds the user moves, the message looks like this:
    /*
         {
          "dialogueID":78,
          "moves":{
            "Patient":[
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
        public Moves moves;
    }
    [Serializable]
    public class Moves
    {
        public List<Move> Patient;
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

	bool networkOpen;
	ISession session;
	IConnectionFactory factory;
	IConnection connection;
	IMessageProducer producer;
	IDestination destination;
	Thread apolloWriterThread;
	Thread apolloReaderThreadControl;
    Thread apolloReaderThreadMoves;
    System.TimeSpan receiveTimeout = System.TimeSpan.FromMilliseconds(250);

    //stuff for the background board: a videoplayer and text controller 
    //public CouchVideoPlayerController couchVideoPlayerController;
    //public CouchBackgroundBoardTextController couchBackgroundBoardTextController;

#endif
    public Button[] buttons;
    private Text[] texts;
    // Use this for initialization
    void Start () {
        texts = new Text[buttons.Length];
        active_btn = new bool[buttons.Length]; // new bool[] { false, false, false, false, false, false, false, false };
        interactive_btn = new bool[buttons.Length];// new bool[] { false, false, false, false, false, false, false, false };
        
        for (int i = 0; i < buttons.Length; i++)
        {
            int btnIdx = i;
            buttons[i].onClick.AddListener(delegate { OnClicked(btnIdx); });
            texts[i] = buttons[i].GetComponentInChildren<Text>();
        }

		web_browser = GameObject.FindGameObjectWithTag ("web_browser");
		back_button = GameObject.FindGameObjectWithTag ("back_button");
		forward_button = GameObject.FindGameObjectWithTag ("forward_button"); 
		global_AMQ_settings = GameObject.FindGameObjectWithTag ("global_amq_settings");

		address = global_AMQ_settings.GetComponent<GlobalAMQSettings>().address;
		port = global_AMQ_settings.GetComponent<GlobalAMQSettings>().port;


		try {
			factory = new ConnectionFactory ("tcp://"+ address + ":" + port.ToString());
			connection = factory.CreateConnection ("admin", "admin");
			Debug.Log("Apollo connecting to tcp://"+ address + ":" + port.ToString());
			session = connection.CreateSession();
			networkOpen = true;
			connection.Start();
		} catch (System.Exception e) {
			Debug.Log("Apollo Start Exception " + e);
		}

        apolloReaderThreadControl = new Thread(() => ApolloReader("COUCH/UI/UI_Control"));
        apolloReaderThreadControl.Start ();

        apolloReaderThreadMoves = new Thread(() => ApolloReader("COUCH/UI/moves_to_buttons"));
        apolloReaderThreadMoves.Start();

        IDestination destination = SessionUtil.GetDestination(session, "topic://COUCH/UI/user_selected_move"); 
		Debug.Log("Using destination: " + destination); 

		producer = session.CreateProducer (destination); 
		// Start the connection so that messages will be processed. 
		connection.Start (); 
	}

    // Update is called once per frame
    void Update() {


        if (userMoveMsg != null) { 
            curDialogueID = userMoveMsg.dialogueID;
            numberOfMoves = userMoveMsg.moves.Patient.Count;
            //Debug.Log("User move msg has " + numberOfMoves.ToString() + " moves");

            for (int i = 0; i < buttons.Length; i++) {
                if (i < numberOfMoves)
                {
                    //Debug.Log("Possible user move = \"" + userMoveMsg.moves.Patient[i].opener + "\"");
                    texts[i].text = userMoveMsg.moves.Patient[i].opener;
                    active_btn[i] = true;
                    interactive_btn[i] = true;
                }
                else
                {
                    active_btn[i] = false;
                    interactive_btn[i] = false;
                }
            }

            for (int i = 0; i < buttons.Length; i++) {
                buttons[i].gameObject.SetActive(showingButtons && active_btn[i]);
                buttons[i].interactable = interactive_btn[i];
            }
        }
        if (web_browser != null) web_browser.SetActive (true);
	}

	public void Awake () {
        //Start ();    
    }

	public void OnApplicationQuit() {
#if !UNITY_EDITOR && UNITY_METRO
		throw new System.NotImplementedException();
#else

        Debug.Log("Attempt to close connections...");
        try
        {
            session.Close();
            Debug.Log("Closed session!");
            connection.Close();
            Debug.Log("Closed connection!");
        }
        catch (Exception)
        {// do nothing, they are already closed            
        }

        Debug.Log("Attempt to close writer and reader threads...");
        networkOpen = false;

        if (apolloWriterThread != null && !apolloWriterThread.Join(500)) {
			Debug.LogWarning("Could not close apolloWriterThread");
			apolloWriterThread.Abort();
		}
                
        if (apolloReaderThreadControl != null && !apolloReaderThreadControl.Join(500)) { 
            Debug.LogWarning("Could not close apolloReaderThreads");
            apolloReaderThreadControl.Abort();
		}

        if ((apolloReaderThreadMoves != null && !apolloReaderThreadMoves.Join(500)))
        {
            Debug.LogWarning("Could not close apolloReaderThreadMoves");
            apolloReaderThreadMoves.Abort();
        }

		if (connection != null) connection.Close();
        if (session != null) session.Close();

        Debug.Log("Closed connections and stopped threads! DONE");

        #endif
    }		

	void ApolloReader(String topic) {
		try {
            ITopic destination_Read = SessionUtil.GetTopic(session, topic);
			IMessageConsumer consumer = session.CreateConsumer(destination_Read);
			Debug.Log("Apollo subscribing to " + destination_Read);            
			consumer.Listener += new MessageListener(OnSTOMPMessage);
			while (networkOpen) {
				semaphore.WaitOne((int)receiveTimeout.TotalMilliseconds, true);
			}
            consumer.Close();
            Debug.Log("Closed apolloReaderThread for " + topic);
        } catch (System.Exception e) {
			Debug.Log("ApolloReader Exception " + e);
		}        
    }

	void Send(String message) {
		ITextMessage request = session.CreateTextMessage(message);
        try
        {
            producer.Send(request);
        } catch (Exception e)
        {
            Debug.LogWarning("Failed to send a message:\n " + e.ToString());
        }
		Debug.Log ("Send: " + request.ToString());
	}

    //on receiving a message, check what topic it came in, and give it to the corresponding handler method
	void OnSTOMPMessage(IMessage receivedMsg) {
        

        Debug.Log("Received: " + receivedMsg);
        message = receivedMsg as ITextMessage;        
        semaphore.Set();
        String msg = message.Text;
        Debug.Log("So Text message is: " + msg + ", and it came in on topic: "+ message.NMSDestination.ToString());

        if (message.NMSDestination.ToString().Equals("topic://COUCH/UI/UI_Control"))
        {
            UI_Control(msg);
        }

        if (message.NMSDestination.ToString().Equals("topic://COUCH/UI/moves_to_buttons"))
        {
            UserMoveMsgHandler(msg);
        }
    }

    //when we get a ui control message, we check if we need to do something, if so, call the changebutton method
    void UI_Control(String msg)
    {
        Debug.Log("UI Message: " + msg);
        if (msg.Equals("true")) {
            showingButtons = true;
            //changeButtons(true);
        } else {
            showingButtons = false;
            //changeButtons(true);
            //Debug.Log("Something is rotten in UI_Control...");
        }       
    }   

    //we try to unpack the msg into our move-datastructure and store dialogID etc for later use
    void UserMoveMsgHandler(String msg)
    {
        Debug.Log("User moves message received, trying to make it a json");        
        try
        {
            userMoveMsg = JsonUtility.FromJson<UserMoveMsg>(msg);
            Debug.Log("Made it a json: " + JsonUtility.ToJson(userMoveMsg).ToString());
        }
        catch (Exception e)
        {
            Debug.LogError("Creating a JSON from the received message failed. "+e);
            throw;
        }

    }

    //we try to match the button that was clicked to the moves we have, if we have a match we get the moveID from that move, create the return json msg, and send this
    void OnClicked(int buttonIdx) {
        Debug.Log("User clicked button: " + buttonIdx);
        if (userMoveMsg == null || buttonIdx >= userMoveMsg.moves.Patient.Count)
        {
            Debug.LogError("This button is not in sync with the state of the userMoveMsg object");
        }

        string moveID = userMoveMsg.moves.Patient[buttonIdx].moveID.ToString();
        //Debug.Log("Usermove opener = " + userMoveMsg.moves.Patient[buttonIdx].opener.ToString() + ", and content of clicked button = " + contentClickedBtn + " match! Found the corresponding moveID = " + moveID);
        string returnJsonMsg = buildResponseJson(moveID, curDialogueID);
        Send(returnJsonMsg);
    }


    public String buildResponseJson(string moveID, int dialogueID)
    {
        String json = "{\"cmd\":\"interaction\",\"params\":{\"dialogueID\":" + dialogueID.ToString() + ",\"speaker\":\"Dummy\",\"interactionID\":\""+ moveID +"\",\"reply\":{}}}";
        return json;
    }
}

