using AspNetCoreHero.ToastNotification.Extensions;
using BAExamApp.Business.Extensions;
using BAExamApp.DataAccess.EFCore.Extensions;
using BAExamApp.DataAccess.Extensions;
using BAExamApp.Dtos.Emails;
using BAExamApp.MVC.Extensions;
using OfficeOpenXml;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDataAccessServices(builder.Configuration)
    .AddEFCoreServices(builder.Configuration)
    .AddBusinessServices()
    .AddMvcServices()
    .Configure<EmailConfigurationDto>(builder.Configuration.GetSection("EmailConfiguration"));


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/");
    app.UseHsts();
}
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
app.UseStatusCodePagesWithReExecute("/ErrorPage/ErrorIndex", "?code={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
var cultures = new List<CultureInfo>
{
    new CultureInfo("tr")
};

app.UseRequestLocalization(options =>
{
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("tr");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;
});
app.UseNotyf();
app.UseSession();
app.MapControllerRoute(name: "default",
                       pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapDefaultControllerRoute();

app.Run();