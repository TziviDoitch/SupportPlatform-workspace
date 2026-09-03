namespace SupportPlatform.Application.Common.Interfaces;

/// <summary>
/// Read-only persistence seam for a small reference set that is loaded whole. Introduced in S8 to
/// lift the last direct <c>DbContext</c> injections out of the search path. Deliberately minimal —
/// no query composition, no writes; purpose-built repositories (e.g. <c>ISavedQueryRepository</c>)
/// keep their own scoped reads and mutations.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<IReadOnlyList<T>> ListAllAsync(CancellationToken ct = default);
}
