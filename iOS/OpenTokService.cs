namespace Zebble.Plugin.Renderer
{
    using System;
    using UIKit;
    using OpenTok;
    using CoreGraphics;
    using AVFoundation;

    public class OpenTokService : BaseOpenTokService.INativeImplementation
    {
        object syncLock = new object();
        string role;

        OTSession Session;
        OTPublisher Publisher;
        OTSubscriber Subscriber;

        UIView PublisherContianer;
        UIView SubscriberContainer;

        public void DoInitSession(
            string apiKey,
            string sessionId,
            string userToken,
            string role = OpenTokRole.PUBLISHER)
        {
            if (Session != null) BaseOpenTokService.Current.EndSession();
            this.role = role;
            Session = new OTSession(apiKey, sessionId, null);
            HandleSessionEvents();
            Session.Init();
            Session.ConnectWithToken(userToken, out var error);
        }

        public void DoSwapCamera()
        {
            if (Publisher == null) return;

            if (Publisher.CameraPosition != AVCaptureDevicePosition.Front) Publisher.CameraPosition = AVCaptureDevicePosition.Front;
            else Publisher.CameraPosition = AVCaptureDevicePosition.Back;
        }

        public void DoSetPublisherContainer(object container)
        {
            var streamContainer = ((UIView)container);
            UIView streamView = null;

            PublisherContianer = streamContainer;
            if (Publisher != null) streamView = Publisher.View;

            ActivateStreamContainer(streamContainer, streamView);
        }

        public void DoSetSubscriberContainer(object container)
        {
            var streamContainer = ((UIView)container);
            UIView streamView = null;

            SubscriberContainer = streamContainer;
            if (Subscriber != null) streamView = Subscriber.View;

            ActivateStreamContainer(streamContainer, streamView);
        }

        void HandleSessionEvents()
        {
            Session.ConnectionDestroyed += OnConnectionDestroyed;
            Session.DidConnect += OnDidConnect;
            Session.StreamCreated += OnStreamCreated;
            Session.StreamDestroyed += OnStreamDestroyed;
            Session.ReceivedSignalType += OnReceivedSignalType;
        }

        private void OnReceivedSignalType(object sender, OTSessionDelegateSignalEventArgs e)
        {
            BaseOpenTokService.Current.SignalReceived?.Invoke(e.Type, e.StringData);
        }

        void ActivateStreamContainer(UIView container, UIView view)
        {
            DeactivateStreamContainer(container);
            if (container == null || view == null) return;

            if (view.Superview != null) view.RemoveFromSuperview();
            view.Frame = new CGRect(0, 0, container.Frame.Width, container.Frame.Height);
            container.Add(view);
        }

        void DeactivateStreamContainer(UIView container)
        {
            if (container == null || container.Subviews == null) return;

            while (container.Subviews.Length > 0)
            {
                var view = container.Subviews[0];
                view.RemoveFromSuperview();
            }
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
                if (role == OpenTokRole.PUBLISHER)
                    SetPublisher();
            }
        }

        private void SetPublisher()
        {
            if (Publisher != null || Session == null)
                return;
            Publisher = new OTPublisher(null, new OTPublisherSettings
            {
                Name = "XamarinOpenTok",
                CameraFrameRate = OTCameraCaptureFrameRate.OTCameraCaptureFrameRate15FPS,
                CameraResolution = OTCameraCaptureResolution.High,
            });

            Session.Publish(Publisher, out var error);
            ActivateStreamContainer(PublisherContianer, Publisher.View);
        }

        void OnStreamCreated(object sender, OTSessionDelegateStreamEventArgs e)
        {
            lock (syncLock)
            {
                if (Subscriber != null || Session == null)
                    return;

                Subscriber = new OTSubscriber(e.Stream, null);
                HandleSubscriptionEvents();

                Session.Subscribe(Subscriber, out var error);
            }
        }

        private void HandleSubscriptionEvents()
        {
            Subscriber.DidConnectToStream += OnSubscriberDidConnectToStream;
            Subscriber.VideoDataReceived += OnSubscriberVideoDataReceived;
            Subscriber.VideoEnabled += OnSubscriberVideoEnabled;
        }

        private void OnSubscriberVideoEnabled(object sender, OTSubscriberKitDelegateVideoEventReasonEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void OnSubscriberVideoDataReceived(object sender, EventArgs e) => ActivateStreamContainer(SubscriberContainer, Subscriber.View);

        void OnSubscriberDidConnectToStream(object sender, EventArgs e)
        {
            lock (syncLock)
            {
                if (Subscriber == null) return;

                ActivateStreamContainer(SubscriberContainer, Subscriber.View);
                if (Subscriber.Stream != null && Subscriber.Stream.HasVideo) DoOnVideoSubscriptionEnabledChanged(true);
            }
        }

        void OnStreamDestroyed(object sender, OTSessionDelegateStreamEventArgs e)
        {
            PublisherContianer?.InvokeOnMainThread(() =>
            {
                DeactivateStreamContainer(PublisherContianer);
            });

            SubscriberContainer?.InvokeOnMainThread(() =>
            {
                DeactivateStreamContainer(SubscriberContainer);
            });
        }

        void OnConnectionDestroyed(object sender, OTSessionDelegateConnectionEventArgs e) => BaseOpenTokService.Current?.EndSession();

        public void DoEndSession()
        {
            lock (syncLock)
            {
                if (Subscriber != null)
                {
                    if (Subscriber.SubscribeToAudio) Subscriber.SubscribeToAudio = false;
                    if (Subscriber.SubscribeToVideo) Subscriber.SubscribeToVideo = false;

                    Subscriber.DidConnectToStream -= OnSubscriberDidConnectToStream;
                    Subscriber.VideoDataReceived -= OnSubscriberVideoDataReceived;
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
                    Session.DidConnect -= OnDidConnect;
                    Session.StreamCreated -= OnStreamCreated;
                    Session.StreamDestroyed -= OnStreamDestroyed;

                    Session.Disconnect();
                    Session.Dispose();
                    Session = null;
                }

                PublisherContianer?.InvokeOnMainThread(() =>
                {
                    DeactivateStreamContainer(PublisherContianer);
                    PublisherContianer = null;
                });

                SubscriberContainer?.InvokeOnMainThread(() =>
                {
                    DeactivateStreamContainer(SubscriberContainer);
                    SubscriberContainer = null;
                });
               
            }
        }

        public void DoSendSignalToAllSubscribers(string type, string message)
        {
            Session.SignalWithType(type, message, null, true, out var error);
        }
    }
}
