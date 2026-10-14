using PFE.Core.Scripts.Pilgrims;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Npcs
{
    public sealed class PilgrimCategory : NpcCategory<PilgrimData>
    {
        public PilgrimCategory() : base("Pilgrim") { }

        public override string DataScriptFolder => "Assets/_Project/Scripts/Core/NPCs/Pilgrims";
        public override string BehaviourScriptFolder => "Assets/_Project/Scripts/Gameplay/Npcs/Pilgrims";
        public override string AssetFolder => "Assets/_Project/Resources/Database/Npc/AllyData";
        public override string DataNamespace => "PFE.Core.Scripts.Pilgrims";

        public override string BuildDataScript(string typeName) =>
$@"using UnityEngine;

namespace PFE.Core.Scripts.Pilgrims
{{
    // Data of the {typeName} pilgrim: only what is specific to it (shared fields live in PilgrimData / NpcData)
    [CreateAssetMenu(menuName = ""PFE/Npc/{typeName}"")]
    public class {typeName}Data : PilgrimData
    {{
    }}
}}
";

        public override string BuildBehaviourScript(string typeName) =>
$@"using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.NPCs;

namespace PFE.Gameplay.Scripts.Pilgrims
{{
    // Rules of the {typeName} pilgrim (stateless, its state lives in PilgrimInstance).
    // Override the Npc defaults here: RedirectTargetPosition, ModifyHealth, Dying...
    public partial class {typeName} : Npc<{typeName}Data, PilgrimInstance>
    {{
        protected override PilgrimInstance Create({typeName}Data data, NpcInstanceContext context)
            => new PilgrimInstance(data, context.splineContainer);
    }}
}}
";
    }
}
