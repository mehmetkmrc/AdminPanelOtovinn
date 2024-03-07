using AuthManual.Data;
using AuthManual.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using System.Data;

namespace AuthManual.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        
        private readonly string _connectionString;

        public UserController()
        {
            
            _connectionString = "DefaultConnection"; // Veritabanı bağlantı dizesiburaya eklendi
        }

        public IActionResult Index()
        {
            var userList = GetUserListFromDatabase();
            var userRoles = GetUserRolesFromDatabase();
            var roles = GetRolesFromDatabase();
            foreach (var user in userList)
            {
                var role = userRoles.FirstOrDefault(u => u.UserId == user.Id.ToString());
                if (role == null)
                {
                    user.Role = new Role { Name = "None" };
                }
                else
                {
                    user.Role = roles.FirstOrDefault(u => u.Id == role.RoleId) ?? new Role { Name = "None" };
                }
            }


            return View(userList);
        }


        [HttpGet]
        public IActionResult Edit(string userId)
        {
            var dbUser = GetUserByIdFromDatabase(userId);
            if (dbUser == null)
            {
                return NotFound();
            }

            var userRole = GetUserRoleFromDatabase(userId);
            var roles = GetRolesFromDatabase();
            if (userRole != null)
            {
                dbUser.RoleId = userRole.RoleId;
            }
            SelectListItem item = new SelectListItem();
            dbUser.RoleList = roles.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = u.Name,
                Value = u.Id, 
            }).ToList();

            return View(dbUser);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(User user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dbUser = GetUserByIdFromDatabase(user.Id.ToString());
                    if (dbUser == null)
                    {
                        return NotFound();
                    }

                    var userRole = GetUserRoleFromDatabase(user.Id.ToString());
                    if (userRole != null) // user already has a role
                    {
                        var previousRoleName = GetRoleNameByIdFromDatabase(userRole.RoleId.ToString());
                        // Removing old role from user
                        RemoveUserFromRoleInDatabase(dbUser.Id.ToString(), previousRoleName);
                    }

                    // Adding new role
                    AddUserToRoleInDatabase(dbUser.Id.ToString(), user.RoleId.ToString());

                    // Updating the name
                    dbUser.Name = user.Name;

                    TempData["success"] = "User Edited Successfully.";

                    return RedirectToAction(nameof(Index));
                }

                //user.RoleList = GetRolesFromDatabase().Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                //{
                //    Text = u.Name,
                //    Value = u.Id.ToString(),
                //});

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string userId)
        {
            try
            {
                var dbUser = GetUserByIdFromDatabase(userId);
                if (dbUser == null)
                {
                    return NotFound();
                }

                DeleteUserFromDatabase(dbUser.Id.ToString());
                TempData["success"] = "User deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private List<User> GetUserListFromDatabase()
        {
            var userList = new List<User>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM Users", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userList.Add(new User
                            {
                                Id = Guid.Parse(reader["Id"].ToString()),
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                }
            }
            return userList;
        }

        private List<Role> GetUserRolesFromDatabase()
        {
            var userRoles = new List<Role>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM Roles", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userRoles.Add(new Role
                            {
                                UserId = reader["UserId"].ToString(),
                                RoleId = reader["RoleId"].ToString()
                            });
                        }
                    }
                }
            }
            return userRoles;
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
                            roles.Add(new Role
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                }
            }
            return roles;
        }

        private User GetUserByIdFromDatabase(string userId)
        {
            User user = null;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM Users WHERE Id = @UserId", connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = Guid.Parse(reader["Id"].ToString()),
                                Name = reader["Name"].ToString()
                            };
                        }
                    }
                }
            }
            return user;
        }

        private User GetUserRoleFromDatabase(string userId)
        {
            User userRole = null;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM Roles WHERE UserId = @UserId", connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userRole = new User
                    {
                        Id = Guid.Parse(reader["UserId"].ToString()),
                        Role = new Role
                        {
                            UserId = reader["UserId"].ToString(),
                            RoleId = reader["RoleId"].ToString()
                        }
                    };
                        }
                    }
                }
            }
            return userRole;
        }

        private string GetRoleNameByIdFromDatabase(string roleId)
        {
            string roleName = null;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT Name FROM Roles WHERE Id = @RoleId", connection))
                {
                    cmd.Parameters.AddWithValue("@RoleId", roleId);
                    roleName = cmd.ExecuteScalar()?.ToString();
                }
            }
            return roleName;
        }

        private void RemoveUserFromRoleInDatabase(string userId, string roleName)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM UserRoles WHERE UserId = @UserId AND RoleId = (SELECT Id FROM Roles WHERE Name = @RoleName)", connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@RoleName", roleName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void AddUserToRoleInDatabase(string userId, string roleId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)", connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@RoleId", roleId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateUserLockoutInDatabase(string userId, DateTimeOffset? lockoutEnd)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("UPDATE Users SET LockoutEnd = @LockoutEnd WHERE Id = @UserId", connection))
                {
                    cmd.Parameters.AddWithValue("@LockoutEnd", lockoutEnd);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DeleteUserFromDatabase(string userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM Users WHERE Id = @UserId", connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
