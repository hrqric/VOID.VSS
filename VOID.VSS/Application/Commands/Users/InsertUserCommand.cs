namespace VOID.VSS.Application.Commands.Users;

public class InsertUserCommand
{
    public Guid UserId { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; } = true;
}