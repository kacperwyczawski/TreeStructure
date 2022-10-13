using MudBlazor;
using MudBlazor.Services;
using TreeStructure.Data;
using TreeStructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSqlite<NodesDbContext>("Data Source=Nodes.db");
builder.Services.AddScoped<NodeService>();

builder.Services.AddMudServices(config =>
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();