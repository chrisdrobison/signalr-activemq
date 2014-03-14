using Apache.NMS;

namespace SignalR.ActiveMQ
{
    internal class ConsumerContext
    {
        private readonly ActiveMqConnectionContext _connectionContext;
        private readonly IMessageConsumer _messageConsumer;
        private readonly int _topicIndex;

        public ConsumerContext(int topicIndex, IMessageConsumer messageConsumer,
            ActiveMqConnectionContext connectionContext)
        {
            _topicIndex = topicIndex;
            _messageConsumer = messageConsumer;
            _connectionContext = connectionContext;
        }

        public int TopicIndex
        {
            get { return _topicIndex; }
        }

        public IMessageConsumer MessageConsumer
        {
            get { return _messageConsumer; }
        }

        public ActiveMqConnectionContext ConnectionContext
        {
            get { return _connectionContext; }
        }

        public void OnMessage(IMessage message)
        {
            _connectionContext.Handler(_topicIndex, message);
        }
    }
}