using LW.BkEndApi;
using LW.BkEndDb;
using LW.BkEndLogic.Commons;
using LW.BkEndLogic.Commons.Interfaces;
using LW.BkEndLogic.FirmaDiscUser;
using LW.BkEndLogic.HybridUser;
using LW.BkEndLogic.MasterUser;
using LW.BkEndLogic.RegularUser;
using LW.BkEndModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography;
using System.Threading.RateLimiting;

var rsaKey = RSA.Create();
rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<LwDBContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction =>
        {
            sqlServerOptionsAction.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }
    );
});

builder.Services
    .AddIdentity<User, Role>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<LwDBContext>()
    .AddDefaultTokenProviders();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(opt =>
    {
        opt.AddPolicy(
            name: "Cors",
            policy =>
            {
                policy
                    .WithOrigins(new string[] { "https://lw-localhost.topodvlp.ro" })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition");
            }
        );
    });
}
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter your JWT token in the field below.",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        }
    );
    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        }
    );
    c.OperationFilter<AuthenticationRequirementOperationFilter>();
});

// Configure JWT authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new RsaSecurityKey(rsaKey),
            ClockSkew = TimeSpan.Zero
        };
        options.MapInboundClaims = false;
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "master-admin",
        policy =>
        {
            policy.RequireClaim("role", "master-admin");
        }
    );
    options.AddPolicy(
        "firma-admin",
        policy =>
        {
            policy.RequireClaim("role", "firma-admin");
        }
    );
    options.AddPolicy(
        "user",
        policy =>
        {
            policy.RequireClaim("role", "user-admin", "user");
        }
    );
});

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IDbRepoCommon, DbRepoCommon>();
builder.Services.AddScoped<IDbRepoFirma, DbRepoFirma>();
builder.Services.AddScoped<IDbRepoUser, DbRepoUser>();
builder.Services.AddScoped<IDbRepoMaster, DbRepoMaster>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("Cors");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter(
    new RateLimiterOptions
    {
        GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            return RateLimitPartition.GetTokenBucketLimiter(
                context.User.Claims.FirstOrDefault(c => c.Type == "conexId")?.Value
                    ?? "GeneralLimit",
                _ =>
                    new TokenBucketRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                        TokenLimit = 100,
                        TokensPerPeriod = 100
                    }
            );
        }),
        RejectionStatusCode = 429,
    }
);

app.MapGet("", () => "Server is up and running!")
    .AllowAnonymous()
    .WithGroupName("Home")
    .WithDisplayName("Index")
    .WithName("");
app.MapControllers();

app.Run();
