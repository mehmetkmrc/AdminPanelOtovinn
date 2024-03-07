var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();






builder.Services.AddRazorPages(); // Razor Pages hizmetlerini ekledik.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();



app.UseEndpoints(endpoints =>
{
    

    endpoints.MapControllerRoute(
        name: "login",
        pattern: "{controller=Account}/{action=Login}");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Screen}/{action=Index}/{id?}");



    endpoints.MapRazorPages();
});

app.Run();
