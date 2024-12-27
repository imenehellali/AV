using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public static class LSStats 
{

    private static void CalculateCasesData(Case CH1, Case CH2, Case CH3, Case CH4, Case CA1, Case CA2, Case CA3, Case CA4)
    {

        if (!CH4.IsUnityNull() && CH4.startedAssigning)
        {
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("ProgressOfSavingWithinCH4", CH4.percentageDone));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("TimeSpentOnCH4", CH4.SpenTimeOnCase()));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CH4DiscoveredAndSaved?", CH4.healed ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CH4DiscoveredAndKilled?", CH4.dead ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CH4WatchedVideo", CH4.watchedVid ? 1f : 0f));
        }
        if (!CH3.IsUnityNull() && CH3.startedAssigning)
        {
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("ProgressOfSavingWithinCH3", CH3.percentageDone));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("TimeSpentOnCH3", CH3.SpenTimeOnCase()));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CH3DiscoveredAndSaved?", CH3.healed ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CH3DiscoveredAndKilled?", CH3.dead ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CH32WatchedVideo", CH3.watchedVid ? 1f : 0f));
        }
        if (!CH2.IsUnityNull() && CH2.startedAssigning)
        {
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("ProgressOfSavingWithinCH2", CH2.percentageDone));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("TimeSpentOnCH2", CH2.SpenTimeOnCase()));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CH2DiscoveredAndSaved?", CH2.healed ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CH2DiscoveredAndKilled?", CH2.dead ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CH2WatchedVideo", CH2.watchedVid ? 1f : 0f));
        }
        if (!CH1.IsUnityNull() && CH1.startedAssigning)
        {
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("ProgressOfSavingWithinCH1", CH1.percentageDone));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("TimeSpentOnCH1", CH1.SpenTimeOnCase()));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CH1DiscoveredAndSaved?", CH1.healed ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CH1DiscoveredAndKilled?", CH1.dead ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CH1WatchedVideo", CH1.watchedVid ? 1f : 0f));
        }


        if (!CA4.IsUnityNull() && CA4.startedAssigning)
        {
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("ProgressOfSavingWithinCA4", CA4.percentageDone));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("TimeSpentOnCA4", CA4.SpenTimeOnCase()));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CA4DiscoveredAndSaved?", CA4.healed ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CA4DiscoveredAndKilled?", CA4.dead ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CA4WatchedVideo", CA4.watchedVid ? 1f : 0f));
        }
        if (!CA3.IsUnityNull() && CA3.startedAssigning)
        {
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("ProgressOfSavingWithinCA3", CA3.percentageDone));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("TimeSpentOnCA3", CA3.SpenTimeOnCase()));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CA3DiscoveredAndSaved?", CA3.healed ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CA3DiscoveredAndKilled?", CA3.dead ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CA3WatchedVideo", CA3.watchedVid ? 1f : 0f));
        }
        if (!CA2.IsUnityNull() && CA2.startedAssigning)
        {
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("ProgressOfSavingWithinCA2", CA2.percentageDone));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("TimeSpentOnCA2", CA2.SpenTimeOnCase()));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CA2DiscoveredAndSaved?", CA2.healed ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CA2DiscoveredAndKilled?", CA2.dead ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CA2WatchedVideo", CA2.watchedVid ? 1f : 0f));
        }
        if (!CA1.IsUnityNull() && CA1.startedAssigning)
        {
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("ProgressOfSavingWithinCA1", CA1.percentageDone));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("TimeSpentOnCA1", CA1.SpenTimeOnCase()));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CA1DiscoveredAndSaved?", CA1.healed ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CA1DiscoveredAndKilled?", CA1.dead ? 1f : 0f));
            ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("CA1WatchedVideo", CA1.watchedVid ? 1f : 0f));
        }

        ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("ProgressOfSavingWithinAStrategy", avgOfSet(CH1!, CH2, CH3, CH4, CA1, CA2, CA3, CA4)));
    }
    public static async Task<KeyValuePair<string, string>> FollowedStrategy(Dictionary<string, Case> VC)  
    {
        var strategy = new StringBuilder();
        Case CH4; VC.TryGetValue("CH4", out CH4);
        Case CH3; VC.TryGetValue("CH4", out CH3);
        Case CH2; VC.TryGetValue("CH4", out CH2);
        Case CH1; VC.TryGetValue("CH4", out CH1);

        Case CA4; VC.TryGetValue("CH4", out CA4);
        Case CA3; VC.TryGetValue("CH4", out CA3);
        Case CA2; VC.TryGetValue("CH4", out CA2);
        Case CA1; VC.TryGetValue("CH4", out CA1);

        await Task.Run(() => CalculateCasesData(CH1, CH2, CH3, CH4, CA1, CA2, CA3, CA4));

        float avgHuman = avg(CH1, CH2, CH3, CH4);
        ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("AvgOfProgressOfHumanCases", avgHuman));
        float avgAnimal = avg(CA1, CA2, CA3, CA4);
        ParticipantSettings.Instance.LSDataPair.Invoke(new KeyValuePair<string, object>("AvgOfProgressOfAnimalCases", avgAnimal));

        //Figuring out if they included optimizing on resources in general

        if (((!CH4.IsUnityNull() && CH4.startedAssigning) &&
            (!CA4.IsUnityNull() && CA4.startedAssigning)) &&
                (((!CA3.IsUnityNull() && CA3.startedAssigning) && (!CA2.IsUnityNull() && CA2.startedAssigning) &&
                    ((!CH1.IsUnityNull() && CH1.startedAssigning) ||
                    (!CH2.IsUnityNull() && CH2.startedAssigning) ||
                    (!CH3.IsUnityNull() && CH3.startedAssigning))) ||
                ((!CA1.IsUnityNull() && CA1.startedAssigning) &&
                    ((!CA3.IsUnityNull() && CA3.startedAssigning) ||
                    (!CA2.IsUnityNull() && CA2.startedAssigning))) ||
                ((!CH3.IsUnityNull() && CH3.startedAssigning) && (!CH2.IsUnityNull() && CH2.startedAssigning) &&
                    ((!CA2.IsUnityNull() && CA2.startedAssigning) ||
                    (!CA3.IsUnityNull() && CA3.startedAssigning))))
            )
        {
            strategy.AppendLine("Individual optimized the allocation of resources to cover most of cases.");
        }

        //figuring out if they also included optimizing on times either way--> most or least

        if (((!CH4.IsUnityNull() && CH4.startedAssigning) &&
            (!CA4.IsUnityNull() && CA4.startedAssigning)) &&
                (((!CH1.IsUnityNull() && CH1.startedAssigning) &&
                    ((!CH2.IsUnityNull() && CH2.startedAssigning) ||
                    (!CH3.IsUnityNull() && CH3.startedAssigning))) ||
                 ((!CA3.IsUnityNull() && CA3.startedAssigning) && (!CA2.IsUnityNull() && CA2.startedAssigning) && (!CH1.IsUnityNull() && CH1.startedAssigning)) ||
                 ((!CA1.IsUnityNull() && CA1.startedAssigning) &&
                    ((!CA3.IsUnityNull() && CA3.startedAssigning) ||
                    (!CA2.IsUnityNull() && CA2.startedAssigning))))
            )
        {
            strategy.AppendLine("Individual generally maximized cases with least time to survive.");
        }
        else if (((!CH4.IsUnityNull() && CH4.startedAssigning) &&
            (!CA4.IsUnityNull() && CA4.startedAssigning)) &&
            (((!CH3.IsUnityNull() && CH3.startedAssigning) && (!CH2.IsUnityNull() && CH2.startedAssigning) &&
                    ((!CA3.IsUnityNull() && CA3.startedAssigning) ||
                    (!CA2.IsUnityNull() && CA2.startedAssigning))) ||
                ((!CA3.IsUnityNull() && CA3.startedAssigning) && (!CA2.IsUnityNull() && CA2.startedAssigning) &&
                    ((!CH3.IsUnityNull() && CH3.startedAssigning) ||
                    (!CH2.IsUnityNull() && CH2.startedAssigning))))
            )
        {
            strategy.AppendLine(" Individual generally maximized cases with longest time to survive.");
        }
        else
        {
            strategy.AppendLine("Unstructured! Individual did not optimize the allocation of resources to cover most of cases.");
            strategy.AppendLine("Individual did not optimize on longest time to survive of cases.");
            strategy.AppendLine("Individual did not optimize on least time to survive of cases.");
        }

        //figuring out from the set if they preferred --> human or animal 
        if (avgAnimal > avgHuman)
        {
            strategy.AppendLine("Individual generally prioritzed Animal cases over Human cases.");
        }
        else if (avgHuman > avgAnimal)
        {

            strategy.AppendLine("Individual generally prioritzed Human cases over Animals cases.");
        }
        else
        {
            strategy.AppendLine("Individual did not distinctinguish nor prefer Animal cases from human cases.");
        }

        //figuring out from the set if they prefered --> critical humans with less critical animal or critical animals with less critical humans
        if (((!CH4.IsUnityNull() && CH4.startedAssigning) &&
                (!CA4.IsUnityNull() && CA4.startedAssigning) &&
                (!CA3.IsUnityNull() && CA3.startedAssigning) &&
                (!CA1.IsUnityNull() && CA1.startedAssigning)) ||
            ((!CH4.IsUnityNull() && CH4.startedAssigning) &&
                (!CA4.IsUnityNull() && CA4.startedAssigning) &&
                (!CA2.IsUnityNull() && CA2.startedAssigning) &&
                (!CA1.IsUnityNull() && CA1.startedAssigning)) ||
            ((!CH4.IsUnityNull() && CH4.startedAssigning) &&
                (!CA4.IsUnityNull() && CA4.startedAssigning) &&
                (!CA3.IsUnityNull() && CA3.startedAssigning) &&
                (!CH3.IsUnityNull() && CH3.startedAssigning) &&
                (!CH2.IsUnityNull() && CH2.startedAssigning)) ||
            ((!CH4.IsUnityNull() && CH4.startedAssigning) &&
                (!CA4.IsUnityNull() && CA4.startedAssigning) &&
                (!CA2.IsUnityNull() && CA2.startedAssigning) &&
                (!CH3.IsUnityNull() && CH3.startedAssigning) &&
                (!CH2.IsUnityNull() && CH2.startedAssigning)))
        {
            //includes 4 sets 
            strategy.AppendLine("Individual did chose longest time to survive for human case and least time to survive for animal cases");
        }

        else if (((!CH4.IsUnityNull() && CH4.startedAssigning) &&
                    (!CA4.IsUnityNull() && CA4.startedAssigning) &&
                    (!CA3.IsUnityNull() && CA3.startedAssigning) &&
                    (!CA2.IsUnityNull() && CA2.startedAssigning) &&
                    (!CH1.IsUnityNull() && CH1.startedAssigning)) ||
                ((!CH4.IsUnityNull() && CH4.startedAssigning) &&
                    (!CA4.IsUnityNull() && CA4.startedAssigning) &&
                    (!CH2.IsUnityNull() && CH2.startedAssigning) &&
                    (!CH3.IsUnityNull() && CH3.startedAssigning)))
        {
            //includes 2 sets 
            strategy.AppendLine("Individual did chose longest time to survive for Animal case and least time to survive for human cases");
        }

        return new KeyValuePair<string, string>("Strategy", strategy.ToString());
    }
    private static float avg(Case c1, Case c2, Case c3, Case c4)
    {
        float p1 = c1.IsUnityNull() && c1.startedAssigning ? 0 : c1.percentageDone;
        float p2 = c2.IsUnityNull() && c2.startedAssigning ? 0 : c2.percentageDone;
        float p3 = c3.IsUnityNull() && c3.startedAssigning ? 0 : c3.percentageDone;
        float p4 = c4.IsUnityNull() && c4.startedAssigning ? 0 : c4.percentageDone;

        float p = (!c1.IsUnityNull() && c1.startedAssigning ? 1 : 0) +
            (!c2.IsUnityNull() && c2.startedAssigning ? 1 : 0) +
            (!c3.IsUnityNull() && c3.startedAssigning ? 1 : 0) +
            (!c4.IsUnityNull() && c4.startedAssigning ? 1 : 0);

        return p != 0f ? (p1 + p2 + p3 + p4) / p : 0f;
    }
    //Avg of progress within a set
    private static float avgOfSet(Case c1, Case c2, Case c3, Case c4, Case c5, Case c6, Case c7, Case c8)
    {
        float p1 = !c1.IsUnityNull() && c1.startedAssigning ? c1.percentageDone : 0;
        float p2 = !c2.IsUnityNull() && c2.startedAssigning ? c2.percentageDone : 0;
        float p3 = !c3.IsUnityNull() && c3.startedAssigning ? c3.percentageDone : 0;
        float p4 = !c4.IsUnityNull() && c4.startedAssigning ? c4.percentageDone : 0;
        float p5 = !c5.IsUnityNull() && c5.startedAssigning ? c5.percentageDone : 0;
        float p6 = !c6.IsUnityNull() && c6.startedAssigning ? c6.percentageDone : 0;
        float p7 = !c7.IsUnityNull() && c7.startedAssigning ? c7.percentageDone : 0;
        float p8 = !c8.IsUnityNull() && c8.startedAssigning ? c8.percentageDone : 0;

        float p = (!c1.IsUnityNull() && c1.startedAssigning ? 1 : 0) +
            (!c2.IsUnityNull() && c2.startedAssigning ? 1 : 0) +
            (!c3.IsUnityNull() && c3.startedAssigning ? 1 : 0) +
            (!c4.IsUnityNull() && c4.startedAssigning ? 1 : 0) +
            (!c5.IsUnityNull() && c5.startedAssigning ? 1 : 0) +
            (!c6.IsUnityNull() && c6.startedAssigning ? 1 : 0) +
            (!c7.IsUnityNull() && c7.startedAssigning ? 1 : 0) +
            (!c8.IsUnityNull() && c8.startedAssigning ? 1 : 0);

        return p != 0f ? (p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8) / p : 0f;
    }

}
