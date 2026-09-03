namespace SupportPlatform.Application.Common;

/// <summary>
/// The caller is authenticated but not permitted: the requested tenant is not theirs, or their
/// role lacks the required permission. Mapped to <c>403 forbidden</c>
/// (<c>docs/contracts/error-model.md</c>). Distinct from <see cref="NotFoundException"/>, which
/// hides existence for out-of-scope resources.
/// </summary>
public sealed class ForbiddenException(string message) : Exception(message);
