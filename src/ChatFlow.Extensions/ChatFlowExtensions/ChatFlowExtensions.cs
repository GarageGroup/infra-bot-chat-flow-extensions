using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

public static partial class ChatFlowExtensions
{
    private static async Task<ResourceResponse> InnerReplaceActivityAsync(
        this ITurnContext turnContext, IActivity activity, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(activity.Id) || (IsAllowedChannelToDelete(activity.ChannelId) is false))
        {
            return await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        }

        var activityDeleteTask = turnContext.DeleteActivityAsync(activity.Id, cancellationToken);
        var activitySendTask = turnContext.SendActivityAsync(activity, cancellationToken);

        await Task.WhenAll(activityDeleteTask, activitySendTask).ConfigureAwait(false);
        return await activitySendTask.ConfigureAwait(false);

        static bool IsAllowedChannelToDelete(string channelId)
            =>
            string.Equals(channelId, Channels.Telegram, StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(channelId, Channels.Msteams, StringComparison.InvariantCultureIgnoreCase);
    }
}