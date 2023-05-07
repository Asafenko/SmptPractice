using Microsoft.Extensions.Options;
using Polly;

namespace SmtpPractice;

public class ServerStartupNotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServerStartupNotificationBackgroundService> _logger;
    //private readonly IConfiguration _configuration;
    private readonly RuleCountConfig _ruleCountConfig;

    public ServerStartupNotificationBackgroundService(IServiceProvider serviceProvider,
        ILogger<ServerStartupNotificationBackgroundService> logger,
        IOptions<RuleCountConfig> options)// IConfiguration configuration
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _ruleCountConfig = options.Value;
        //_configuration = configuration;
    }


    //async and nothing not return
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       await using var scope = _serviceProvider.CreateAsyncScope();
        //var services = scope.ServiceProvider;
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var ToEmail = ""; // recipient123@gmail.com
        //var RetryCount = Int32.Parse(_configuration["RetryCount"]); 
        // POLLY BUILDER
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(_ruleCountConfig.RetryCount,CountAttempt=>
                TimeSpan.FromSeconds(Math.Pow(2,_ruleCountConfig.RetryCount)),onRetry: (exception, CountAttempt) =>
                {
                    _logger.LogWarning(exception, "WARNING TRYING TO SEND: ATTEMPT:{Attempt}", CountAttempt);
                });
            // Just Way 
            // .Retry(RetryCount, onRetry: (exception, CountAttempt) =>
            // {
            //     _logger.LogWarning(
            //         exception, "WARNING TRYING TO SEND: ATTEMPT:{Attempt}", CountAttempt);
            // });
           var result = await retryPolicy.ExecuteAndCaptureAsync(_ => sender.Send("sender123@.ru",
            ToEmail,
            $"Opened General",
            "Hello for u of Future"), stoppingToken);
        
        if (result.Outcome == OutcomeType.Failure)
        {
            _logger.LogError(result.FinalException, "ERROR: Fatality");
        }

        //return Task.CompletedTask;
    }
}


// that is an easy way

// for (var attempt = 1; attempt <= AttemptsCount; attempt++)
// {
//     try
//     {
//         sender.Send("recipient123@gmail.com",
//             ToEmail,
//             $"Opened General",
//             "Hello for u of Future");
//         _logger.LogInformation($"THE MESSAGE WAS SENT SUCCESSFULLY");
//     }
//     catch (Exception e)
//     {
//         if (attempt < AttemptsCount)
//             _logger.LogWarning(e, "WARNING TRYING TO SEND: ATTEMPT:{Attempt} ", attempt, ToEmail,
//                 sender.GetType());
//         else
//             _logger.LogError(e, "ERROR");
//     }
//     // catch (Exception e) when (attempt < AttemptsCount)
//     // {
//     //     _logger.LogWarning(e, "WARNING TRYING TO SEND: ATTEMPT:{Attempt} ", attempt, ToEmail, sender.GetType());
//     // }
//     // catch (Exception e)
//     // {
//     //     _logger.LogError(e, "ERROR");
//     // }
// }