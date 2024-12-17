using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class BillsSlotMachine : MonoBehaviour
{
    [SerializeField] private Animator billsAnimator;
    [SerializeField] private BlockSpinButton buttonBlocker;
    [SerializeField] private TextMeshProUGUI gewinn;

    [SerializeField] private AudioClip spinSE;
    [SerializeField] private AudioClip _50WinSE;
    [SerializeField] private AudioClip _10WinSE;
    [SerializeField] private AudioClip _20WinSE;
    [SerializeField] private AudioClip _200WinSE;
    [SerializeField] private AudioClip billLoseSE;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip notEnoughClip;
    [SerializeField] private GameObject notEnoughPanel;

    public UnityEvent OnBillsWin = new UnityEvent();
    private void Start()
    {
        OnBillsWin.AddListener(() => NotifyMoneyManager(0));
        gewinn.text = "GEWINN: 0";
        notEnoughPanel.SetActive(false);
    }

    public void OnPlayMachine(bool _fromMystery)
    {
        if (PopUpWerkManager.Instance.GetCoins() >= 2)
        {
            buttonBlocker.BlockButton();
            if (!_fromMystery)
                PopUpWerkManager.Instance.OnPlayMachine.Invoke("BillSlot");

            
            DetermineWinnings();
        }
        else
        {
            notEnoughPanel.SetActive(true);
            PlayAnimation("NotEnoughCoins");
            PlaySound(notEnoughClip);
        }
    }


    private void DetermineWinnings()
    {
        PlayAnimation("BillSpinAnim");
        PlaySound(spinSE);

        float randomValue = Random.Range(0f, 1f);
        float winnings = 0f;
        string animName = "LosingAnim";
        AudioClip winSE = null;

        if (randomValue >= 0f && randomValue < 0.25f) // 2/8 chance of winning 50 euros
        {
            winnings = 50f;
            animName = "50WinAnim";
            winSE = _50WinSE;
        }
        else if (randomValue >= 0.25f && randomValue < 0.375f) // 1/8 chance of winning 200 euros
        {
            winnings = 200f;
            animName = "200WinAnim";
            winSE = _200WinSE;
        }
        else if (randomValue >= 0.375f && randomValue < 0.5f) // 1/8 chance of winning 10 euros
        {
            winnings = 10f;
            animName = "10WinAnim";
            winSE = _10WinSE;
        }
        else if (randomValue >= 0.5f && randomValue < 0.625f) // 1/8 chance of winning 20 euros
        {
            winnings = 20f;
            animName = "20WinAnim";
            winSE = _20WinSE;
        }
        gewinn.text = $"GEWINN: {winnings}";
        StartCoroutine(PlaySpinAndWinAnimation(animName,winSE, winnings));
        // Else 3/8 chance of winning nothing, winnings remain 0
        
    }
    private IEnumerator PlaySpinAndWinAnimation(string winAnimation,AudioClip winSE, float winnings)
    {
        yield return new WaitForSeconds(GetAnimationClipLength("BillSpinAnim"));

        PlayAnimation(winAnimation);
        if (winSE != null)
        {
            PlaySound(winSE);
        }


        yield return new WaitForSeconds(GetAnimationClipLength(winAnimation));

        if (winnings > 0)
        {
            NotifyMoneyManager(winnings);
            OnBillsWin.Invoke();
        }

        buttonBlocker.UnblockButton();
        ResetAnimatorTriggers();
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
        billsAnimator.ResetTrigger("BillSpinAnim");
        billsAnimator.ResetTrigger("50WinAnim");
        billsAnimator.ResetTrigger("200WinAnim");
        billsAnimator.ResetTrigger("10WinAnim");
        billsAnimator.ResetTrigger("20WinAnim");
        billsAnimator.ResetTrigger("LoseAnim");
        billsAnimator.ResetTrigger("NotEnoughCoins");
        notEnoughPanel.SetActive(false);
    }
    private float GetAnimationClipLength(string animName)
    {
        float animduration = 0f;
        AnimationClip[] clips = billsAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animName)
               return clip.length;

        }
        return animduration;
    }
    private void PlayAnimation(string animName)
    {
        if (billsAnimator != null)
        {
            billsAnimator.Play(animName, -1, 0f);
        }
    }

    private void NotifyMoneyManager(float amount)
    {
        if (amount > 0)
        {
            MoneyManager.instance.UpdateMoney(amount);
        }
    }
}

