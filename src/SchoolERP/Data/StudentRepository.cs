using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SchoolERP.Models;

namespace SchoolERP.Data
{
    public class StudentRepository
    {
        public List<Student> GetAll()
        {
            const string sql = @"
SELECT StudentID, RegistrationNo, Name, FatherName, DOB, Class, Address, Phone, AdmissionDate
FROM dbo.Students
ORDER BY StudentID DESC;";

            var students = new List<Student>();

            using (var connection = Database.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add(MapStudent(reader));
                    }
                }
            }

            return students;
        }

        public Student GetById(int studentId)
        {
            const string sql = @"
SELECT StudentID, RegistrationNo, Name, FatherName, DOB, Class, Address, Phone, AdmissionDate
FROM dbo.Students
WHERE StudentID = @StudentID;";

            using (var connection = Database.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@StudentID", studentId);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapStudent(reader);
                    }
                }
            }

            return null;
        }

        public List<Student> Search(string query)
        {
            const string sql = @"
SELECT StudentID, RegistrationNo, Name, FatherName, DOB, Class, Address, Phone, AdmissionDate
FROM dbo.Students
WHERE RegistrationNo LIKE @Query
   OR Name LIKE @Query
   OR FatherName LIKE @Query
   OR Class LIKE @Query
ORDER BY Name;";

            var students = new List<Student>();
            var searchTerm = "%" + (query ?? string.Empty).Trim() + "%";

            using (var connection = Database.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Query", searchTerm);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add(MapStudent(reader));
                    }
                }
            }

            return students;
        }

        public int Add(Student student)
        {
            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            const string sql = @"
INSERT INTO dbo.Students (RegistrationNo, Name, FatherName, DOB, Class, Address, Phone, AdmissionDate)
VALUES (@RegistrationNo, @Name, @FatherName, @DOB, @Class, @Address, @Phone, @AdmissionDate);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var connection = Database.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddStudentParameters(command, student);
                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        public bool Update(Student student)
        {
            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }

            const string sql = @"
UPDATE dbo.Students
SET RegistrationNo = @RegistrationNo,
    Name = @Name,
    FatherName = @FatherName,
    DOB = @DOB,
    Class = @Class,
    Address = @Address,
    Phone = @Phone,
    AdmissionDate = @AdmissionDate
WHERE StudentID = @StudentID;";

            using (var connection = Database.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                AddStudentParameters(command, student);
                command.Parameters.AddWithValue("@StudentID", student.StudentID);
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int studentId)
        {
            const string sql = "DELETE FROM dbo.Students WHERE StudentID = @StudentID;";

            using (var connection = Database.GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@StudentID", studentId);
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        private static void AddStudentParameters(SqlCommand command, Student student)
        {
            command.Parameters.AddWithValue("@RegistrationNo", (object)student.RegistrationNo ?? DBNull.Value);
            command.Parameters.AddWithValue("@Name", (object)student.Name ?? DBNull.Value);
            command.Parameters.AddWithValue("@FatherName", (object)student.FatherName ?? DBNull.Value);
            command.Parameters.AddWithValue("@DOB", (object)student.DOB ?? DBNull.Value);
            command.Parameters.AddWithValue("@Class", (object)student.Class ?? DBNull.Value);
            command.Parameters.AddWithValue("@Address", (object)student.Address ?? DBNull.Value);
            command.Parameters.AddWithValue("@Phone", (object)student.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@AdmissionDate", (object)student.AdmissionDate ?? DBNull.Value);
        }

        private static Student MapStudent(SqlDataReader reader)
        {
            return new Student
            {
                StudentID = reader.GetInt32(reader.GetOrdinal("StudentID")),
                RegistrationNo = reader["RegistrationNo"] as string,
                Name = reader["Name"] as string,
                FatherName = reader["FatherName"] as string,
                DOB = reader["DOB"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DOB"]),
                Class = reader["Class"] as string,
                Address = reader["Address"] as string,
                Phone = reader["Phone"] as string,
                AdmissionDate = reader["AdmissionDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["AdmissionDate"])
            };
        }
    }
}
