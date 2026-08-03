using System.Text;
using CalConnect.Api.Database;
using CalConnect.Api.Endpoints;
using CalConnect.Api.Extensions;
using CalConnect.Api.Users;
using CalConnect.Api.Users.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth();

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Database")).UseSnakeCaseNamingConvention());

builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddScoped<EmailVerificationLinkFactory>();

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddFluentEmail(builder.Configuration["Email:SenderEmail"], builder.Configuration["Email:Sender"])
    .AddSmtpSender(builder.Configuration["Email:Host"], builder.Configuration.GetValue<int>("Email:Port"));

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddScoped<RegisterUser>();
builder.Services.AddScoped<LoginUser>();
builder.Services.AddScoped<VerifyEmail>();
builder.Services.AddScoped<GetUser>();
builder.Services.AddScoped<UpdateUser>();

//builder.Services.AddScoped<CreateMeeting>();
//builder.Services.AddScoped<UpdateMeeting>();
//builder.Services.AddScoped<CancelMeeting>();
//builder.Services.AddScoped<RescheduleMeeting>();
//builder.Services.AddScoped<InviteParticipant>();
//builder.Services.AddScoped<RemoveParticipant>();
//builder.Services.AddScoped<UpdateParticipantResponse>();

//builder.Services.AddScoped<MeetingPolicyService>();
//builder.Services.AddScoped<IEmailService, EmailService>();
//builder.Services.AddScoped<ICalendarSyncService, CalendarSyncService>();

builder.Services.AddEndpoints();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
}

app.MapEndpoints();

app.UseAuthentication();

app.UseAuthorization();

app.Run();
