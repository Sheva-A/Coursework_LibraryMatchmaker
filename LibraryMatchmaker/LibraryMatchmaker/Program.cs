using System.Data;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Dapper;
using LibraryMatchmaker;
using LibraryMatchmaker.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.WebEncoders;

// Dapper зіставляє колонки з підкресленням із властивостями моделей: author_id -> AuthorId
DefaultTypeMap.MatchNamesWithUnderscores = true;

// Українські формати дат і чисел
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("uk-UA");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    // Українські тексти помилок, коли у формі введено не те, що очікувалось
    var messages = options.ModelBindingMessageProvider;
    messages.SetValueMustNotBeNullAccessor(_ => Msg.Required);
    messages.SetAttemptedValueIsInvalidAccessor((value, _) => $"Некоректне значення «{value}»");
    messages.SetValueIsInvalidAccessor(value => $"Некоректне значення «{value}»");
});

// Кирилиця в HTML без перетворення на коди &#x...; (зручніше читати код сторінки)
builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// Рядок підключення береться з appsettings.json.
// Для кожного запиту браузера своє з'єднання, Dapper сам відкриває і закриває його
string connectionString = builder.Configuration.GetConnectionString("LibraryMatchmaker")!;
builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(connectionString));

builder.Services.AddScoped<AuthorRepository>();
builder.Services.AddScoped<PublisherRepository>();
builder.Services.AddScoped<GenreRepository>();
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<ReaderRepository>();
builder.Services.AddScoped<LoanRepository>();
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<ReportRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
