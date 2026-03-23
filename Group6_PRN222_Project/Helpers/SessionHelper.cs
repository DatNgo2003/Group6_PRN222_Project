namespace Project_PRN222.Helpers
{
    public static class SessionHelper
    {
        private const string KeyUserID   = "UserID";
        private const string KeyUserName = "UserName";
        private const string KeyFullName = "FullName";
        private const string KeyRole     = "Role";

        public static void SetUser(ISession session, int userID, string username, string fullName, string role)
        {
            session.SetInt32(KeyUserID, userID);
            session.SetString(KeyUserName, username);
            session.SetString(KeyFullName, fullName);
            session.SetString(KeyRole, role);
        }

        public static void Clear(ISession session) => session.Clear();

        public static int?   GetUserID(ISession session)   => session.GetInt32(KeyUserID);
        public static string? GetUserName(ISession session) => session.GetString(KeyUserName);
        public static string? GetFullName(ISession session) => session.GetString(KeyFullName);
        public static string? GetRole(ISession session)     => session.GetString(KeyRole);

        public static bool IsLoggedIn(ISession session)  => session.GetInt32(KeyUserID).HasValue;
        public static bool IsAdmin(ISession session)     => GetRole(session) == "Admin";
        public static bool IsOrganizer(ISession session) => GetRole(session) == "Organizer" || GetRole(session) == "Event Manager";
    }
}
