using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Silverfly
{
    /// <summary>
    /// Extension methods for silverfly.
    /// </summary>
    public static class Extensions
    {
        #region Threading

        /// <summary>
        /// Marshalls an event handler to the current synchronization context.
        /// </summary>
        /// <param name="handler">The handler to be marshalled.</param>
        /// <returns>A new event handler proxy the marshalls the handler to the current synchronization context.</returns>
        public static EventHandler<TEventArgs> Marshall<TEventArgs>(
            this EventHandler<TEventArgs> handler
        ) where TEventArgs : EventArgs
        {
            if (null == handler) return null;
            return handler.Marshall<TEventArgs>(SynchronizationContext.Current);
        }

        /// <summary>
        /// Marshalls an event handler to the specified synchronization context.
        /// </summary>
        /// <param name="handler">The handler to be marshalled.</param>
        /// <param name="context">The synchronization context to which the handler should be marshalled.</param>
        /// <returns>A new event handler proxy the marshalls the handler to the specified synchronization context.</returns>
        public static EventHandler<TEventArgs> Marshall<TEventArgs>(
            this EventHandler<TEventArgs> handler,
            SynchronizationContext context
        ) where TEventArgs : EventArgs
        {
            if (null == handler) return null;
            return new EventHandler<TEventArgs>(
                (sender, eventArgs) =>
                {
                    if (null == context)
                    {
                        handler(sender, eventArgs);
                    }
                    else
                    {
                        context.Post(
                            new SendOrPostCallback(
                                (o) =>
                                {
                                    var info = (EventHandlerMarshallingInfo<TEventArgs>)o;
                                    handler(
                                        info.Sender,
                                        info.EventArgs
                                    );
                                }
                            ),
                            new EventHandlerMarshallingInfo<TEventArgs>()
                            {
                                Sender = sender,
                                EventArgs = eventArgs
                            }
                        );
                    }
                }
            );
        }

        /// <summary>
        /// Event handler marshalling info used by the Marshall extension method.
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        private class EventHandlerMarshallingInfo<TEventArgs> where TEventArgs : EventArgs
        {
            public object Sender { get; set; }
            public TEventArgs EventArgs { get; set; }
        }

        #endregion
    }
}
