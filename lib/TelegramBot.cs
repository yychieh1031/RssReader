using System;
using System.Configuration;
using System.Collections.Generic;
using Telegram.Bot;

namespace lib
{
    public class TelegramBot
    {
        static ITelegramBotClient botClient;
        static async public void send(News news){
            string token = ConfigurationManager.AppSettings["TelegramBot"];
            string chat_Id = ConfigurationManager.AppSettings["chatId"];
            botClient = new TelegramBotClient(token);
            await botClient.SendTextMessageAsync(
                        chatId: chat_Id, // or a chat id: 123456789
                        text: String.Format("{0}", news.url)
                    );
        }

        //
        // Send Lastest News URL to telegram
        //
        static public void send()
        {
            List<News_Source> newsdata = new List<News_Source>();

            var connection = SqliteHelper.DBContext("NewsRss_URL.db");

            using (connection)
            {
                connection.Open();

                //Get News Rss URL List
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM NewsRssURL";

                List<News_Source> tempList = new List<News_Source>();
                RssReader rss = new RssReader();
                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tempList = rss.getNews(reader["rssURL"].ToString());
                        newsdata.AddRange(tempList);
                        
                    }
                }
            }

            newsdata.ForEach(o => {
                var count = 0;
                //Get lastest Date of News in Database 
                var lstDt = NewsFeeds.getLstDt(o) == "" ? "1911-01-01 00:00" : NewsFeeds.getLstDt(o);
                //send data
                o.newsList.ForEach(item => {
                    //only get newest news
                    if(DateTime.Compare(Convert.ToDateTime(item.pubDate),Convert.ToDateTime(lstDt))>0)
                    {                                
                        TelegramBot.send(item);
                        count++;
                    }
                });
                Console.WriteLine("{0} has {1} news", o.title, count);
            });
            NewsFeeds.updateAll();
        }
    }   
}