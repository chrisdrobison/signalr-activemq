using System;
using System.Configuration;
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
        private const string BrokerUrlKey = "brokerUrl";
        private static string _brokerUrl = null;

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (_brokerUrl == null)
                {
                    var key = ConfigPrefix + BrokerUrlKey;
                    AppSettingsReader settingsReader = new AppSettingsReader();
                    _brokerUrl = settingsReader.GetValue(key, typeof(string)) as string;
                    if (_brokerUrl == null)
                    {
                        throw new ConfigurationException(String.Format("Could not load value for appSettingsKey \"{0}\".", key));
                    }
                }
                
                
                return _brokerUrl;
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
