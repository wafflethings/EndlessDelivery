namespace EndlessDelivery.Common.Communication.Scores;

public class SubmitScoreData
{
    public Score Score;
    public short Difficulty;

    public SubmitScoreData(Score score, short difficulty)
    {
        Score = score;
        Difficulty = difficulty;
    }
}
