using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Gallary36Bot.Models;
using System;
using System.IO;
using System.Net;
using Telegram.Bot;
using Gallary36Bot.Controllers;
using System.Web;
using OAuth;

namespace Gallary36Bot.Controllers
{
    [Route("https://bot.spartajobs.com.ua/api/message/update")]
    public class MessageController : Controller
    {
        // GET api/values
        [HttpGet]
        public string Get()
        {
            return "Method GET ";
        }

        // POST api/values
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Update update)
        {


            // return Ok(new { response = "123" });
            if (update == null) return Ok();

            var commands = Bot.Commands;
            var message = update.Message;
            var botClient = await Bot.GetBotClientAsync();

            foreach (var command in commands)
            {
                if (command.Contains(message))
                {
                    await command.Execute(message, botClient);
                    break;
                }
            }
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                {
                    await botClient.SendTextMessageAsync(message.From.Id, "Pls. Send photo content.");
                }
                if (message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
                {
                    string ftpPath = $@"/web/gallery36.pl/public_html/wp-content/uploads/{DateTime.Now.Year}/{DateTime.Now.Month.ToString("D2")}";
                    string webPath = $"https://www.gallery36.pl/wp-content/uploads/{DateTime.Now.Year}/{DateTime.Now.Month.ToString("D2")}";
                    Directory.CreateDirectory("photos");

                    for (int i = 2; i <= update.Message.Photo.Length; i += 3)
                    {
                        string filename = DateTime.Now.Ticks.ToString();
                        string remotePath = ftpPath + $@"/{filename}.png";
                        string localPath = $@"photos\{filename}.png";
                        string currentWebPath = webPath + $@"/{filename}.png";

                        var photo = await botClient.GetFileAsync(update.Message.Photo[i].FileId);
                        FileStream fs = new FileStream(localPath, FileMode.Create);
                        await botClient.DownloadFileAsync(photo.FilePath, fs);
                        fs.Close();

                        Bot.ftpClient.RetryAttempts = 10;
                        Bot.ftpClient.UploadFile(localPath, remotePath, createRemoteDir: true);
                        Console.WriteLine(Bot.ftpClient.FileExists(remotePath));

                        FileInfo fileInf = new FileInfo(localPath);
                        if (fileInf.Exists)
                        {
                            fileInf.Delete();
                        }
                        string URI = getOAuthDataUrl(@"https://www.gallery36.pl/wp-json/wc/v3/products");
                        var httpWebRequest = (HttpWebRequest)WebRequest.Create(URI);
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";

                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            string json = "{\"images\": [{ \"src\":\"{" + currentWebPath + "}\"}]}";
                            Console.WriteLine(json);
                            streamWriter.Write(json);
                        }

                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                        }

                        await botClient.SendPhotoAsync(message.From.Id, photo: update.Message.Photo[i].FileId);

                    }
                }
                if (message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
                {
                    await botClient.SendDocumentAsync(message.From.Id, document: update.Message.Document.FileId);
                }
                //await botClient.SendTextMessageAsync(message.Chat, "Привет-привет!!");
            }
            return Ok();
        }
        private static string getOAuthDataUrl(string url)
        {
            OAuthBase oAuth = new OAuthBase();
            Uri uri = new Uri(url);
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string normalizedUrl, normalizedRequestParameters;

            string sig = oAuth.GenerateSignature(uri, "ck_dcb3bb1a9f583a0c36f4b79a8e02cf89fa31a33e", "cs_be57387702ae6add1c19b99015edc245097f37e4", null, null, "POST", timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters);
            sig = HttpUtility.UrlEncode(sig);
            return normalizedUrl + "?" + normalizedRequestParameters + "&oauth_signature=" + sig;
        }
    }
   
}
