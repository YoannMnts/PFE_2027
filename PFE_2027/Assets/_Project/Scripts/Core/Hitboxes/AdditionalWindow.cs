using System;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    [Serializable]
    public struct AdditionalWindow
    {
        [field: SerializeReference]
        public IWindowFlag Flag { get; private set; }
    }
}
