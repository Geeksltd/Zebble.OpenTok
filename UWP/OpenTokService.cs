namespace Zebble.Plugin.Renderer
{
    using System;
    using System.ComponentModel;
    using Zebble;
    using Windows.UI;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class OpenTokService : BaseOpenTokService.INativeImplementation
    {
        public void DoEndSession()
        {
            //throw new NotImplementedException();
        }

        public void DoInitSession(string apiKey, string sessionId, string userToken)
        {
            //throw new NotImplementedException();
        }

        public void DoInitSession(string apiKey, string sessionId, string userToken, string role)
        {
            throw new NotImplementedException();
        }

        public void DoOnAudioPublishingEnabledChanged(bool value)
        {
            //throw new NotImplementedException();
        }

        public void DoOnAudioSubscriptionEnabledChanged(bool value)
        {
            //throw new NotImplementedException();
        }

        public void DoOnVideoPublishingEnabledChanged(bool value)
        {
            //throw new NotImplementedException();
        }

        public void DoOnVideoSubscriptionEnabledChanged(bool value)
        {
            //throw new NotImplementedException();
        }

        public void DoSendSignalToAllSubscribers(string type, string message)
        {
            //throw new NotImplementedException();
        }

        public void DoSetPublisherContainer(object container)
        {
            //throw new NotImplementedException();
        }

        public void DoSetSubscriberContainer(object container)
        {
            //throw new NotImplementedException();
        }

        public void DoSwapCamera()
        {
            //throw new NotImplementedException();
        }
    }
}