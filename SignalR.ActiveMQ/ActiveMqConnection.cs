using System;
using System.Collections.Generic;
using System.Diagnostics;
using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace SignalR.ActiveMQ
{
    internal class ActiveMqConnection : IDisposable
    {
        private readonly ActiveMqConnectionConfiguration _configuration;
        private readonly TraceSource _trace;
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private ISession _session;
        private readonly object _connectionLock = new object();

        public ActiveMqConnection(ActiveMqConnectionConfiguration configuration, TraceSource traceSource)
        {
            _configuration = configuration;
            _trace = traceSource;
            _connectionFactory = new ConnectionFactory(configuration.ConnectionString) {AsyncSend = true};
        }

        internal ActiveMqConnectionContext Subscribe(IList<string> topicNames, Action<int, byte[]> handler, Action<int, Exception> errorHandler, Action<int> openStream)
        {
            if (topicNames == null)
            {
                throw new ArgumentNullException("topicNames");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            _trace.TraceInformation("Subscribing to {0} topic(s) in the service bus...", topicNames.Count);
            EnsureConnection();
            var connectionContext = new ActiveMqConnectionContext(_configuration, _session, topicNames, _trace, handler, errorHandler, openStream);

            for (var topicIndex = 0; topicIndex < topicNames.Count; ++topicIndex)
            {
                CreateTopic(connectionContext, topicIndex);
            }

            _trace.TraceInformation("Subscription to {0} topics in the ActiveMQ Topic service completed successfully.", topicNames.Count);

            return connectionContext;
        }

        private void CreateTopic(ActiveMqConnectionContext connectionContext, int topicIndex)
        {
            lock (connectionContext.ProducerLock)
            {
                if (connectionContext.IsDisposed)
                {
                    return;
                }

                var topicName = connectionContext.TopicNames[topicIndex];
                var messageProducer = _session.CreateProducer(_session.GetTopic(topicName));
                connectionContext.SetMessageProducer(messageProducer, topicIndex);

                _trace.TraceInformation("Creation of a topic producer {0} completed successfully.", topicName);
            }
            CreateSubscription(connectionContext, topicIndex);
        }

        private void CreateSubscription(ActiveMqConnectionContext connectionContext, int topicIndex)
        {
            lock (connectionContext.ConsumerLock)
            {
                if (connectionContext.IsDisposed)
                {
                    return;
                }

                var topicName = connectionContext.TopicNames[topicIndex];
                var messageConsumer = _session.CreateConsumer(_session.GetTopic(topicName));
                connectionContext.SetMessageConsumer(messageConsumer, topicIndex);
                _trace.TraceInformation("Creation of a topic consumer {0} completed successfully.", topicName);

                ProcessMessages(new ConsumerContext(topicIndex, messageConsumer, connectionContext));
                
                connectionContext.OpenStream(topicIndex);
            }
        }

        private void ProcessMessages(ConsumerContext consumerContext)
        {
            consumerContext.MessageConsumer.Listener += consumerContext.OnMessage;
        }

        private void EnsureConnection()
        {
            if (_connection != null) 
                return;
            lock (_connectionLock)
            {
                if (_connection == null)
                {
                    _connection = _connectionFactory.CreateConnection();
                    _session = _connection.CreateSession();
                    _connection.Start();
                }
            }
        }

        public void Dispose()
        {
            _session.Dispose();
            _connection.Dispose();
        }
    }
}