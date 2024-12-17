
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MysterySlotBehavior : MonoBehaviour

{
    [SerializeField] private GameObject mysteryPanel;
    [SerializeField] private GameObject diamondPanel;
    [SerializeField] private GameObject billsPanel;
    [SerializeField] private GameObject cakePanel;
    [SerializeField] private Animator nothingAnimator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip nothingSE;
    [SerializeField] private TextMeshProUGUI gewinn;

    private DiamondSlotBehavior diamondSlotBehavior;
    private BillsSlotMachine billsSlotBehavior;
    private CakesSlotBehavior cakesSlotBehavior;

    [SerializeField] private BlockSpinButton buttonBlocker;
    [SerializeField] private AudioClip notEnoughClip;
    [SerializeField] private GameObject notEnoughPanel;

    private const float diamondSlotDuration = 7.0f;
    private const float billSlotDuration = 5.5f;
    private const float cakeSlotDuration = 5.6f;
    private const float nothingSlotDuration = 7.45f;

    private void Start()
    {
        diamondSlotBehavior = diamondPanel.GetComponent<DiamondSlotBehavior>();
        billsSlotBehavior = billsPanel.GetComponent<BillsSlotMachine>();
        cakesSlotBehavior = cakePanel.GetComponent<CakesSlotBehavior>();

        gewinn.text = "GEWINN: 0";
    }

    public void SwitchPanel()
    {
        if (PopUpWerkManager.Instance.GetCoins() >= 1)
        {
            buttonBlocker.BlockButton();
            PopUpWerkManager.Instance.OnPlayMachine.Invoke("MysterySlot");

            DeactivateAllPanels();

            float randomValue = Random.Range(0f, 1f);

            if (randomValue >= 0f && randomValue < 0.25f)
            {
                ActivatePanel(diamondPanel, diamondSlotBehavior);
                PUWStats.UpdateMysterySlotsTotalDurations(diamondSlotDuration);

            }
            else if (randomValue >= 0.25f && randomValue < 0.5f)
            {
                ActivatePanel(billsPanel, billsSlotBehavior);
                PUWStats.UpdateMysterySlotsTotalDurations(billSlotDuration);

            }
            else if (randomValue >= 0.5f && randomValue < 0.75f)
            {
                ActivatePanel(cakePanel, cakesSlotBehavior);
                PUWStats.UpdateMysterySlotsTotalDurations(cakeSlotDuration);

            }
            else
            {
                ActivateNothingPanel();
                PUWStats.UpdateMysterySlotsTotalDurations(nothingSlotDuration);
            }
        }
        else
        {
            // Trigger the NotEnoughCoins animation and play the NotEnoughCoins sound effect
            notEnoughPanel.SetActive(true);
            PlayAnimation("NotEnoughCoins");
            PlaySound(notEnoughClip);
        }
    }

    private void ActivatePanel(GameObject panel, MonoBehaviour slotBehavior)
    {
        panel.SetActive(true);

        if (slotBehavior != null)
        {
            if (slotBehavior is DiamondSlotBehavior)
            {
                ((DiamondSlotBehavior)slotBehavior).OnPlayMachine(true);
            }
            else if (slotBehavior is BillsSlotMachine)
            {
                ((BillsSlotMachine)slotBehavior).OnPlayMachine(true);

            }
            else if (slotBehavior is CakesSlotBehavior)
            {
                ((CakesSlotBehavior)slotBehavior).OnPlayMachine(true);

            }
        }
    }
    private void ActivateNothingPanel()
    {
        gewinn.text = "GEWINN: 0";
        mysteryPanel.SetActive(true);
        PlayAnimation("NothingAnim");
        PlaySound(nothingSE);
        StartCoroutine(WaitForCompletion("NothingAnim"));
    }
    private IEnumerator WaitForCompletion(string animName)
    {
        float animDuration = GetAnimationClipLength(animName);
        yield return new WaitForSeconds(animDuration);
        buttonBlocker.UnblockButton();
        ResetAnimatorTriggers();
    }

    private void PlayAnimation(string animName)
    {
        if (nothingAnimator != null)
        {
            nothingAnimator.SetTrigger(animName);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    private void ResetAnimatorTriggers()
    {
        nothingAnimator.ResetTrigger("DiamondSpinAnim");
        nothingAnimator.ResetTrigger("NotEnoughCoins");
        notEnoughPanel.SetActive(false);
    }

    private float GetAnimationClipLength(string animName)
    {
        AnimationClip[] clips = nothingAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animName)
            {
                return clip.length;
            }
        }
        return 0f;
    }

    private void DeactivateAllPanels()
    {
        mysteryPanel.SetActive(false);
        diamondPanel.SetActive(false);
        billsPanel.SetActive(false);
        cakePanel.SetActive(false);
        notEnoughPanel.SetActive(false);
    }
}


