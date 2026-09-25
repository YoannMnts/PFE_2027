// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Synty.SidekickCharacters.UI
{
    public static class SkAssetOpenHandler
    {
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        ///     Opens .sk assets double clicked in the Project window in the Sidekick Character Tool.
        /// </summary>
        /// <param name="entityId">The entity ID of the opened asset.</param>
        /// <param name="line">The line number the asset was opened at (unused).</param>
        /// <returns>True if the asset was handled; otherwise false.</returns>
        [OnOpenAsset]
        public static bool OnOpenSkAsset(UnityEngine.EntityId entityId, int line)
        {
            return OpenSkAsset(AssetDatabase.GetAssetPath(entityId));
        }
#else
        /// <summary>
        ///     Opens .sk assets double clicked in the Project window in the Sidekick Character Tool.
        /// </summary>
        /// <param name="instanceID">The instance ID of the opened asset.</param>
        /// <param name="line">The line number the asset was opened at (unused).</param>
        /// <returns>True if the asset was handled; otherwise false.</returns>
        [OnOpenAsset]
        public static bool OnOpenSkAsset(int instanceID, int line)
        {
            return OpenSkAsset(AssetDatabase.GetAssetPath(instanceID));
        }
#endif

        /// <summary>
        ///     Opens the asset at the given path in the Sidekick Character Tool if it is a .sk file.
        /// </summary>
        /// <param name="path">The asset path of the opened asset.</param>
        /// <returns>True if the asset was handled; otherwise false.</returns>
        private static bool OpenSkAsset(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".sk", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            ModularCharacterWindow.OpenAndLoadCharacter(path);
            return true;
        }
    }
}
#endif
