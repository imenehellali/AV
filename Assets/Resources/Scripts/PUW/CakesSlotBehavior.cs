using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
public class CakesSlotBehavior : MonoBehaviour
{
    [SerializeField] private Animator cakeAnimator;
    [SerializeField] private BlockSpinButton buttonBlocker;
    [SerializeField] private TextMeshProUGUI gewinn;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spinSE;
    [SerializeField] private AudioClip cakeWinSE;
    [SerializeField] private AudioClip cakeLoseSE;

    [SerializeField] private AudioClip notEnoughClip;
    [SerializeField] private GameObject notEnoughPanel;

    public UnityEvent OnCakeWin = new UnityEvent();

    private void Start()
    {
        OnCakeWin.AddListener(() => NotifyMoneyManager(8f));
        gewinn.text = "GEWINN: 0";
        notEnoughPanel.SetActive(false);

    }

    public void OnPlayMachine(bool _fromMystery)
    {
        if (PopUpWerkManager.Instance.GetCoins() >= 5)
        {
            buttonBlocker.BlockButton();
            if (!_fromMystery)
                PopUpWerkManager.Instance.OnPlayMachine.Invoke("CakeSlot");

            PlayAnimation("CakeSpinAnim");
            PlaySound(spinSE);

            float randomValue = Random.Range(0f, 1f);
            string winAnimation = randomValue < 0.7f ? "CakeWinAnim" : "LoseAnim";
            AudioClip winLoseSE = randomValue < 0.7f ? cakeWinSE : cakeLoseSE;

            StartCoroutine(PlaySpinAndWinAnimation(winAnimation, winLoseSE, randomValue < 0.7f ? 8f : 0f));
        }
        else
        {
            notEnoughPanel.SetActive(true);
            PlayAnimation("NotEnoughCoins");
            PlaySound(notEnoughClip);
        }
    }


    private IEnumerator PlaySpinAndWinAnimation(string winAnimation, AudioClip winSE, float winnings)
    {
        yield return new WaitForSeconds(GetAnimationClipLength("CakeSpinAnim"));

        PlayAnimation(winAnimation);
        PlaySound(winSE);

        yield return new WaitForSeconds(GetAnimationClipLength(winAnimation));

        if (winnings > 0)
        {
            gewinn.text = "GEWINN: 8";
            NotifyMoneyManager(winnings);
            OnCakeWin.Invoke();
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

    private void PlayAnimation(string animName)
    {
        if (cakeAnimator != null)
        {
            cakeAnimator.Play(animName, -1, 0f);
        }
    }
    private float GetAnimationClipLength(string animName)
    {
        AnimationClip[] clips = cakeAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {          if (clip.name == animName)
            {
                return clip.length;
            }
        }
        return 0f;
    }

    private void ResetAnimatorTriggers()
    {
        cakeAnimator.ResetTrigger("CakeSpinAnim");
        cakeAnimator.ResetTrigger("CakeWinAnim");
        cakeAnimator.ResetTrigger("LoseAnim");
        cakeAnimator.ResetTrigger("NotEnoughCoins");
        notEnoughPanel.SetActive(false);
    }

    private void NotifyMoneyManager(float amount)
    {
        if (amount > 0)
        {
            MoneyManager.instance.UpdateMoney(amount);
        }
    }
}
