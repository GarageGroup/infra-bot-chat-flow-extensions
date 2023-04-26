using System;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class ChatFlowExtensions
{
    public static ChatFlow<T> SetTypingStatus<T>(this ChatFlow<T> chatFlow)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        return chatFlow.SendActivity(InnerCreateActivity);

        static Activity InnerCreateActivity(IChatFlowContext<T> _)
            =>
            new()
            {
                Type = ActivityTypes.Typing
            };
    }
}