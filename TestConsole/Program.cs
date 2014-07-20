using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Burrow;
using Burrow.Net.AutoRegistration.Core;

namespace TestConsole {
    class Program {
        static void Main(string[] args) {

            Setup.SetRouteFinder(new TestRouteFinder());
            Setup.SetSerializer(new Burrow.Extras.JsonSerializer());
            Setup.Configure();
            RegistrationHelper.RegisterType(typeof(TestReceiver));

            //Setup.Publish(new Test() {
            //    TestProperty = 42
            //});

            for (int i = 0; i < 1000; i++) {
                Setup.Publish(new Test() {
                    TestProperty = i
                });
            }
        }
    }

    [MessageHandlerConfiguration(
        DefaultMessageTimeToLive = 240,
        //LockDuration = 120,
        MaxConcurrentCalls = 4,
        //MaxRetries = 2,
        PrefetchCount = 10,
        PauseTimeIfErrorWasThrown = 20000,
        //ReceiveMode = ReceiveMode.PeekLock,
        Singleton = true)]
    public class TestReceiver : IHandleMessages<Test> {

        public void Handle(IReceivedMessage<Test> message) {
            Console.WriteLine("Received:" + message.Message.TestProperty);
        }
    }
    public class Test {

        public Test() {

        }

        public int TestProperty { get; set; }

    }

    public class TestRouteFinder : IRouteFinder {
        public string FindExchangeName<T>() {
            return "zzz.DropShipper";
        }

        public string FindRoutingKey<T>() {
            return typeof(T).FullName.Replace(".", "_");
        }

        public string FindQueueName<T>(string subscriptionName) {
            return string.IsNullOrEmpty(subscriptionName)
                ? string.Format("zzz.DropShipper.{0}", typeof(T).FullName)
                : string.Format("zzz.DropShipper.{0}.{1}", subscriptionName, typeof(T).Name);
        }
    }
}
