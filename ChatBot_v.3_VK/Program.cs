using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Utils;

namespace ChatBot_v._3_VK
{
    class Program
    {
        static void Main(string[] args)
        {
            string apikey = File.ReadAllText("_apikey.txt"); //standalone app api key
            ulong appid = ulong.Parse(File.ReadAllText("_appid.txt")); //app id
            string login = File.ReadAllText("_login.txt"); //user login
            string password = File.ReadAllText("_pswd.txt"); //user pswrd
            string group_id = "your group id";

            VkApi vkClient = new VkApi();
            WebClient webClient = new WebClient();

            vkClient.Authorize(
                new ApiAuthParams
                {
                    ApplicationId = appid,
                    Login = login,
                    Password = password,
                    Settings = Settings.All,
                });

            var param = new VkParameters(new Dictionary<string, string>() { { "group_id", group_id } });
            //vk.com/dev/utils.resolveScreenName

            dynamic longPoll = JObject.Parse(vkClient.Call("groups.getLongPollServer", param).RawJson);
            //vk.com/dev/groups.getLongPollServer
            //получаем server, key, ts
            //key - секретный ключ сессии
            //server - aдрес сервера
            //ts - номер последнего события, начиная с которого нужно получать данные

            string json = string.Empty;

            string url = string.Empty;

            while (true)
            {
                url = string.Format("{0}?act=a_check&key={1}&ts={2}&wait=3",
                    longPoll.response.server.ToString(),
                    longPoll.response.key.ToString(),
                    json != string.Empty ? JObject.Parse(json)["ts"].ToString() : longPoll.response.ts.ToString()
                    );

                json = webClient.DownloadString(url);

                var jsonMsg = json.IndexOf(":[]}") > -1 ? "" : $"{json}\n";

                var msgCollection = JObject.Parse(json)["updates"].ToList();

                foreach (var item in msgCollection)
                {
                    if (item["type"].ToString() == "message_new")
                    {
                        string key = apikey;
                        string urlBotMsg = $"https://api.vk.com/method/messages.send?v=5.41&access_token{key}&user_id=";

                        string msg = item["object"]["body"].ToString();

                        Console.WriteLine($"{msg} ");

                        var arrayData = msg.Split(' ');
                        try
                        {
                            if (arrayData[0] == "/time")
                            {
                                msg = DateTime.Now.ToString();
                            }
                            else if (arrayData[0] == "/sum")
                            {
                                double a = Convert.ToDouble(arrayData[1]);
                                double b = Convert.ToDouble(arrayData[2]);
                                msg = $"{a} + {b} = {a + b}";
                            }
                            else msg = "я вас не понимаю";
                        }

                        catch (Exception)
                        {

                            msg = "Ошибочная команда";
                        }
                        webClient.DownloadString(
                            string.Format(urlBotMsg + "{0}&message={1}",
                            item["object"]["user_id"].ToString(),
                            msg
                            //$"\"{msg}\" символов:{msg.Length}"
                            ));
                        Thread.Sleep(1000);
                        Console.WriteLine("+");

                    }
                }
            }
        }
    }
}
