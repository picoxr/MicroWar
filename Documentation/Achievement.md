# PICO VR Achievement Integration Guide

This guide will walk you through the process of integrating achievements into your PICO VR application using the Achievement Manager. Achievements add a layer of engagement and motivation for users to explore and enjoy your VR experience.

## Table of Contents
- [Overview](#overview)
- [Get Achievement data](#overview)
  - [Get Achievement Information](#get-achievement-information)
  - [Get Achievement Information](#get-achievement-information)
  - [Get Basic Information of Achievements on a Page](#get-basic-information-of-achievements-on-a-page)
- [Update Achievement Progress](#update-achievement-progress)
  - [Unlock a Simple Achievement](#unlock-a-simple-achievement)
  - [Update Progress for a Count Achievement](#update-progress-for-a-count-achievement)
  - [Update Progress for a Bitfield Achievement](#update-progress-for-a-bitfield-achievement)
    
![image](https://github.com/picoxr/MicroWar/assets/46362299/21b51957-587e-42bf-b003-12c5caeed5f9)

## Overview


The Achievement Manager is a tool for managing and tracking achievements in your PICO VR application. It provides a way to integrate achievements, display them in your user interface, and interact with the PICO VR achievement API.

## Get Achievement data

## Get Achievement Information
Retrieve information about achievements, including their API name, description, type, and unlock criteria.

```csharp
AchievementsService.GetDefinitionsByName(new string[] { "yourAchievementName" }).OnComplete(
    (msg) => { // Message<AchievementDefinitionList>
        if (!msg.IsError)
        {
            var list = msg.Data.GetEnumerator();
            while (list.MoveNext())
            {
                var item = list.Current;
                Debug.Log($"Name: {item.Name}" +
                          $"Target: {item.Target}" +
                          $"Type: {item.Type}" +
                          $"BitfieldLength: {item.BitfieldLength}" +
                          $"Description: {item.Description}" +
                          $"Title: {item.Title}" +
                          $"IsArchived: {item.IsArchived}" +
                          $"IsSecret: {item.IsSecret}" +
                          $"ID: {item.ID}" +
                          $"UnlockedDescription: {item.UnlockedDescription}" +
                          $"WritePolicy: {item.WritePolicy}" +
                          $"LockedImageURL: {item.LockedImageURL}" +
                          $"UnlockedImageURL: {item.UnlockedImageURL}");
            }
        }
    }
);
```


### Get Basic Information of Achievements on a Page
To retrieve basic information about achievements on a specified page:

```csharp
AchievementsService.GetAllDefinitions(0, 5).OnComplete(
    (msg) => { // Message<AchievementDefinitionList>
        if (!msg.IsError)
        {
            var list = msg.Data.GetEnumerator();
            var totalSize = msg.Data.TotalSize; // The total number of achievements
            while (list.MoveNext())
            {
                var item = list.Current;
                Debug.Log($"Name: {item.Name}" +
                          $"Target: {item.Target}" +
                          $"Type: {item.Type}" +
                          $"BitfieldLength: {item.BitfieldLength}" +
                          $"Description: {item.Description}" +
                          $"Title: {item.Title}" +
                          $"IsArchived: {item.IsArchived}" +
                          $"IsSecret: {item.IsSecret}" +
                          $"ID: {item.ID}" +
                          $"UnlockedDescription: {item.UnlockedDescription}" +
                          $"WritePolicy: {item.WritePolicy}" +
                          $"LockedImageURL: {item.LockedImageURL}" +
                          $"UnlockedImageURL: {item.UnlockedImageURL}");
            }
        }
    }
);
```


### Get Achievement Progress
Retrieve the progress a user has made on a specific achievement, including whether it's unlocked, the unlock time, and more.

```csharp
AchievementsService.GetProgressByName(new string[] { "yourAchievementName" }).OnComplete(
    (msg) => { // Message<AchievementProgressList>
        var list = obj.GetEnumerator();
        while (list.MoveNext())
        {
            var item = list.Current;
            Debug.Log($"IsUnlocked: {item.IsUnlocked}" + // Simple achievements have no progress information and they only have two statuses: locked, unlocked.
                      $"UnlockTime: {item.UnlockTime}" +
                      $"ID: {item.ID}" +
                      $"Name: {item.Name}" +
                      $"Bitfield: {item.Bitfield}" + // The progress of a bitfield achievement
                      $"Count: {item.Count}" + // The progress of a count achievement
                      $"ExtraData: {Encoding.UTF8.GetString(item.ExtraData)}");
        }
    }
);
```

## Update Achievement Progress

### Unlock a Simple Achievement
Unlock an achievement when a user reaches the specified goal:

```csharp
AchievementManager.Instance.UnlockAchievement("Achievement_Name");
```

### Update Progress for a Count Achievement
For count-based achievements, use the AddCount method:

```csharp
Copy code
int count = 1;
byte[] bytes = new byte[] { };
AchievementsService.AddCount("yourAchievementName", count, bytes).OnComplete(
    (msg) => { // msg:Message<AchievementUpdate>
        if (!msg.IsError)
        {
            var updateData = msg.Data;
            Debug.Log($"achievementName: {updateData.Name}");
            Debug.Log($"JustUnlocked: {updateData.JustUnlocked}");
        }
    }
);
```


### Update Progress for a Bitfield Achievement
For bitfield-based achievements, use the AddFields method:

```csharp
Copy code
byte[] bytes = new byte[] { };
string fields = "100011";
AchievementsService.AddFields("yourAchievementName", fields, bytes).OnComplete(
    (msg) => { // msg:Message<AchievementUpdate>
        if (!msg.IsError)
        {
            var updateData = msg.Data;
            Debug.Log($"achievementName: {updateData.Name}");
            Debug.Log($"JustUnlocked: {updateData.JustUnlocked}");
        }
    }
);
```
