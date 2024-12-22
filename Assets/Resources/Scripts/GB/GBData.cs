using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class GBData : MonoBehaviour
{
    public static GBData Data { get; private set; }

    private void Awake()
    {
        if (Data == null)
        {
            Data = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    public void SaveData()
    {
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgReactionTimeToBustAnyGhost", GBStats.GetAvgReactionTimeAnyGhost(1)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgReactionTimeToBustCorrectGhost", GBStats.GetAvgReactionTimeCorrectGhost(1)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgReactionTimeToBustWrongGhost", GBStats.GetAvgReactionTimeWrongGhost(1)));

        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgTimeGazeOnCorrectGhost", GBStats.GetAvgGazeTimeCorrectGhost(1)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgTimeGazeOnWrongGhost ", GBStats.GetAvgGazeTimeWrongGhost(1)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgNumberCorrectBustedGhosts", GBStats.GetCorrectGhostBustingRate(1)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgNumberWrongBusterGhosts ", GBStats.GetWrongGhostBustingRate(1)));

        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgReactionTimeToBustAnyGhost ", GBStats.GetAvgReactionTimeAnyGhost(2)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgReactionTimeToBustCorrectGhost ", GBStats.GetAvgReactionTimeCorrectGhost(2)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgReactionTimeToBustWrongGhost ", GBStats.GetAvgReactionTimeWrongGhost(2)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgTimeGazeOnCorrectGhost ", GBStats.GetAvgGazeTimeCorrectGhost(2)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgTimeGazeOnWrongGhost ", GBStats.GetAvgGazeTimeWrongGhost(2)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgNumberCorrectBustedGhosts ", GBStats.GetCorrectGhostBustingRate(2)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q1AvgNumberWrongBusterGhosts ", GBStats.GetWrongGhostBustingRate(2)));

        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q12AvgReactionTimeToBustAnyGhost", GBStats.GetAvg12("reactionAny")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q12AvgReactionTimeToBustCorrectGhost", GBStats.GetAvg12("reactionCorrect")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q12AvgReactionTimeToBustWrongGhost", GBStats.GetAvg12("reactionWrong")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q12AvgTimeGazeOnCorrectGhost ", GBStats.GetAvg12("gazeCorrect")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q12AvgTimeGazeOnWrongGhost ", GBStats.GetAvg12("gazeWrong")));


        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q3AvgTimeGazeOnCorrectGhost ", GBStats.GetAvgGazeTimeCorrectGhost(3)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q3AvgTimeGazeOnWrongGhost ", GBStats.GetAvgGazeTimeWrongGhost(3)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("incDecMarginReactionTimeAnyQ3To21 ", GBStats.GetincDecMarginQ3To21("reactionAny")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("incDecMarginReactionTimeCorrectQ3To21", GBStats.GetincDecMarginQ3To21("reactionCorrect")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("incDecMarginReactionTimeWrongQ3To21", GBStats.GetincDecMarginQ3To21("reactionWrong")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("incDecMarginNumberBustAnyQ3To21", GBStats.GetincDecMarginQ3To21("NumAny")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("incDecMarginNumberBustCorrectQ3To21 ", GBStats.GetincDecMarginQ3To21("NumCorrect")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("incDecMarginNumberBustWrongQ3To21", GBStats.GetincDecMarginQ3To21("NumWrong")));

        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q4AvgTimeGazeOnCorrectGhost", GBStats.GetAvgGazeTimeCorrectGhost(3)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("q4AvgTimeGazeOnWrongGhost ", GBStats.GetAvgGazeTimeWrongGhost(3)));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("incDecMarginReactionTimeAnyQ4To3", GBStats.GetincDecMarginQ4To3("reactionAny")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("incDecMarginReactionTimeCorrectQ4To3", GBStats.GetincDecMarginQ4To3("reactionCorrect")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("incDecMarginReactionTimeWrongQ4To3", GBStats.GetincDecMarginQ4To3("reactionWrong")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("incDecMarginNumberBustAnyQ4To3", GBStats.GetincDecMarginQ4To3("NumAny")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("incDecMarginNumberBustCorrectQ4To3", GBStats.GetincDecMarginQ4To3("NumCorrect")));
        ParticipantSettings.Instance.GBDataPair.Invoke(new KeyValuePair<string, float>("incDecMarginNumberBustWrongQ4To3", GBStats.GetincDecMarginQ4To3("NumWrong")));

    }

}
