using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_MY_SUPER_SECRET_KEY_12345"))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("Data Source=todos.db"));
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWTを貼り付けてね（Bearer なしでOK）"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(error.Error, "予期しないエラーが発生しました");
            await context.Response.WriteAsJsonAsync(new
            {
                message = "サーバーエラーが発生しました"
            });
        }
    });
});

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.Migrate();
}

app.MapGet("/todos", async (ITodoService service) =>
    Results.Ok(await service.GetAllAsync()))
    .RequireAuthorization();

app.MapPost("/todos", async (TodoCreateDto dto, ITodoService service) =>
{
    var (isConflict, result) = await service.CreateAsync(dto);
    return isConflict
        ? Results.Conflict("そのIDは既に存在してるよ！")
        : Results.Created($"/todos/{result!.Id}", result);
})
.RequireAuthorization();

app.MapPut("/todos/{id}", async (int id, TodoCreateDto dto, ITodoService service) =>
{
    var (isNotFound, result) = await service.UpdateAsync(id, dto);
    return isNotFound
        ? Results.NotFound("そのIDは存在しないよ！")
        : Results.Ok(result);
})
.RequireAuthorization();

app.MapDelete("/todos/{id}", async (int id, ITodoService service) =>
{
    var deleted = await service.DeleteAsync(id);
    return deleted
        ? Results.Ok("削除したよ！")
        : Results.NotFound("そのIDは存在しないよ！");
})
.RequireAuthorization();

app.MapPost("/register", async (RegisterDto dto, TodoDbContext db) =>
{
    var exists = db.Users.Any(x => x.Username == dto.Username);
    if (exists)
    {
        return Results.Conflict("そのユーザー名は既に使われてるよ！");
    }

    var user = new User
    {
        Username = dto.Username,
        Password = dto.Password
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok("登録成功！");
});

app.MapPost("/login", (LoginDto dto, TodoDbContext db) =>
{
    var user = db.Users
        .FirstOrDefault(x => x.Username == dto.Username && x.Password == dto.Password);

    if (user == null)
    {
        return Results.Unauthorized();
    }

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("THIS_IS_MY_SUPER_SECRET_KEY_12345"));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        claims: null,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: creds
    );

    var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new { token = jwt });
});

app.UseAuthentication();
app.UseAuthorization();

app.Run();