namespace TickTask.Server.Data.Helpers
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        // New roles can be added here, and must be seeded in SeedRolesAsync() in AppDbInitializer
    }
}
