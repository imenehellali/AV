using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartStartInstrManager : MonoBehaviour
{
    [Header("Audio of TryOut")]
    [SerializeField]
    private AudioClip _participantInstrClip;

    [Header("Setup instr audios")]
    [SerializeField]
    private AudioSource _audioSourceInstr;
    [SerializeField]
    private AudioClip _initInstr0Clip; //click menu
   

    public void PlayParticipantInstr()
    {
        _audioSourceInstr.PlayOneShot(_participantInstrClip);
    }

    public void PlayInitInstr0OnProximityOREye()
    {
        _audioSourceInstr.PlayOneShot(_initInstr0Clip);
    }

}
