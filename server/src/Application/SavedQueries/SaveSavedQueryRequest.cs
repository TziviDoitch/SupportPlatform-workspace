using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.SavedQueries;

/// <summary>Body of <c>POST</c> / <c>PUT /api/saved-queries</c>. The definition is validated
/// exactly like <c>POST /api/search</c>.</summary>
public sealed record SaveSavedQueryRequest(string Name, QueryDefinition Definition);
