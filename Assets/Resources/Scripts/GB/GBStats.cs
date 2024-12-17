using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class GBStats 
{
    // Quest 1 Variables
    private static float q1CorrectBustedGhosts = 0f;
    private static float q1WrongBustedGhosts = 0f;
    private static List<float> q1CorrectGhostReactionTimes = new List<float>();
    private static List<float> q1WrongGhostReactionTimes = new List<float>();
    private static List<float> q1GazeTimesCorrectGhosts = new List<float>();
    private static List<float> q1GazeTimesWrongGhosts = new List<float>();
    private static int q1TotalGhosts =0;  //To fill from random incr

    // Quest 2 Variables
    private static float q2CorrectBustedGhosts = 0f;
    private static float q2WrongBustedGhosts = 0f;
    private static List<float> q2CorrectGhostReactionTimes = new List<float>();
    private static List<float> q2WrongGhostReactionTimes = new List<float>();
    private static List<float> q2GazeTimesCorrectGhosts = new List<float>();
    private static List<float> q2GazeTimesWrongGhosts = new List<float>();
    private static int q2TotalGhosts = 0;

    // Quest 3 Variables
    private static float q3CorrectBustedGhosts = 0f;
    private static float q3WrongBustedGhosts = 0f;
    private static List<float> q3CorrectGhostReactionTimes = new List<float>();
    private static List<float> q3WrongGhostReactionTimes = new List<float>();
    private static List<float> q3GazeTimesCorrectGhosts = new List<float>();
    private static List<float> q3GazeTimesWrongGhosts = new List<float>();
    private static int q3TotalGhosts = 0;

    // Quest 4 Variables
    private static float q4CorrectBustedGhosts = 0f;
    private static float q4WrongBustedGhosts = 0f;
    private static List<float> q4CorrectGhostReactionTimes = new List<float>();
    private static List<float> q4WrongGhostReactionTimes = new List<float>();
    private static List<float> q4GazeTimesCorrectGhosts = new List<float>();
    private static List<float> q4GazeTimesWrongGhosts = new List<float>();
    private static int q4TotalGhosts = 0;


    // Quest Methods

    public static void IncQTotalGhost(int quest)
    {
        switch (quest)
            {
            case 1:
                q1TotalGhosts++;
                break;
            case 2:
                q2TotalGhosts++;
                break;
            case 3:
                q3TotalGhosts++;
                break;
            case 4:
                q4TotalGhosts++;
                break;

        }
    }
    public static void AddCorrectGhostBusted(float quest, float time)
    {
        if (quest == 1)
        {
            q1CorrectBustedGhosts++;
            q1CorrectGhostReactionTimes.Add(time);
        }
        else if (quest == 2)
        {
            q2CorrectBustedGhosts++;
            q2CorrectGhostReactionTimes.Add(time);
        }
        else if (quest == 3)
        {
            q3CorrectBustedGhosts++;
            q3CorrectGhostReactionTimes.Add(time);
        }
        else if (quest == 4)
        {
            q4CorrectBustedGhosts++;
            q4CorrectGhostReactionTimes.Add(time);
        }
    }

    public static void AddWrongGhostBusted(float quest, float time)
    {
        if (quest == 1)
        {
            q1WrongBustedGhosts++;
            q1WrongGhostReactionTimes.Add(time);
        }
        else if (quest == 2)
        {
            q2WrongBustedGhosts++;
            q2WrongGhostReactionTimes.Add(time);
        }
        else if (quest == 3)
        {
            q3WrongBustedGhosts++;
            q3WrongGhostReactionTimes.Add(time);
        }
        else if (quest == 4)
        {
            q4WrongBustedGhosts++;
            q4WrongGhostReactionTimes.Add(time);
        }
    }

    public static void AddGazeTimeCorrectGhost(float quest, float gazeTime)
    {
        if (quest == 1)
            q1GazeTimesCorrectGhosts.Add(gazeTime);
        else if (quest == 2)
            q2GazeTimesCorrectGhosts.Add(gazeTime);
        else if (quest == 3)
            q3GazeTimesCorrectGhosts.Add(gazeTime);
        else if (quest == 4)
            q4GazeTimesCorrectGhosts.Add(gazeTime);
    }

    public static void AddGazeTimeWrongGhost(float quest, float gazeTime)
    {
        if (quest == 1)
            q1GazeTimesWrongGhosts.Add(gazeTime);
        else if (quest == 2)
            q2GazeTimesWrongGhosts.Add(gazeTime);
        else if (quest == 3)
            q3GazeTimesWrongGhosts.Add(gazeTime);
        else if (quest == 4)
            q4GazeTimesWrongGhosts.Add(gazeTime);
    }




    // Get Stats Methods
    public static float GetCorrectGhostBustingRate(float quest)
    {
        if (quest == 1)
            return q1CorrectBustedGhosts / q1TotalGhosts;
        if (quest == 2)
            return q2CorrectBustedGhosts / q2TotalGhosts;
        if (quest == 3)
            return q3CorrectBustedGhosts / q3TotalGhosts;
        if (quest == 4)
            return q4CorrectBustedGhosts / q4TotalGhosts;
        return 0;
    }

    public static float GetAnyGhostBustingRate(float quest)
    {
        if (quest == 1)
            return (q1WrongBustedGhosts + q1CorrectBustedGhosts) / q1TotalGhosts;
        if (quest == 2)
            return (q2WrongBustedGhosts + q2CorrectBustedGhosts) / q2TotalGhosts;
        if (quest == 3)
            return (q3WrongBustedGhosts + q3CorrectBustedGhosts) / q3TotalGhosts;
        if (quest == 4)
            return (q4WrongBustedGhosts + q4CorrectBustedGhosts) / q4TotalGhosts;
        return 0;
    }

    public static float GetWrongGhostBustingRate(int quest)
    {
        if (quest == 1)
            return q1WrongBustedGhosts / q1TotalGhosts;
        if (quest == 2)
            return q2WrongBustedGhosts / q2TotalGhosts;
        if (quest == 3)
            return q3WrongBustedGhosts / q3TotalGhosts;
        if (quest == 4)
            return q4WrongBustedGhosts / q4TotalGhosts;
        return 0;
    }

    public static float GetAvgReactionTimeCorrectGhost(int quest)
    {
        if (quest == 1)
            return q1CorrectGhostReactionTimes.Average();
        if (quest == 2)
            return q2CorrectGhostReactionTimes.Average();
        if (quest == 3)
            return q3CorrectGhostReactionTimes.Average();
        if (quest == 4)
            return q4CorrectGhostReactionTimes.Average();
        return 0;
    }

    public static float GetAvgReactionTimeWrongGhost(int quest)
    {
        if (quest == 1)
            return q1WrongGhostReactionTimes.Average();
        if (quest == 2)
            return q2WrongGhostReactionTimes.Average();
        if (quest == 3)
            return q3WrongGhostReactionTimes.Average();
        if (quest == 4)
            return q4WrongGhostReactionTimes.Average();
        return 0;
    }
    public static float GetAvgReactionTimeAnyGhost(int quest)
    {
        if (quest == 1)
            return (q1CorrectGhostReactionTimes.Concat(q1WrongGhostReactionTimes)).Average();
        if (quest == 2)
            return (q2CorrectGhostReactionTimes.Concat(q2WrongGhostReactionTimes)).Average();
        if (quest == 3)
            return (q3CorrectGhostReactionTimes.Concat(q3WrongGhostReactionTimes)).Average();
        if (quest == 4)
            return (q4CorrectGhostReactionTimes.Concat(q4WrongGhostReactionTimes)).Average();
        return 0;
    }

    
    public static float GetAvgGazeTimeCorrectGhost(int quest)
    {
        if (quest == 1)
            return q1GazeTimesCorrectGhosts.Average();
        if (quest == 2)
            return q2GazeTimesCorrectGhosts.Average();
        if (quest == 3)
            return q3GazeTimesCorrectGhosts.Average();
        if (quest == 4)
            return q4GazeTimesCorrectGhosts.Average();
        return 0;
    }

    public static float GetAvgGazeTimeWrongGhost(int quest)
    {
        if (quest == 1)
            return q1GazeTimesWrongGhosts.Average();
        if (quest == 2)
            return q2GazeTimesWrongGhosts.Average();
        if (quest == 3)
            return q3GazeTimesWrongGhosts.Average();
        if (quest == 4)
            return q4GazeTimesWrongGhosts.Average();
        return 0;
    }

    public static float GetAvg12(string measurement)
    {
        switch (measurement)
        {
            case "reactionAny":
                return (GetAvgReactionTimeAnyGhost(1) + GetAvgReactionTimeAnyGhost(2)) / 2;

            case "reactionCorrect":
                return (GetAvgReactionTimeCorrectGhost(1) + GetAvgReactionTimeCorrectGhost(2)) / 2;

            case "reactionWrong":
                return (GetAvgReactionTimeWrongGhost(1) + GetAvgReactionTimeWrongGhost(2)) / 2;

            case "gazeCorrect":
                return (GetAvgGazeTimeCorrectGhost(1) + GetAvgGazeTimeCorrectGhost(2)) / 2;

            case "gazeWrong":
                return (GetAvgGazeTimeWrongGhost(1) + GetAvgGazeTimeWrongGhost(2)) / 2;

            case "NumAny":
                return (GetAnyGhostBustingRate(1) + GetAnyGhostBustingRate(2)) / 2;

            case "NumCorrect":
                return (GetCorrectGhostBustingRate(1) + GetCorrectGhostBustingRate(2)) / 2;

            case "NumWrong":
                return (GetWrongGhostBustingRate(1) + GetWrongGhostBustingRate(2)) / 2;
        }

        return 0f;
    }

    
    public static float GetincDecMarginQ3To21(string measurement)
    {
        switch (measurement)
        {
            case "reactionAny":
                return GetAvgReactionTimeAnyGhost(3) - GetAvg12("reactionAny");

            case "reactionCorrect":
                return GetAvgReactionTimeCorrectGhost(3) - GetAvg12("reactionCorrect");

            case "reactionWrong":
                return GetAvgReactionTimeCorrectGhost(3) - GetAvg12("reactionWrong");

            case "NumAny":
                return GetAnyGhostBustingRate(3) - GetAvg12("NumAny");

            case "NumCorrect":
                return GetCorrectGhostBustingRate(3) - GetAvg12("NumCorrect");

            case "NumWrong":
                return GetWrongGhostBustingRate(3) - GetAvg12("NumWrong");
        }

        return 0f;
    }

    public static float GetincDecMarginQ4To3(string measurement)
    {
        switch (measurement)
        {
            case "reactionAny":
                return GetAvgReactionTimeAnyGhost(4) - GetAvgReactionTimeAnyGhost(3);

            case "reactionCorrect":
                return GetAvgReactionTimeCorrectGhost(4) - GetAvgReactionTimeCorrectGhost(3);

            case "reactionWrong":
                return GetAvgReactionTimeCorrectGhost(4) - GetAvgReactionTimeCorrectGhost(3);

            case "NumAny":
                return GetAnyGhostBustingRate(4) - GetAnyGhostBustingRate(3);

            case "NumCorrect":
                return GetCorrectGhostBustingRate(4) - GetCorrectGhostBustingRate(3);

            case "NumWrong":
                return GetWrongGhostBustingRate(4) - GetWrongGhostBustingRate(3);
        }

        return 0f;
    }
}

