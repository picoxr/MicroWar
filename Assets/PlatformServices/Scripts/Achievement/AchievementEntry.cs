using Pico.Platform;
using Pico.Platform.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class AchievementEntry : MonoBehaviour
{
    [SerializeField] private Image achievementImage;
    [SerializeField] private TextMeshProUGUI unlockStateText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countOrBitfieldText;
    [SerializeField] private Image rankBorder;


    public Slider sliderAchievement;

    private bool isUnlocked; // New boolean variable for unlock state

    public Image AchievementImage => achievementImage;
    public bool UnlockState => isUnlocked; // Return the boolean unlock state
    public string Name => nameText.text;
    public string CountOrBitfield => countOrBitfieldText.text;

    public void SetAchievementImage(Sprite image)
    {
        achievementImage.sprite = image;
    }

    public void SetUnlockState(bool state)
    {
        isUnlocked = state;

        if (state)
        {
            unlockStateText.text = "Unlocked";
            unlockStateText.color = Color.green; // Set text color to green for unlocked
        }
        else
        {
            unlockStateText.text = "Locked";
            unlockStateText.color = Color.red; // Set text color to red for locked
        }
    }

    public void SetName(string name)
    {
        nameText.text = name;
    }

    private void SetCountOrBitfieldText(string text)
    {
        countOrBitfieldText.text = text;
        SetCountOrBitfieldVisible(true); // Show the count/bitfield information

        // Update the Slider value based on the progress text
        UpdateSliderValue();
    }

    private void SetCountOrBitfieldVisible(bool visible)
    {
        countOrBitfieldText.gameObject.SetActive(visible);
    }


    private void UpdateSliderValue()
    {
        if (sliderAchievement != null)
        {
            string[] parts = CountOrBitfield.Split('/');
            if (parts.Length == 2 && float.TryParse(parts[0], out float currentValue) && float.TryParse(parts[1], out float maxValue) && maxValue > 0)
            {
                float percentage = currentValue / maxValue;
                sliderAchievement.value = percentage;
            }
            else
            {
                // Something went wrong, log an error or warning
                Debug.LogError("Failed to calculate slider value: Invalid data or division by zero.");
            }
        }
    }

    public void SetAchievementData(AchievementProgress progress, AchievementDefinition definition, Sprite lockedSprite, Sprite unlockedSprite)
    {
        SetAchievementImage(progress.IsUnlocked ? unlockedSprite : lockedSprite);
        SetUnlockState(progress.IsUnlocked);

        // Extract the rank from the achievement string
        string rankString = ExtractRankFromString(definition.Title, out string achievementName);

        // Use the new function to set the formatted name and color
        SetFormattedNameAndColor(achievementName, rankString);

        if (definition.Type == AchievementType.Simple)
        {
            SetCountOrBitfieldVisible(false);
            if (progress.IsUnlocked && sliderAchievement != null)
            {
                sliderAchievement.value = 1;
            }
        }
        else
        {
            SetCountOrBitfieldVisible(true);
            string countText = $"{progress.Count}/{definition.Target}";
            SetCountOrBitfieldText(countText);
        }
    }

    // Function to extract rank and name from the achievement string
    private string ExtractRankFromString(string achievementString, out string achievementName)
    {
        int colonIndex = achievementString.IndexOf(":");
        if (colonIndex >= 0)
        {
            string rankString = achievementString.Substring(0, colonIndex).Trim();
            achievementName = achievementString.Substring(colonIndex + 1).Trim();
            return rankString;
        }

        achievementName = "";
        return "";
    }

    // Function to set the formatted name and color
    private void SetFormattedNameAndColor(string achievementName, string rankString)
    {
        string colorCode = GetColorCodeForRank(rankString);
        string formattedRank = $"<color={colorCode}>{rankString}</color>";
        string formattedName = $"{achievementName} \n {formattedRank}";
        SetName(formattedName);

        // Set the color of the rankBorder image component
        rankBorder.color = ColorUtility.TryParseHtmlString(colorCode, out Color rankColor) ? rankColor : Color.black;
    }

    // Function to get the color code based on rank string
    private string GetColorCodeForRank(string rankString)
    {
        switch (rankString)
        {
            case "Bronze":
                return "#A97142"; // Color code for bronze
            case "Silver":
                return "#C0C0C0"; // Color code for silver
            case "Gold":
                return "#FFD700"; // Color code for gold
            case "Platinum":
                return "#00FFFF"; // Color code for platinum
            default:
                return "#FFFFFF"; // Default color code (white)
        }
    }
}
