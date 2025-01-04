using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Pico.Platform;
using System.Net;
using System.Linq;

public class DiamondSlotBehavior : MonoBehaviour
{
    [SerializeField] private Animator diamondAnimator;
    [SerializeField] private BlockSpinButton buttonBlocker;
    [SerializeField] private TextMeshProUGUI gewinn;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip diamondClip;
    [SerializeField] private AudioClip notEnoughClip;
    [SerializeField] private GameObject notEnoughPanel;


    private void Start()
    {
        gewinn.text = "GEWINN: 0"; 
        notEnoughPanel.SetActive(false);

    }

    public void OnPlayMachine(bool _fromMystery)
    {
        Debug.Log("Entered Diamond Slot MAchine");

        if (PopUpWerkManager.Instance.GetCoins() >= 2)
        {
            buttonBlocker.BlockButton();
            if (!_fromMystery)
                PopUpWerkManager.Instance.OnPlayMachine.Invoke("DiamondSlot");

            StartCoroutine(PlaySlot());
        }
        else
        {
            notEnoughPanel.SetActive(true);
            StartCoroutine(ShowNotEnoughPanel());
        }
    }

    private IEnumerator ShowNotEnoughPanel()
    {
        Debug.Log("Not Enough Coins to play");

        audioSource.PlayOneShot(notEnoughClip);
        diamondAnimator.Play("NotEnoughCoinsDiamond");

        float _dur = notEnoughClip.length;
        yield return new WaitForSeconds(10f);

        notEnoughPanel.SetActive(false);
    }
    private IEnumerator PlaySlot()
    {
        diamondAnimator.Play("diamondAnim");
        audioSource.PlayOneShot(diamondClip);


        AnimationClip _clip = diamondAnimator.runtimeAnimatorController.animationClips.ToList<AnimationClip>().FirstOrDefault(x => x.name.Equals("diamondAnim"));
        float _dur = _clip != null ? _clip.length : 0f;
        _dur += 3f;

        yield return new WaitForSeconds(_dur);
        gewinn.text = "GEWINN: 1.000";
        MoneyManager.instance.UpdateMoney(1000f);
        buttonBlocker.UnblockButton();
        ResetAnimatorTriggers();
    }

    private void ResetAnimatorTriggers()
    {
        diamondAnimator.ResetTrigger("DiamondSpinAnim");
        diamondAnimator.ResetTrigger("NotEnoughCoins");
        notEnoughPanel.SetActive(false);
    }

    private void NotifyMoneyManager(float amount)
    {
        MoneyManager.instance.UpdateMoney(amount);
    }
}

