using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class ChatFlowExtensions
{
    public static ChatFlow<T> GetBotUserOrBreak<T>(
        this ChatFlow<T> chatFlow, string? failureUserMessage, Func<T, BotUser, T> mapFlowState)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return chatFlow.ForwardValue(InnerInvokeValueAsync);

        async ValueTask<ChatFlowJump<T>> InnerInvokeValueAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
        {
            var jump = await context.InnerGetBotUserAsync(failureUserMessage, cancellationToken).ConfigureAwait(false);
            return jump.Fold(InnerMapFlowState, ChatFlowJump<T>.Break);

            ChatFlowJump<T> InnerMapFlowState(BotUser botUser)
                =>
                mapFlowState.Invoke(context.FlowState, botUser);
        }
    }

    private static async ValueTask<Result<BotUser, ChatFlowBreakState>> InnerGetBotUserAsync<T>(
        this IChatFlowContext<T> context, string? failureUserMessage, CancellationToken cancellationToken)
    {
        var botUser = await context.BotUserProvider.GetCurrentUserAsync(cancellationToken).ConfigureAwait(false);

        if (botUser is null)
        {
            return ChatFlowBreakState.From(failureUserMessage, "Bot user must be specified");
        }

        return botUser;
    }
}