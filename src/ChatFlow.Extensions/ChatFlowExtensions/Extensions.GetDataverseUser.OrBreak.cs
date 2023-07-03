using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class ChatFlowExtensions
{
    public static ChatFlow<T> GetDataverseUserOrBreak<T>(
        this ChatFlow<T> chatFlow, string? failureUserMessage, Func<T, DataverseUserData, T> mapFlowState)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return chatFlow.ForwardValue(InnerInvokeValueAsync, mapFlowState);

        ValueTask<ChatFlowJump<DataverseUserData>> InnerInvokeValueAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
            =>
            InnerGetDataverseUserJumpAsync(context, failureUserMessage, cancellationToken);
    }

    private static async ValueTask<ChatFlowJump<DataverseUserData>> InnerGetDataverseUserJumpAsync<T>(
        IChatFlowContext<T> context, string? failureUserMessage, CancellationToken cancellationToken)
    {
        var botUserJump = await context.InnerGetBotUserJumpAsync(failureUserMessage, cancellationToken).ConfigureAwait(false);
        return botUserJump.Forward(GetDataverseUserDataJump);

        ChatFlowJump<DataverseUserData> GetDataverseUserDataJump(BotUser botUser)
        {
            var claims = botUser.Claims.AsEnumerable();
            var dataverseUserIdResult = claims.GetValueOrAbsent("DataverseSystemUserId").Fold(ParseOrBreak, CreateAbsentClaimBreak);

            if (dataverseUserIdResult.IsFailure)
            {
                return dataverseUserIdResult.FailureOrThrow();
            }

            return new DataverseUserData(
                botUser: botUser,
                systemUserId: dataverseUserIdResult.SuccessOrThrow(),
                firstName: claims.GetValueOrAbsent("DataverseSystemUserFirstName").OrDefault(),
                lastName: claims.GetValueOrAbsent("DataverseSystemUserLastName").OrDefault(),
                fullName: claims.GetValueOrAbsent("DataverseSystemUserFullName").OrDefault());
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