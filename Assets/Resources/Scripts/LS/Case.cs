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


    private float timeOut = 0f; //initialized with init time that is decreased


    public bool startedAssigning = false;
    public float percentageDone = 0f;
    public bool healed = false;
    public bool dead = false;
    private bool stoppedHelping = true;
    public bool StoppedHelping() => stoppedHelping;
    //add variables video + sound ....

    public bool watchedVid = false; //at least ones
    //watching video is not necessary to assign resources....

    //Tracking Variables
    private bool startCase = false;
    private float startTime = 0f;
    private float endTime = 0f;

    private List<ResourceElement> _resources = new List<ResourceElement>();
    public UnityAction<ResourceElement.Type> UpdateCaseToIndividual;

    [SerializeField]
    private TextMeshProUGUI _timerDisplay;
    private bool startUrgency = false;
    [SerializeField]
    private Light _urgencyLight;

    [SerializeField]
    private VideoPlayer _vp;
    [SerializeField]
    private GameObject _vpPlayButton;
   
    public float SpenTimeOnCase() => endTime - startTime;
    public Case InitCase(bool animalCase, bool requiredSocial, int requiredAntidote, int requiredWater, int requiredAir, float timeOut)
    {
        this.animalCase = animalCase;
        this.requiredSocial = requiredSocial;
        this.requiredAntidote = requiredAntidote;
        if (requiredAntidote > 0) _resources.Add(new ResourceElement(ResourceElement.Type.Antidote, ResourceElement.BelongTo.Case, requiredAntidote));
        
        this.requiredWater = requiredWater;
        if(requiredWater > 0) _resources.Add(new ResourceElement(ResourceElement.Type.Water, ResourceElement.BelongTo.Case, requiredWater));
        
        this.requiredAir = requiredAir;
        if (requiredAir>0) _resources.Add(new ResourceElement(ResourceElement.Type.Air, ResourceElement.BelongTo.Case, requiredAir));
        
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
    private void UpdateResources(int amount, ResourceElement.Type type)
    {
        if (stoppedHelping)
        {
            stoppedHelping = false;
            if (!startedAssigning) 
                startedAssigning = true;
            timeOut += incTime;
            UpdateCaseToIndividual?.Invoke(type);
            switch (type)
            {
                case ResourceElement.Type.Air:
                    {
                        assignedAir++;
                        break;
                    }
                case ResourceElement.Type.Water:
                    {
                        assignedWater++;
                        break;
                    }
                case ResourceElement.Type.Antidote:
                    {
                        assignedAntidote++;
                        break;
                    }
            }
        }
    }
    private void OnEnable()
    {
        _resources.ForEach(r => { r.AmountChanged += UpdateResources; });
    }
    private void OnDisable()
    {
        _resources.ForEach(r => { r.AmountChanged -= UpdateResources; });
    }
    public void UpdateProgress()
    {
        percentageDone = (assignedAir + assignedAntidote + assignedWater + SocialFulfilled()) / (requiredAir + requiredAntidote + requiredWater + 1); //one for social
        CheckDone();
    }
    private void CheckDone()
    {
        if (percentageDone >= 0.9f ||
            (requiredAir == assignedAir && requiredAntidote == assignedAntidote && requiredWater == assignedWater && requiredSocial == assignedSocial))
        {
            healed = true;
            if (endTime <= 0.1f)
                endTime = Time.deltaTime;
        }
    }
    private int SocialFulfilled()
    {
        return XOR(assignedSocial, requiredSocial) ? 0 : 1;
    }

    private void AssignSocial()
    {
        if (!assignedSocial)
        {
            assignedSocial = true;
            //turn on mic //MIC behavior and recording !!!!! Clas upcoming
        }
        else return;
    }
    //Called from Level Manager because it handels the remaining time within a level !
    public void StopHelping()
    {
        stoppedHelping = true;
    }
    public void Startcase()
    {
        startCase = true;
        startTime = Time.deltaTime;
    }
    //TBCCCCC
    public void UpdateCaseForTesting(bool assignedSocial,
        int assignedAntidote, int assignedWater, int assignedAir, 
        bool startedAssigning, float percentageDone,
        bool healed, bool dead, bool stoppedHelping, bool watchedVid, 
        float startTime, float endTime)
    {
        this.assignedSocial = assignedSocial;
        this.assignedAntidote = assignedAntidote;
        this.assignedWater = assignedWater;
        this.assignedAir = assignedAir;
        this.startedAssigning = startedAssigning;
        this.percentageDone = percentageDone;
        this.healed = healed;
        this.dead = dead;
        this.stoppedHelping = stoppedHelping;
        this.watchedVid = watchedVid; //at least ones
        this.startCase = true;
        this.startTime = startTime;
        this.endTime = endTime;
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
                    if (endTime <= 0.1f)
                        endTime = Time.deltaTime;
                }
            }
        }


    }
    private void StartUrgencyOfCase()
    {
        startUrgency = true;
        if(!_urgencyLight.enabled)
            _urgencyLight.enabled = true;
        
    }
    private void RemoveUrgency()
    {
        startUrgency = false;
        if(_urgencyLight.enabled)
            _urgencyLight.enabled=false;
    }

}
