using System;
using System.Configuration;
using System.Threading;
using System.Collections.Generic;
using lib;
/*
https://medium.com/edgefund/c-development-with-visual-studio-code-b860cc71a5ec
https://stackoverflow.com/questions/6052992/how-can-i-disable-close-button-of-console-window-in-a-visual-studio-console-appl
*/
namespace RssRead
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "";
            var userInput = String.Empty;
            Console.SetWindowSize(80,30);
            
            SystemString(out userInput, "withGreeting");
            while(userInput.ToLower() != "exit")
            {
                List<News_Source> newsList = new List<News_Source>();
                if(!Int32.TryParse(userInput, out int n)){
                    Console.WriteLine("Error Input");
                    userInput = Console.ReadLine();
                }else{
                    switch(Int32.Parse(userInput)){
                        case 1://Insert
                            List<News_Source> insertdata = new List<News_Source>();
                            RssReader newsMethod = new RssReader();
                            Console.WriteLine("Please Insert New Feeds URL:");
                            userInput = Console.ReadLine();
                            insertdata = newsMethod.getNews(String.Format("{0}", userInput));
                            NewsFeeds.post(insertdata);
                            break;
                        case 2://Update
                            NewsFeeds.updateAll();
                            break;
                        case 3://Search
                            newsList.Clear();
                            newsList = NewsFeeds.get();
                            Console.WriteLine("Choose one news to list.\n(Insert number)");
                            userInput = Console.ReadLine();

                            if(Int32.Parse(userInput)<=newsList.Count && userInput != "0"){
                                Console.WriteLine(userInput);
                                NewsFeeds.get(newsList[Int32.Parse(userInput)-1]);
                            }
                            break;
                        case 4://Remove
                            newsList.Clear();
                            newsList = NewsFeeds.get();
                            Console.WriteLine("Choose one news to delete.\n(Insert number)");
                            userInput = Console.ReadLine();
                            NewsFeeds.delete(newsList[Int32.Parse(userInput)-1]);
                            break;
                        case 5://Get Feeds URL list
                            NewsFeeds.getFeeds();
                            break;
                        case 6://Auto Update and Send via Telegram bot
                            Timer t = new Timer(TimerCallback, null, 0, 60*1000);
                            break;
                        default:
                            break;
                    };
                    SystemString(out userInput, "withoutGreeting");
                }
            };
        }
        private static void SystemString(out string userInput, string mode)
        {
            userInput = String.Empty;
            if(mode == "withGreeting")
            {
                string ver = ConfigurationManager.AppSettings["ver"];
                //Greeting content
                Console.WriteLine("YC RSS Reader - {0}", ver);
            }
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("(1)Add New Feeds. (2)Check for Updates. (3)List News data. (4)Remove News Feeds.");
            Console.WriteLine("(5)List Rss Feeds URL (6)Auto Update and Send");
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("Insert 'EXIT' to leave");
            userInput = Console.ReadLine();
        }
        private static void TimerCallback(Object o) 
        {
            TelegramBot.send();
            // Force a garbage collection to occur for this project.
            GC.Collect();
        }
    }
}
