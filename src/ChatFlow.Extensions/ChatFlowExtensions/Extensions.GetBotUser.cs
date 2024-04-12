using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class ChatFlowExtensions
{
    public static ChatFlow<T> GetBotUser<T>(
        this ChatFlow<T> chatFlow,
        Func<T, BotUser?, T> mapFlowState,
        Func<IChatFlowContext<T>, SkipOption>? skipFactory = null)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return chatFlow.ForwardValue(InnerGetBotUserJumpAsync);

        async ValueTask<ChatFlowJump<T>> InnerGetBotUserJumpAsync(
            IChatFlowContext<T> context, CancellationToken cancellationToken)
        {
            if (skipFactory?.Invoke(context).Skip is true)
            {
                return context.FlowState;
            }

            var botUser = await context.BotUserProvider.GetCurrentUserAsync(cancellationToken).ConfigureAwait(false);
            return mapFlowState.Invoke(context.FlowState, botUser);
        }
    }
}