namespace Zebble.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Zebble;

    public partial class OpenTokView : Stack
    {
        string sessionId;
        string userToken;

        public Canvas PublisherContianer = new Canvas().Id("PublisherContianer");
        public Canvas SubscriberContianer = new Canvas().Id("SubscriberContianer");
        public Stack EndSessionButton = new Stack().Id("EndSessionButton");
        public Stack MuteVideoButton = new Stack().Id("MuteVideoButton");
        public Stack MuteAudioButton = new Stack().Id("MuteAudioButton");
        public Stack SwapCameraButton = new Stack().Id("SwapCameraButton");

        public Action<string, string> MessageReceived;

        public string SessionId
        {
            get => sessionId; set
            {
                if (sessionId == value) return;
                sessionId = value;
            }
        }

        public string UserToken
        {
            get => userToken; set
            {
                if (userToken == value) return;
                userToken = value;
            }
        }

        public override async Task OnInitializing()
        {
            await base.OnInitializing();

            MuteVideoButton.On(x => x.Tapped, () => BaseOpenTokService.Current.VideoPublishingEnabled = !BaseOpenTokService.Current.VideoPublishingEnabled);

            MuteAudioButton.On(x => x.Tapped, () => BaseOpenTokService.Current.AudioPublishingEnabled = !BaseOpenTokService.Current.AudioPublishingEnabled);

            EndSessionButton.On(x => x.Tapped, () => BaseOpenTokService.Current.EndSession());

            SwapCameraButton.On(x => x.Tapped, () => BaseOpenTokService.Current.SwapCamera());

            await Add(PublisherContianer);
            await Add(SubscriberContianer);
            await Add(EndSessionButton);
            await Add(MuteVideoButton);
            await Add(MuteAudioButton);
            await Add(SwapCameraButton);

            await WhenShown(() => Thread.UI.Run(() => OnShown()));
        }

        public void OnShown()
        {
            BaseOpenTokService.Current.SignalReceived += MessageReceived;
            BaseOpenTokService.Current.SetPublisherContainer(PublisherContianer.Native());
            BaseOpenTokService.Current.SetSubscriberContainer(SubscriberContianer.Native());
            BaseOpenTokService.Current.InitSession(sessionId, userToken);
        }

        public void SendEmojiToAll(string name)
        {
            BaseOpenTokService.Current.SendSignallToAllSubscribers("Emoji", name);
        }
    }
}