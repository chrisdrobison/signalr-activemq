using System;
using System.Collections.Generic;
using System.Diagnostics;
using Apache.NMS;

namespace SignalR.ActiveMQ
{
    internal class ActiveMqConnectionContext : IDisposable
    {
        private ActiveMqConnectionConfiguration _configuration;
        private ISession _session;
        private Action<int, Exception> _errorHandler;
        private Action<int, byte[]> _handler;
        private Action<int> _openStream;
        private TraceSource _trace;

        private readonly IMessageProducer[] _topicProducers;
        private readonly IMessageConsumer[] _topicConsumers;

        public object ProducerLock { get; private set; }
        public object ConsumerLock { get; private set; }

        public IList<string> TopicNames { get; private set; }
        public bool IsDisposed { get; private set; }

        public ActiveMqConnectionContext(ActiveMqConnectionConfiguration configuration,
            ISession session, IList<string> topicNames, TraceSource trace,
            Action<int, byte[]> handler, Action<int, Exception> errorHandler, Action<int> openStream)
        {
            _configuration = configuration;
            _session = session;
            _trace = trace;
            _handler = handler;
            _errorHandler = errorHandler;
            _openStream = openStream;
            _topicProducers = new IMessageProducer[topicNames.Count];
            _topicConsumers = new IMessageConsumer[topicNames.Count];

            TopicNames = topicNames;
            ConsumerLock = new object();
            ProducerLock = new object();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            foreach (var topicConsumer in _topicConsumers)
            {
                try
                {
                    topicConsumer.Dispose();
                }
                catch (Exception ex)
                {
                    _trace.TraceError(ex.Message);
                }
            }
        }

        internal void Publish(int topicIndex, byte[] message)
        {
            var producer = _topicProducers[topicIndex];
            var bytesMessage = producer.CreateBytesMessage(message);
            bytesMessage.NMSTimeToLive = _configuration.TimeToLive;
            producer.Send(bytesMessage);
        }

        internal void SetMessageProducer(IMessageProducer producer, int topicIndex)
        {
            lock (ProducerLock)
            {
                if (!IsDisposed)
                {
                    _topicProducers[topicIndex] = producer;
                }
            }
        }

        internal void SetMessageConsumer(IMessageConsumer consumer, int topicIndex)
        {
            lock (ConsumerLock)
            {
                if (!IsDisposed)
                {
                    _topicConsumers[topicIndex] = consumer;
                }
            }
        }

        internal void OpenStream(int topicIndex)
        {
            _openStream(topicIndex);
        }

        internal void Handler(int topicIndex, IMessage message)
        {
            var byteMessage = message as IBytesMessage;
            if (byteMessage == null)
            {
                _trace.TraceWarning("Unsupported message type received on topic index {0} or type {1}", topicIndex, message.GetType().Name);
                return;
            }

            _handler(topicIndex, byteMessage.Content);
        }
    }
}