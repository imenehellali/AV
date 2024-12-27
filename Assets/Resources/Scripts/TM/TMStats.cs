
using System.Collections.Generic;
using System.Linq;

public static class TMStats
{
    public static void ChosenPath(List<ThrillMinerManager.ChosenPath> _pathList)
    {
        int idx = 0; 

        foreach (var chosenPath in _pathList)
        {
            
            float percentageOfProgress = chosenPath.percentageOfProgress;
            float IncreaseOrDecreaseLearningFactorOverNodes = chosenPath.incDecOverNodes;
            string pathKey = string.Join("->", chosenPath.pathNodes.ConvertAll(n => $"{n.rp}({(n.isPassed ? "Passed" : "Failed")})"));

            bool fullyChosen = percentageOfProgress == 1.0f;
            bool partiallyChosen = percentageOfProgress > 0.0f && percentageOfProgress < 1.0f;
            bool reachedR4P4 = chosenPath.pathNodes.Any(node => node.rp == PathSetting.RP.R4P4 && node.isPassed);

            if (fullyChosen && reachedR4P4)
            {
                ParticipantSettings.Instance.TMDataPair.Invoke(new KeyValuePair<string, object>($"FullyChosenPath{idx}", pathKey));
                ParticipantSettings.Instance.TMDataPair.Invoke(new KeyValuePair<string, object>($"PercentageOfProgressPath{idx}", percentageOfProgress));
                ParticipantSettings.Instance.TMDataPair.Invoke(new KeyValuePair<string, object>($"IncreaseOrDecreaseLearningFactorPath{idx}", IncreaseOrDecreaseLearningFactorOverNodes));
            }
            else if (partiallyChosen && reachedR4P4)
            {
                ParticipantSettings.Instance.TMDataPair.Invoke(new KeyValuePair<string, object>($"DroppedPath{idx}", pathKey));
                ParticipantSettings.Instance.TMDataPair.Invoke(new KeyValuePair<string, object>($"PercentageOfProgressPath{idx}", percentageOfProgress));
                ParticipantSettings.Instance.TMDataPair.Invoke(new KeyValuePair<string, object>($"IncreaseOrDecreaseLearningFactorPath{idx}", IncreaseOrDecreaseLearningFactorOverNodes));
            }
            else
            {
                ParticipantSettings.Instance.TMDataPair.Invoke(new KeyValuePair<string, object>($"NotVisitedPath{idx}", pathKey));
            }

            idx++;
        }
    }

}
