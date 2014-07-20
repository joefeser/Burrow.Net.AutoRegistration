using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burrow.Net.AutoRegistration.Core {
    
    public interface IHandleMessages<T> {
        /// <summary>
        /// Process a Message with the given signature.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        /// <remarks>
        /// Every message received by the bus with this message type will call this method.
        /// </remarks>
        void Handle(IReceivedMessage<T> message);
    }
}
