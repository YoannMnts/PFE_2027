using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Databases
{
    public abstract class GameDatabaseObject : ScriptableObject
    {
        [SerializeField, ReadOnly, HideLabel]
        private string guidText;

        public Guid ID
        {
            get
            {
                if (guid == Guid.Empty)
                    guid = Guid.Parse(guidText);

                return guid;
            }
            private set
            {
                guidText = value.ToString();
                guid = value;
            }
        }

        private Guid guid = Guid.Empty;

        protected virtual void Reset()
        {
            guidText = string.Empty;
            OnValidate();
        }

        protected virtual void OnValidate()
        {
            if(string.IsNullOrEmpty(guidText))
                SetNewGuid();
        }

        internal void SetNewGuid() =>  ID = Guid.NewGuid();
    }
}