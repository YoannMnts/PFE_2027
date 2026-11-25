using System;
using System.Collections.Generic;
using Helteix.Singletons.SceneServices;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.Generators;
using UnityEngine;
using UnityEngine.Audio;

namespace PFE.Gameplay
{
    //SoundManager : gère ce qui est relatif au SD (créer, associer, retirer)
    //Comportement global des musiques et SFX
    [RequireComponent(typeof(AudioListener), typeof(AudioSource))]
    public class SoundManager : SceneService<SoundManager>
    {
        //player's ears
        public AudioListener audioListener;
        //JOUE les sons
        public AudioSource audioSource;
        
        //mp3
        private List<AudioClip> currentClips;

        private void Awake()
        {
            if (audioListener.TryGetComponent(out AudioListener listener))
            {
                audioListener = listener;
            }
            
            if (audioSource.TryGetComponent(out AudioSource source))
            {
                audioSource = source;
            }
            
            currentClips = new List<AudioClip>();
            Debug.Log($"[Sound Manager] is created");
        }

        private void Start()
        {
        }

        public void Play(AudioClip clip)
        {
            currentClips.Add(clip);
            
            audioSource.loop = true;
            audioSource.clip = clip;
            audioSource.Play();
        }
        
        public void PauseOneClip(AudioClip clip)
        {
        }
        
        public void GlobalPause()
        {
        }
        
        public void Stop(AudioClip clip)
        {
            audioSource.Stop();
            currentClips.Remove(clip);
            Debug.Log($"[Sound Manager] {clip.name} is stopped");
        }
        
    }
}
