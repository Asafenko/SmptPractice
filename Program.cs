using System.Diagnostics;
using SmtpPractice;
using Serilog;




Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger(); //означает, что глобальный логер будет заменен на вариант из Host.UseSerilog
Log.Information("Starting up");
try
{
    

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
    builder.Services.AddHostedService<ServerStartupNotificationBackgroundService>();

// #$# Configuration Connection
// 1 Way
    builder.Services.Configure<SmtpConfig>(builder.Configuration.GetSection("SmtpConfig"));
    builder.Services.Configure<RuleCountConfig>(builder.Configuration.GetSection("RuleCountConfig"));
// 2 Way
// var config = builder.Configuration.GetSection("SmtpConfig").Get<SmtpConfig>();




// #$# SERILOG

// Way 1 only in Console
// builder.Host.UseSerilog((_, conf) => conf.WriteTo.Console());
 
// Way 2 in File of Configuration
    builder.Host.UseSerilog((ctx, conf) =>
    {
        conf
            .WriteTo.Console()
            .WriteTo.File("log-.txt", rollingInterval:RollingInterval.Day)
            .ReadFrom.Configuration(ctx.Configuration);
    });
    var app = builder.Build();
// Activation LOG
    app.UseStaticFiles();
    app.UseSerilogRequestLogging();
//


    app.MapGet("/", (IEmailSender sender) =>
    {
        var sw = new Stopwatch();
        var list = new List<string>();
        for (int i = 3; i < 3; i++)
        {
            sw.Restart();
            sender.Send("sender123@.com", "recipient123@.com", $"Opened General{i}",
                "Hello for u of Future");
            list.Add(sw.ElapsedMilliseconds + " ms");
        }

        return "Hello World! Message Sent! " + string.Join(", ", list);
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush(); //перед выходом дожидаемся пока все логи будут записаны
}








;