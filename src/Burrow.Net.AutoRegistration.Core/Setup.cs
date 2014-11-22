using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burrow.Net.AutoRegistration.Core {
    public static class Setup {

        internal static ITunnel tunnel;
        internal static ISerializer serializer;
        internal static string exchangeName;

        public static void Publish<T>(T message) {
            tunnel.Publish(message, typeof(T).FullName.Replace(".", "_"));
        }

        public static void SetExchange(string exchange) {
            exchangeName = exchange ?? "Default";
        }

        public static void SetSerializer(ISerializer serial) {
            serializer = serial;
        }

        public static void Configure() {
            if (tunnel != null) {
                throw new Exception("Tunnel is already initialized");
            }
            tunnel = RabbitTunnel.Factory.Create();
            tunnel.SetRouteFinder(new AutoRouteFinder(exchangeName ));
            if (serializer != null) {
                tunnel.SetSerializer(serializer);
            }
        }

    }
}
