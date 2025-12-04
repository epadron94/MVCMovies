using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MvcMovie.Data;
using Microsoft.Azure.Cosmos;
using MvcMovie.Services;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.DataProtection;
/*
reto TODO CONNECT TO CosmosDB
// ...existing code...
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

string keyVaultUrl = "https://<your-keyvault-name>.vault.azure.net/";
var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

// Read a secret
KeyVaultSecret secret = await client.GetSecretAsync("CosmosDbKey");
string secretValue = secret.Value;
// ...use secretValue...
*/
//CONNECT TO AZ KEYVAULT
var builder = WebApplication.CreateBuilder(args);

var keyVaultConfig = builder.Configuration.GetSection("KeyVault");
string keyVaultUri = keyVaultConfig["VaultUri"];
var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());

KeyVaultSecret cosmosDbAccountSecret = await secretClient.GetSecretAsync("CosmosDbAccount");
string cosmosDbAccount = cosmosDbAccountSecret.Value;

KeyVaultSecret cosmosDbKeySecret = await secretClient.GetSecretAsync("CosmosDbKey");
string cosmosDbKey = cosmosDbKeySecret.Value;


builder.Services.AddDbContext<MvcMovieContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("MvcMovieContext") ?? throw new InvalidOperationException("Connection string 'MvcMovieContext' not found.")));
 
var CosmosDbConfig = builder.Configuration.GetSection("CosmosDb");
builder.Services.AddSingleton(t =>
    {
        var cosmosClientOptions = new CosmosClientOptions
        {
            ApplicationPreferredRegions = CosmosDbConfig.GetSection("PreferredRegions").Get<List<string>>()
        };
        return new CosmosClient(
            accountEndpoint:cosmosDbAccount,// CosmosDbConfig["Account"],
            authKeyOrResourceToken: cosmosDbKey,//CosmosDbConfig["Key"],
            clientOptions: cosmosClientOptions
        );
    }
);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<CosmosDbService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();