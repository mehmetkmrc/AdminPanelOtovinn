using AuthManual.Models;

using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace AuthManual.Controllers
{
   
    public class RolesController : Controller
    {

        private readonly string _connectionString;

        public RolesController()
        {

            _connectionString = "DefaultConnection"; // Veritabanı bağlantı dizesini buraya ekleyin
        }

        public IActionResult Index()
        {
            var roles = GetRolesFromDatabase();
            return View(roles);
        }

        [HttpGet]
        public IActionResult Upsert(string id)
        {
            if (String.IsNullOrEmpty(id)) // if id is null this is create
            {
                return View();
            }
            else // if not it is update
            {
                var role = GetRoleById(id);
                return View(role);
            }
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(Role role)
        {
            try
            {
                if (RoleExists(role.Name))
                {
                    TempData["error"] = "Role already exists.";
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrEmpty(role.Id))
                {
                    // create
                    AddRoleToDatabase(role.Name);
                    TempData["success"] = "Role created successfully.";
                }
                else
                {
                    // update
                    UpdateRoleInDatabase(role.Id, role.Name);
                    TempData["success"] = "Role updated successfully.";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Delete(string id)
        {
            try
            {
                if (id == null)
                {
                    TempData["error"] = "Role ID is null.";
                    return RedirectToAction(nameof(Index));
                }

                if (RoleHasUsers(id))
                {
                    TempData["error"] = "Cannot delete role. There are users with this role.";
                    return RedirectToAction(nameof(Index));
                }

                DeleteRoleFromDatabase(id);
                TempData["success"] = "Role deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private List<Role> GetRolesFromDatabase()
        {
            var roles = new List<Role>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM Roles", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var role = new Role
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString()
                            };
                            roles.Add(role);
                        }
                    }
                }
            }

            return roles;
        }

        private Role GetRoleById(string id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM Roles WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Role
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        private bool RoleExists(string roleName)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Roles WHERE Name = @Name", connection))
                {
                    cmd.Parameters.AddWithValue("@Name", roleName);
                    var count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private void AddRoleToDatabase(string roleName)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO Roles (Id, Name) VALUES (@Id, @Name)", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                    cmd.Parameters.AddWithValue("@Name", roleName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateRoleInDatabase(string id, string roleName)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("UPDATE Roles SET Name = @Name WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Name", roleName);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DeleteRoleFromDatabase(string id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM Roles WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private bool RoleHasUsers(string id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Users WHERE Role = @RoleId", connection))
                {
                    cmd.Parameters.AddWithValue("@RoleId", id);
                    var count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }
    }
}
