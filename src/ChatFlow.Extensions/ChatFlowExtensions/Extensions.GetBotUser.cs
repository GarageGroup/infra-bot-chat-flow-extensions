using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class ChatFlowExtensions
{
    public static ChatFlow<T> GetBotUser<T>(
        this ChatFlow<T> chatFlow, Func<T, BotUser?, T> mapFlowState)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return chatFlow.ForwardValue(InnerGetBotUserJumpAsync);

        async ValueTask<ChatFlowJump<T>> InnerGetBotUserJumpAsync(
            IChatFlowContext<T> context, CancellationToken cancellationToken)
        {
            var botUser = await context.BotUserProvider.GetCurrentUserAsync(cancellationToken).ConfigureAwait(false);
            return mapFlowState.Invoke(context.FlowState, botUser);
        }
    }
}