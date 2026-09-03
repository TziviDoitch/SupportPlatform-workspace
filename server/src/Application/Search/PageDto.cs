namespace SupportPlatform.Application.Search;

/// <summary>Echoes <see cref="Paging"/> plus <see cref="TotalRows"/> — total groups before paging.</summary>
public sealed record PageDto(int PageNumber, int PageSize, int TotalRows);
