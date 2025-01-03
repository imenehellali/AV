
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            Debug.Log($"Radnom Value for the slots:   {randomValue}");
            if (randomValue >= 0f && randomValue < 0.25f)
            {
                ActivatePanel(diamondPanel, diamondSlotBehavior);
                PUWStats.UpdateMysterySlotsTotalDurations(diamondSlotDuration);
                StartCoroutine(WaitForTheSlot());
            }
            else if (randomValue >= 0.25f && randomValue < 0.5f)
            {
                ActivatePanel(billsPanel, billsSlotBehavior);
                PUWStats.UpdateMysterySlotsTotalDurations(billSlotDuration);
                StartCoroutine(WaitForTheSlot());
            }
            else if (randomValue >= 0.5f && randomValue < 0.75f)
            {
                ActivatePanel(cakePanel, cakesSlotBehavior);
                PUWStats.UpdateMysterySlotsTotalDurations(cakeSlotDuration);
                StartCoroutine(WaitForTheSlot());
            }
            else
            {

                gewinn.text = "GEWINN: 0";
                mysteryPanel.SetActive(true);
                StartCoroutine(PlayNothingPanel());
                PUWStats.UpdateMysterySlotsTotalDurations(nothingSlotDuration);
            }
        }
        else
        {
            // Trigger the NotEnoughCoins anim + clip
            notEnoughPanel.SetActive(true);
            StartCoroutine(PlaySlot());
           
        }
    }
    private IEnumerator WaitForTheSlot()
    {
        yield return new WaitForSeconds(8f);
        buttonBlocker.UnblockButton();
    }
    private IEnumerator PlaySlot()
    {
        nothingAnimator.Play("NotEnoughCoins");
        audioSource.PlayOneShot(notEnoughClip);

        AnimationClip _clip = nothingAnimator.runtimeAnimatorController.animationClips.ToList<AnimationClip>().FirstOrDefault(x => x.name.Equals("NotEnoughCoins"));
        float _dur = _clip != null ? _clip.length : 0f;
        _dur += 3f;
        yield return new WaitForSeconds(_dur);

        notEnoughPanel.SetActive(false);
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

    private IEnumerator PlayNothingPanel()
    {
        nothingAnimator.Play("nothingAnim");
        audioSource.PlayOneShot(nothingSE);

        AnimationClip _clip = nothingAnimator.runtimeAnimatorController.animationClips.ToList<AnimationClip>().FirstOrDefault(x => x.name.Equals("nothingAnim"));
        float _dur = _clip != null ? _clip.length : 0f;
        _dur += 3f;
        yield return new WaitForSeconds(_dur);

        buttonBlocker.UnblockButton();
        ResetAnimatorTriggers();
    }

    private void ResetAnimatorTriggers()
    {
        nothingAnimator.ResetTrigger("NothingAnim");
        nothingAnimator.ResetTrigger("NotEnoughCoins");
        notEnoughPanel.SetActive(false);
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


