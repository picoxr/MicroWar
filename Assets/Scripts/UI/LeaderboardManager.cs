using Pico.Platform.Models;
using Pico.Platform;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LeaderboardManager : MonoBehaviour
{
    private const string leaderboardName = "leaderboardMicrowar";
    private const long DefaultIncrementScore = 3;

    // Singleton instance
    public static LeaderboardManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI leaderboardTitleText;
    [SerializeField] private TextMeshProUGUI noEntriesText;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Transform leaderboardEntryParent;
    [SerializeField] private LeaderboardEntry loggedInUserLeaderboardEntry;

    private User loggedInUser;
    private List<GameObject> leaderboardEntries = new List<GameObject>();

    [Header("Toggle Elements")]
    [SerializeField] private Toggle toggleGlobal;
    [SerializeField] private Toggle toggleFriends;

    private void Awake()
    {
        // Set the singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize leaderboard
        ClearLeaderboard();

        // Get the logged-in user and set the username
        UserService.GetLoggedInUser().OnComplete(HandleLoggedInUserResponse);

        // Hide the no entries text
        noEntriesText.gameObject.SetActive(false);
    }

    #region Public Methods

    public void SetUsername(string username)
    {
        usernameText.text = username;

        if (loggedInUserLeaderboardEntry != null)
        {
            loggedInUserLeaderboardEntry.SetDisplayName(username);
        }
    }

    public void SetInfoText(string text)
    {
        infoText.text = text;
    }

    public void SetLeaderboardTitle(string title)
    {
        leaderboardTitleText.text = title;
    }

    public void AddScoreEntry(string winningPlayer = null, long incrementScore = DefaultIncrementScore)
    {
        if (loggedInUser.DisplayName != winningPlayer)
        {
            Debug.Log($"Incorrect winning player. Score not incremented for: {winningPlayer}");
            return;
        }

        Debug.Log($"## [Debug] AddScoreEntry with value: {incrementScore} for player: {winningPlayer}");

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
                        Debug.Log("Entry for the logged-in user not found. Score not incremented.");
                        WriteEntryInLeaderboard(1);
                    }
                }
                else
                {
                    var errorText = $"<color=red>Error receiving leaderboard entries:</color> {message.GetError().Message}";
                    Debug.Log(errorText);
                }
            });
    }

    public void LoadGlobal()
    {
        SetLeaderboardTitle("Global");
        ClearLeaderboard();

        // Load global leaderboard entries
        LoadEntries(LeaderboardFilterType.None, LeaderboardStartAt.Top);
    }

    public void LoadFriends()
    {
        SetLeaderboardTitle("Friends");
        ClearLeaderboard();

        // Load leaderboard entries filtered by friends
        LoadEntries(LeaderboardFilterType.Friends, LeaderboardStartAt.Top);
    }

    public void LoadTop10()
    {
        SetLeaderboardTitle("Top 10");
        ClearLeaderboard();

        // Load top 10 leaderboard entries
        LoadEntries(LeaderboardFilterType.None, LeaderboardStartAt.Top);
    }

    public void ReloadLeaderboard()
    {
        if (toggleFriends.isOn)
        {
            LoadFriends();
        }
        else if (toggleGlobal.isOn)
        {
            LoadGlobal();
        }
        else
        {
            // No toggle is active, default to global
            LoadGlobal();
        }
    }

    #endregion

    #region Private Methods

    private void WriteEntryInLeaderboard(long score)
    {
        LeaderboardService.WriteEntry(leaderboardName, score)
            .OnComplete(message =>
            {
                if (!message.IsError)
                {
                    Debug.Log($"## [Debug] Score incremented!\nPlayer current score: {score}");
                    ReloadLeaderboard();
                }
                else
                {
                    var error = message.GetError();
                    Debug.Log($"## [Debug] Entry NOT added! Error: {error.Message}");
                }
            });
    }

    private void LoadEntries(LeaderboardFilterType filterType, LeaderboardStartAt startAt)
    {
        // Retrieve leaderboard entries
        LeaderboardService.GetEntries(leaderboardName, 10, 0, filterType, startAt)
            .OnComplete(HandleGetEntriesResponse);
    }

    private void HandleLoggedInUserResponse(Message<User> message)
    {
        if (!message.IsError)
        {
            loggedInUser = message.Data;
            SetUsername(loggedInUser.DisplayName);
        }
        else
        {
            SetUsername("Error");

            var error = message.GetError();
            var errorText = $"Error: {error.Code}\n{error.Message}";
            SetInfoText(errorText);
        }
    }

    private void HandleGetEntriesResponse(Message<LeaderboardEntryList> message)
    {
        if (!message.IsError)
        {
            // Check if the leaderboard has any entries
            if (message.Data.Count == 0)
            {
                // Show the no entries text
                noEntriesText.gameObject.SetActive(true);
            }
            else
            {
                // Hide the no entries text
                noEntriesText.gameObject.SetActive(false);

                // Clear the previous entries
                ClearLeaderboard();

                foreach (var entryData in message.Data)
                {
                    // Instantiate the leaderboard entry prefab and add it as a child of the parent
                    var entryObject = Instantiate(leaderboardEntryPrefab, leaderboardEntryParent);
                    leaderboardEntries.Add(entryObject);

                    // Get the leaderboard entry component from the prefab
                    var leaderboardEntry = entryObject.GetComponent<LeaderboardEntry>();

                    // Set the values of the components in the leaderboard entry
                    leaderboardEntry.SetRank(entryData.Rank);
                    leaderboardEntry.SetDisplayName(entryData.User.DisplayName);
                    leaderboardEntry.SetScore(entryData.Score);

                    // Check if the current player's entry is found
                    if (entryData.User.DisplayName == loggedInUser.DisplayName)
                    {
                        // Update all values for the logged-in user's entry
                        loggedInUserLeaderboardEntry.SetRank(entryData.Rank);
                        loggedInUserLeaderboardEntry.SetDisplayName(entryData.User.DisplayName);
                        loggedInUserLeaderboardEntry.SetScore(entryData.Score);
                        loggedInUserLeaderboardEntry.SetTrophyColor(leaderboardEntry.GetTrophyColor(entryData.Rank));
                    }
                }
            }
        }
        else
        {
            var errorText = $"<color=red>Error receiving leaderboard entries:</color> {message.GetError().Message}";
            SetInfoText(errorText);
        }
    }

    private void ClearLeaderboard()
    {
        // Destroy all previous entries and clear the list
        foreach (var entryObject in leaderboardEntries)
        {
            Destroy(entryObject);
        }
        leaderboardEntries.Clear();
    }

    #endregion
}
