using System;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public class HitboxAnchors : MonoBehaviour
    {
        [Serializable]
        private struct Entry
        {
            [field: SerializeField]
            public HitboxAnchor Anchor { get; private set; }
            
            [field: SerializeField]
            public Transform Bone { get; private set; }
        }
        
        [SerializeField]
        private Entry[] entries = Array.Empty<Entry>();

        public bool TryGet(HitboxAnchor anchor, out Transform bone)
        {
            if (anchor is null)
            {
                bone = transform;
                return true;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                
                if (!ReferenceEquals(entry.Anchor, anchor)) 
                    continue;
                
                bone = entry.Bone;
                return bone != null;
            }
            
            bone = null;
            return false;
        }
    }
}