using System;
using System.Configuration;
using Microsoft.AspNet.SignalR.Messaging;

namespace SignalR.ActiveMQ
{
    internal class ActiveMqConnectionConfiguration : ScaleoutConfiguration
    {
        private const string ConfigPrefix = "signalr-activemq:";
        private const string BrokerUrlKey = "brokerUrl";

        private static string _brokerUrl;

        /// <summary>
        /// </summary>
        public string ConnectionString
        {
            get
            {
                const string key = ConfigPrefix + BrokerUrlKey;
                if (_brokerUrl == null && (_brokerUrl = ConfigurationManager.AppSettings[key]) == null)
                {
                    throw new ConfigurationErrorsException(String.Format("The {0} app setting is missing", key));
                }
                return _brokerUrl;
            }
        }

        /// <summary>
        /// </summary>
        public TimeSpan TimeToLive
        {
            get
            {
                // Get from config manager
                return TimeSpan.FromSeconds(30);
            }
        }

        public string TopicPrefix
        {
            get { return "SignalR."; }
        }
    }
}