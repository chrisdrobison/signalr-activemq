using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apache.NMS;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Tracing;

namespace SignalR.ActiveMQ
{
    internal class ActiveMqMessageBus : ScaleoutMessageBus
    {
        private const string SignalRTopicPrefix = "SIGNALR_TOPIC";

        private readonly ActiveMqConnection _connection;
        private readonly string[] _topics;
        private readonly TraceSource _trace;
        private ActiveMqConnectionContext _connectionContext;

        public ActiveMqMessageBus(IDependencyResolver resolver, ActiveMqConnectionConfiguration configuration)
            : base(resolver, configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentException("configuration");
            }

            var traceManager = resolver.Resolve<ITraceManager>();
            _trace = traceManager["SignalR." + typeof (ActiveMqMessageBus).Name];
            _connection = new ActiveMqConnection(configuration, _trace);
            _topics = Enumerable.Range(0, configuration.TopicCount)
                .Select(topicIndex => SignalRTopicPrefix + "_" + configuration.TopicPrefix + "_" + topicIndex)
                .ToArray();

            ThreadPool.QueueUserWorkItem(Subscribe);
        }

        protected override int StreamCount
        {
            get { return _topics.Length; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_connectionContext != null)
                    _connectionContext.Dispose();
                if (_connection != null)
                    _connection.Dispose();
            }
        }

        protected override Task Send(int streamIndex, IList<Message> messages)
        {
            return Task.Run(() =>
            {
                byte[] message = ActiveMqMessage.ToMessage(messages);
                TraceMessages(messages, "Sending");
                _connectionContext.Publish(streamIndex, message);
            });
        }

        private void Subscribe(object state)
        {
            _connectionContext = _connection.Subscribe(_topics, OnMessage, OnError, Open);
        }

        private void OnMessage(int topicIndex, byte[] message)
        {
            var scaleoutMessage = ActiveMqMessage.FromMessage(message);
            TraceMessages(scaleoutMessage.Messages, "Receiving");
            OnReceived(topicIndex, scaleoutMessage.Messages.First().MappingId, scaleoutMessage);
        }

        private void TraceMessages(IList<Message> messages, string messageType)
        {
            if (!_trace.Switch.ShouldTrace(TraceEventType.Verbose))
            {
                return;
            }

            foreach (Message message in messages)
            {
                _trace.TraceVerbose("{0} {1} bytes over Service Bus: {2}", messageType, message.Value.Array.Length, message.GetString());
            }
        }
    }
}