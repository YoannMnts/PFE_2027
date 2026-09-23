using PFE.Core.Scripts.Enemy;
using PFE.Editor._Project.Scripts.Editor.DatabaseBrowser;

namespace PFE.Editor._Project.Scripts.Editor.Tools
{
    public sealed class EnemyBrowserTool : DatabaseBrowserTool<EnemyData>
    {
        public override string DisplayName => "Enemy";
        public override string Icon => "♛";
        public override int Order => 10;
    }
}
