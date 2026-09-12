using PFE.Core;
using PFE.Editor.DatabaseBrowser;

namespace PFE.Editor.Tools
{
    public sealed class BossBrowserTool : DatabaseBrowserTool<BossData>
    {
        public override string DisplayName => "Boss";
        public override string Icon => "♛";
        public override int Order => 10;
    }
}
