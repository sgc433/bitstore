using System.Text;
using Bitstore.Application.Abstractions;
using Bitstore.Application.DTO.User;
using Bitstore.Application.Services;
using Bitstore.Core.Abstractions;
using Bitstore.DataAccess;
using Bitstore.DataAccess.Repositories;
using Bitstore.DTO.Auth;
using Bitstore.DTO.Beat;
using Bitstore.Infrastructure;
using Bitstore.Validators;
using Bitstore.Validators.Auth;
using Bitstore.Validators.Beat;
using Bitstore.Validators.User;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bitstore API",
        Version = "v1",
        Description = "API для управления битами"
    });

    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header. Пример: 'Bearer {token}'"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions =  builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();

        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidIssuer = "Bitstore",
            
            ValidateAudience = false,
            ValidAudience = "BitstoreClient",
            
            ValidateLifetime = true,
            
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions!.SecretKey))
            
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
builder.Services.AddDbContext<BitstoreDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("BitstoreDbContext"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

builder.Services.AddScoped<IValidator<LoginUserRequest>, LoginUserRequestValidator>();
builder.Services.AddScoped<IValidator<RegisterUserRequest>, RegisterUserRequestValidator>();

builder.Services.AddScoped<IValidator<BeatRequest>, BeatRequestValidator>();
builder.Services.AddScoped<IValidator<BeatResponse>, BeatResponseValidator>();
builder.Services.AddScoped<IValidator<UpdateBeatRequest>, UpdateBeatRequestValidator>();

builder.Services.AddScoped<IValidator<UserUpdateBalanceRequest>, UserUpdateBalanceRequestValidator>();
builder.Services.AddScoped<IValidator<UserUpdateRequest>, UserUpdateRequestValidator>();
builder.Services.AddScoped<IValidator<UserResponse>, UserResponseValidator>();

builder.Services.AddScoped<IBeatRepository, BeatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ILicenseRepository, LicenseRepository>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IBeatService, BeatService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseCors("DefaultPolicy");


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
