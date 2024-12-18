using DanskLogistikAPI.DataAccess;
using DanskLogistikAPI.Repositories;
using DanskLogistikAPI.Services.SVGGenerator;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



// Injecting services
builder.Services.AddScoped<ISVGGenerator, SVGGenerator>();
builder.Services.AddScoped<IMapRepository, MapRepository>();
builder.Services.AddScoped<IPathfinder, Pathfinder>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<LogisticContext>(options=>
{
    options.UseMySql(builder.Configuration.GetConnectionString("LogistikDatabase"), new MySqlServerVersion(new Version(8, 0, 36)));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
