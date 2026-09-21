using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VetSanJose.Client;
using VetSanJose.Client.Handlers;
using VetSanJose.Client.Services;
using VetSanJose.Client.Services.Api;
using VetSanJose.Client.Services.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<TokenStorage>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<CartService>();

builder.Services.AddScoped<AuthorizationMessageHandler>();

builder.Services.AddHttpClient("Api", client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddHttpClient("ApiAnonima", client => client.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddScoped<AuthApiClient>(sp =>
    new AuthApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiAnonima")));

builder.Services.AddScoped<ProductosApiClient>(sp =>
    new ProductosApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

builder.Services.AddScoped<CategoriasApiClient>(sp =>
    new CategoriasApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

builder.Services.AddScoped<MascotasApiClient>(sp =>
    new MascotasApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

builder.Services.AddScoped<CitasApiClient>(sp =>
    new CitasApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

builder.Services.AddScoped<HistorialMedicoApiClient>(sp =>
    new HistorialMedicoApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

builder.Services.AddScoped<ProveedoresApiClient>(sp =>
    new ProveedoresApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

builder.Services.AddScoped<UsuariosApiClient>(sp =>
    new UsuariosApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

builder.Services.AddScoped<VentasApiClient>(sp =>
    new VentasApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

builder.Services.AddScoped<DashboardApiClient>(sp =>
    new DashboardApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

builder.Services.AddScoped<ArchivosApiClient>(sp =>
    new ArchivosApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

builder.Services.AddScoped<ReportesApiClient>(sp =>
    new ReportesApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

await builder.Build().RunAsync();
