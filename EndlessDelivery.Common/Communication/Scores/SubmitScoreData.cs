namespace EndlessDelivery.Common.Communication.Scores;

public class SubmitScoreData
{
    public Score Score;
    public short Difficulty;
    public string Version;

    public SubmitScoreData(Score score, short difficulty, string version)
    {
        Score = score;
        Difficulty = difficulty;
        Version = version;
    }
}
