namespace Zebble.Plugin.Renderer
{
    using System;
    using Android.Widget;
    using Com.Opentok.Android;

    public class OpenTokService : BaseOpenTokService.INativeImplementation
    {
        object syncLock = new object();
        string role;

        Session Session;
        Publisher Publisher;
        Subscriber Subscriber;
        SignalListener MessageListener = new SignalListener();

        Android.Content.Context Context => UIRuntime.CurrentActivity;

        Android.Views.ViewGroup PublisherContianer;
        Android.Views.ViewGroup SubscriberContainer;

        public void DoInitSession(
            string apiKey,
            string sessionId,
            string userToken,
            string role = OpenTokRole.PUBLISHER)
        {
            this.role = role;

            if (role != OpenTokRole.SUBSCRIBER)
            {
                Device.Permission.Camera.Request().RunInParallel();
                Device.Permission.Microphone.Request().RunInParallel();
            }

            if (Session != null) BaseOpenTokService.Current.EndSession();

            Session = new Session(Context, apiKey, sessionId);
            HandleSessionEvents();
            Session.Connect(userToken);
        }

        public void DoSwapCamera()
        {
            if (Publisher == null) return;

            Publisher.CycleCamera();
        }

        public void DoSetPublisherContainer(object container)
        {
            var streamContainer = ((Android.Views.ViewGroup)container);
            Android.Views.View streamView = null;

            PublisherContianer = streamContainer;
            if (Publisher != null) streamView = Publisher.View;

            ActivateStreamContainer(streamContainer, streamView);
        }

        public void DoSetSubscriberContainer(object container)
        {
            var streamContainer = ((Android.Views.ViewGroup)container);
            Android.Views.View streamView = null;

            SubscriberContainer = streamContainer;
            if (Subscriber != null) streamView = Subscriber.View;

            ActivateStreamContainer(streamContainer, streamView);
        }

        void HandleSessionEvents()
        {
            Session.ConnectionDestroyed += OnConnectionDestroyed;
            Session.Connected += OnDidConnect;
            Session.StreamReceived += OnStreamCreated;
            Session.StreamDropped += OnStreamDestroyed;
            Session.SetSignalListener(MessageListener);
        }

        void ActivateStreamContainer(Android.Views.ViewGroup container, Android.Views.View view)
        {
            DeactivateStreamContainer(container);
            if (container == null || view == null) return;

            if (view.Parent != null) (view.Parent as Android.Views.ViewGroup).RemoveView(view);

            var layoutParams = new FrameLayout.LayoutParams(Android.Views.ViewGroup.LayoutParams.MatchParent, Android.Views.ViewGroup.LayoutParams.MatchParent);
            container.AddView(view, layoutParams);
        }

        void DeactivateStreamContainer(Android.Views.ViewGroup container)
        {
            if (container == null) return;

            container.RemoveAllViews();
            container.SetBackgroundColor(Android.Graphics.Color.Transparent);
        }

        public void DoOnAudioPublishingEnabledChanged(bool value)
        {
            if (Publisher == null) return;
            if (Publisher.PublishAudio != value) Publisher.PublishAudio = value;
        }

        public void DoOnVideoPublishingEnabledChanged(bool value)
        {
            if (Publisher == null) return;
            if (Publisher.PublishVideo != value) Publisher.PublishVideo = value;
        }

        public void DoOnVideoSubscriptionEnabledChanged(bool value)
        {
            if (Subscriber == null) return;
            if (Subscriber.SubscribeToVideo != value) Subscriber.SubscribeToVideo = value;
        }

        public void DoOnAudioSubscriptionEnabledChanged(bool value)
        {
            if (Subscriber == null) return;
            if (Subscriber.SubscribeToAudio != value) Subscriber.SubscribeToAudio = value;
        }

        void OnDidConnect(object sender, EventArgs e)
        {
            lock (syncLock)
            {
                if (role == OpenTokRole.PUBLISHER) SetPublisher();
            }
        }

        void SetPublisher()
        {
            if (Publisher != null || Session == null)
                return;

            Publisher = new Publisher(Context, Environment.TickCount.ToString());
            Publisher.SetStyle(BaseVideoRenderer.StyleVideoScale, BaseVideoRenderer.StyleVideoFill);

            Session.Publish(Publisher);
            ActivateStreamContainer(PublisherContianer, Publisher.View);
        }

