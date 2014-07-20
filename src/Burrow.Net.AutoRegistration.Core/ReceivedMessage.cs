using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burrow.Net.AutoRegistration.Core {

    class ReceivedMessage<T> : IReceivedMessage<T> {

        ulong messageId; 
        T message;
        IDictionary<string, object> metadata;

        public ReceivedMessage(T message, ulong messageId, IDictionary<string, object> metadata) {
            //Guard.ArgumentNotNull(brokeredMessage, "brokeredMessage");
            //Guard.ArgumentNotNull(message, "message");
            //Guard.ArgumentNotNull(metadata, "metadata");
            //this.brokeredMessage = brokeredMessage;
            this.message = message;
            this.messageId = messageId;
            this.metadata = metadata;
        }

        public ulong MessageId {
            get {
                return this.messageId;
            }
        }

        public T Message {
            get {
                return this.message;
            }
        }

        public IDictionary<string, object> Metadata {
            get {
                return metadata;
            }
        }
    }
}
