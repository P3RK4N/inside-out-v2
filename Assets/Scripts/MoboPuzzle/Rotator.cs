using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotatingSpeed = 100.0f;
    AudioSource audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Stop();
        audioSource.loop = false;
    }

    public void turnOn(float speed)
    {
        rotatingSpeed = speed;
        audioSource.PlayOneShot(audioSource.clip, 1.0f);
    }

}
