using LW.BkEndDb;
using LW.BkEndModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<LwDatabase>(options =>
	{
		options.UseSqlServer(builder.Configuration
			.GetConnectionString("DefaultConnection"),
			sqlServerOptionsAction => sqlServerOptionsAction.EnableRetryOnFailure(
			maxRetryCount: 10,
			maxRetryDelay: TimeSpan.FromSeconds(10),
			errorNumbersToAdd: null)
		);
	});

builder.Services.AddIdentity<User, IdentityRole>(options =>
	{
		options.Password.RequiredLength = 8;
		options.Password.RequireDigit = true;
		options.Password.RequireNonAlphanumeric = true;
		options.Password.RequireUppercase = true;
		options.SignIn.RequireConfirmedEmail = true;
	})
	.AddEntityFrameworkStores<LwDatabase>()
	.AddDefaultTokenProviders();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
