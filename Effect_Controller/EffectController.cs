using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    public List<ParticleEffect> ParticleEffects;
    public bool IsPlayingParticles;
    public List<AudioEffect> AudioEffects;    
    public bool IsPlayingAudio;

    private void Update()
    {
        if (IsPlayingParticles)
        {
            foreach (var p in ParticleEffects)
            {
                p.Update(Time.deltaTime);
            }
        }

        if (IsPlayingAudio)
        {
            foreach (var a in AudioEffects)
            {
                a.Update(Time.deltaTime);
            }
        }
    }

    [ContextMenu("Play")]
    public void PlayEffects()
    {
        if (!IsPlayingParticles)
        {            
            foreach (var p in ParticleEffects)
            {
                p.Play();
            }
            IsPlayingParticles = true;
        }
        if (!IsPlayingAudio)
        {
            foreach (var a in AudioEffects)
                a.Play();
            IsPlayingAudio = true;
        }
    } 
    [ContextMenu("Stop")]
    public void StopEffects()
    {
        if (IsPlayingParticles)
        {            
            foreach (var p in ParticleEffects)
            {
                p.Stop();
            }
            IsPlayingParticles = false;
        }
        if (IsPlayingAudio)
        {
            foreach (var a in AudioEffects)
                a.Stop();
            IsPlayingAudio = false;
        }
    }     
}

[System.Serializable]
public class ParticleEffect
{
    [SerializeField]
    private ParticleSystem System;
    public ParticleSystem.MinMaxCurve StartDelay;
    public bool IsLooping;
    public bool IsPrewarm;
    public bool NeverStop;
    public bool ClearOnStop;
    public float MaxLifeTime;   
    private float CurrentLifeTime;

    public void Play()
    {
        var main = System.main;        
        main.loop = IsLooping;
        main.startDelay = StartDelay; 
        main.prewarm = IsPrewarm;
        if(IsPrewarm && !IsLooping)
        {
            main.loop = true;
            main.prewarm = true;
            System.Play();
            main.loop = false;
            return;
        }
        System.Play();
    }

    public void Update(float dt)
    {
        if (System == null)
            return;
        CurrentLifeTime += dt;
        if(!NeverStop)
        {
            if (CurrentLifeTime >= MaxLifeTime)
                Stop();
        }        
    }

    public void Stop()
    {

        if (ClearOnStop)
            System.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        else
            System.Stop();
    }

    public void Pause()
    {
        if (!System.isPaused)
            System.Pause();
        else
            System.Play();
    }
}

[System.Serializable]
public class AudioEffect
{
    [SerializeField]
    private AudioSource Audio;
    public bool IsLooping;
    public bool NeverStop;
    //public bool FadeOnDestroy;
    public float MaxLifeTime;
    public float StartDelay;
    private float CurrentLifeTime;

    public void Play()
    {
        Audio.PlayDelayed(StartDelay);
    }

    public void Update(float dt)
    {
        CurrentLifeTime += dt;
        if (!NeverStop)
        {
            if (CurrentLifeTime >= MaxLifeTime)
                Stop();
        }        
    }

    public void Stop()
    {
        Audio.Stop();
    }

    public void Pause()
    {
        if (Audio.isPlaying)
            Audio.Pause();
        else
            Audio.UnPause();
    }
}