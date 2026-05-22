using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SchoolERP.Data;

namespace SchoolERP.Services
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string ErrorMessage { get; set; }
    }

    public class AuthenticationService
    {
        public static byte[] HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is required.", nameof(password));
            }

            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public AuthResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthResult { Success = false, ErrorMessage = "Username and password are required." };
            }

            const string sql = @"
SELECT UserId, Username, FullName, PasswordHash
FROM dbo.Users
WHERE Username = @Username AND IsActive = 1;";

            using (var connection = Database.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Username", username.Trim());
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return new AuthResult { Success = false, ErrorMessage = "Invalid username or password." };
                    }

                    var userId = reader.GetInt32(reader.GetOrdinal("UserId"));
                    var storedHash = (byte[])reader["PasswordHash"];

                    if (!VerifyPassword(password, storedHash))
                    {
                        return new AuthResult { Success = false, ErrorMessage = "Invalid username or password." };
                    }

                    return new AuthResult
                    {
                        Success = true,
                        UserId = userId,
                        Username = reader["Username"] as string,
                        FullName = reader["FullName"] as string,
                        Roles = GetUserRoles(userId)
                    };
                }
            }
        }

        public bool UserHasRole(int userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return false;
            }

            var roles = GetUserRoles(userId);
            return roles.Any(r => string.Equals(r, roleName, StringComparison.OrdinalIgnoreCase));
        }

        private static bool VerifyPassword(string password, byte[] storedHash)
        {
            if (storedHash == null || storedHash.Length == 0)
            {
                return false;
            }

            var incomingHash = HashPassword(password);
            return incomingHash.SequenceEqual(storedHash);
        }

        private static List<string> GetUserRoles(int userId)
        {
            const string sql = @"
SELECT r.RoleName
FROM dbo.UserRoles ur
INNER JOIN dbo.Roles r ON r.RoleId = ur.RoleId
WHERE ur.UserId = @UserId;";

            var roles = new List<string>();

            using (var connection = Database.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roles.Add(reader["RoleName"] as string);
                    }
                }
            }

            return roles;
        }
    }
}
