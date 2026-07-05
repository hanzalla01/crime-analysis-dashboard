using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Crime_analysis.Services
{
    public class DatabaseHelper
    {
        // ── Change server name if needed ──
        private readonly string _connectionString =
            "Server=DESKTOP-OJP2OV5\\SQLEXPRESS;" +
            "Database=CrimeAnalysis;" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True;";

        // ═════════════════════════════════════════
        //  TEST CONNECTION
        // ═════════════════════════════════════════
        public bool TestConnection()
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                return true;
            }
            catch { return false; }
        }

        // ═════════════════════════════════════════
        //  USER QUERIES  (for login system)
        // ═════════════════════════════════════════

        public DataTable GetUser(string username, string password)
        {
            string sql = @"
                SELECT UserId, Username, Role, FullName
                FROM   Users
                WHERE  Username = @Username
                AND    Password = @Password";

            return RunQuery(sql,
                ("@Username", username),
                ("@Password", password));
        }

        public DataTable GetAllUsers()
        {
            return RunQuery("SELECT UserId, Username, Role, FullName, CreatedAt FROM Users");
        }

        public void AddUser(string username, string password,
                            string role, string fullName)
        {
            string sql = @"
                INSERT INTO Users (Username, Password, Role, FullName)
                VALUES (@Username, @Password, @Role, @FullName)";

            RunNonQuery(sql,
                ("@Username", username),
                ("@Password", password),
                ("@Role", role),
                ("@FullName", fullName));
        }

        public void UpdateUser(int userId, string username,
                               string role, string fullName)
        {
            string sql = @"
                UPDATE Users
                SET    Username = @Username,
                       Role     = @Role,
                       FullName = @FullName
                WHERE  UserId   = @UserId";

            RunNonQuery(sql,
                ("@UserId", userId),
                ("@Username", username),
                ("@Role", role),
                ("@FullName", fullName));
        }

        public void DeleteUser(int userId)
        {
            RunNonQuery("DELETE FROM Users WHERE UserId = @UserId",
                ("@UserId", userId));
        }

        // ═════════════════════════════════════════
        //  CRIME QUERIES
        // ═════════════════════════════════════════

        public DataTable GetAllCrimes(string city = "All",
                                      string type = "All",
                                      string severity = "All",
                                      string status = "All",
                                      string from = "2024-01-01",
                                      string to = "2024-12-31")
        {
            string sql = @"
                SELECT c.CrimeId,
                       ct.TypeName,
                       a.AreaName,
                       a.City,
                       o.FullName  AS OfficerName,
                       c.CrimeDate,
                       c.Severity,
                       c.Status,
                       c.Description
                FROM   Crimes c
                JOIN   Areas      a  ON c.AreaId    = a.AreaId
                JOIN   CrimeTypes ct ON c.TypeId    = ct.TypeId
                JOIN   Officers   o  ON c.OfficerId = o.OfficerId
                WHERE  (@City     = 'All' OR a.City      = @City)
                AND    (@Type     = 'All' OR ct.TypeName = @Type)
                AND    (@Severity = 'All' OR c.Severity  = @Severity)
                AND    (@Status   = 'All' OR c.Status    = @Status)
                AND    c.CrimeDate BETWEEN @From AND @To
                ORDER  BY c.CrimeDate DESC";

            return RunQuery(sql,
                ("@City", city),
                ("@Type", type),
                ("@Severity", severity),
                ("@Status", status),
                ("@From", from),
                ("@To", to));
        }

        public void AddCrime(int typeId, int areaId,
                             int officerId, string date,
                             string severity, string status,
                             string description)
        {
            string sql = @"
                INSERT INTO Crimes
                    (TypeId, AreaId, OfficerId, CrimeDate,
                     Severity, Status, Description)
                VALUES
                    (@TypeId, @AreaId, @OfficerId, @CrimeDate,
                     @Severity, @Status, @Description)";

            RunNonQuery(sql,
                ("@TypeId", typeId),
                ("@AreaId", areaId),
                ("@OfficerId", officerId),
                ("@CrimeDate", date),
                ("@Severity", severity),
                ("@Status", status),
                ("@Description", description));
        }

        public void UpdateCrime(int crimeId, int typeId,
                                int areaId, int officerId,
                                string date, string severity,
                                string status, string description)
        {
            string sql = @"
                UPDATE Crimes
                SET    TypeId      = @TypeId,
                       AreaId      = @AreaId,
                       OfficerId   = @OfficerId,
                       CrimeDate   = @CrimeDate,
                       Severity    = @Severity,
                       Status      = @Status,
                       Description = @Description
                WHERE  CrimeId     = @CrimeId";

            RunNonQuery(sql,
                ("@CrimeId", crimeId),
                ("@TypeId", typeId),
                ("@AreaId", areaId),
                ("@OfficerId", officerId),
                ("@CrimeDate", date),
                ("@Severity", severity),
                ("@Status", status),
                ("@Description", description));
        }

        public void DeleteCrime(int crimeId)
        {
            // Delete suspects first (foreign key)
            RunNonQuery("DELETE FROM Suspects WHERE CrimeId = @CrimeId",
                ("@CrimeId", crimeId));

            RunNonQuery("DELETE FROM Crimes WHERE CrimeId = @CrimeId",
                ("@CrimeId", crimeId));
        }

        // ═════════════════════════════════════════
        //  DASHBOARD QUERIES  (charts + stats)
        // ═════════════════════════════════════════

        public int GetTotalCrimes(string city = "All",
                                  string from = "2024-01-01",
                                  string to = "2024-12-31")
        {
            string sql = @"
                SELECT COUNT(*)
                FROM   Crimes c
                JOIN   Areas a ON c.AreaId = a.AreaId
                WHERE  (@City = 'All' OR a.City = @City)
                AND    c.CrimeDate BETWEEN @From AND @To";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@City", city);
            cmd.Parameters.AddWithValue("@From", from);
            cmd.Parameters.AddWithValue("@To", to);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public DataTable GetCrimesByArea(string city = "All",
                                         string from = "2024-01-01",
                                         string to = "2024-12-31")
        {
            string sql = @"
                SELECT a.AreaName,
                       COUNT(*) AS CrimeCount
                FROM   Crimes c
                JOIN   Areas a ON c.AreaId = a.AreaId
                WHERE  (@City = 'All' OR a.City = @City)
                AND    c.CrimeDate BETWEEN @From AND @To
                GROUP  BY a.AreaName
                ORDER  BY CrimeCount DESC";

            return RunQuery(sql,
                ("@City", city),
                ("@From", from),
                ("@To", to));
        }

        public DataTable GetCrimesByType(string city = "All",
                                         string from = "2024-01-01",
                                         string to = "2024-12-31")
        {
            string sql = @"
                SELECT ct.TypeName,
                       COUNT(*) AS Total
                FROM   Crimes c
                JOIN   Areas      a  ON c.AreaId = a.AreaId
                JOIN   CrimeTypes ct ON c.TypeId = ct.TypeId
                WHERE  (@City = 'All' OR a.City = @City)
                AND    c.CrimeDate BETWEEN @From AND @To
                GROUP  BY ct.TypeName
                ORDER  BY Total DESC";

            return RunQuery(sql,
                ("@City", city),
                ("@From", from),
                ("@To", to));
        }

        public DataTable GetMonthlyTrend(string city = "All",
                                         string from = "2024-01-01",
                                         string to = "2024-12-31")
        {
            string sql = @"
                SELECT FORMAT(c.CrimeDate, 'yyyy-MM') AS Month,
                       COUNT(*)                        AS Total
                FROM   Crimes c
                JOIN   Areas a ON c.AreaId = a.AreaId
                WHERE  (@City = 'All' OR a.City = @City)
                AND    c.CrimeDate BETWEEN @From AND @To
                GROUP  BY FORMAT(c.CrimeDate, 'yyyy-MM')
                ORDER  BY Month";

            return RunQuery(sql,
                ("@City", city),
                ("@From", from),
                ("@To", to));
        }

        public DataTable GetHighRiskAreas(int threshold = 30)
        {
            string sql = @"
                SELECT a.AreaName,
                       a.City,
                       COUNT(*)        AS CrimeCount,
                       MAX(c.Severity) AS Severity,
                       (SELECT TOP 1 ct2.TypeName
                        FROM   Crimes     c2
                        JOIN   CrimeTypes ct2 ON c2.TypeId = ct2.TypeId
                        WHERE  c2.AreaId = a.AreaId
                        GROUP  BY ct2.TypeName
                        ORDER  BY COUNT(*) DESC) AS TopCrime
                FROM   Crimes c
                JOIN   Areas a ON c.AreaId = a.AreaId
                GROUP  BY a.AreaId, a.AreaName, a.City
                HAVING COUNT(*) > @Threshold
                ORDER  BY CrimeCount DESC";

            return RunQuery(sql, ("@Threshold", threshold));
        }

        public DataTable GetCrimesBySeverity(string city = "All")
        {
            string sql = @"
                SELECT c.Severity,
                       COUNT(*) AS Total
                FROM   Crimes c
                JOIN   Areas a ON c.AreaId = a.AreaId
                WHERE  (@City = 'All' OR a.City = @City)
                GROUP  BY c.Severity";

            return RunQuery(sql, ("@City", city));
        }

        // ═════════════════════════════════════════
        //  AREA QUERIES
        // ═════════════════════════════════════════

        public DataTable GetAllAreas()
        {
            return RunQuery("SELECT * FROM Areas ORDER BY City, AreaName");
        }

        // ═════════════════════════════════════════
        //  CRIMETYPE QUERIES
        // ═════════════════════════════════════════

        public DataTable GetAllCrimeTypes()
        {
            return RunQuery("SELECT * FROM CrimeTypes ORDER BY TypeName");
        }

        // ═════════════════════════════════════════
        //  OFFICER QUERIES
        // ═════════════════════════════════════════

        public DataTable GetAllOfficers()
        {
            return RunQuery("SELECT * FROM Officers ORDER BY FullName");
        }

        public void AddOfficer(string fullName, string badgeNumber,
                               string rank, string city,
                               string phone)
        {
            string sql = @"
                INSERT INTO Officers
                    (FullName, BadgeNumber, Rank, City, Phone)
                VALUES
                    (@FullName, @BadgeNumber, @Rank, @City, @Phone)";

            RunNonQuery(sql,
                ("@FullName", fullName),
                ("@BadgeNumber", badgeNumber),
                ("@Rank", rank),
                ("@City", city),
                ("@Phone", phone));
        }

        public void DeleteOfficer(int officerId)
        {
            RunNonQuery("DELETE FROM Officers WHERE OfficerId = @OfficerId",
                ("@OfficerId", officerId));
        }

        // ═════════════════════════════════════════
        //  SUSPECT QUERIES
        // ═════════════════════════════════════════

        public DataTable GetSuspectsByCrime(int crimeId)
        {
            return RunQuery(
                "SELECT * FROM Suspects WHERE CrimeId = @CrimeId",
                ("@CrimeId", crimeId));
        }

        public DataTable GetAllSuspects()
        {
            return RunQuery(@"
                SELECT s.SuspectId, s.FullName, s.Age,
                       s.Gender, s.Status,
                       c.CrimeId, a.City
                FROM   Suspects s
                JOIN   Crimes c ON s.CrimeId = c.CrimeId
                JOIN   Areas  a ON c.AreaId  = a.AreaId
                ORDER  BY s.FullName");
        }

        public void AddSuspect(int crimeId, string fullName,
                               int age, string gender,
                               string status, string description)
        {
            string sql = @"
                INSERT INTO Suspects
                    (CrimeId, FullName, Age, Gender, Status, Description)
                VALUES
                    (@CrimeId, @FullName, @Age, @Gender, @Status, @Description)";

            RunNonQuery(sql,
                ("@CrimeId", crimeId),
                ("@FullName", fullName),
                ("@Age", age),
                ("@Gender", gender),
                ("@Status", status),
                ("@Description", description));
        }

        public void DeleteSuspect(int suspectId)
        {
            RunNonQuery("DELETE FROM Suspects WHERE SuspectId = @SuspectId",
                ("@SuspectId", suspectId));
        }

        // ═════════════════════════════════════════
        //  REPORT QUERIES
        // ═════════════════════════════════════════

        public void LogReport(string generatedBy, string reportType,
                              string filePath)
        {
            string sql = @"
                INSERT INTO Reports (GeneratedBy, ReportType, FilePath)
                VALUES (@GeneratedBy, @ReportType, @FilePath)";

            RunNonQuery(sql,
                ("@GeneratedBy", generatedBy),
                ("@ReportType", reportType),
                ("@FilePath", filePath));
        }

        // ═════════════════════════════════════════
        //  PRIVATE HELPERS
        // ═════════════════════════════════════════

        private DataTable RunQuery(string sql,
            params (string Name, object Value)[] parameters)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            using var adapter = new SqlDataAdapter(cmd);

            foreach (var (name, value) in parameters)
                cmd.Parameters.AddWithValue(name, value);

            conn.Open();
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        private void RunNonQuery(string sql,
            params (string Name, object Value)[] parameters)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);

            foreach (var (name, value) in parameters)
                cmd.Parameters.AddWithValue(name, value);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    


    public int GetSolvedCrimes(string city = "All",
                                   string from = "2024-01-01",
                                   string to = "2024-12-31")
        {
            string sql = @"
                SELECT COUNT(*)
                FROM   Crimes c
                JOIN   Areas a ON c.AreaId = a.AreaId
                WHERE  (@City = 'All' OR a.City = @City)
                AND    c.Status    = 'Closed'
                AND    c.CrimeDate BETWEEN @From AND @To";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@City", city);
            cmd.Parameters.AddWithValue("@From", from);
            cmd.Parameters.AddWithValue("@To", to);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public int GetTotalSuspects()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Suspects", conn);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public int GetTotalOfficers()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Officers", conn);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }
}