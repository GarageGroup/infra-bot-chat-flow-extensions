using System;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

partial class ChatFlowJumpExtensions
{
    public static ChatFlowBreakState ToChatFlowBreakState<TFailureCode>(
        this Failure<TFailureCode> failure, [AllowNull] string userMessage = null)
        where TFailureCode : struct
        =>
        new(userMessage, $"An unexpected failure occured. Code: {failure.FailureCode}. Message: '{failure.FailureMessage}'")
        {
            SourceException = failure.SourceException
        };
}