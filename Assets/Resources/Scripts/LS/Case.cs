using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class Case : MonoBehaviour
{
    private Func<bool, bool, bool> XOR = (X, Y) => ((!X) && Y) || (X && (!Y));

    //For init
    private bool animalCase = false;
    private bool requiredSocial = false;
    private int requiredAntidote = 0;
    private int requiredWater = 0;
    private int requiredAir = 0;

    //For assignment
    private bool assignedSocial = false;
    private int assignedAntidote = 0;
    private int assignedWater = 0;
    private int assignedAir = 0;
    private float incTime = 10f;

    [Header("Social Variables")]
    [SerializeField] private Animator _socialAnimator;
    [SerializeField] private GameObject _socialResourceElement;
    [SerializeField] private GameObject _socialAssignable;


    private float timeOut = 0f; //initialized with init time that is decreased

    [HideInInspector] public bool startedAssigning = false;
    [HideInInspector] public float percentageDone = 0f;
    [HideInInspector] public bool healed = false;
    [HideInInspector] public bool dead = false;

    private bool stoppedHelping = false;
    public bool StoppedHelping() => stoppedHelping;
    //add variables video + sound ....

    public bool watchedVid = false; //at least ones
    //watching video is not necessary to assign resources....

    //Tracking Variables
    private bool startCase = false;
    private float startTime = 0f;
    private float endTime = 0f;

    [Header("Air, Water, Anti, !!Social")]
    [SerializeField]
    private List<ResourceElementCase> _resources = new List<ResourceElementCase>();
    public UnityAction<ResourceElementCase.Type> UpdateCase;

    [SerializeField]
    private TextMeshProUGUI _timerDisplay;
    [SerializeField]
    private Light _urgencyLight;

    [SerializeField]
    private VideoPlayer _vp;
    [SerializeField]
    private GameObject _vpPlayButton;
   
    public float SpenTimeOnCase() => endTime - startTime;
    public Case InitCase(bool animalCase, bool requiredSocial, int requiredAntidote, int requiredWater, int requiredAir, float timeOut)
    {
        int _curr = 0;

        this.animalCase = animalCase;
        this.requiredSocial = requiredSocial;

        this.requiredAir = requiredAir;
        if (requiredAir > 0)
        {
           _resources[_curr].UpdateResource(requiredAir);
            Debug.Log($"intiialized air to case with amount {requiredAir}");
            _curr++;
        }
        this.requiredWater = requiredWater;
        if (requiredWater > 0)
        {
            _resources[_curr].UpdateResource(requiredWater);
            _curr++;
        }
        this.requiredAntidote = requiredAntidote;
        if (requiredAntidote > 0)
        {
           _resources[_curr].UpdateResource(requiredAntidote);
            _curr++;
        }
            
       
        this.timeOut = timeOut;

        return this;
    }

    public void PlayButtonHandler(bool play)
    {
        if (play)
        {
            watchedVid = true;
            _vp.Play();
            _vpPlayButton.SetActive(false);
            StartCoroutine(ShowButtonAfterPlay());
        }
    }

    private IEnumerator ShowButtonAfterPlay()
    {
        while (_vp.isPlaying)
        {
            yield return null;
        }
        _vpPlayButton.SetActive(true);
    }
    private void UpdateResources(ResourceElementCase.Type type)
    {
        if (startCase)
        {
            Debug.Log("from case --> to update resource");
            stoppedHelping = false;
            Debug.Log("updating resource now");
            if (!startedAssigning)
                startedAssigning = true;
            timeOut += incTime;
            switch (type)
            {
                case ResourceElementCase.Type.Air:
                    {
                        assignedAir++;
                        break;
                    }
                case ResourceElementCase.Type.Water:
                    {
                        assignedWater++;
                        break;
                    }
                case ResourceElementCase.Type.Antidote:
                    {
                        assignedAntidote++;
                        break;
                    }
            }
        }
        
    }
    private void OnEnable()
    {
        UpdateCase += UpdateResources;
    }
    private void OnDisable()
    {
        UpdateCase -= UpdateResources;
    }
    public void UpdateProgress()
    {
        percentageDone = (float)(assignedAir + assignedAntidote + assignedWater + SocialFulfilled()) / (float)(requiredAir + requiredAntidote + requiredWater + 1); //one for social
        
        CheckDone();
    }
    private void CheckDone()
    {
        if (percentageDone >= 0.9f ||
            (requiredAir == assignedAir && requiredAntidote == assignedAntidote && requiredWater == assignedWater && requiredSocial == assignedSocial))
        {
            healed = true;
            percentageDone = 1f;
            endTime = Time.deltaTime;
        }
    }
    private int SocialFulfilled()
    {
        return (assignedSocial == requiredSocial)? 1:0;
    }

    public void AssignSocial()
    {
        if (!assignedSocial && startCase)
        {
            assignedSocial = true;
            StartCoroutine(SocialAnimationPlay());
        }
        else return;
    }
    private IEnumerator SocialAnimationPlay()
    {
        _socialAnimator.Play("SocialAnimatior");
        _socialAssignable.SetActive(false);
        float _dur = _socialAnimator.runtimeAnimatorController.animationClips[0].length;
        yield return new WaitForSeconds(_dur);
        _socialAnimator.ResetTrigger("Assign");
        _socialResourceElement.SetActive(false);    
    }
    //Called from Level Manager because it handels the remaining time within a level !
    public void StopHelping()
    {
        stoppedHelping = true;
        Debug.Log("stopped helping from case");
    }
    public void Startcase()
    {
        startCase = true;
        startTime = Time.deltaTime;
    }
    public void StopCase(string caseName)
    {
        startCase = false;
        Debug.Log($"percentage done for {caseName} case:   {percentageDone} which is Healed ? {healed}");
        foreach (ResourceElementCase _resource in _resources)
        {
            _resource.StopResource(caseName);
        }
    }
   
    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    private void Update()
    {
        if (startCase)
        {
            if (!healed && !dead)
            {
                timeOut -= Time.deltaTime;
                _timerDisplay.text=FormatTime(timeOut);


                if (timeOut > 0f)
                {
                    UpdateProgress();
                    if (timeOut <= 31f)
                        StartUrgencyOfCase();
                    else
                        RemoveUrgency();
                }
                else
                {
                    dead = true;
                    endTime = Time.deltaTime;
                }
            }
        }


    }
    private void StartUrgencyOfCase()
    {
        if(!_urgencyLight.enabled)
            _urgencyLight.enabled = true;
        
    }
    private void RemoveUrgency()
    {
        if(_urgencyLight.enabled)
            _urgencyLight.enabled=false;
    }

}
