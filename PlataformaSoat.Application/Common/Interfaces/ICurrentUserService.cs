namespace PlataformaSoat.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? IPAddress { get; }
    string? UserAgent { get; }
}
