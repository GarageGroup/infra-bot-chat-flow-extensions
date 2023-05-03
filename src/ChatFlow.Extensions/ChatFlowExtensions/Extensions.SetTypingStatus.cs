using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class ChatFlowExtensions
{
    public static ChatFlow<T> SetTypingStatus<T>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, IActivity?>? temporaryActivityProvider = null,
        Func<T, ResourceResponse, T>? resultMapper = null)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        return chatFlow.NextValue(InnerInvokeAsync);

        async ValueTask<T> InnerInvokeAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
        {
            var temporaryActivity = temporaryActivityProvider?.Invoke(context);
            var temporaryActivitySendTask = temporaryActivity switch
            {
                not null => context.SendActivityAsync(temporaryActivity, cancellationToken),
                _ => null
            };

            var typingActivity = new Activity
            {
                Type = ActivityTypes.Typing
            };

            var typingActivitySendTask = context.SendActivityAsync(typingActivity, cancellationToken);
            if (temporaryActivitySendTask is null)
            {
                _ = await typingActivitySendTask.ConfigureAwait(false);
                return context.FlowState;
            }

            _ = await Task.WhenAll(temporaryActivitySendTask, typingActivitySendTask).ConfigureAwait(false);

            if (resultMapper is null)
            {
                return context.FlowState;
            }

            var response = await temporaryActivitySendTask.ConfigureAwait(false);
            return resultMapper.Invoke(context.FlowState, response);
        }
    }
}