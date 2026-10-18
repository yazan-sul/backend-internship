using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.API.Contracts;
using RestaurantReservation.API.Endpoints;
using RestaurantReservation.API.Errors;
using RestaurantReservation.API.Validators;
using RestaurantReservation.Db;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.AddScoped<IValidator<ReservationRequest>, ReservationRequestValidator>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
// Send JSON binding failures through the same handler in every environment.
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
builder.Services.AddDbContext<RestaurantReservationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("RestaurantReservationCore")
        ?? throw new InvalidOperationException(
            "Set ConnectionStrings__RestaurantReservationCore before running the API.");
    options.UseSqlServer(connectionString);
});

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapReservationEndpoints();

app.Run();

