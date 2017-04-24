using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Gossage.System
{

    // sample code from https://gist.github.com/bendangelo
    public class GameEvent
    {
    }
    public class TGameEvent<T>: GameEvent
    {
        public T value;
    }

    public class EventManager : MonoBehaviour
    {
        public bool LimitQueueProcesing = false;
        public float QueueProcessTime = 0.0f;
        private static EventManager s_Instance = null;
        private Queue m_eventQueue = new Queue();

        public delegate void EventDelegate<T>(T e) where T : GameEvent;
        private delegate void EventDelegate(GameEvent e);

        private Dictionary<Type, EventDelegate> delegates = new Dictionary<Type, EventDelegate>();
        private Dictionary<Delegate, EventDelegate> delegateLookup = new Dictionary<Delegate, EventDelegate>();
        private Dictionary<Delegate, bool> onceLookups = new Dictionary<Delegate, bool>();

        #region Public Static Fns, Use these First
        public static void AddListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            EventManager inst = Instance;
            if (inst != null) inst.AddDelegate(del);
        }
        public static void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            EventManager inst = Instance;
            if (inst != null) inst.DoRemoveListener(del);
        }
        public static void TriggerEvent(GameEvent e)
        {
            EventManager inst = Instance;
            if (inst != null) inst.DoTriggerEvent(e);
        }
        #endregion

        // override so we don't have the typecast the object
        public static EventManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = GameObject.FindObjectOfType(typeof(EventManager)) as EventManager;
                }
                return s_Instance;
            }
        }

        private EventDelegate AddDelegate<T>(EventDelegate<T> del) where T : GameEvent
        {
            // Early-out if we've already registered this delegate
            if (delegateLookup.ContainsKey(del))
                return null;

            // Create a new non-generic delegate which calls our generic one.
            // This is the delegate we actually invoke.
            EventDelegate internalDelegate = (e) => del((T)e);
            delegateLookup[del] = internalDelegate;

            EventDelegate tempDel;
            if (delegates.TryGetValue(typeof(T), out tempDel))
            {
                delegates[typeof(T)] = tempDel += internalDelegate;
            }
            else {
                delegates[typeof(T)] = internalDelegate;
            }

            return internalDelegate;
        }

        public void DoAddListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            AddDelegate<T>(del);
        }

        public void AddListenerOnce<T>(EventDelegate<T> del) where T : GameEvent
        {
            EventDelegate result = AddDelegate<T>(del);

            if (result != null)
            {
                // remember this is only called once
                onceLookups[result] = true;
            }
        }

        public void DoRemoveListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            EventDelegate internalDelegate;
            if (delegateLookup.TryGetValue(del, out internalDelegate))
            {
                EventDelegate tempDel;
                if (delegates.TryGetValue(typeof(T), out tempDel))
                {
                    tempDel -= internalDelegate;
                    if (tempDel == null)
                    {
                        delegates.Remove(typeof(T));
                    }
                    else {
                        delegates[typeof(T)] = tempDel;
                    }
                }

                delegateLookup.Remove(del);
            }
        }

        public void RemoveAll()
        {
            delegates.Clear();
            delegateLookup.Clear();
            onceLookups.Clear();
        }

        public bool HasListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            return delegateLookup.ContainsKey(del);
        }

        public void DoTriggerEvent(GameEvent e)
        {
            EventDelegate del;
            if (delegates.TryGetValue(e.GetType(), out del))
            {
                del.Invoke(e);

                // remove listeners which should only be called once
                foreach (EventDelegate k in delegates[e.GetType()].GetInvocationList())
                {
                    if (onceLookups.ContainsKey(k))
                    {
                        onceLookups.Remove(k);
                    }
                }
            }
            else {
                Debug.LogWarning("Event: " + e.GetType() + " has no listeners");
            }
        }

        //Inserts the event into the current queue.
        public bool QueueEvent(GameEvent evt)
        {
            if (!delegates.ContainsKey(evt.GetType()))
            {
                Debug.LogWarning("EventManager: QueueEvent failed due to no listeners for event: " + evt.GetType());
                return false;
            }

            m_eventQueue.Enqueue(evt);
            return true;
        }

        //Every update cycle the queue is processed, if the queue processing is limited,
        //a maximum processing time per update can be set after which the events will have
        //to be processed next update loop.
        void Update()
        {
            float timer = 0.0f;
            while (m_eventQueue.Count > 0)
            {
                if (LimitQueueProcesing)
                {
                    if (timer > QueueProcessTime)
                        return;
                }

                GameEvent evt = m_eventQueue.Dequeue() as GameEvent;
                TriggerEvent(evt);

                if (LimitQueueProcesing)
                    timer += Time.deltaTime;
            }
        }

        public void OnApplicationQuit()
        {
            RemoveAll();
            m_eventQueue.Clear();
            s_Instance = null;
        }
    }

}