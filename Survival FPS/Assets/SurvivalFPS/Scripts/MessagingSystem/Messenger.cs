#undef DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivalFPS.Messaging
{
    [Serializable]
    public class MessengerEventData
    {
    }

    public static class Messenger
    {
        private static MessengerHelper messengerHelper;

        static Messenger()
        {
            messengerHelper = new GameObject("MessengerHelper").AddComponent<MessengerHelper>();
            messengerHelper.hideFlags = HideFlags.None;

            #if DEBUG
            (new GameObject("TestListener")).AddComponent<MessengerTestListener>();
            (new GameObject("TestEventProducer")).AddComponent<MessengerTestProducer>();
            #endif
        }

        //exceptions
        public class ListenerException : Exception
        {
            public ListenerException(string msg) : base(msg) { }
        }

        public class BroadcastException : Exception
        {
            public BroadcastException(string msg) : base(msg) { }
        }

        //helper
        private sealed class MessengerHelper : MonoBehaviour
        {
            private void Awake()
            {
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneUnloaded += (Scene Scene) => Cleanup();
            }
        }

        private static class MessengerInternal
        {
            private readonly static Dictionary<M_EventType, Delegate> m_EventTable = new Dictionary<M_EventType, Delegate>();
            private readonly static Dictionary<M_EventType, Delegate> m_PersistentEventTable = new Dictionary<M_EventType, Delegate>();

            public static void AddListenerInternal(M_EventType eventType, Delegate function, bool isPersistentListener)
            {
                if (function == null)
                {
                    throw new ListenerException("Messenger: attempting to add a null function delegate for event type " + eventType.ToString());
                }

                var table = isPersistentListener ? m_PersistentEventTable : m_EventTable;
                Delegate d;
                if (table.TryGetValue(eventType, out d))
                {
                    //inconsistent signature
                    if (d.GetType() != function.GetType())
                    {
                        throw new ListenerException(string.Format("Messenger: attempting to add a listener with inconsistent signature for event type {0}. " +
                                                                  "Current listeners have type {1} and the listener being added has type {2}", eventType.ToString(), d.GetType().Name, function.GetType().Name));
                    }

                    //already contains the handler
                    if (d.GetInvocationList().Contains(function))
                    {
                        Debug.LogWarning("Messenger: attempting to add a listener that already exists in the invocation list for event type " + eventType.ToString());
                        return;
                    }

                    table[eventType] = Delegate.Combine(table[eventType], function);
                }
                else
                {
                    table.Add(eventType, function);
                }
            }

            public static void RemoveListenerInternal(M_EventType eventType, Delegate function, bool isPersistentListener)
            {
                if (function == null)
                {
                    throw new ListenerException("Messenger: attempting to remove a null function delegate for event type " + eventType.ToString());
                }

                var table = isPersistentListener ? m_PersistentEventTable : m_EventTable;
                Delegate d;
                if (table.TryGetValue(eventType, out d))
                {
                    //inconsistent signature
                    if (d.GetType() != function.GetType())
                    {
                        throw new ListenerException(string.Format("Messenger: attempting to remove a listener with inconsistent signature for event type {0}. " +
                                                                  "Current listeners have type {1} and the listener being added has type {2}", eventType.ToString(), d.GetType().Name, function.GetType().Name));
                    }

                    //does not contain the handler
                    if (!d.GetInvocationList().Contains(function))
                    {
                        Debug.LogWarning("Messenger: attempting to remove a listener that is not in the invocation list for event type " + eventType.ToString());
                        return;
                    }

                    table[eventType] = Delegate.Remove(table[eventType], function);
                }
                else
                {
                    throw new ListenerException("Messenger: attempting to remove a function from an event type that does not exist");
                }
            }

            public static T[] GetInvocationList<T>(M_EventType eventType)
            {
                IEnumerable<T> first = null, second = null;

                Delegate d;
                Delegate[] temp;
                if (m_EventTable.TryGetValue(eventType, out d))
                {
                    if(d != null)
                    {
                        temp = d.GetInvocationList();

                        if (temp.Length != 0)
                        {
                            try
                            {
                                first = temp.Cast<T>();
                            }
                            catch
                            {
                                throw new BroadcastException("Messenger: attempting to invoke functions with wrong signatures from event type " + eventType.ToString());
                            }
                        }
                    }
                }

                if (m_PersistentEventTable.TryGetValue(eventType, out d))
                {
                    if(d != null)
                    {
                        temp = d.GetInvocationList();

                        if (temp.Length != 0)
                        {
                            try
                            {
                                second = temp.Cast<T>();
                            }
                            catch
                            {
                                throw new BroadcastException("Messenger: attempting to invoke functions with wrong signatures from event type " + eventType.ToString());
                            }
                       }
                    }
                }

                if(first == null && second == null)
                {
                    return null;
                }

                if(first == null)
                {
                    return second.ToArray();
                }

                if(second == null)
                {
                    return first.ToArray();
                }

                return first.Concat(second).ToArray();
            }

            public static void CleanupInternal()
            {
                m_EventTable.Clear();
            }
        }

        #region Messenger interface
        /// <summary>
        /// Adds a listener that takes nothing
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <param name="function">Function to invoke when the event happens.</param>
        public static void AddListener(M_EventType eventType, Action function)
        {
            MessengerInternal.AddListenerInternal(eventType, function, false);
        }

        /// <summary>
        /// Adds a persistent listener that takes nothing; the listener will survive scene reloading.
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <param name="function">Function to invoke when the event happens.</param>
        public static void AddPersistentListener(M_EventType eventType, Action function)
        {
            MessengerInternal.AddListenerInternal(eventType, function, true);
        }

        /// <summary>
        /// Adds a listener that takes an event data payload of type T
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <param name="function">Function to invoke when the event happens.</param>
        /// <typeparam name="T">Event data payload class</typeparam>
        public static void AddListener<T>(M_DataEventType eventType, Action<T> function) where T : MessengerEventData
        {
            MessengerInternal.AddListenerInternal(eventType, function, false);
        }

        /// <summary>
        /// Adds a persistent listener that takes an event data payload of type T; the listener will survive scene reloading.
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <param name="function">Function to invoke when the event happens.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void AddPersistentListener<T>(M_DataEventType eventType, Action<T> function) where T : MessengerEventData
        {
            MessengerInternal.AddListenerInternal(eventType, function, true);
        }

        /// <summary>
        /// Broadcast an event that has no event data
        /// </summary>
        /// <param name="eventType">Event type.</param>
        public static void Broadcast(M_EventType eventType)
        {
            Action[] invocationList = MessengerInternal.GetInvocationList<Action>(eventType);

            if (invocationList == null) return;

            foreach (Action function in invocationList)
            {
                function.Invoke();
            }
        }

        /// <summary>
        /// Broadcast an event that has an event data of type T
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <typeparam name="T">Event data payload class</typeparam>
        public static void Broadcast<T>(M_DataEventType eventType, T data) where T : MessengerEventData
        {
            Action<T>[] invocationList = MessengerInternal.GetInvocationList<Action<T>>(eventType);

            if (invocationList == null) return;

            foreach (Action<T> function in invocationList)
            {
                function.Invoke(data);
            }
        }

        /// <summary>
        /// Removes a listener that takes nothing. To remove a persistent listener, please use the RemovePersistentListener.
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <param name="function">Function to remove.</param>
        public static void RemoveListener(M_EventType eventType, Action function)
        {
            MessengerInternal.RemoveListenerInternal(eventType, function, false);
        }

        /// <summary>
        /// Removes a persistent listener that takes nothing.
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <param name="function">Function to remove.</param>
        public static void RemovePersistentListener(M_EventType eventType, Action function)
        {
            MessengerInternal.RemoveListenerInternal(eventType, function, true);
        }

        /// <summary>
        /// Removes a listener that takes an event data of type T. To remove a persistent listener, please use the RemovePersistentListener.
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <param name="function">Function to remove.</param>
        public static void RemoveListener<T>(M_DataEventType eventType, Action<T> function) where T : MessengerEventData
        {
            MessengerInternal.RemoveListenerInternal(eventType, function, false);
        }

        /// <summary>
        /// Removes a persistent listener that takes nothing.
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <param name="function">Function to remove.</param>
        public static void RemovePersistentListener<T>(M_DataEventType eventType, Action<T> function) where T : MessengerEventData
        {
            MessengerInternal.RemoveListenerInternal(eventType, function, true);
        }

        public static void Cleanup()
        {
            MessengerInternal.CleanupInternal();
        }
        #endregion
    }
}
