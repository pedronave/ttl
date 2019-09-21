﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource tickSource;
    public AudioSource collisionSource;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayTick()
    {
        tickSource.Play();
    }

    public void PlayCollision()
    {
        collisionSource.pitch = Random.Range(0.9f, 1.1f);
        collisionSource.Play();
    }
}
