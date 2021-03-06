﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burrow.Net.AutoRegistration.Core {

    /// <summary>
    /// Main interface that is passed to the Handlers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReceivedMessage<T> {

        /// <summary>
        /// The message that was sent.
        /// </summary>
        T Message {
            get;
        }

        //The message Id
        ulong MessageId {
            get;
        }

        /// <summary>
        /// The metadata passed with the message from the user.
        /// </summary>
        IDictionary<string, object> Metadata {
            get;
        }
    }
}
