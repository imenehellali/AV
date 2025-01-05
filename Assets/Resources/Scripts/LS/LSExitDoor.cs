using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSExitDoor : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource; //Fixed 30s length of door squeeking
    [SerializeField]
    private Transform _door;
    [SerializeField]
    private float speed = 2.0f;

   
    public void PlayEnd()
    {
        _audioSource.Play();
        StartCoroutine(OpenDoor());
    }
    private IEnumerator OpenDoor()
    {
        float _dur = 27f;
        float _x=_door.position.x;
        while (_dur>0f)
        {
            _dur -= Time.deltaTime;
            _x -= speed * Time.deltaTime;
            _door.position = new Vector3(_x, _door.position.y, _door.position.z);
            yield return null;
        }
    }
    
}
