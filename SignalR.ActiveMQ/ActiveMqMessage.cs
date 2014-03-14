using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Messaging;

namespace SignalR.ActiveMQ
{
    internal static class ActiveMqMessage
    {
        public static byte[] ToMessage(IList<Message> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException("messages");
            }

            var scaleoutMessage = new ScaleoutMessage(messages);
            return scaleoutMessage.ToBytes();
        }

        public static ScaleoutMessage FromMessage(byte[] message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            return ScaleoutMessage.FromBytes(message);
        }
    }
}