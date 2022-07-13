using UserAuthMinimalApi.Models;

namespace UserAuthMinimalApi.Repositories;

public static class UserRepository
{
    public static User Find(string username, string password)
    {
        var users = new List<User>()
        {
            new User() { Id = 42, Username = "manager", Password = "&u*eVFG95%", Role = "manager" },
            new User() { Id = 42, Username = "operator", Password = "3!xeTwVNvc", Role = "operator" },
        };
        return users.FirstOrDefault(user => user.Username.ToLower() == username.ToLower() && user.Password == password);
    }
}