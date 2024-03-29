﻿using LW.BkEndDb;
using LW.BkEndLogic.Commons;
using LW.BkEndLogic.Commons.Interfaces;
using LW.BkEndModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace LW.BkEndApi
{
	public class MockStartup
	{
		public IConfiguration Configuration { get; }

		public MockStartup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			var rsaKey = RSA.Create();
			rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);
			// Add services to the container.
			services.AddDbContext<LwDBContext>(options =>
			{
				options.UseSqlServer(Configuration
					.GetConnectionString("DefaultConnection"),
					sqlServerOptionsAction => sqlServerOptionsAction.EnableRetryOnFailure(
					maxRetryCount: 10,
					maxRetryDelay: TimeSpan.FromSeconds(10),
					errorNumbersToAdd: null)
				);
			});

			services.AddIdentity<User, Role>(options =>
			{
				options.Password.RequiredLength = 8;
				options.Password.RequireDigit = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequireUppercase = true;
				options.SignIn.RequireConfirmedEmail = false;
			})
				.AddEntityFrameworkStores<LwDBContext>()
				.AddDefaultTokenProviders();

			services.AddControllers();
			// Configure JWT authentication
			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateIssuerSigningKey = true,
					ValidateLifetime = true,
					ValidIssuer = Configuration["Jwt:Issuer"],
					ValidAudience = Configuration["Jwt:Issuer"],
					IssuerSigningKey = new RsaSecurityKey(rsaKey),
					ClockSkew = TimeSpan.Zero
				};
				options.MapInboundClaims = false;
			});

			services.AddScoped<ITokenService, TokenService>();
			services.AddScoped<IEmailSender, EmailSender>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
