using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Burrow.Extras;

namespace Burrow.Net.AutoRegistration.Core {

    public static class RegistrationHelper {

        static object lockObject = new object();

        //TODO log the subscriptions

        public static void RegisterAssembly(Assembly assembly) {
            foreach (var type in assembly.GetTypes()) {
                var interfaces = type.GetInterfaces()
                                .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IHandleMessages<>)))
                                .ToList();
                if (interfaces.Count > 0) {
                    RegisterType(type);
                }
            }
        }

        public static void RegisterType(Type type) {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IHandleMessages<>)))
                .ToList();

            if (interfaces.Count == 0) {
                throw new ApplicationException(string.Format("Type {0} does not implement IHandleMessages", type.FullName));
            }

            //for each interface we find, we need to register it with the bus.
            foreach (var foundInterface in interfaces) {

                var implementedMessageType = foundInterface.GetGenericArguments()[0];
                var routeFinder = new AutoRouteFinder(Setup.exchangeName);
                var methodInfo = typeof(IRouteFinder).GetMethod("FindRoutingKey");
                var genericMethodInfo = methodInfo.MakeGenericMethod(new Type[] { implementedMessageType });
                var fullName = (string)genericMethodInfo.Invoke(routeFinder, null);

                var info = new HandlerEnpointData() {
                    AttributeData = type.GetCustomAttributes(typeof(MessageHandlerConfigurationAttribute), false).FirstOrDefault() as MessageHandlerConfigurationAttribute,
                    DeclaredType = type,
                    MessageType = implementedMessageType,
                    RouteFinder = routeFinder,
                    SubscriptionName = fullName,
                    ServiceType = foundInterface
                };

                CreateSubscription(info);
            }
        }

        /// <summary>
        /// Create a new Subscription.
        /// </summary>
        /// <param name="value">The data used to create the subscription</param>
        private static void CreateSubscription(HandlerEnpointData data) {
            //Guard.ArgumentNotNull(value, "value");

            lock (lockObject) {

                var setup = new RabbitSetup(ConfigurationManager.ConnectionStrings["RabbitMQ"].ConnectionString);

                var routeData = new RouteSetupData() {
                    ExchangeSetupData = new ExchangeSetupData() {
                        AutoDelete = false,
                        Durable = true,
                        ExchangeType = "direct"
                    },
                    QueueSetupData = new QueueSetupData() {
                        AutoDelete = false,
                        AutoExpire = 1000 * 60 * 60 * 96, //96 hours
                        Durable = true
                    },
                    SubscriptionName = data.DeclaredType.FullName,
                    RouteFinder = data.RouteFinder
                };

                var methodInfo = setup.GetType().GetMethod("CreateRoute");
                var genericMethodInfo = methodInfo.MakeGenericMethod(new Type[] { data.MessageType });

                genericMethodInfo.Invoke(setup, new object[] { routeData });

                var gt = typeof(RegistrationRunner<>).MakeGenericType(data.MessageType);
                var rr = Activator.CreateInstance(gt, data);

                var configMethod = rr.GetType().GetMethod("Configure");
                configMethod.Invoke(rr, null);

            } //lock end

        }
    }
}
