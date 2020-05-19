using System;
using System.Threading;
using UnityEngine;
using thrift.gen_csharp;
using thrift.services;

namespace thriftImpl
{
    public class CommandSender : Sender
    {
        private volatile Message message;
        int cpt;

        public CommandSender() : this(DEFAULT_THRIFT_HOST, DEFAULT_THRIFT_PORT)
        {

        }

        public CommandSender(String host, int port) : base(host, port)
        {
            cpt = 0;
        }

        public void playAnimation(String animationID, InterpersonalAttitude attitude)
        {
            if (this.isConnected())
            {
                message = new Message();
                message.Type = "animID";
                message.Time = 0;
                message.Id = cpt.ToString();
                message.String_content = animationID;

                // Add property about social attitude model
                if (attitude != null)
                {
                    message.Properties = new System.Collections.Generic.Dictionary<string, string>();
                    message.Properties.Add("Dominance", attitude.Dominance.ToString());
                    message.Properties.Add("Liking", attitude.Liking.ToString());
                }

                cpt++;
                Thread workThread = new Thread(this.DoSend);
                workThread.Start();
                //send (message);
            }
            else {
                Debug.Log("animationReceiver on host: " + this.getHost() + " and port: " + this.getPort() + " not connected");
            }
        }

        public void playAnimation(String animationID)
        {
            this.playAnimation(animationID, null);
        }

        public void DoSend()
        {
            send(message);
        }
    }
}
