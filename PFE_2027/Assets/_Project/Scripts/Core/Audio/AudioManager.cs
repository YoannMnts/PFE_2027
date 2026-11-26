using System;
using System.Collections.Generic;
using Helteix.Singletons.SceneServices;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Audio;

namespace PFE.Core.Audio
{
    //AudioManager : gère ce qui est relatif au SD (créer, associer, retirer)
    //Comportement global des musiques et SFX
    
    //plutot singleton MONOSINGLETON
    //audio listener sur camera
    //get position cam, et SD se positionne sur cam (mieux pour les ss écrans)
    [RequireComponent(typeof(AudioListener), typeof(AudioSource))]
    public class AudioManager : SceneService<AudioManager>
    {
        
    }
}
