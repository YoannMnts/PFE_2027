// Copyright (c) 2026 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Synty.SidekickCharacters
{
    /// <summary>
    ///     Copies the Sidekick database into a Resources TextAsset (.bytes) before every build, so that player builds
    ///     can open it read-only in memory (see DatabaseManager.GetDbConnection).
    /// </summary>
    public class DatabaseBuildSync : IPreprocessBuildWithReport
    {
        private const string _DATABASE_PATH = "Assets/Synty/SidekickCharacters/Database/Side_Kick_Data.db";
        private const string _RUNTIME_DATABASE_PATH = "Assets/Synty/SidekickCharacters/Resources/Database/Side_Kick_Data.bytes";

        public int callbackOrder => 0;

        /// <inheritdoc cref="IPreprocessBuildWithReport.OnPreprocessBuild"/>
        public void OnPreprocessBuild(BuildReport report)
        {
            SyncRuntimeDatabase();
        }

        /// <summary>
        ///     Copies the Sidekick database to the runtime Resources location, replacing any existing copy.
        ///     Also available as the "Sync Runtime Database" button in the Sidekick tool's Options tab.
        /// </summary>
        public static void SyncRuntimeDatabase()
        {
            if (!File.Exists(_DATABASE_PATH))
            {
                Debug.LogError($"Sidekick database not found at '{_DATABASE_PATH}'; unable to sync the runtime database.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_RUNTIME_DATABASE_PATH));
            File.Copy(_DATABASE_PATH, _RUNTIME_DATABASE_PATH, true);
            AssetDatabase.ImportAsset(_RUNTIME_DATABASE_PATH);
        }
    }
}
