using Newtonsoft.Json;

namespace EndlessDelivery.Common;

public class Score
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="score">The score that you're checking; true if this is larger.</param>
    /// <param name="other">The score being compared to; false if this is larger.</param>
    /// <returns></returns>
    private static bool IsLargerThanOtherScore(Score score, Score other)
    {
        if (score.Rooms != other.Rooms)
        {
            if (score.Rooms > other.Rooms)
            {
                return true;
            }

            return false;
        }

        if (score.Deliveries != other.Deliveries)
        {
            if (score.Deliveries > other.Deliveries)
            {
                return true;
            }

            return false;
        }

        if (score.Kills != other.Kills)
        {
            if (score.Kills > other.Kills)
            {
                return true;
            }

            return false;
        }

        if (score.Time != other.Time)
        {
            if (score.Time < other.Time)
            {
                return true;
            }

            return false;
        }

        return false;
    }

    public readonly int Rooms;
    public readonly int Kills;
    public readonly int Deliveries;
    public readonly float Time;

    public Score(int rooms, int kills, int deliveries, float time)
    {
        Rooms = rooms;
        Kills = kills;
        Deliveries = deliveries;
        Time = time;
    }

    public static Score operator +(Score a, Score b) => new(a.Rooms + b.Rooms, a.Kills + b.Kills, a.Deliveries + b.Deliveries, a.Time + b.Time);
    public static Score operator -(Score a, Score b) => new(a.Rooms - b.Rooms, a.Kills - b.Kills, a.Deliveries - b.Deliveries, a.Time - b.Time);
    public static bool operator >(Score a, Score b) => IsLargerThanOtherScore(a, b);
    public static bool operator <(Score a, Score b) => IsLargerThanOtherScore(b, a);

    [JsonIgnore] public int MoneyGain => (int)(Time * 10);
}
