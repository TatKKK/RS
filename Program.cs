using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RS;
using RS.Auth;
using RS.Models;
using RS.Packages;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Text;
using static RS.Packages.IPKG_APPPOINTMENTS;

var builder = WebApplication.CreateBuilder(args);



builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IPKG_DOCTOR, PKG_DOCTOR>();
builder.Services.AddScoped<IPKG_PATIENT, PKG_PATIENT>();
builder.Services.AddScoped<IPKG_USER, PKG_USER>();
builder.Services.AddScoped<IPKG_APPPOINTMENTS, PKG_APPOINTMENT>();
builder.Services.AddScoped<IPKG_CODES, PKG_CODES>();
builder.Services.AddScoped<IJwtManager, JwtManager>();
builder.Services.AddSignalR(); 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
    //option.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    //option.OperationFilter<FileUploadOperationFilter>();

});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["JWT:key"]);
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            //ValidIssuer = builder.Configuration["JWT:Issuer"],
            //ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("RequiredAdminRole", policy => policy.RequireRole("admin"));
    o.AddPolicy("RequiredDoctorRole", policy => policy.RequireRole("doctor"));
    o.AddPolicy("RequiredPatientRole", policy => policy.RequireRole("patient"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.MapHub<ViewCountHub>("/viewcounthub");

app.Run();


