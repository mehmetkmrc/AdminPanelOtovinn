using AuthManual.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace AuthManual.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        public AccountController(IConfiguration configuration)
        {
            _connectionString = "DefaultConnection"; // Veritabanı bağlantı dizesini buraya ekleyin
            _configuration = configuration;
        }
        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand("INSERT INTO Users (Name, Surname, MailAddress, Password, Role, EmailConfirmed, Status, CreatedDate, Last_Login) " +
                                                       "VALUES (@Name, @Surname, @MailAddress, @Password, @Role, @EmailConfirmed, @Status, @CreatedDate, @Last_Login)", connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", model.Name);
                        cmd.Parameters.AddWithValue("@Surname", model.Surname);
                        cmd.Parameters.AddWithValue("@MailAddress", model.MailAddress);
                        cmd.Parameters.AddWithValue("@Password", model.Password);
                        cmd.Parameters.AddWithValue("@Role", model.Role);
                        cmd.Parameters.AddWithValue("@EmailConfirmed", model.EmailConfirmed);
                        cmd.Parameters.AddWithValue("@Status", model.Status);
                        cmd.Parameters.AddWithValue("@CreatedDate", model.CreatedDate);
                        cmd.Parameters.AddWithValue("@Last_Login", model.Last_Login);

                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["Message"] = "Registration successful.";
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
                return View(model);
            }
        }
        private async Task SignInUser(User user, bool isPersistent)
        {
            // Oturum anahtarı oluştur
            string sessionToken = Guid.NewGuid().ToString();

            // Veritabanına oturum anahtarını kaydet
            string connectionString = "DefaultConnection";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // SQL sorgusu oluştur
                string sql = "UPDATE Users SET SessionToken = @SessionToken WHERE id = @UserId";

                // Sorguyu çalıştır
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("SessionToken", sessionToken);
                    command.Parameters.AddWithValue("UserId", user.Id);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        // Kullanıcıya oturum anahtarı ataması başarısız oldu
                        // Gerekirse uygun bir hata işlemi yapabilirsiniz
                        return;
                    }
                }
            }

            // Oturum anahtarını kullanıcıya bir şekilde iletin, örneğin çerez olarak
            // HttpContext.Current.Response.Cookies.Add(new HttpCookie("SessionToken", sessionToken));
        }
        [HttpGet] // Display all the properties the user has to enter
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                return View(model);
            }

            // Kullanıcıyı veritabanından kontrol et
            var user = GetUserByEmail(model.MailAddress);

            // Kullanıcı yoksa veya şifre uyuşmazsa
            if (user == null || user.Password != model.Password)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // Kullanıcı giriş yaptı, ancak e-postası doğrulanmamışsa, giriş yapmaktan alıkoy
            if (!user.EmailConfirmed)
            {
                TempData["Message"] = "Email adresinizi doğrulamadan giriş yapamazsınız. Lütfen e-postanızı kontrol edin ve doğrulama linkine tıklayın.";
                return RedirectToAction(nameof(Login));
            }

            // Eğer kullanıcı doğrulanmışsa normal şekilde giriş yapılabilir.
            return RedirectToAction("Index", "Screen");
        }

        // Kullanıcıyı e-posta adresine göre veritabanından alır
        private User GetUserByEmail(string email)
        {
            
            string connectionStrings = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new NpgsqlConnection(connectionStrings))
            {
                connection.Open();

                // SQL sorgusu oluştur
                string sql = "SELECT * FROM Users WHERE MailAddress = @Email";

                // Sorguyu çalıştır
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("Email", email);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = Guid.Parse(reader["id"].ToString()),
                                Name = reader["Name"].ToString(),
                                Surname = reader["Surname"].ToString(),
                                MailAddress = reader["MailAddress"].ToString(),
                                Password = reader["Password"].ToString(),
                                RoleId = Guid.Parse(reader["RoleId"].ToString()),
                                EmailConfirmed = Convert.ToBoolean(reader["EmailConfirmed"]),
                                Status = Convert.ToInt32(reader["Status"]),
                                CreatedDate = Convert.ToDateTime(reader["createddate"]),
                                Last_Login = Convert.ToDateTime(reader["Last_Login"])
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout(Guid userId)
		{
			try
			{
				await SignOutUser(userId);
				return RedirectToAction("Login", "Account");
			}
			catch (Exception ex)
			{
				// Log the exception for debugging
				Console.WriteLine($"Logout error: {ex.Message}");
				// Consider adding a user-friendly error message if appropriate
				return RedirectToAction("Login", "Account"); // Redirect to a general error page or home page
			}
		}

		private async Task SignOutUser(Guid userId)
		{
			string connectionString = _configuration.GetConnectionString("DefaultConnection");
			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();

				// Kullanıcıyı al, eğer kullanıcı bulunamazsa null olabilir
				var user = await GetUserByIdAsync(userId);

				// Kullanıcı null değilse devam edin
				if (user != null)
				{
					var userOffset = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId).GetUtcOffset(DateTime.UtcNow);
					DateTime lastLogin = DateTime.UtcNow.Add(userOffset);

					// Last_Login alanını güncelle
					var sqlCommand = new NpgsqlCommand("UPDATE Users SET Last_Login = @lastLogin WHERE Id = @Id", connection);
					sqlCommand.Parameters.AddWithValue("@lastLogin", lastLogin);
					sqlCommand.Parameters.AddWithValue("@Id", userId);

					int rowsAffected = await sqlCommand.ExecuteNonQueryAsync();

					if (rowsAffected == 0)
					{
						Console.WriteLine("Last_Login güncellenemedi!");
					}
				}
				else
				{
					// Kullanıcı bulunamadıysa uygun şekilde işlem yapın, örneğin loglayın veya hata mesajı gösterin
					Console.WriteLine("Kullanıcı bulunamadı!");
				}
			}
		}

		private async Task<User> GetUserByIdAsync(Guid userId)
		{
			string connectionString = _configuration.GetConnectionString("DefaultConnection");
			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();

				using (var command = new NpgsqlCommand("SELECT Id, Name, Surname, MailAddress, Password, RoleId, EmailConfirmed, Status, CreatedDate, Last_Login, TimeZoneId FROM Users WHERE Id = @Id", connection))
				{
					command.Parameters.AddWithValue("@Id", userId);

					using (var reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							return new User
							{
								Id = reader.GetGuid(reader.GetOrdinal("Id")),
								Name = reader.GetString(reader.GetOrdinal("Name")),
								Surname = reader.GetString(reader.GetOrdinal("Surname")),
								MailAddress = reader.GetString(reader.GetOrdinal("MailAddress")),
								Password = reader.GetString(reader.GetOrdinal("Password")),
								RoleId = reader.GetGuid(reader.GetOrdinal("RoleId")),
								EmailConfirmed = reader.GetBoolean(reader.GetOrdinal("EmailConfirmed")),
								Status = reader.GetInt32(reader.GetOrdinal("Status")),
								CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
								Last_Login = reader.GetDateTime(reader.GetOrdinal("Last_Login")),
								TimeZoneId = reader.GetString(reader.GetOrdinal("TimeZoneId"))
							};
						}
						else
						{
							// Handle user not found scenario (e.g., log an error)
							return null;
						}
					}
				}
			}
		}



		// Other methods like Login, ForgotPassword, ResetPassword, etc. can be implemented similarly
	}
}
