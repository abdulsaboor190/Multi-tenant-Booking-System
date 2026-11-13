namespace BookWise.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);

    /// Pass a null hash when the user does not exist: the work is still done (to keep timing similar) and false is returned.
    bool Verify(string? hash, string password);
}
