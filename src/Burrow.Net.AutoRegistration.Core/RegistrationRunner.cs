using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Burrow.Net.AutoRegistration.Core {

    /// <summary>
    /// Class used to deal with the messages.
    /// </summary>
    internal class RegistrationRunner<T> {

        public RegistrationRunner(HandlerEnpointData handler) {
            this.EndpointData = handler;
            UnAcked = new List<ulong>();
        }

        public object ackLock = new object();

        /// <summary>
        /// Sometimes messages come before we receive the Subscription Object
        /// </summary>
        public List<ulong> UnAcked { get; set; }

        public HandlerEnpointData EndpointData { get; set; }

        private MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// The Subscription
        /// </summary>
        public Subscription Subscription { get; set; }

        public object SingletonInstance { get; set; }

        public void Configure() {
            var gt = typeof(IReceivedMessage<>).MakeGenericType(EndpointData.MessageType);
            MethodInfo = EndpointData.DeclaredType.GetMethod("Handle", new Type[] { gt });

            if (EndpointData.IsReusable) {
                SingletonInstance = Activator.CreateInstance(EndpointData.DeclaredType);
            }

            gt = typeof(AsyncSubscriptionOption<>).MakeGenericType(EndpointData.MessageType);
            var subscriptionOption = Activator.CreateInstance(gt);

            gt.GetProperty("RouteFinder").SetValue(subscriptionOption, EndpointData.RouteFinder);

            var methodInfoMessageHandler = gt.GetProperty("MessageHandler");
            Action<T, MessageDeliverEventArgs> action = (msg, args) => ProcessMessage(msg, args);
            methodInfoMessageHandler.SetValue(subscriptionOption, action);

            gt.GetProperty("SubscriptionName").SetValue(subscriptionOption, EndpointData.DeclaredType.FullName);

            if (EndpointData.AttributeData != null) {
                if (EndpointData.AttributeData.MaxConcurrentCalls > 0) {
                    gt.GetProperty("BatchSize").SetValue(subscriptionOption, (UInt16)EndpointData.AttributeData.MaxConcurrentCalls);
                }
                if (EndpointData.AttributeData.PrefetchCount > 0) {
                    gt.GetProperty("QueuePrefetchSize").SetValue(subscriptionOption, (UInt32)EndpointData.AttributeData.PrefetchCount);
                }
            }

            var tunnelType = Setup.tunnel.GetType();
            var subscribeMethod = tunnelType.GetMethods().Where(item => item.Name == "SubscribeAsync")
                .FirstOrDefault(item => item.GetParameters().Count() == 1 && item.GetParameters().First().Name == "subscriptionOption");

            var subscribeGenericMethod = subscribeMethod.MakeGenericMethod(EndpointData.MessageType);
            Subscription = subscribeGenericMethod.Invoke(Setup.tunnel, new object[] { subscriptionOption }) as Subscription;
        }

        public void ProcessMessage(T message, MessageDeliverEventArgs args) {
            try {
                var receivedMessage = EndpointData.GetReceivedMessage(new object[] { message, args.DeliveryTag, new Dictionary<string, object>() });

                if (SingletonInstance != null) {
                    MethodInfo.Invoke(SingletonInstance, new object[] { receivedMessage }); //new object[] { message }
                }
                else {
                    var handler = Activator.CreateInstance(EndpointData.DeclaredType);
                    MethodInfo.Invoke(handler, new object[] { receivedMessage });
                }
                if (Subscription != null) {
                    Subscription.Ack(args.DeliveryTag);
                }
                else {
                    UnAcked.Add(args.DeliveryTag);
                    var t = new Task(() => {
                        while (true) {
                            try {
                                lock (ackLock) {
                                    if (UnAcked.Count > 0 && Subscription != null) {
                                        foreach (var item in UnAcked.ToList()) {
                                            Subscription.Ack(item);
                                            UnAcked.Remove(item);
                                        }
                                    }
                                }
                                if (UnAcked.Count == 0) {
                                    return;
                                }
                                Thread.Sleep(1000);
                            }
                            catch (Exception ex) {

                            }
                        }
                    });
                    t.Start();
                }
            }
            catch (Exception ex) {

            }
        }
    }

}
