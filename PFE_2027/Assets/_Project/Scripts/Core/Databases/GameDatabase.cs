using System;
using System.Collections.Generic;
using UnityEngine;

namespace PFE.Core.Scripts.Databases
{
    public class GameDatabase
    {
        private readonly Dictionary<Guid, GameDatabaseObject> objects;
        private readonly Dictionary<string, Guid> guidCache;

        internal GameDatabase()
        {
            objects = new Dictionary<Guid, GameDatabaseObject>();
            guidCache = new Dictionary<string, Guid>();
        }

        public void Load()
        {
            var objs = Resources.LoadAll<GameDatabaseObject>("Database");
            for (int i = 0; i < objs.Length; i++)
            {
                GameDatabaseObject databaseObject = objs[i];
                objects.TryAdd(databaseObject.ID, databaseObject);
            }
        }


        public IEnumerable<T> GetAll<T>()  where T : GameDatabaseObject
            => GetAll<T>(ctx => true);
        public IEnumerable<T> GetAll<T>(Func<T, bool> filter) where T : GameDatabaseObject
        {
            foreach ((_, GameDatabaseObject gameDatabaseObject) in objects)
            {
                if (gameDatabaseObject is T t && filter(t))
                    yield return t;
            }
        }

        public bool TryGetObject<T>(string id, out T item) where T : GameDatabaseObject
        {
            if (!guidCache.TryGetValue(id, out Guid guid))
            {
                guid = Guid.Parse(id);
                guidCache.Add(id, guid);
            }

            return TryGetObject(guid, out item);
        }

        public bool TryGetObject<T>(Guid id, out T item) where T : GameDatabaseObject
        {
            if (objects.TryGetValue(id, out GameDatabaseObject value) && value is T t)
            {
                item = t;
                return true;
            }

            item = null;
            return false;
        }
    }
}