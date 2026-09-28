namespace cse325_group_project.Services;

// Contract for securely hashing and verifying passwords using industry-standard cryptography.
public interface IPasswordHasher
{
    // Hashes a plaintext password with a salt and key derivation.
    string HashPassword(string password);

    // Verifies whether a plaintext password matches a stored cryptographic hash.
    bool VerifyPassword(string password, string hashedPassword);
}

// Password hashing implementation utilizing BCrypt.Net-Next with SHA384 pre-hashing (Enhanced)
// and a recommended work factor of 12.
public class PasswordHasher : IPasswordHasher
{
    // Generates a salted and hashed representation of the given password using enhanced BCrypt.
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, 12);
    }

    // Verifies that the provided plaintext password corresponds to the given BCrypt hash.
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
    }
}
