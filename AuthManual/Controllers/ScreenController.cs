using AuthManual.Data;
using AuthManual.Models;

using Microsoft.AspNetCore.Mvc;
using Npgsql;


//[Authorize]
public class ScreenController : Controller
{
   
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    public ScreenController(IConfiguration configuration)
    {
        
        _connectionString = "DefaultConnection"; // Veritabanı bağlantı dizesini buraya ekleyin
        _configuration = configuration;
    }

    // GET: Screen/Create
    
    public IActionResult Create()
    {
        return View();
    }

    // POST: Screen/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Screen screen)
    {
        if (ModelState.IsValid)
        {
            return View(screen);
        }
		byte[] imageData = null;
		if (screen.ImageFile != null && screen.ImageFile.Length > 0)
		{
			using (var memoryStream = new MemoryStream())
			{
				await screen.ImageFile.CopyToAsync(memoryStream);
				imageData = memoryStream.ToArray();
			}
		}

		string connectionString = _configuration.GetConnectionString("DefaultConnection");
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = "INSERT INTO Screens (Id, Name, Status, ImageData) VALUES (@Id, @Name, @Status, @ImageData)";
                cmd.Parameters.AddWithValue("@Id", Guid.NewGuid()); // Yeni bir GUID oluştur
                cmd.Parameters.AddWithValue("@Name", screen.Name);
                cmd.Parameters.AddWithValue("@Status", screen.Status);
				cmd.Parameters.AddWithValue("@ImageData", imageData);
				

				cmd.ExecuteNonQuery();
            }
        }

        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public IActionResult Index(Guid id)
    {
        var screens = new List<Screen>();

        string connectionString = _configuration.GetConnectionString("DefaultConnection");
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM Screens WHERE Id = @Id"; // Id'ye göre filtreleme
            using (var cmd = new NpgsqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Id", id); // Id parametresini ekleyin
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var screen = new Screen
                        {
                            Id = Guid.Parse(reader["Id"].ToString()),
                            Name = reader["Name"].ToString(),
                            Status = Convert.ToInt32(reader["Status"]),

                        };

                        if (reader["Tab_ScreenId"] != DBNull.Value)
                        {
                            screen.Tab_ScreenId = Guid.Parse(reader["Tab_ScreenId"].ToString());
                        }

                        if (reader["ImageData"] != DBNull.Value)
                        {
                            screen.ImageData = (byte[])reader["ImageData"];
                        }

                        screens.Add(screen);
                    }
                }
            }
        }

        return View(screens);
    }


}
