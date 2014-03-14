using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;

namespace SignalR.ActiveMQ
{
    public static class DependencyResolverExtensions
    {
        public static IDependencyResolver UseActiveMQ(this IDependencyResolver resolver)
        {
            var config = new ActiveMqConnectionConfiguration();
            
            var bus = new Lazy<ActiveMqMessageBus>(() => new ActiveMqMessageBus(resolver, config));
            resolver.Register(typeof(IMessageBus), () => bus.Value);
            
            return resolver;
        }
    }
}
