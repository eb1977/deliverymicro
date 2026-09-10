using AutoMapper;
using Confluent.Kafka;
using Ddd;
using DeliveryApp.Api;
using DeliveryApp.Api.Adapters.BackgroundJobs;
using DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Mapping;
using DeliveryApp.Api.Adapters.Kafka.BasketConfirmed;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrderCommand;
using DeliveryApp.Core.Domain.Services.OrderAssignment;
using DeliveryApp.Core.Ports;
using DeliveryApp.Infrastructure.Adapters.Gprc.GeoService;
using DeliveryApp.Infrastructure.Adapters.PostgeSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Quartz;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// AutoMapper
builder.Services.AddSingleton<IMapper>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    var assemblies = AppDomain.CurrentDomain.GetAssemblies();

    var config = new MapperConfiguration(
        cfg => { cfg.AddMaps(assemblies); },
        loggerFactory
    );

    config.AssertConfigurationIsValid();
    return config.CreateMapper();
});

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Health Checks
builder.Services.AddHealthChecks();

// Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin(); // Не делайте так в проде!
        });
});

builder.Services.AddSingleton<IOrderAssignmentService, OrderAssignmentService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

// Configuration
builder.Services.ConfigureOptions<SettingsSetup>();
var connectionString = builder.Configuration["CONNECTION_STRING"];
var geo = builder.Configuration["GEO_SERVICE_GRPC_HOST"];
// 6 модуль
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        connectionString,
        sql => sql.MigrationsAssembly("DeliveryApp.Infrastructure")
    );
    options.EnableSensitiveDataLogging();
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICourierRepository, CourierRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IGeoClient, GeoClient>();

builder.Services.AddQuartz(configure =>
{
    var assignOrdersJobKey = new JobKey(nameof(AssignOrdersJob));
    var moveCouriersJobKey = new JobKey(nameof(MoveCouriersJob));

    configure
        .AddJob<AssignOrdersJob>(assignOrdersJobKey, job => { })
        .AddTrigger(trigger => trigger
            .ForJob(assignOrdersJobKey)
            .WithSimpleSchedule(schedule => schedule
                .WithIntervalInSeconds(1)
                .RepeatForever()))

        .AddJob<MoveCouriersJob>(moveCouriersJobKey, job => { })
        .AddTrigger(trigger => trigger
            .ForJob(moveCouriersJobKey)
            .WithSimpleSchedule(schedule => schedule
                .WithIntervalInSeconds(2)
                .RepeatForever()));
});

builder.Services.AddQuartzHostedService();

// 8 модуль
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver =
            new CamelCasePropertyNamesContractResolver();

        options.SerializerSettings.Converters.Add(
            new StringEnumConverter(new CamelCaseNamingStrategy()));
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("1.0.0", new OpenApiInfo
    {
        Title = "Basket Service",
        Description = "Сервис корзины",
        Contact = new OpenApiContact
        {
            Name = "Kirill Vetchinkin",
            Url = new Uri("https://microarch.ru"),
            Email = "info@microarch.ru"
        }
    });

    // XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    // Устранение конфликтов DTO
    options.CustomSchemaIds(type => type.FullName);
});

// 10 модуль
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior =
        BackgroundServiceExceptionBehavior.Ignore;
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHostedService<BasketConfirmedService>();

var app = builder.Build();

// -----------------------------------
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHealthChecks("/health");
app.UseRouting();
app.UseCors();

// 8 модуль
app.UseSwagger(c => { c.RouteTemplate = "openapi/{documentName}/openapi.json"; });

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "openapi";
    options.SwaggerEndpoint(
        "/openapi/1.0.0/openapi.json",
        "Basket Service v1");
});

app.MapControllers();

// 6 модуль
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    db.Database.Migrate();
//}

app.Run();