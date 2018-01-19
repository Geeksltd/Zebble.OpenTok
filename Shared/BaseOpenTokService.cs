﻿namespace Zebble.Plugin
{
    using System;
    using Zebble;

    public partial class BaseOpenTokService
    {

        internal static string ApiKey => Config.Get("OpenTok.Api.Key");
        internal static string SessionUrl => Config.Get("OpenTok.Api.Session.url");
        internal static string ToeknUrl => Config.Get("OpenTok.Api.Token.url");

        bool videoPublishingEnabled;
        bool audioPublishingEnabled;
        bool videoSubscriptionEnabled;
        bool audioSubscriptionEnabled;
        bool subscriberVideoEnabled;

        internal readonly AsyncEvent VideoPublishingEnabledChanged = new AsyncEvent();
        internal readonly AsyncEvent AudioPublishingEnabledChanged = new AsyncEvent();
        internal readonly AsyncEvent VideoSubscriptionEnabledChanged = new AsyncEvent();
        internal readonly AsyncEvent AudioSubscriptionEnabledChanged = new AsyncEvent();
        internal readonly AsyncEvent SubscriberVideoEnabledChanged = new AsyncEvent();

        public Action<string, string> SignalReceived;

        public static BaseOpenTokService Current = new BaseOpenTokService();

        public bool VideoPublishingEnabled
        {
            get => videoPublishingEnabled;
            set
            {
                if (videoPublishingEnabled == value) return;
                videoPublishingEnabled = value;
                VideoPublishingEnabledChanged.RaiseOn(Thread.UI);
            }
        }

        public bool AudioPublishingEnabled
        {
            get => audioPublishingEnabled;
            set
            {
                if (audioPublishingEnabled == value) return;
                audioPublishingEnabled = value;
                AudioPublishingEnabledChanged.RaiseOn(Thread.UI);
            }
        }

        public bool VideoSubscriptionEnabled
        {
            get => videoSubscriptionEnabled;
            set
            {
                if (videoSubscriptionEnabled == value) return;
                videoSubscriptionEnabled = value;
                VideoSubscriptionEnabledChanged.RaiseOn(Thread.UI);
            }
        }

        public bool AudioSubscriptionEnabled
        {
            get => audioSubscriptionEnabled;
            set
            {
                if (audioSubscriptionEnabled == value) return;
                audioSubscriptionEnabled = value;
                AudioSubscriptionEnabledChanged.RaiseOn(Thread.UI);
            }
        }

        public bool SubscriberVideoEnabled
        {
            get => subscriberVideoEnabled;
            set
            {
                if (subscriberVideoEnabled == value) return;
                subscriberVideoEnabled = value;
                SubscriberVideoEnabledChanged.RaiseOn(Thread.UI);
            }
        }

        public void InitSession(string sessionId, string userToken)
        {
            if (!ValidateSession(sessionId, userToken)) return;

            VideoPublishingEnabled = true;
            AudioPublishingEnabled = true;
            VideoSubscriptionEnabled = true;
            AudioSubscriptionEnabled = true;
            SubscriberVideoEnabled = true;

            Implementation.DoInitSession(ApiKey, sessionId, userToken);

            VideoPublishingEnabledChanged.Handle(() => OnVideoPublishingEnabledChanged());
            AudioPublishingEnabledChanged.Handle(() => OnAudioPublishingEnabledChanged());
            VideoSubscriptionEnabledChanged.Handle(() => OnVideoSubscriptionEnabledChanged());
            AudioSubscriptionEnabledChanged.Handle(() => OnAudioSubscriptionEnabledChanged());
        }

        public bool ValidateSession(string sessionId, string userToken)
        {
            if (ApiKey.LacksValue())
            {
                Device.Log.Error("The OpenTok api key is not provided. Add an entry to your config.xml for the key 'OpenTok.Api.Key'. You can find the project Api key in you OpenTok project dashboard.");
                return false;
            }

            if (sessionId.LacksValue())
            {
                Device.Log.Error("The session id is not provided.");
                return false;
            }

            if (userToken.LacksValue())
            {
                Device.Log.Error("The user token is not provided.");
                return false;
            }

            return true;
        }

        void OnAudioPublishingEnabledChanged() => Implementation.DoOnAudioPublishingEnabledChanged(AudioPublishingEnabled);

        void OnVideoPublishingEnabledChanged() => Implementation.DoOnVideoPublishingEnabledChanged(VideoPublishingEnabled);

        void OnVideoSubscriptionEnabledChanged() => Implementation.DoOnVideoSubscriptionEnabledChanged(VideoSubscriptionEnabled);

        void OnAudioSubscriptionEnabledChanged() => Implementation.DoOnAudioSubscriptionEnabledChanged(AudioSubscriptionEnabled);

        public void SwapCamera()
        {
            Implementation.DoSwapCamera();

        }

        public void SetPublisherContainer(object container)
        {
            Implementation.DoSetPublisherContainer(container);
        }

        public void SetSubscriberContainer(object container)
        {
            Implementation.DoSetSubscriberContainer(container);
        }

        public void EndSession()
        {
            Implementation.DoEndSession();

            VideoPublishingEnabled = false;
            AudioPublishingEnabled = false;
            VideoSubscriptionEnabled = false;
            AudioSubscriptionEnabled = false;
            SubscriberVideoEnabled = false;
        }

        public void SendSignallToAllSubscribers(string type, string message)
        {
            Implementation.DoSendSignalToAllSubscribers(type, message);
        }
    }
}