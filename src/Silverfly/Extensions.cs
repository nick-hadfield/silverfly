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
        public static Action<object, TNotification> Marshall<TNotification>(
            this Action<object, TNotification> handler
        ) where TNotification : class
        {
            if (null == handler) return null;
            return handler.Marshall<TNotification>(SynchronizationContext.Current);
        }

        /// <summary>
        /// Marshalls an event handler to the specified synchronization context.
        /// </summary>
        /// <param name="handler">The handler to be marshalled.</param>
        /// <param name="context">The synchronization context to which the handler should be marshalled.</param>
        /// <returns>A new event handler proxy the marshalls the handler to the specified synchronization context.</returns>
        public static Action<object, TNotification> Marshall<TNotification>(
            this Action<object, TNotification> handler,
            SynchronizationContext context
        ) where TNotification : class
        {
            if (null == handler) return null;
            return new Action<object, TNotification>(
                (sender, eventArgs) =>
                {
                    if (null == context)
                    {
                        handler(sender, eventArgs);
                    }
                    else
                    {
                        context.Send(
                            new SendOrPostCallback(
                                (o) =>
                                {
                                    var info = (EventHandlerMarshallingInfo<TNotification>)o;
                                    handler(
                                        info.Sender,
                                        info.EventArgs
                                    );
                                }
                            ),
                            new EventHandlerMarshallingInfo<TNotification>()
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
        /// <typeparam name="TNotification"></typeparam>
        private class EventHandlerMarshallingInfo<TNotification> where TNotification : class
        {
            public object Sender { get; set; }
            public TNotification EventArgs { get; set; }
        }

        #endregion
    }
}
