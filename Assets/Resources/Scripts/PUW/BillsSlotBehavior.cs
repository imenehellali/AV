using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Linq;

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

    private void Start()
    {
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

        float _dur = notEnoughClip.length;
        audioSource.PlayOneShot(notEnoughClip);
        billsAnimator.Play("NotEnoughCoinsBill");
        yield return new WaitForSeconds(10f);
        notEnoughPanel.SetActive(false);
    }


    private IEnumerator PlaySlot()
    {
        audioSource.PlayOneShot(spinSE);
        billsAnimator.Play("BillSpinAnim");

        AnimationClip _clip = billsAnimator.runtimeAnimatorController.animationClips.ToList<AnimationClip>().FirstOrDefault(x => x.name.Equals("BillSpinAnim"));
        float _dur = _clip != null ? _clip.length : 0f;
        yield return new WaitForSeconds(_dur);


        float randomValue = Random.Range(0f, 1f);
        float winnings = 0f;
        string winLoseAnimation = "BillLoseAnim";
        AudioClip winLoseSE = null;

        if (randomValue >= 0f && randomValue < 0.25f) // 2/8 chance of winning 50 euros
        {
            winnings = 50f;
            winLoseAnimation = "50WinAnim";
            winLoseSE = _50WinSE;
        }
        else if (randomValue >= 0.25f && randomValue < 0.375f) // 1/8 chance of winning 200 euros
        {
            winnings = 200f;
            winLoseAnimation = "200WinAnim";
            winLoseSE = _200WinSE;
        }
        else if (randomValue >= 0.375f && randomValue < 0.5f) // 1/8 chance of winning 10 euros
        {
            winnings = 10f;
            winLoseAnimation = "10WinAnim";
            winLoseSE = _10WinSE;
        }
        else if (randomValue >= 0.5f && randomValue < 0.625f) // 1/8 chance of winning 20 euros
        {
            winnings = 20f;
            winLoseAnimation = "20WinAnim";
            winLoseSE = _20WinSE;
        }
        gewinn.text = $"GEWINN: {winnings}";
        StartCoroutine(PlayResult(winLoseAnimation, winLoseSE, winnings));
    }


    private IEnumerator PlayResult(string winLooseAnimation, AudioClip winLooseSE, float winnings)
    {
        audioSource.PlayOneShot(winLooseSE);
        billsAnimator.Play(winLooseAnimation);

        AnimationClip _clip = billsAnimator.runtimeAnimatorController.animationClips.ToList<AnimationClip>().FirstOrDefault(x => x.name.Equals(winLooseAnimation));
        float _dur = _clip != null ? _clip.length : 0f;
        _dur += 3f;
        yield return new WaitForSeconds(_dur);

        if (winnings > 0)
        {
            MoneyManager.instance.UpdateMoney(winnings);
        }

        buttonBlocker.UnblockButton();
        ResetAnimatorTriggers();
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

}

