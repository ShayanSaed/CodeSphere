using Microsoft.AspNetCore.Identity.UI.Services;

namespace CodeSphere.Web.Services;

/// <summary>
/// The default Identity UI pages (Register, ForgotPassword, ResendEmailConfirmation, ...)
/// all require an <see cref="IEmailSender"/> in their constructor. If nothing implements
/// this interface, ASP.NET Core's DI container throws
/// "Unable to resolve service for type 'IEmailSender'" the moment any of those pages is
/// requested — which is exactly why /Identity/Account/Register was failing to load.
///
/// This is a development-friendly no-op sender: it just logs what *would* have been sent.
/// Since <c>RequireConfirmedAccount = false</c>, the app never depends on the email actually
/// arriving, but registering *some* implementation is mandatory for Identity's DI graph to
/// resolve at all.
///
/// Swap this out for a real sender (SendGrid, SMTP, etc.) before going to production.
/// </summary>
public class NoOpEmailSender : IEmailSender
{
    private readonly ILogger<NoOpEmailSender> _logger;

    public NoOpEmailSender(ILogger<NoOpEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("(Email sending is not configured) Would send to {Email} — Subject: {Subject}", email, subject);
        return Task.CompletedTask;
    }
}
