using Microsoft.Extensions.Options;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.Shared.Common.NotificationSettings;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HMS.PL.Senders
{
    public sealed class TwilioSmsSender(IOptions<TwilioSettings> _options) : ISmsSender
    {
        public async Task<string?> SendAsync(string toPhone, string body)
        {
            var opts = _options.Value;
            TwilioClient.Init(opts.AccountSid, opts.AuthToken);

            var message = await MessageResource.CreateAsync(
                body: body,
                from: new PhoneNumber(opts.FromNumber),
                to: new PhoneNumber(toPhone));

            if (message.ErrorCode.HasValue)
                throw new InvalidOperationException(
                    $"Twilio error {message.ErrorCode}: {message.ErrorMessage}");

            return message.Sid;
        }
    }
}