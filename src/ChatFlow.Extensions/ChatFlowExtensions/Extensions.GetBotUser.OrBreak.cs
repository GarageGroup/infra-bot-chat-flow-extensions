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

        return chatFlow.ForwardValue(InnerInvokeValueAsync, mapFlowState);

        ValueTask<ChatFlowJump<BotUser>> InnerInvokeValueAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
            =>
            InnerGetBotUserJumpAsync(context, failureUserMessage, cancellationToken);
    }

    private static async ValueTask<ChatFlowJump<BotUser>> InnerGetBotUserJumpAsync<T>(
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