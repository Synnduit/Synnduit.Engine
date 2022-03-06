namespace Synnduit.Persistence
{
    /// <summary>
    /// A safe wrapper around the <see cref="IRepository" /> implementation; ensures that
    /// data returned by individual methods is (formally) valid (e.g., no null references
    /// returned).
    /// </summary>
    internal interface ISafeRepository : IRepository
    {
    }
}
