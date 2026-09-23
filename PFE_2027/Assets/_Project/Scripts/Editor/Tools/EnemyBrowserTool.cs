using PFE.Core;
using PFE.Editor.DatabaseBrowser;

namespace PFE.Editor.Tools
{
    public sealed class EnemyBrowserTool : DatabaseBrowserTool<EnemyData>
    {
        public override string DisplayName => "Enemy";
        public override string Icon => "♛";
        public override int Order => 10;
    }
}
