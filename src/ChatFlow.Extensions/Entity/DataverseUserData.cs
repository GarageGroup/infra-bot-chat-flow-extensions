using System;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class DataverseUserData
{
    public DataverseUserData(BotUser botUser, Guid systemUserId, string? firstName, string? lastName, string? fullName)
    {
        BotUser = botUser;
        SystemUserId = systemUserId;
        FirstName = firstName;
        LastName = lastName;
        FullName = fullName;
    }

    public BotUser BotUser { get; }

    public Guid SystemUserId { get; }

    public string? FirstName { get; }

    public string? LastName { get; }

    public string? FullName { get; }
}