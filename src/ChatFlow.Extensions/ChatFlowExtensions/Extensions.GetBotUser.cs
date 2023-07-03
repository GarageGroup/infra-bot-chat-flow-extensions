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

        return chatFlow.ForwardValue(InnerGetBotUserJumpAsync, mapFlowState);
    }

    private static async ValueTask<ChatFlowJump<BotUser?>> InnerGetBotUserJumpAsync<T>(
        IChatFlowContext<T> context, CancellationToken cancellationToken)
        =>
        await context.BotUserProvider.GetCurrentUserAsync(cancellationToken).ConfigureAwait(false);
}