using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class ChatFlowExtensions
{
    public static ChatFlow<T> GetDataverseUserOrBreak<T>(
        this ChatFlow<T> chatFlow,
        string? failureUserMessage, Func<T, DataverseUserData, T> mapFlowState,
        Func<IChatFlowContext<T>, SkipOption>? skipFactory = null)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return chatFlow.ForwardValue(InnerInvokeValueAsync);

        ValueTask<ChatFlowJump<T>> InnerInvokeValueAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
            =>
            InnerGetDataverseUserJumpAsync(context, failureUserMessage, mapFlowState, skipFactory, cancellationToken);
    }

    private static async ValueTask<ChatFlowJump<T>> InnerGetDataverseUserJumpAsync<T>(
        IChatFlowContext<T> context,
        string? failureUserMessage,
        Func<T, DataverseUserData, T> mapFlowState,
        Func<IChatFlowContext<T>, SkipOption>? skipFactory,
        CancellationToken cancellationToken)
    {
        if (skipFactory?.Invoke(context).Skip is true)
        {
            return context.FlowState;
        }

        var botUserJump = await context.InnerGetBotUserAsync(failureUserMessage, cancellationToken).ConfigureAwait(false);
        return botUserJump.Fold(GetDataverseUserDataJump, ChatFlowJump<T>.Break);

        ChatFlowJump<T> GetDataverseUserDataJump(BotUser botUser)
        {
            var claims = botUser.Claims.AsEnumerable();
            var dataverseUserIdResult = claims.GetValueOrAbsent("DataverseSystemUserId").Fold(ParseOrBreak, CreateAbsentClaimBreak);

            if (dataverseUserIdResult.IsFailure)
            {
                return dataverseUserIdResult.FailureOrThrow();
            }

            var data = new DataverseUserData(
                botUser: botUser,
                systemUserId: dataverseUserIdResult.SuccessOrThrow(),
                firstName: claims.GetValueOrAbsent("DataverseSystemUserFirstName").OrDefault(),
                lastName: claims.GetValueOrAbsent("DataverseSystemUserLastName").OrDefault(),
                fullName: claims.GetValueOrAbsent("DataverseSystemUserFullName").OrDefault());

            return mapFlowState.Invoke(context.FlowState, data);
        }

        Result<Guid, ChatFlowBreakState> ParseOrBreak(string value)
            =>
            Guid.TryParse(value, out var guid) switch
            {
                true => guid,
                _ => ChatFlowBreakState.From(failureUserMessage, $"DataverseUserId Claim {value} is not a Guid")
            };

        Result<Guid, ChatFlowBreakState> CreateAbsentClaimBreak()
            =>
            ChatFlowBreakState.From(failureUserMessage, "Dataverse user claim must be specified");
    }
}