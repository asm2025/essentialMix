using System.Text;
using essentialMix.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Swashbuckle.AspNetCore.Filters;

Console.OutputEncoding = Encoding.UTF8;
Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

WebApplicationOptions options = new WebApplicationOptions
{
	ApplicationName = AppDomain.CurrentDomain.FriendlyName,
	WebRootPath = AppDomain.CurrentDomain.BaseDirectory,
	ContentRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot"),
	Args = args
};
WebApplicationBuilder builder = WebApplication.CreateBuilder(options);

// Logging
LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
if (builder.Configuration.GetValue<bool>("LoggingEnabled")) loggerConfiguration.ReadFrom.Configuration(builder.Configuration);
Log.Logger = loggerConfiguration.CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services
		// logging
		.AddSingleton(typeof(ILogger<>), typeof(Logger<>))
		// MVC
		.AddRazorPages();

if (builder.Environment.IsDevelopment())
{
	builder.Services
			// Swagger
			.AddSwaggerGen(opt =>
			{
				opt.Setup(builder.Configuration, builder.Environment)
						.AddJwtBearerSecurity();
				//options.OperationFilter<FormFileFilter>();
				opt.ExampleFilters();
			})
			.AddSwaggerExamplesFromAssemblyOf<Program>();
}

builder.Services
		// Cookies
		.Configure<CookiePolicyOptions>(opt =>
		{
			// This lambda determines whether user consent for non-essential cookies is needed for a given request.
			opt.CheckConsentNeeded = _ => true;
			opt.MinimumSameSitePolicy = SameSiteMode.None;
		})
		// FormOptions
		.Configure<FormOptions>(opt =>
		{
			opt.ValueLengthLimit = int.MaxValue;
			opt.MultipartBodyLengthLimit = int.MaxValue;
			opt.MemoryBufferThreshold = int.MaxValue;
		})
		// Helpers
		.AddHttpContextAccessor()
		// Mapper
		.AddAutoMapper((_, bld) => bld.AddProfile(new AutoMapperProfiles()),
						new[] { typeof(AutoMapperProfiles).Assembly },
						ServiceLifetime.Singleton);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
