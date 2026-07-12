using Bookstore.Application;
using Bookstore.Application.Jobs;
using Bookstore.Data.Persistence;
using Bookstore.Data.Seed;
using Bookstore.PublicAPI.Auth;
using Bookstore.PublicAPI.Filters;
using Bookstore.PublicAPI.Middleware;
using Bookstore.PublicAPI.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(config => config
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console());

builder.Services.AddDbContext<BookstoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddApplication();

builder.Services.AddControllers();
builder.Services.AddControllers(o => o.Filters.Add<ValidationFilter>());

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", o =>
    {
        o.PermitLimit = 5; //for testin
        o.Window = TimeSpan.FromMinutes(1);
    });
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey(nameof(BookImportJob));
    q.AddJob<BookImportJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(t =>
        t.ForJob(jobKey).WithIdentity($"{nameof(BookImportJob)} - trigger").WithCronSchedule("0 0 * * * ?"));
});

builder.Services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<JwtTokenService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.CanRead, p => p.RequireRole(Roles.Read, Roles.ReadWrite));
    options.AddPolicy(Policies.CanWrite, p => p.RequireRole(Roles.ReadWrite));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookstoreContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db);
}

app.UseExceptionHandler();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseSerilogRequestLogging(); 

app.MapOpenApi();

app.UseSwaggerUI(options => 
{
    options.SwaggerEndpoint("/openapi/v1.json", "Bookstore API v1");
    options.RoutePrefix = "swagger";
});

app.MapScalarApiReference();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
