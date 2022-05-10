using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Gallary36Bot.Models.Commands;
using FluentFTP;
using System.Net;

namespace Gallary36Bot.Models
{
    public class Bot
    {
        private static TelegramBotClient botClient;
        private static List<Command> commandsList;
        public static FtpClient ftpClient;
        public static IReadOnlyList<Command> Commands => commandsList.AsReadOnly();

        public static async Task<TelegramBotClient> GetBotClientAsync()
        {
            if (botClient != null)
            {
                return botClient;
            }
            ftpClient = new FtpClient("5.187.4.188", "admin", "qRLiIK4hafVrju0x");
            ftpClient.EncryptionMode = FtpEncryptionMode.Explicit;
            ftpClient.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);
            


            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ftpClient.Connect();
            commandsList = new List<Command>();
            commandsList.Add(new StartCommand());
            //TODO: Add more commands

            botClient = new TelegramBotClient(AppSettings.Key);
          // string hook = string.Format(AppSettings.Url, "api/message/update");
          //  await botClient.SetWebhookAsync(AppSettings.Url);
            return botClient;
        }
        private static void OnValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            if (e.PolicyErrors != System.Net.Security.SslPolicyErrors.None)
            {
                // invalid cert, do you want to accept it?
                e.Accept = true;
            }
            else
            {
                e.Accept = true;
            }
        }
    }
}
