namespace SupportPlatform.Application.Common;

/// <summary>
/// A resource does not exist within the caller's scope. Mapped to <c>404</c>; saved-query access
/// to another user's record raises this (not <c>403</c>) so existence is not leaked
/// (<c>docs/contracts/api-contract.md</c> §5).
/// </summary>
public sealed class NotFoundException(string message) : Exception(message);
