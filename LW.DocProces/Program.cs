using LW.BkEndDb;
using LW.BkEndModel;
using LW.DocProces;
using LW.DocProcLogic.Anaf;
using LW.DocProcLogic.DbRepo;
using LW.DocProcLogic.FileManager;
using LW.DocProcLogic.MicrosoftOcr;
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
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
    builder.Configuration.GetValue<string>("Syncfusion")
);

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
                    .WithOrigins(new string[] { "http://localhost:4201" })
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
            ValidIssuer = "http://localhost:5000",
            ValidAudience = "http://localhost:5000",
            IssuerSigningKey = new RsaSecurityKey(rsaKey),
            ClockSkew = TimeSpan.Zero
        };
        options.MapInboundClaims = false;
    });

builder.Services.AddScoped<IAnafApiCall, AnafApiCall>();
builder.Services.AddScoped<IDbRepo, DbRepo>();
builder.Services.AddScoped<IFileManager, FileManager>();

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
app.MapGet("", () => "Server 2 is up and running!")
    .AllowAnonymous()
    .WithGroupName("Home")
    .WithDisplayName("Index")
    .WithName("");

app.MapControllers();

app.Run();
