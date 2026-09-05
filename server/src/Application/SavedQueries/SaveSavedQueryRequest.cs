using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.SavedQueries;

public sealed record SaveSavedQueryRequest(string Name, QueryDefinition Definition);
