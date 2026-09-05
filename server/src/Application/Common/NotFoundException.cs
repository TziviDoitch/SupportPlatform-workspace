namespace SupportPlatform.Application.Common;

// A resource does not exist within the caller's scope. Mapped to 404; out-of-scope saved-query
// access raises this (not 403) so existence is not leaked.
public sealed class NotFoundException(string message) : Exception(message);