        void OnStreamCreated(object sender, Session.StreamReceivedEventArgs e)
        {
            var stream = e.P1;
            lock (syncLock)
            {
                if (Subscriber != null || Session == null)
                    return;

                Subscriber = new Subscriber(Context, stream);
                HandleSubscriptionEvents();

                Subscriber.SetStyle(BaseVideoRenderer.StyleVideoScale, BaseVideoRenderer.StyleVideoFill);

                Session.Subscribe(Subscriber);
            }
        }

        void HandleSubscriptionEvents()
        {
            Subscriber.Connected += OnSubscriberDidConnectToStream;
            Subscriber.VideoEnabled += OnSubscriberVideoEnabled;
            Subscriber.VideoDisabled += OnSubscriberVideoDisabled;
        }

        void OnSubscriberVideoEnabled(object sender, Subscriber.VideoEnabledEventArgs e)
        {
            lock (syncLock)
            {
                if (Subscriber != null && Subscriber.Stream != null && Subscriber.Stream.HasVideo)
                    DoOnVideoSubscriptionEnabledChanged(true);
            }
        }

        void OnSubscriberVideoDisabled(object sender, Subscriber.VideoDisabledEventArgs e)
        {
            DoOnVideoSubscriptionEnabledChanged(false);
        }

        void OnSubscriberDidConnectToStream(object sender, EventArgs e)
        {
            lock (syncLock)
            {
                if (Subscriber == null) return;

                ActivateStreamContainer(SubscriberContainer, Subscriber.View);
                if (Subscriber.Stream != null && Subscriber.Stream.HasVideo) DoOnVideoSubscriptionEnabledChanged(true);
            }
        }

        void OnStreamDestroyed(object sender, Session.StreamDroppedEventArgs e)
        {
            DeactivateStreamContainer(PublisherContianer);
            DeactivateStreamContainer(SubscriberContainer);
        }

        void OnConnectionDestroyed(object sender, Session.ConnectionDestroyedEventArgs e) => BaseOpenTokService.Current.EndSession();

        public void DoEndSession()
        {
            lock (syncLock)
            {
                if (Subscriber != null)
                {
                    if (Subscriber.SubscribeToAudio) Subscriber.SubscribeToAudio = false;
                    if (Subscriber.SubscribeToVideo) Subscriber.SubscribeToVideo = false;

                    Subscriber.Connected -= OnSubscriberDidConnectToStream;
                    Subscriber.VideoDisabled -= OnSubscriberVideoDisabled;
                    Subscriber.VideoEnabled -= OnSubscriberVideoEnabled;

                    Subscriber.Dispose();
                    Subscriber = null;
                }

                if (Publisher != null)
                {
                    if (Publisher.PublishAudio) Publisher.PublishAudio = false;
                    if (Publisher.PublishVideo) Publisher.PublishVideo = false;
                    Publisher.Dispose();
                    Publisher = null;
                }

                if (Session != null)
                {
                    Session.ConnectionDestroyed -= OnConnectionDestroyed;
                    Session.Connected -= OnDidConnect;
                    Session.StreamReceived -= OnStreamCreated;
                    Session.StreamDropped -= OnStreamDestroyed;

                    Session.Disconnect();
                    Session.Dispose();
                    Session = null;
                }

                if (role != OpenTokRole.SUBSCRIBER)
                {
                    DeactivateStreamContainer(PublisherContianer);
                    PublisherContianer = null;
                }

                DeactivateStreamContainer(SubscriberContainer);
                SubscriberContainer = null;
            }
        }

        public void DoSendSignalToAllSubscribers(string type, string message)
        {
            Session.SendSignal(type, message);
        }

        public class SignalListener : Java.Lang.Object, Session.ISignalListener
        {
            public void Dispose() { }

            public void OnSignalReceived(Session p0, string p1, string p2, Connection p3)
            {
                BaseOpenTokService.Current.SignalReceived?.Invoke(p1, p2);
            }
        }
    }
}
