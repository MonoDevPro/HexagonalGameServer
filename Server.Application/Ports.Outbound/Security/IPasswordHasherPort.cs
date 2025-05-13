namespace Server.Application.Ports.Outbound.Security;

public interface IPasswordHasherPort
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}