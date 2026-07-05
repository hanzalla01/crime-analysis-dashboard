using Crime_analysis.Models;

namespace Crime_analysis.Services
{
    // ─────────────────────────────────────────
    //  AuthService
    //  Handles login and stores current user
    //  throughout the app session
    // ─────────────────────────────────────────
    public class AuthService
    {
        // ── Currently logged in user ──────────
        // Any window can check this to know
        // who is logged in and what role they have
        public static User? CurrentUser { get; private set; }

        // ── Check if logged in ────────────────
        public static bool IsLoggedIn
            => CurrentUser != null;

        // ── Check if Admin ────────────────────
        public static bool IsAdmin
            => CurrentUser?.Role == "Admin";

        // ─────────────────────────────────────
        //  LOGIN
        //  Returns true if username + password
        //  match a record in the database
        // ─────────────────────────────────────
        public static bool Login(string username, string password)
        {
            var db = new DatabaseHelper();
            var data = db.GetUser(username, password);

            if (data.Rows.Count > 0)
            {
                // Save logged in user
                CurrentUser = new User
                {
                    UserId = (int)data.Rows[0]["UserId"],
                    Username = data.Rows[0]["Username"].ToString(),
                    Role = data.Rows[0]["Role"].ToString(),
                    FullName = data.Rows[0]["FullName"].ToString()
                };
                return true;
            }

            return false;
        }

        // ─────────────────────────────────────
        //  LOGOUT
        //  Clears the current user
        // ─────────────────────────────────────
        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}