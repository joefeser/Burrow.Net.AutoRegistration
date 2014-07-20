﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burrow.Net.AutoRegistration.Core {
    public static class Setup {

        internal static ITunnel tunnel;
        internal static IRouteFinder routeFinder;
        internal static ISerializer serializer;

        //public static ITunnel Tunnel {
        //    get {
        //        return tunnel;
        //    }
        //}

        public static void Publish<T>(T message) {
            tunnel.Publish(message, typeof(T).FullName.Replace(".", "_"));
        }

        public static void SetRouteFinder(IRouteFinder finder) {
            routeFinder = finder;
        }

        public static void SetSerializer(ISerializer serial) {
            serializer = serial;
        }

        public static void Configure() {
            if (tunnel != null) {
                throw new Exception("Tunnel is already initialized");
            }
            tunnel = RabbitTunnel.Factory.Create();
            if (routeFinder != null) {
                tunnel.SetRouteFinder(routeFinder);
            }
            if (serializer != null) {
                tunnel.SetSerializer(serializer);
            }
        }

    }
}
