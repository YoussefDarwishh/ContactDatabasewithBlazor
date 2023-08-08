using ContactDatabasewithBlazor.Client;
using EdgeDB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ContactDatabasewithBlazor.Client.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddEdgeDB(EdgeDBConnection.FromInstanceName("ContactDatabasewithBlazor"), config =>
{
    config.SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy;
});
builder.Services.AddAuthorizationCore();
builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet("/allcontacts", async (HttpContext context, EdgeDBClient client) =>
{
    var result = await client.QueryAsync<ContactView>("SELECT Contact {*};");
    return Results.Ok(result.ToList());
});

app.MapGet("/getcontact/{username}", async (string username, HttpContext context, EdgeDBClient client) =>
{
    ContactView result = await client.QuerySingleAsync<ContactView>("SELECT Contact{*} FILTER .username = <str>$username", new Dictionary<string, object?> { { "username", username } });
    return result;
});

app.MapGet("/SearchContact/{searchText}", async (string searchText, HttpContext context, EdgeDBClient client) =>
{
    List<ContactView> contacts = new List<ContactView>();
    var output = await client.QueryAsync<ContactView>("SELECT Contact {*};");
    contacts = output.ToList();

    if (string.IsNullOrEmpty(searchText))
    {
        return Results.Ok(contacts);
    }

    searchText = searchText.ToLower();
    List<ContactView> filteredContacts = contacts.Where(contact => contact.FirstName.ToLower().Contains(searchText) || contact.LastName.ToLower().Contains(searchText) || contact.Email.ToLower().Contains(searchText)).ToList();

    return Results.Ok(filteredContacts);

});

app.MapGet("/logout", async (HttpContext context, EdgeDBClient client) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

app.MapPost("/editcontact", async (HttpContext context, EdgeDBClient client, ContactInput contact) =>
{
    var passwordHasher = new PasswordHasher<string>();
    contact.Password = passwordHasher.HashPassword(null, contact.Password);
    var query = "Update Contact FILTER .username = <str>$username SET {username := <str>$username, password := <str>$password, role := <str>$role,first_name := <str>$first_name, last_name := <str>$last_name, email := <str>$email, title := <str>$title, birth_date := <datetime>$birth_date, description := <str>$description, marriage_status := <bool>$marriage_status} ";
    await client.ExecuteAsync(query, new Dictionary<string, object?>
    {
       {"username", contact.Username},
       {"password", contact.Password},
       {"role", contact.Role},
       {"first_name", contact.FirstName},
       {"last_name", contact.LastName},
       {"email", contact.Email},
       {"title", contact.Title},
       {"birth_date", contact.DateOfBirth},
       {"description", contact.Description},
       {"marriage_status", contact.MarriageStatus}
    });

});

app.MapPost("/addcontact", async (HttpContext context, EdgeDBClient client, ContactInput contact) =>
{
    var passwordHasher = new PasswordHasher<string>();
    contact.Password = passwordHasher.HashPassword(null, contact.Password);
    var query = "INSERT Contact {username := <str>$username, password := <str>$password, role := <str>$role,first_name := <str>$first_name, last_name := <str>$last_name, email := <str>$email, title := <str>$title, birth_date := <datetime>$birth_date, description := <str>$description, marriage_status := <bool>$marriage_status}";
    await client.ExecuteAsync(query, new Dictionary<string, object?>
    {
       {"username", contact.Username},
       {"password", contact.Password},
       {"role", contact.Role},
       {"first_name", contact.FirstName},
       {"last_name", contact.LastName},
       {"email", contact.Email},
       {"title", contact.Title},
       {"birth_date", contact.DateOfBirth},
       {"description", contact.Description},
       {"marriage_status", contact.MarriageStatus}
    });
});

app.MapPost("/login", async (HttpContext context, EdgeDBClient client, LoginInput loginInput) =>
{
    string username = loginInput.UserName;
    string password = loginInput.Password;
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Username and password are required.");
        return;
    }
    var query = @"SELECT Contact {username, password, role } FILTER Contact.username = <str>$username";
    var result = await client.QueryAsync<ContactView>(query, new Dictionary<string, object?>
    {
       { "username",loginInput.UserName }
    });
    if (result.Count > 0)
    {
        var passwordHasher = new PasswordHasher<string>();
        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null, result.First().Password, password);
        if (passwordVerificationResult == PasswordVerificationResult.Success)
        {
            var claims = new List<Claim>
            {
               new Claim(ClaimTypes.Name, result.First().Username),
               new Claim(ClaimTypes.Role, result.First().Role),
            };
            var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
            var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true

            };
            var user = new ClaimsPrincipal(claimsIdentity);
            await context.SignInAsync(scheme, user, authProperties);
            context.Response.StatusCode = 200;
        }
        else
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Invalid password.");
            return;
        }
    }
    else
    {
        context.Response.StatusCode = 401; // Unauthorized
        await context.Response.WriteAsync("Unsuccessful login attempt.");
        return;
    }
});



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
