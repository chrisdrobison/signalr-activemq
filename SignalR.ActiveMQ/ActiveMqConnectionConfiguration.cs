using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Messaging;

namespace SignalR.ActiveMQ
{
    class ActiveMqConnectionConfiguration : ScaleoutConfiguration
    {
        private const string ConfigPrefix = "signalr-activemq:";

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString
        {
            get
            {
                // Get from config manager
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan TimeToLive
        {
            get
            {
                // Get from config manager
                return TimeSpan.FromSeconds(30);
            }
        }

        public int TopicCount
        {
            get
            {
                return 1;
            }
        }

        public string TopicPrefix
        {
            get
            {
                return "signalr";
            }
        }


    }
}
