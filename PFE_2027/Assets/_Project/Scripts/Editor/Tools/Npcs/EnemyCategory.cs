using PFE.Core.Scripts.Enemy;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Npcs
{
    public sealed class EnemyCategory : NpcCategory<EnemyData>
    {
        public EnemyCategory() : base("Enemy") { }

        public override string DataScriptFolder => "Assets/_Project/Scripts/Core/NPCs/Enemy";
        public override string BehaviourScriptFolder => "Assets/_Project/Scripts/Gameplay/Npcs/Enemies";
        public override string AssetFolder => "Assets/_Project/Resources/Database/Npc/EnemyData";
        public override string DataNamespace => "PFE.Core.Scripts.Enemy";

        public override string BuildDataScript(string typeName) =>
$@"using UnityEngine;

namespace PFE.Core.Scripts.Enemy
{{
    // Data of the {typeName} enemy: only what is specific to it (shared fields live in EnemyData / NpcData)
    [CreateAssetMenu(menuName = ""PFE/Enemy/{typeName}"")]
    public class {typeName}Data : EnemyData
    {{
    }}
}}
";

        public override string BuildBehaviourScript(string typeName) =>
$@"using PFE.Core.Scripts.Enemy;

namespace PFE.Gameplay.Scripts.Enemy
{{
    // Rules of the {typeName} enemy (stateless, its state lives in EnemyInstance).
    // Override the Enemy<TData> defaults here: RedirectTargetPosition, ModifyHealth, Dying...
    public partial class {typeName} : Enemy<{typeName}Data>
    {{
    }}
}}
";
    }
}
