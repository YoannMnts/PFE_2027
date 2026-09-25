// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using UnityEngine;

namespace Synty.SidekickCharacters
{
    /// <summary>
    ///     Editor-only undo target for the Sidekick tool. The tool's state lives in plain C# fields, so this
    ///     ScriptableObject holds a serialized snapshot of it purely so the Unity Undo system has an object to
    ///     record. Never saved to disk; owned and destroyed by ModularCharacterWindow.
    /// </summary>
    internal class SidekickUndoProxy : ScriptableObject
    {
        [SerializeField]
        public string CharacterSnapshot;

        /// <summary>
        ///     YAML dictionary of the presets-tab dropdown selections (row label -> preset name), captured together
        ///     with CharacterSnapshot so undo/redo restores what the preset dropdowns displayed. May be empty for
        ///     undo steps recorded before this field existed.
        /// </summary>
        [SerializeField]
        public string PresetSelections;
    }
}
