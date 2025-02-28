using Microsoft.AspNetCore.Connections;
using Yarp.ReverseProxy.Configuration;
using YarpManager.Abstractions;
using YarpManager.Acme;
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

        });

    });

    // Ports for UI config
    kestrel.ListenAnyIP(5080);
});

// Register Yarp provider
builder.Services.AddSingleton<IProxyConfigProvider, DbConfigProvider>();

builder.Services.AddReverseProxy()
    .AddDnsDestinationResolver();

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

//using (var scope = app.Services.CreateScope()) {

//    var factory = scope.ServiceProvider.GetRequiredService<IAcmeServiceFactory>();
//    var acme = factory.CreateService(
//        "https://acme-staging-v02.api.letsencrypt.org/directory");

//    var accountResponse = await acme.NewAccount(["test@adsver.com"], true, JsonSignAlgorithm.PS512);

//    if (accountResponse.TryGet(out var account)) {

//        account.SaveKey("test.key");

//        account.Dispose();
//    }

//    if (AsymmetricKeyInfo.TryLoadFromFile("test.key", out var key)) {

//        var accountResponse2 = await acme.Account(key);

//        if (accountResponse2.TryGet(out var account2)) {

//            var orderRes = await account2.NewOrder(["adsver.com", "*.adsver.eu"]);

//            if (orderRes.TryGet(out var orderService)) {

//                var orderData = await orderService.Order();

//                var authzsRes = await orderService.Authorizations();

//                if (authzsRes.TryGet(out var authzServices)) {

//                    var authzService = authzServices.First();

//                    var challengesRes = await authzService.Challenges();

//                    if (challengesRes.TryGet(out var challengeServices)) {

//                        var challengeService = challengeServices.First();

//                        var validate = await challengeService.Validate();


//                    }

//                    var authz = await authzService.Authorization();

//                }

//            }
//            //var newKey = AsymmetricKey.Create(JsonSignAlgorithm.ES256);

//            //// TODO: Save the file path of the key if exists one,
//            //// if exists on the ChangeKey method Rewrite the file with the new key
//            //var changeKeyRes = await account2.ChangeKey(newKey);

//            //if (changeKeyRes.IsSuccessStatusCode) {
//            //    newKey.SaveToFile("test.key");
//            //}

//            // var response = await account2.Deactivate();

//            account2.Dispose();
//        }
//    }


//}

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