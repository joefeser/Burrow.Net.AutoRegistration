using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burrow.Net.AutoRegistration.Core {

    internal class AutoRouteFinder : IRouteFinder {

        public Type Type { get; private set; }

        public string Exchange { get; private set; }

        public AutoRouteFinder(string exchange) {
            this.Exchange = exchange;
        }

        public string FindExchangeName<T>() {
            return Exchange;
        }

        public string FindRoutingKey<T>() {
            return typeof(T).FullName.Replace(".", "_");
        }

        public string FindQueueName<T>(string subscriptionName) {
            return string.IsNullOrEmpty(subscriptionName)
                ? string.Format(Exchange + ".{0}", typeof(T).FullName)
                : string.Format(Exchange + ".{0}.{1}", subscriptionName, typeof(T).Name);
        }
    }
}
