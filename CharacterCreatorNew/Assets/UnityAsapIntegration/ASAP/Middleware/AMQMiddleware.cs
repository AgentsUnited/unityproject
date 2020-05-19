using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Apache.NMS;
using Apache.NMS.Util;
using Apache.NMS.Stomp;

public class AMQMiddleware : Middleware {

    public string topicWrite;
    public string topicRead;

    Thread amqWriterThread;
    Thread amqReaderThread;

    bool networkOpen;
    ISession session;
    IConnectionFactory factory;
    IConnection connection;
    IMessageProducer producer;
    IDestination destination;

    System.TimeSpan receiveTimeout = System.TimeSpan.FromMilliseconds(250);
    AutoResetEvent semaphore = new AutoResetEvent(false);

    void Awake() {
        AMQStart();
    }

	void Start () {
		
	}
	
	void Update () {
		
	}

    void AMQStart() {
        GlobalAMQSettings global_AMQ_settings = FindObjectOfType<GlobalAMQSettings>();
        string address = global_AMQ_settings.GetComponent<GlobalAMQSettings>().address;
        int port = global_AMQ_settings.GetComponent<GlobalAMQSettings>().port;
        try {
            factory = new ConnectionFactory("tcp://" + address + ":" + port.ToString());
            connection = factory.CreateConnection("admin", "admin");
            Debug.Log("AMQ connecting to tcp://" + address + ":" + port.ToString());
            session = connection.CreateSession();
            networkOpen = true;
            connection.Start();
        } catch (System.Exception e) {
            Debug.Log("AMQ Start Exception " + e);
        }

        amqWriterThread = new Thread(new ThreadStart(AMQWriter));
        amqWriterThread.Start();

        amqReaderThread = new Thread(new ThreadStart(AMQReader));
        amqReaderThread.Start();
    }


    void AMQWriter() {
        try {
            IDestination destination_Write = SessionUtil.GetDestination(session, topicWrite);
            IMessageProducer producer = session.CreateProducer(destination_Write);
            producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
            producer.RequestTimeout = receiveTimeout;
            while (networkOpen) {
                string msg = "";
                lock (_sendQueueLock) {
                    if (_sendQueue.Count > 0) {
                        msg = _sendQueue.Dequeue();
                        Debug.Log("amq send: " + msg);
                    }
                }

                if (msg != null && msg.Length > 0)
                    producer.Send(session.CreateTextMessage(msg));
            }
        } catch (System.Exception e) {
            Debug.Log("ApolloWriter Exception " + e);
        }
    }

    void AMQReader() {
        try {
            //IDestination destination_Read = SessionUtil.GetDestination(session, topicRead);
            //destination_Read
            ITopic destination_Read = SessionUtil.GetTopic(session, topicRead);
            IMessageConsumer consumer = session.CreateConsumer(destination_Read);
            Debug.Log("Apollo subscribing to " + destination_Read);
            /*
            IMessageConsumer consumer;
            if (durable) {
                consumer = session.CreateConsumer(destination_Read);
            } else {
                consumer = session.CreateDurableConsumer(destination_Read, "test", null, false);
            }*/
            consumer.Listener += new MessageListener(OnAMQMessage);
            while (networkOpen) {
                semaphore.WaitOne((int)receiveTimeout.TotalMilliseconds, true);
            }
        } catch (System.Exception e) {
            Debug.Log("ApolloReader Exception " + e);
        }
    }

    void OnAMQMessage(IMessage receivedMsg) {
        lock (_receiveQueueLock) {
            _receiveQueue.Enqueue((receivedMsg as ITextMessage).Text);
        }
        semaphore.Set();
    }


    public void OnApplicationQuit() {
        networkOpen = false;
        if (amqWriterThread != null && !amqWriterThread.Join(500)) {
            Debug.LogWarning("Could not close apolloWriterThread");
            amqWriterThread.Abort();
        }

        if (amqReaderThread != null && !amqReaderThread.Join(500)) {
            Debug.LogWarning("Could not close apolloReaderThread");
            amqReaderThread.Abort();
        }

        if (connection != null) connection.Close();
    }

}
