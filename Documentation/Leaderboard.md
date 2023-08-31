# Leaderboard Documentation

This guide provides information on integrating and utilizing leaderboard functionality in your PICO VR application.

## Table of Contents

- [Overview](#overview)
- [Adding Entries to the Leaderboard](#adding-entries-to-the-leaderboard)
- [Retrieving Leaderboard Data](#retrieving-leaderboard-data)
  - [Loading Global Leaderboard](#loading-global-leaderboard)
  - [Loading Friends Leaderboard](#loading-friends-leaderboard)

## Overview

The Leaderboard system allows players to view and compare their scores with others. This documentation explains how to set up and use the LeaderboardManager class to create and manage leaderboards in your PICO VR application.

## Adding Entries to the Leaderboard

Once you've set up the LeaderboardManager, you can add entries to the leaderboard. Here's an example of adding a score entry to the leaderboard:

Using the following fonction :
```csharp
    public void AddScoreEntry(string winningPlayer = null, long incrementScore = DefaultIncrementScore)
    {
        // Retrieve leaderboard entries
        LeaderboardService.GetEntries(leaderboardName, 10, 0, LeaderboardFilterType.None, LeaderboardStartAt.Top)
            .OnComplete(message =>
            {
                if (!message.IsError)
                {
                    // Find the entry of the logged-in user by searching for the first data entry in the message where the user's display name matches the display name of the logged-in user.
                    var loggedInUserEntry = message.Data.FirstOrDefault(entryData => entryData.User.DisplayName == loggedInUser.DisplayName);

                    if (loggedInUserEntry != null)
                    {
                        // Calculate the new score based on the existing or default score
                        var newScore = loggedInUserEntry.Score + incrementScore;
                        WriteEntryInLeaderboard(newScore);
                    }
                    else
                    {
                        Debug.Log("Entry for the logged-in user not found. New score added.");
                        WriteEntryInLeaderboard(incrementScore);
                    }
                }
                else
                {
                    var errorText = $"<color=red>Error receiving leaderboard entries:</color> {message.GetError().Message}";
                    Debug.Log(errorText);
                }
            });
    }
```

Which can be called this way in your script :
```csharp
LeaderboardManager.Instance.AddScoreEntry(roundWinner.m_userName);
```

## Retrieving Leaderboard Data
You can retrieve leaderboard data to display it to your players.
```csharp
    private void LoadEntries(LeaderboardFilterType filterType, LeaderboardStartAt startAt)
    {
        // Retrieve leaderboard entries
        LeaderboardService.GetEntries(leaderboardName, 10, 0, filterType, startAt)
            .OnComplete(HandleGetEntriesResponse);
    }

    private void HandleGetEntriesResponse(Message<LeaderboardEntryList> message)
    {
        if (!message.IsError)
        {
         
            foreach (var entryData in message.Data)
            {
                // Display the leaderboard entry prefab and add it as a child of the parent
                var entryObject = Instantiate(leaderboardEntryPrefab, leaderboardEntryParent);
            }
        }
        else
        {
            var errorText = $"<color=red>Error receiving leaderboard entries:</color> {message.GetError().Message}";
        }
    }
```

## Loading Global Leaderboard
Load global leaderboard entries:

```csharp
public void LoadGlobal()
{
    LoadEntries(LeaderboardFilterType.None, LeaderboardStartAt.Top);
}
```

## Loading Friends Leaderboard
Load leaderboard entries filtered by friends:

```csharp
public void LoadFriends()
{
    // Load leaderboard entries filtered by friends
    LoadEntries(LeaderboardFilterType.Friends, LeaderboardStartAt.Top);
}
```
