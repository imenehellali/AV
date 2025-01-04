using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LSEnvSounds : MonoBehaviour
{
    [SerializeField]
    private List<AudioClip> _clips = new List<AudioClip>();
    [SerializeField]
    private AudioSource _audioSource;
    void Start()
    {
        PlayEnvironmentSound();
    }
    private void PlayEnvironmentSound()
    {
        int _curr = 0;
        StartCoroutine(PlaySound(_clips[_curr], _curr));
    }
    private IEnumerator PlaySound(AudioClip _clip, int curr)
    {
        if (SceneManager.GetSceneByName("LSScene").isLoaded)
        {
            float _dur = _clip.length;
            _audioSource.PlayOneShot(_clip);
            yield return new WaitForSeconds(_dur);
            curr++;
            if (curr >= _clips.Count)
                curr = 0;

            StartCoroutine(PlaySound(_clips[curr], curr));
        }

    }
    private void OnDestroy()
    {
        StopAllCoroutines();
        _audioSource.Stop();
    }
}
