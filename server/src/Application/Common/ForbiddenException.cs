namespace SupportPlatform.Application.Common;

// Authenticated but not permitted: wrong tenant, or a role missing the required permission.
// Mapped to 403. Distinct from NotFoundException, which hides existence for out-of-scope resources.
public sealed class ForbiddenException(string message) : Exception(message);
