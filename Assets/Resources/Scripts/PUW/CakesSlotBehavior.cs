using System.Collections;
using TMPro;
using UnityEngine;
using System.Linq;
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


    private void Start()
    {
        gewinn.text = "GEWINN: 0";
        notEnoughPanel.SetActive(false);

    }

    public void OnPlayMachine(bool _fromMystery)
    {
        Debug.Log("Entered Cake Slot MAchine");

        if (PopUpWerkManager.Instance.GetCoins() >= 5)
        {
            buttonBlocker.BlockButton();
            if (!_fromMystery)
                PopUpWerkManager.Instance.OnPlayMachine.Invoke("CakeSlot");

            float randomValue = Random.Range(0f, 1f);
            string winLooseAnimation = randomValue < 0.7f ? "CakeWin" : "CakeLose";
            AudioClip winLoseSE = randomValue < 0.7f ? cakeWinSE : cakeLoseSE;

            StartCoroutine(PlaySlot(winLooseAnimation, winLoseSE, randomValue < 0.7f ? 8f : 0f));
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
        cakeAnimator.Play("NotEnoughCoinsCake");
        yield return new WaitForSeconds(10f);
        notEnoughPanel.SetActive(false);
    }

    private IEnumerator PlaySlot(string winLooseAnimation, AudioClip winLooseSE, float winnings)
    {
        // Play Spin Anim + sound
        cakeAnimator.Play("CakeSpin");
        audioSource.PlayOneShot(spinSE);

        AnimationClip _clip= cakeAnimator.runtimeAnimatorController.animationClips.ToList<AnimationClip>().FirstOrDefault(x=>x.name.Equals("CakeSpin"));
        float _dur = _clip!=null?_clip.length:0f;
        yield return new WaitForSeconds(_dur);

        //Play either win or loose behavior

        StartCoroutine(PlayResult(winLooseAnimation, winLooseSE, winnings));

    }
    private IEnumerator PlayResult(string winLooseAnimation, AudioClip winLooseSE, float winnings)
    {
        cakeAnimator.Play(winLooseAnimation);
        audioSource.PlayOneShot(winLooseSE);


        AnimationClip _clip = cakeAnimator.runtimeAnimatorController.animationClips.ToList<AnimationClip>().FirstOrDefault(x => x.name.Equals(winLooseAnimation));
        float _dur = _clip != null ? _clip.length : 0f;
        _dur += 3f;
        yield return new WaitForSeconds(_dur);

        if (winnings > 0)
        {
            gewinn.text = "GEWINN: 8";
            MoneyManager.instance.UpdateMoney(winnings);
        }
        buttonBlocker.UnblockButton();
        ResetAnimatorTriggers();
    }


    private void ResetAnimatorTriggers()
    {
        cakeAnimator.ResetTrigger("CakeSpinAnim");
        cakeAnimator.ResetTrigger("CakeWinAnim");
        cakeAnimator.ResetTrigger("LoseAnim");
        cakeAnimator.ResetTrigger("NotEnoughCoins");
        notEnoughPanel.SetActive(false);
    }

   
}
