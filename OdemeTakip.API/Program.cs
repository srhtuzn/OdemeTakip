using OdemeTakip.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔧 EF Core bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔧 Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// ⚙️ Pipeline ayarları
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Razor Pages ve Controller'lar
app.MapRazorPages();  // İstersen bunu bırak veya kaldır
// app.MapControllers(); // API controller eklersen aktif edersin

app.Run();
