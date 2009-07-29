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
using System.Threading;
using Silverfly.Scopes;

namespace Silverfly
{
    /// <summary>
    /// A publish/subscribe event bus (threadsafe).
    /// </summary>
    /// <remarks>
    /// Weak references (on subscriptions) that have been garbage collected or finalised will be marked 
    /// for cleanup (on a separate thread) when a notification is next published to that subscription.
    /// </remarks>
    public class Bus : IBus
    {
        /// <summary>
        /// A dictionary of notification handlers.
        /// </summary>
        private Dictionary<Type, IList<HandlerInfo>> _handlersIndex = new Dictionary<Type, IList<HandlerInfo>>();

        /// <summary>
        /// Provides locking for the bus.
        /// </summary>
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Enumerate through handlers supporting the <see cref="System.Type"/> as specified by <typeparamref name="TNotification"/>.
        /// </summary>
        /// <typeparam name="TNotification">Specifies the <see cref="System.Type"/> supported by handlers to be retrieved.</typeparam>
        /// <returns>The  handlers supporting the <see cref="System.Type"/> as specified by <typeparamref name="TNotification"/>.</returns>
        private IEnumerable<Action<object, object>> Handlers<TNotification>() where TNotification : class
        {
            foreach (var key in _handlersIndex.Keys)
            {
                if (key.IsAssignableFrom(typeof(TNotification)))
                {
                    foreach (HandlerInfo handlerInfo in _handlersIndex[key])
                    {
                        if (null == handlerInfo.Handler)
                        {
                            ThreadPool.QueueUserWorkItem(
                                new WaitCallback((hi) => Unsubscribe<TNotification>((HandlerInfo)hi)),
                                handlerInfo
                            );
                        }
                        else
                        {
                            yield return handlerInfo.Wrapper;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Publish a notification onto the event bus.
        /// </summary>
        /// <typeparam name="TNotification">Specifies the <see cref="System.Type"/> of the notification to be published.</typeparam>
        /// <param name="sender">Identifies the sender of the notification.</param>
        /// <param name="notification">The notification to be published.</param>
        public void Publish<TNotification>(object sender, TNotification notification) where TNotification : class
        {
            IEnumerable<Action<object, object>> handlers = null;
            using (
                new Disposable(
                    () => _lock.EnterReadLock(),
                    () => _lock.ExitReadLock()
                )
            )
            {
                handlers = Handlers<TNotification>().ToList();
            }

            using (
                new Disposable(
                    () => new ScopeStack<BusScope>("").Push(new BusScope()),
                    () => new ScopeStack<BusScope>("").Pop()
                )
            )
            {
                foreach (var handler in handlers)
                {
                    if (null != handler)
                    {
                        handler(sender, notification);
                    }
                }
            }
        }

        /// <summary>
        /// Subscribe to notifications of type <typeparamref name="TNotification"/>.
        /// </summary>
        /// <typeparam name="TNotification">Specifies the <see cref="System.Type"/> of the notifications to be handled.</typeparam>
        /// <param name="handler">The handler for the notifications.</param>
        public void Subscribe<TNotification>(Action<object, TNotification> handler) where TNotification : class
        {
            // Cannot subscribe from within a publish //
            if (null != new ScopeStack<BusScope>("").Peek())
            {
                throw new SilverflyException("Attempting to Subscribe from within a Publish will result in a deadlock");
            }

            using (
                new Disposable(
                    () => _lock.EnterWriteLock(),
                    () => _lock.ExitWriteLock()
                )
            )
            {
                // Add type to dictionary if not previously handled //
                if (!_handlersIndex.ContainsKey(typeof(TNotification)))
                {
                    _handlersIndex.Add(typeof(TNotification), new List<HandlerInfo>());
                }

                // Add new handler info for specified type/handler //
                _handlersIndex[typeof(TNotification)].Add(
                    HandlerInfo.Create<TNotification>(handler)
                );
            }
        }

        /// <summary>
        /// Unsubscribe handler from notifications of type <typeparamref name="TNotification"/>.
        /// </summary>
        /// <typeparam name="TNotification">Specifies the <see cref="System.Type"/> of the notifications from which to remove the handler.</typeparam>
        /// <param name="handler">The handler to be removed.</param>
        public void Unsubscribe<TNotification>(Action<object, TNotification> handler) where TNotification : class
        {
            // Cannot unsubscribe from within a publish //
            if (null != new ScopeStack<BusScope>("").Peek())
            {
                throw new SilverflyException("Attempting to Unsubscribe from within a Publish will result in a deadlock");
            }

            using (
                new Disposable(
                    () => _lock.EnterWriteLock(),
                    () => _lock.ExitWriteLock()
                )
            )
            {
                if (_handlersIndex.ContainsKey(typeof(TNotification)))
                {
                    // Find handler infos for specified handler //
                    var handlerInfos = (
                        from h in _handlersIndex[typeof(TNotification)]
                        where h.Handler.Equals(handler)
                        select h
                    ).ToList();

                    // Remove any handler infos found //
                    foreach (var handlerInfo in handlerInfos)
                    {
                        _handlersIndex[typeof(TNotification)].Remove(handlerInfo);
                    }

                    // If type no longer has any handlers, remove from dictionary //
                    if (0 == _handlersIndex[typeof(TNotification)].Count)
                    {
                        _handlersIndex.Remove(typeof(TNotification));
                    }
                }
            }
        }

        /// <summary>
        /// Unsubscribe handle info from notifications of type <typeparamref name="TNotification"/>.
        /// </summary>
        /// <typeparam name="TNotification">Specifies the <see cref="System.Type"/> of the notifications from which to remove the handle info.</typeparam>
        /// <param name="handlerInfo">The handler info to be removed.</param>
        private void Unsubscribe<TNotification>(HandlerInfo handlerInfo) where TNotification : class
        {
            // Cannot unsubscribe from within a publish //
            if (null != new ScopeStack<BusScope>("").Peek())
            {
                throw new SilverflyException("Attempting to Unsubscribe from within a Publish will result in a deadlock");
            }

            using (
                new Disposable(
                    () => _lock.EnterWriteLock(),
                    () => _lock.ExitWriteLock()
                )
            )
            {
                if (_handlersIndex.ContainsKey(typeof(TNotification)))
                {
                    // Find handler infos for specified handler //
                    var handlerInfos = (
                        from h in _handlersIndex[typeof(TNotification)]
                        where h == handlerInfo
                        select h
                    ).ToList();

                    // Remove any handler infos found //
                    foreach (var hi in handlerInfos)
                    {
                        _handlersIndex[typeof(TNotification)].Remove(hi);
                        Console.WriteLine("Handler removed");
                    }

                    // If type no longer has any handlers, remove from dictionary //
                    if (0 == _handlersIndex[typeof(TNotification)].Count)
                    {
                        _handlersIndex.Remove(typeof(TNotification));
                    }
                }
            }
        }
    }
}
