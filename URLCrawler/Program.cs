using HtmlAgilityPack;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URLCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            HashSet<string> Urls = new HashSet<string>();

            HtmlWeb web = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8  
            };

           HtmlDocument document = new HtmlDocument();
            document = web.Load("https://vnexpress.net/suc-khoe/dinh-duong");
            var linkList = document.DocumentNode.SelectNodes("//a[@href]");

            foreach (HtmlNode link in linkList)
            {
                HtmlAttribute att = link.Attributes["href"];

                if (att.Value.EndsWith(".html"))
                {
                    Urls.Add(att.Value);
                }
            }

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "amqCrawler",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                    foreach (var item in Urls)
                    {
                        string message = item;
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "",
                                             routingKey: "amqCrawler",
                                             basicProperties: null,
                                             body: body);
                    }

                }
            }
        }
    }
}



