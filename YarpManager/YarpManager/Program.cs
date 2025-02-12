using LettuceEncrypt.Acme;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using YarpManager.Acme;
using YarpManager.Acme.Factories;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Acme.Utils;
using YarpManager.WebUI;

// TODO:
//  - Implement own acme client + cert generator
//  - Implement own HttpsRedirection based on clients requires

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(kestrel => {

    // Ports for proxy
    kestrel.ListenAnyIP(80);
    kestrel.ListenAnyIP(443, options => {

        options.UseHttps(https => {
            https.UseLettuceEncrypt(options.ApplicationServices);
        });

    });

    // Ports for UI config
    kestrel.ListenAnyIP(5080);
});

// Register Yarp provider

builder.Services.AddReverseProxy()
    .AddDnsDestinationResolver();

builder.Services.AddLettuceEncrypt(options => {
    options.AcceptTermsOfService = true;
    options.AllowedChallengeTypes = ChallengeType.Any;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAcmeServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseWebAssemblyDebugging();
}
else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}

using (var scope = app.Services.CreateScope()) {

    var factory = scope.ServiceProvider.GetRequiredService<IAcmeServiceFactory>();
    var acme = factory.CreateService(
        "https://acme-staging-v02.api.letsencrypt.org/directory");
    
    var directory = await acme.NewAccount(["test@example.com"], true);

}

// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseWebSockets();

//app.MapReverseProxy()
//    .RequireHost("*:80", "*:443");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(YarpManager.Client._Imports).Assembly)
    .RequireHost("*:5080");

app.Run();