using Microsoft.EntityFrameworkCore;
using PersonalizedLibraryAPI.Data;
using Microsoft.AspNetCore.Identity;
using PersonalizedLibraryAPI.Repository;
using PersonalizedLibraryAPI.Repository.IRepository;
using PersonalizedLibraryAPI.Data;
using PersonalizedLibraryAPI.Models;

var builder = WebApplication.CreateBuilder(args);
//Adding identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<DBContext>()
    .AddDefaultTokenProviders();
        
// Add services to the container.
builder.Services.AddRazorPages();

// Add DbContext from the API project
builder.Services.AddDbContext<PersonalizedLibraryAPI.Data.DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.UseAuthentication();
app.UseDeveloperExceptionPage();
app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});

app.Run();
