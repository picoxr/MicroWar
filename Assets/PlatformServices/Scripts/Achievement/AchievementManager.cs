using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Pico.Platform.Models;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Pico.Platform;
using TMPro;
using MicroWar.Platform;
using static AchievementManager;
using System.Linq;

public static class CountAchievementsExtensions
{
    public static string GetName(this CountAchievements achievement)
    {
        return achievement.ToString().Replace("_", "");
    }
}

public class AchievementManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private TextMeshProUGUI _usernameText;
    [SerializeField] private TextMeshProUGUI _achievementErrorInfo;
    [SerializeField] private TextMeshProUGUI _achievementInfoText;
    [SerializeField] private GameObject achievementEntryPrefab;
    [SerializeField] private Toggle toggleAchievementLocked;
    [SerializeField] private Toggle toggleAchievementUnlocked;

    #endregion

    #region Public Fields

    public Transform lockedAchievementPanel;
    public Transform unlockedAchievementPanel;
    public Transform lockedAchievementParent;
    public Transform unlockedAchievementParent;
    public Sprite loadingPlaceholderSprite;
    public enum CountAchievements
    {
        // Achievement : Win a round
        Bronze_RoundMaster,
        Silver_RoundMaster,
        Gold_RoundMaster,
        Platinum_RoundMaster,

        // Achievement : Play a round
        Bronze_TenaciousRounder,
        Silver_TenaciousRounder,
        Gold_TenaciousRounder,
        Platinum_TenaciousRounder,

        // Achievement : Win a match
        Bronze_MatchDominator,
        Silver_MatchDominator,
        Gold_MatchDominator,
        Platinum_MatchDominator,

        // Play a match
        Bronze_Matchmaker,
        Silver_Matchmaker,
        Gold_Matchmaker,
        Platinum_Matchmaker,
    }

    #endregion

    #region Private Fields

    private static AchievementManager _instance;
    public static AchievementManager Instance => _instance;
    private List<GameObject> achievementEntries = new List<GameObject>();

    // Constants for maximum achievement definitions and progress
    private const int MAX_ACHIEVEMENT_DEFINITION = 100;
    private const int MAX_ACHIEVEMENT_PROGRESS = 100;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        InitializeUI();
    }

    #endregion

    #region User Interface Initialization

    private void InitializeUI()
    {
        // Set the username
        _usernameText.text = PlatformServiceManager.Instance.Me.DisplayName;


        // Hide the locked achievements panel and show the unlocked achievements panel by default
        lockedAchievementPanel.gameObject.SetActive(false);
        unlockedAchievementPanel.gameObject.SetActive(true);
    }

    #endregion

    #region Achievement Loading and Display

    private void RequestAchievementDefinitions()
    {
        AchievementsService.GetAllDefinitions(0, MAX_ACHIEVEMENT_DEFINITION).OnComplete(HandleDefinitionsResponse);
    }

    private void HandleDefinitionsResponse(Message<AchievementDefinitionList> definitionMsg)
    {
        if (!definitionMsg.IsError)
        {
            var definitionList = definitionMsg.Data;
            RequestAchievementProgress(definitionList);
        }
        else
        {
            HandleError("Error getting achievement definitions", definitionMsg.Error);
        }
    }

    private void RequestAchievementProgress(AchievementDefinitionList definitionList)
    {
        AchievementsService.GetAllProgress(0, MAX_ACHIEVEMENT_PROGRESS).OnComplete((Message<AchievementProgressList> progressMsg) =>
        {
            if (!progressMsg.IsError)
            {
                var progressList = progressMsg.Data;
                InstantiateAchievementEntries(definitionList, progressList);
            }
            else
            {
                HandleError("Error getting achievement progress", progressMsg.Error);
            }
        });
    }

    private List<AchievementDefinition> SortAchievementsByUnlockTime(AchievementDefinitionList definitionList, AchievementProgressList progressList)
    {
        return progressList
            .Join(definitionList, progress => progress.Name, definition => definition.Name, (progress, definition) => new { Definition = definition, Progress = progress })
            .OrderBy(entry => entry.Progress.UnlockTime)
            .Select(entry => entry.Definition)
            .ToList();
    }

    private void InstantiateAchievementEntries(AchievementDefinitionList definitionList, AchievementProgressList progressList)
    {
        var sortedAchievements = SortAchievementsByUnlockTime(definitionList, progressList);

        foreach (var definition in sortedAchievements)
        {
            if (!definition.IsArchived) // Check if the achievement is archived
            {
                var matchingProgress = progressList.FirstOrDefault(progress => progress.Name == definition.Name);

                if (matchingProgress != null)
                {
                    // Instantiate the achievement entry prefab
                    var entryObject = Instantiate(achievementEntryPrefab);
                    var achievementEntry = entryObject.GetComponent<AchievementEntry>();

                    // Set loading placeholder initially to indicate data is being loaded
                    achievementEntry.SetAchievementData(matchingProgress, definition, loadingPlaceholderSprite, loadingPlaceholderSprite);

                    // Load and set the actual achievement images and data
                    StartCoroutine(LoadAndSetAchievementImages(definition, achievementEntry, matchingProgress));

                    achievementEntries.Add(entryObject);
                }
            }
        }
    }

    private IEnumerator LoadAndSetAchievementImages(AchievementDefinition achievementDefinition, AchievementEntry achievementEntry, AchievementProgress progress)
    {
        // Load the locked and unlocked achievement images concurrently
        var lockedRequest = UnityWebRequestTexture.GetTexture(achievementDefinition.LockedImageURL);
        var unlockedRequest = UnityWebRequestTexture.GetTexture(achievementDefinition.UnlockedImageURL);

        lockedRequest.SendWebRequest();
        unlockedRequest.SendWebRequest();

        // Wait for both requests to complete
        while (!lockedRequest.isDone || !unlockedRequest.isDone)
        {
            yield return null;
        }

        // Check for errors in loading images
        if (lockedRequest.result != UnityWebRequest.Result.Success || unlockedRequest.result != UnityWebRequest.Result.Success)
        {
            HandleError("Error downloading achievement images", null);
            yield break;
        }
        else
        {
            ClearInfoText();
        }

        // Create sprites from loaded textures
        var lockedTexture = DownloadHandlerTexture.GetContent(lockedRequest);
        var unlockedTexture = DownloadHandlerTexture.GetContent(unlockedRequest);

        var lockedSprite = Sprite.Create(lockedTexture, new Rect(0, 0, lockedTexture.width, lockedTexture.height), Vector2.zero);
        var unlockedSprite = Sprite.Create(unlockedTexture, new Rect(0, 0, unlockedTexture.width, unlockedTexture.height), Vector2.zero);

        // Update achievement entry with actual data
        achievementEntry.SetAchievementData(progress, achievementDefinition, lockedSprite, unlockedSprite);

        if (progress.IsUnlocked)
        {
            SetAchievementProgress(achievementEntry, unlockedAchievementParent);
        }
        else
        {
            SetAchievementProgress(achievementEntry, lockedAchievementParent);
        }
    }


    private void SetAchievementProgress(AchievementEntry achievementEntry, Transform panel)
    {
        achievementEntry.transform.SetParent(panel, false);
    }

    #endregion

    #region User Interaction

    public void UnlockSimpleAchievement(string achievementName)
    {
        byte[] extraData = null; // Optional extra data
        AchievementsService.Unlock(achievementName, extraData).OnComplete(
            (Message<AchievementUpdate> msg) =>
            {
                HandleAchievementUpdate(msg, $"unlocking '{achievementName}'");
            }
        );
    }

    public void UpdateAchievementProgress(string achievementName, long newCountOrBitfield, bool isBitfield, byte[] extraData = null)
    {
        if (isBitfield)
        {
            AchievementsService.AddFields(achievementName, newCountOrBitfield.ToString(), extraData).OnComplete(
                (Message<AchievementUpdate> msg) =>
                {
                    HandleAchievementUpdate(msg, $"updating progress for '{achievementName}' with new bitfield '{newCountOrBitfield}'");
                }
            );
        }
        else
        {
            AchievementsService.AddCount(achievementName, newCountOrBitfield, extraData).OnComplete(
                (Message<AchievementUpdate> msg) =>
                {
                    HandleAchievementUpdate(msg, $"updating progress for '{achievementName}' with new count '{newCountOrBitfield}'");
                }
            );
        }
    }

    public void UpdateCountAchievement(string achievementName, string playerToGrantAchievement = null, long newCount = 1, byte[] extraData = null)
    {
        // Check if the player for whom the achievement should be granted is specified
        if (playerToGrantAchievement == null)
        {
            // If not, use the display name of the current player from the platform service
            playerToGrantAchievement = PlatformServiceManager.Instance.Me.DisplayName;
        }

        // Check if the current player matches the player for whom the achievement should be granted
        if (PlatformServiceManager.Instance.Me.DisplayName != playerToGrantAchievement)
        {
            // Display a debug message in case of incorrect player
            Debug.Log($"Incorrect winning player. Achievement not incremented for: {playerToGrantAchievement}");
            // The line below was originally in a commented state, I'm leaving it as is for your reference
            return; // Return without performing further actions if the player is incorrect
        }

        // Add the new count to the specified achievement and perform actions when it's complete
        AchievementsService.AddCount(achievementName, newCount, extraData).OnComplete(
            (Message<AchievementUpdate> msg) =>
            {
                // Call a function to handle the achievement update
                HandleAchievementUpdate(msg, $"Updating Count progress for '{achievementName}' with new count '{newCount}'");
            }
        );
    }
    public void UpdateWinRoundsAchievement(string playerToGrantAchievement = null)
    {
        // Achievement : Win a round
        UpdateCountAchievement(CountAchievements.Bronze_RoundMaster.ToString(), playerToGrantAchievement);
        UpdateCountAchievement(CountAchievements.Silver_RoundMaster.ToString(), playerToGrantAchievement);
        UpdateCountAchievement(CountAchievements.Gold_RoundMaster.ToString(), playerToGrantAchievement);
        UpdateCountAchievement(CountAchievements.Platinum_RoundMaster.ToString(), playerToGrantAchievement);
    }

    public void UpdatePlayRoundAchievement()
    {
        // Achievement : Play a round
        UpdateCountAchievement(CountAchievements.Bronze_TenaciousRounder.ToString());
        UpdateCountAchievement(CountAchievements.Silver_TenaciousRounder.ToString());
        UpdateCountAchievement(CountAchievements.Gold_TenaciousRounder.ToString());
        UpdateCountAchievement(CountAchievements.Platinum_TenaciousRounder.ToString());
    }


    public void UpdateWinMatchAchievement(string playerToGrantAchievement = null)
    {
        // Achievement : Win a match
        UpdateCountAchievement(CountAchievements.Bronze_MatchDominator.ToString(), playerToGrantAchievement);
        UpdateCountAchievement(CountAchievements.Silver_MatchDominator.ToString(), playerToGrantAchievement);
        UpdateCountAchievement(CountAchievements.Gold_MatchDominator.ToString(), playerToGrantAchievement);
        UpdateCountAchievement(CountAchievements.Platinum_MatchDominator.ToString(), playerToGrantAchievement);
    }

    public void UpdatePlayMatchAchievement()
    {
        // Achievement : Play a match
        UpdateCountAchievement(CountAchievements.Bronze_Matchmaker.ToString());
        UpdateCountAchievement(CountAchievements.Silver_Matchmaker.ToString());
        UpdateCountAchievement(CountAchievements.Gold_Matchmaker.ToString());
        UpdateCountAchievement(CountAchievements.Platinum_Matchmaker.ToString());
    }

    public void IncrementCount(string achievementName)
    {
        long incrementAmount = 1;
        UpdateAchievementProgress(achievementName, incrementAmount, false);
    }

    public void ReloadAchievements()
    {
        ClearAchievementEntries();
        RequestAchievementDefinitions();
    }

    private void ClearAchievementEntries()
    {
        foreach (var entry in achievementEntries)
        {
            Destroy(entry);
        }

        foreach (Transform child in unlockedAchievementParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in lockedAchievementParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnToggleChanged(bool isOn, Transform panel)
    {
        panel.gameObject.SetActive(isOn);
        UpdateAchievementInfoText(isOn, panel);
    }

    private void UpdateAchievementInfoText(bool isPanelActive, Transform panel)
    {
        if (isPanelActive && panel.childCount == 0)
        {
            if (panel == lockedAchievementPanel)
            {
                _achievementInfoText.text = "No locked achievements.";
            }
            else if (panel == unlockedAchievementPanel)
            {
                _achievementInfoText.text = "No unlocked achievements.";
            }

            _achievementInfoText.gameObject.SetActive(true);
        }
        else
        {
            _achievementInfoText.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Error Handling

    private void HandleError(string message, Error error)
    {
        Debug.LogError(message + (error != null ? $": {error}" : ""));
        SetInfoText(message + (error != null ? $": {error}" : ""));
    }

    private void HandleAchievementUpdate(Message<AchievementUpdate> msg, string actionDescription)
    {
        if (!msg.IsError)
        {
            Debug.Log($"Achievement {actionDescription} successfully");
            SetInfoText($"Achievement {actionDescription} successfully");
        }
        else
        {
            HandleError($"Error {actionDescription} achievement", msg.Error);
        }
    }

    #endregion

    #region UI Utility

    public void SetInfoText(string text)
    {
        _achievementErrorInfo.text = text;
    }

    public void ClearInfoText()
    {
        _achievementErrorInfo.text = "";
    }

    #endregion

    public void ShowLockedAchievements()
    {
        lockedAchievementPanel.gameObject.SetActive(true);
        unlockedAchievementPanel.gameObject.SetActive(false);
    }

    public void ShowUnlockedAchievements()
    {
        lockedAchievementPanel.gameObject.SetActive(false);
        unlockedAchievementPanel.gameObject.SetActive(true);
    }
}
