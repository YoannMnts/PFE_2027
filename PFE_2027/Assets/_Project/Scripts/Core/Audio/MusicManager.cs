
using System.Collections.Generic;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using PFE.Core.Scripts.GameSettings;
using PrimeTween;
using UnityEngine;
using UnityEngine.Assertions;

namespace PFE.Core.Audio
{
    public class MusicManager
    {
        public struct MusicProfile
        {
            public AudioClip clip;
            public AudioSource source;
            public float volume;
        }


        private Priority<MusicProfile> activeMusic;
        private List<ChannelKey> activeKeys;
        
        private Transform musicRoot;


        public MusicManager()
        {
            activeKeys = new List<ChannelKey>();
            activeMusic = new Priority<MusicProfile>();
            GameAudioListener.Instance.OnAudioUpdate += Refresh;
        }

        private Transform GetMusicRoot()
        {
            if (musicRoot == null)
            {
                GameObject root = new GameObject("MusicRoot");
                root.transform.position = Vector3.zero;
                root.transform.rotation = Quaternion.identity;
                
                musicRoot = root.transform;
                
                Object.DontDestroyOnLoad(root);
            }
            return musicRoot;
        }
        

        public void ModifyVolume(ChannelKey key, float volume)
        {
            if (activeMusic.TryGetValue(key, out MusicProfile profile))
            {
                profile.volume = volume;
                activeMusic[key] = profile;
            }
        }

        public void ModifyPriority(ChannelKey key, int priority)
        {
            activeMusic.ChangeChannelPriority(key, priority);
        }
        
        public void AddMusic(AudioClip music, ChannelKey key, int priority, float volume = 1f)
        {
            if(activeMusic.HasChannel(key))
            {
                Debug.LogError("Cannot add music to active music with an active key");
                return;
            }
            
            Transform root = GetMusicRoot();
            GameAssets gameAssets = GameAssets.Current;
            
            AudioSource source = Object.Instantiate(gameAssets.BGMAudioSource, root);
            
            source.clip = music;
            source.volume = 0;
            
            activeKeys.Add(key);
            activeMusic.AddPriority(key, priority, new MusicProfile()
            {
                source = source,
                clip = music,
                volume = volume,
            });
        }

        public void RemoveMusic(ChannelKey key)
        {
            if (activeMusic.TryGetValue(key, out MusicProfile profile))
            {
                activeMusic.RemovePriority(key);
                activeKeys.Remove(key);
                
                float fadeTime = GameMetricsSettings.Current.BGMFadeOutTime;
                Tween.AudioVolume(profile.source, 0, fadeTime)
                    .OnComplete(() => Object.Destroy(profile.source));
            }
        }

        private void Refresh()
        {
            foreach (ChannelKey key in activeKeys)
            {
                MusicProfile musicProfile = activeMusic[key];
                float targetVolume = activeMusic.IsMainChannel(key) ? musicProfile.volume : 0;
                float currentVolume = musicProfile.source.volume;

                float deltaTime = Time.deltaTime;
                float smoothness = GameMetricsSettings.Current.BGMTransitionSmoothness;
                
                musicProfile.source.volume = Mathf.Lerp(currentVolume, targetVolume, smoothness * deltaTime);
                
                if(targetVolume > 0 && !musicProfile.source.isPlaying)
                    musicProfile.source.Play();
            }
        }
    }
}