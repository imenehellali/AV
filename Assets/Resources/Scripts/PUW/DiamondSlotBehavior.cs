using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Pico.Platform;
using System.Net;

public class DiamondSlotBehavior : MonoBehaviour
{
    [SerializeField] private Animator diamondAnimator;
    [SerializeField] private BlockSpinButton buttonBlocker;
    [SerializeField] private TextMeshProUGUI gewinn;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip diamondClip;
    [SerializeField] private AudioClip notEnoughClip;
    [SerializeField] private GameObject notEnoughPanel;

    public UnityEvent OnDiamondWin = new UnityEvent();

    private void Start()
    {
        OnDiamondWin.AddListener(() => NotifyMoneyManager(1000f));
        gewinn.text = "GEWINN: 0"; 
        notEnoughPanel.SetActive(false);

    }

    public void OnPlayMachine(bool _fromMystery)
    {
        if (PopUpWerkManager.Instance.GetCoins() >= 2)
        {
            buttonBlocker.BlockButton();
            if (!_fromMystery)
                PopUpWerkManager.Instance.OnPlayMachine.Invoke("DiamondSlot");

            PlayAnimation("WinningAnim");
            PlaySound(diamondClip);

            StartCoroutine(WaitForCompletion("WinningAnim"));

        }
        else
        {
            // Trigger the NotEnoughCoins animation and play the NotEnoughCoins sound effect
            notEnoughPanel.SetActive(true);
            PlayAnimation("NotEnoughCoins");
            PlaySound(notEnoughClip);
        }
    }

    private IEnumerator WaitForCompletion(string animName)
    {
        float animDuration = GetAnimationClipLength(animName);
        yield return new WaitForSeconds(animDuration);
        gewinn.text = "GEWINN: 1.000";
        OnDiamondWin.Invoke(); // Trigger the win event
        buttonBlocker.UnblockButton();
        ResetAnimatorTriggers();
    }

    private void PlayAnimation(string animName)
    {
        if (diamondAnimator != null)
        {
            diamondAnimator.Play(animName, -1, 0f);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private float GetAnimationClipLength(string animName)
    {
        AnimationClip[] clips = diamondAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animName)
            {
                return clip.length;
            }
        }
        return 0f;
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

