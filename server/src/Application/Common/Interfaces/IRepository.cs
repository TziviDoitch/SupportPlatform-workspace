namespace SupportPlatform.Application.Common.Interfaces;

// Read-only persistence seam for a small reference set loaded whole. Deliberately minimal — no
// query composition, no writes; purpose-built repositories keep their own scoped reads.
public interface IRepository<T> where T : class
{
    Task<IReadOnlyList<T>> ListAllAsync(CancellationToken ct = default);
}
