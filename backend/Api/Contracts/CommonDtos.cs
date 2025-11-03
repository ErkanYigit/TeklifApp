namespace Api.Contracts;

public record Paged<T>(IEnumerable<T> Items, int TotalCount);


