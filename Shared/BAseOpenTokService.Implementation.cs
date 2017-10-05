namespace Zebble.Plugin
{
    using System;
    using System.Threading.Tasks;

    public partial class BaseOpenTokService
    {
        INativeImplementation implementation;

        INativeImplementation Implementation => implementation ?? throw new Exception("OpenTok is not initialized.");

        public void Initialize<TNativeImplementation>() where TNativeImplementation : INativeImplementation, new()
        {
            if (implementation != null) return;
            else implementation = new TNativeImplementation();
        }

        public interface INativeImplementation
        {
            void DoInitSession(string apiKey, string sessionId, string userToken);
            void DoSwapCamera();
            void DoSetPublisherContainer(object container);
            void DoSetSubscriberContainer(object container);
            void DoOnAudioPublishingEnabledChanged(bool value);
            void DoOnVideoPublishingEnabledChanged(bool value);
            void DoOnVideoSubscriptionEnabledChanged(bool value);
            void DoOnAudioSubscriptionEnabledChanged(bool value);
            void DoEndSession();
        }
    }
}