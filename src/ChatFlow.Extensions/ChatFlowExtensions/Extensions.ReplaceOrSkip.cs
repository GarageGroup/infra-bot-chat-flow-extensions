using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class ChatFlowExtensions
{
    public static ChatFlow<T> ReplaceActivityOrSkip<T>(
        this ChatFlow<T> chatFlow, Func<IChatFlowContext<T>, IActivity?> activityProvider, Func<T, ResourceResponse, T>? resultMapper = null)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(activityProvider);

        return chatFlow.NextValue(InnerInvokeAsync);

        async ValueTask<T> InnerInvokeAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
        {
            var activity = activityProvider.Invoke(context);
            if (activity is null)
            {
                return context.FlowState;
            }

            var response = await context.InnerReplaceActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            if (resultMapper is null)
            {
                return context.FlowState;
            }

            return resultMapper.Invoke(context.FlowState, response);
        }
    }
}