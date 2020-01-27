﻿using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PubSubEngine
{
    public class Subscribers
    {
        private static ILogger Logger = LoggerWrapper.Instance;
        private ConcurrentDictionary<ISubscriberCallback, Queue<IPublishableMessage>> subscribers;

        private static Subscribers instance;

        public static Subscribers Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Subscribers();
                }
                return instance;
            }
        }

        private Subscribers()
        {
            subscribers = new ConcurrentDictionary<ISubscriberCallback, Queue<IPublishableMessage>>();
        }

        public bool TryAddSubscriber(ISubscriberCallback subscriber)
        {
            bool success = subscribers.TryAdd(subscriber, new Queue<IPublishableMessage>());
            string subscriberName = "";
            try
            {
                subscriberName = subscriber.GetSubscriberName();
            }
            catch (Exception ex)
            {
                Logger.LogDebug($"Couldn't get subscriber name. Execption message: {ex.Message}");
            }


            if (success)
            {
                Logger.LogDebug($"Subscriber [{subscriberName}] SUCCESSFYLLY added to collection of all subscribers.");
            }
            else if(subscribers.ContainsKey(subscriber))
            {
                Logger.LogWarn($"Subscriber [{subscriberName}, HashCode: {subscriber.GetHashCode()}] already exists in collection of all subscibers.");
            }

            return success;
        }

        public void RemoveSubscriber(ISubscriberCallback subscriber)
        {
            bool success = subscribers.TryRemove(subscriber, out Queue<IPublishableMessage> queue);
            string subscriberName = "";
            try
            {
                subscriberName = subscriber.GetSubscriberName();
            }
            catch (Exception ex)
            {
                Logger.LogDebug($"Couldn't get subscriber name. Execption message: {ex.Message}");
            }

            if (success)
            {
                Logger.LogDebug($"Subscriber [{subscriberName}] SUCCESSFYLLY removed from collection of all subscribers.");
            }
            else if(subscribers.ContainsKey(subscriber))
            {
                Logger.LogWarn($"Try to remove Subscriber [{subscriberName}] FAILED for unknown reason.");
            }
        }

        public void PublishMessage(ISubscriberCallback subscriber, IPublishableMessage message)
        {
            bool success = subscribers.TryGetValue(subscriber, out Queue<IPublishableMessage> queueOfMessages);
            string subscriberName = "";
            try
            {
                subscriberName = subscriber.GetSubscriberName();
            }
            catch (Exception ex)
            {
                Logger.LogDebug($"Couldn't get subscriber name. Execption message: {ex.Message}");
            }

            if (success)
            {
                queueOfMessages.Enqueue(message);
                //TODO: check this log in particular
                Logger.LogDebug($"Published message [{message}] SUCCESSFYLLY enqueued on Subscriber [{subscriberName}]");
            }
            else if(!subscribers.ContainsKey(subscriber))
            {
                Logger.LogWarn($"Subscriber [{subscriberName}, HasCode: {subscriber.GetHashCode()}] does not exist in collection of all subscribers.");
            }
        }

        public IPublishableMessage GetNextMessage(ISubscriberCallback subscriber, bool lastMessageWasNull = false)
        {
            IPublishableMessage message = null;

            bool success = subscribers.TryGetValue(subscriber, out Queue<IPublishableMessage> queueOfMessages) && queueOfMessages.Count > 0;
            string subscriberName = "";

            try
            {
                subscriberName = subscriber.GetSubscriberName();
            }
            catch (Exception ex)
            {
                Logger.LogDebug($"Couldn't get subscriber name. Execption message: {ex.Message}");
            }

            if (success)
            {
                message = queueOfMessages.Dequeue();
                Logger.LogDebug($"Published message [{message}] SUCCESSFYLLY dequeued from Subscriber's queue of messages [Subscriber name: '{subscriberName}'].");
            }

            return message;
        }
    }
}
