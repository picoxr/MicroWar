using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntry : MonoBehaviour
{
    [SerializeField] private Image trophyImage;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private TextMeshProUGUI scoreText;

    // Private fields to store leaderboard entry data
    private int rank;
    private string displayName;
    private long score;

    // Define constant colors for trophy types
    private static readonly Color GOLD_COLOR = new Color(1f, 0.84f, 0f);
    private static readonly Color SILVER_COLOR = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color BRONZE_COLOR = new Color(0.8f, 0.5f, 0.2f);
    private static readonly Color DEFAULT_COLOR = Color.white;

    public int GetRank()
    {
        return rank;
    }

    public string GetDisplayName()
    {
        return displayName;
    }

    public long GetScore()
    {
        return score;
    }

    public void SetRank(int rank)
    {
        this.rank = rank;
        rankText.text = rank.ToString();
        trophyImage.color = GetTrophyColor(rank);
    }

    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
        displayNameText.text = displayName;
    }

    public void SetScore(long score)
    {
        this.score = score;
        scoreText.text = score.ToString();
    }

    public void SetTrophyOpacity(float opacity)
    {
        var color = trophyImage.color;
        color.a = opacity;
        trophyImage.color = color;
    }

    public void SetTrophyColor(Color color)
    {
        trophyImage.color = color;
    }

    // Determine trophy color based on rank
    public Color GetTrophyColor(int rank)
    {
        // Return the appropriate color based on the rank
        switch (rank)
        {
            case 1:
                SetTrophyOpacity(1f);
                return GOLD_COLOR;
            case 2:
                SetTrophyOpacity(1f);
                return SILVER_COLOR;
            case 3:
                SetTrophyOpacity(1f);
                return BRONZE_COLOR;
            default:
                SetTrophyOpacity(0f);
                return DEFAULT_COLOR;
        }
    }
}
