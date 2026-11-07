namespace PFE.Core.Scripts.ECS
{
    public partial class World
    {
        public QueryResult Query(in EntityQuery query, bool onlyActive = false)
        {
            return new QueryResult(this, query, onlyActive);
        }
    }
}