#region License
//
// Copyright 2009 Nicholas Hadfield
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silverfly
{
    /// <summary>
    /// Information about a handler.
    /// </summary>
    internal class HandlerInfo
    {
        /// <summary>
        /// Creates a <see cref="HandlerInfo"/> for the specified handler.
        /// </summary>
        /// <typeparam name="TNotification">The notification.</typeparam>
        /// <param name="handler">The handler for a specific notification.</param>
        /// <returns>A <see cref="HandlerInfo"/> instance for the specified handler.</returns>
        public static HandlerInfo Create<TNotification>(Action<object, TNotification> handler) where TNotification : class
        {
            return new HandlerInfo(
                (object)handler,
                (h) => (o, n) => ((Action<object, TNotification>)h)(o, (TNotification)n)
            );
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="HandlerInfo"/> class.
        /// </summary>
        /// <param name="handler">The original handler.</param>
        /// <param name="handlerWrapperCreator">
        /// A function that generates a handler of type Action&lt;object, object&gt; that wraps a more specific
        /// handler. This code would be a lot simpler if we just wrote a wrapper handler here directly
        /// but we can't do that because that would result in another reference to the handler (which
        /// defeats the purpose of using a weak reference.</param>
        private HandlerInfo(object handler, Func<object, Action<object, object>> handlerWrapperCreator)
        {
            _handler = new WeakReference(handler);
            _handlerWrapperCreator = handlerWrapperCreator;
        }

        /// <summary>
        /// A weak reference to the original handler.
        /// </summary>
        private WeakReference _handler;

        /// <summary>
        /// Gets a weak reference to the original handler.
        /// </summary>
        public object Handler
        {
            get { return _handler.Target; }
        }

        /// <summary>
        /// A function that returns an action of type Action&lt;object, object&gt; that wraps the original handler.
        /// </summary>
        private Func<object, Action<object, object>> _handlerWrapperCreator;

        /// <summary>
        /// Gets a function that returns an action of type Action&lt;object, object%gt; that wraps the original handler.
        /// </summary>
        public Action<object, object> Wrapper
        {
            get
            {
                object o = _handler.Target;
                if (null == o) return null;
                return _handlerWrapperCreator(o);
            }
        }
    }
}
