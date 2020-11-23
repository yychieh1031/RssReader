using System;
using System.Collections.Generic;
/*
https://www.developersoapbox.com/connecting-to-a-sqlite-database-using-net-core/
*/
namespace lib
{
    public class NewsFeeds
    {
        //
        // Add new news feeds in db
        //
        static public void updateSts(List<News_Source> newsdata)
        {
            var connection = SqliteHelper.DBContext("NewsRss_URL.db");

            using (connection)
            {
                connection.DefaultTimeout = 60;
                //If table not exist than create one
                connection.Open();
                 var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = String.Format(@"CREATE TABLE IF NOT EXISTS NewsRssURL(
                                                            news_Id INTEGER PRIMARY KEY,
                                                            LstMntDt NUMERIC,
                                                            title VARCHAR(255),
                                                            rssURL VARCHAR(100))"
                                                            );
                createTableCmd.ExecuteNonQuery();

                // Get Last update datetime from NewsRssRUL
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM NewsRssURL";
                List<News_Source> tempList = new List<News_Source>();
                RssReader rss = new RssReader();

                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        News_Source temp = new News_Source{
                            LstMntDt = reader["LstMntDt"].ToString(),
                            title = reader["title"].ToString()
                        };
                        tempList.Add(temp);
                    }
                }

                newsdata.ForEach(o => {

                    using (var transaction = connection.BeginTransaction())
                    {
                        var LstMntDt_his = "1911-01-01 00:00";
                        var insertCmd = connection.CreateCommand();
                        var deleteCmd = connection.CreateCommand();
                        var titleOs_res = YCLib.stringHandler(o.title);

                        tempList.ForEach(x => {
                            if(x.title == titleOs_res){
                                LstMntDt_his = x.LstMntDt;
                            }
                        });
                        // Require to update NewsRssRUL or not
                        if(DateTime.Compare(Convert.ToDateTime(o.LstMntDt), Convert.ToDateTime(LstMntDt_his))>0){
                            deleteCmd.CommandText = String.Format(@"DELETE FROM NewsRssURL WHERE title = '{0}'", titleOs_res);
                            deleteCmd.ExecuteNonQuery();

                            insertCmd.CommandText = String.Format(@"INSERT INTO NewsRssURL (LstMntDt , title, rssURL) VALUES('{0}', '{1}', '{2}')",
                                                                    o.LstMntDt, titleOs_res, o.rssURL);
                            insertCmd.ExecuteNonQuery();

                            try{
                                transaction.Commit();
                            }
                            catch(Exception ex){
                                Console.WriteLine("{0}", ex.ToString());
                            }

                        };
                    }
                });
            }
        }

        static public void updateAll(){
            
            List<News_Source> insertData = new List<News_Source>();

            var connection = SqliteHelper.DBContext("NewsRss_URL.db");

            using (connection)
            {
                connection.Open();

                //Read the newly inserted data
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM NewsRssURL";

                
                List<News_Source> tempList = new List<News_Source>();
                RssReader rss = new RssReader();
                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tempList = rss.getNews(reader["rssURL"].ToString());
                        insertData.AddRange(tempList);
                        
                    }
                }
            }
            post(insertData);
        }


        //
        // Create News Feed Table and Insert Data
        //
        static public void post(List<News_Source> newsdata)
        {
            var connection = SqliteHelper.DBContext("NewsFeeds_Main.db");

            using (connection)
            {
                updateSts(newsdata);

                connection.Open();
                //Create a table (drop if already exists first):
                newsdata.ForEach(o => {
                    var titleOs_res = YCLib.stringHandler(o.title);

                    var createTableCmd = connection.CreateCommand();
                    createTableCmd.CommandText = String.Format(@"CREATE TABLE IF NOT EXISTS {0}(
                                                                news_Id INTEGER PRIMARY KEY,
                                                                pubDt VARCHAR(100),
                                                                title VARCHAR(255),
                                                                summary VARCHAR(255),
                                                                url VARCHAR(255))"
                                                                , titleOs_res);
                    try{
                        createTableCmd.ExecuteNonQuery();
                    }
                    catch(Exception ex){
                        Console.WriteLine("{0}", ex.ToString());
                    }
                    //Get Lst news date
                    var lstDt = getLstDt(o) == "" ? "1911-01-01 00:00" : getLstDt(o);
                    //Seed some data:
                    using (var transaction = connection.BeginTransaction())
                    {
                        var titleOs = String.Empty;
                        var summaryOs = String.Empty;
                        var urlOs = String.Empty;
                        var insertCmd = connection.CreateCommand();
                        var count = 0;
                        o.newsList.ForEach(item => {
                            if(DateTime.Compare(Convert.ToDateTime(item.pubDate),Convert.ToDateTime(lstDt))>0)
                            {
                                titleOs = item.title.Replace("'","''");
                                summaryOs = item.summary.Replace("'","''");
                                urlOs = item.url.Replace("'","''");
                                insertCmd.CommandText = String.Format(@"INSERT INTO {4} (pubDt , title, summary, url) VALUES('{0}', '{1}', '{2}', '{3}')",
                                                                        item.pubDate, titleOs, summaryOs, urlOs, titleOs_res);
                                insertCmd.ExecuteNonQuery();
                                count ++;
                            }
                        });
                        try{
                            transaction.Commit();
                            if(count == 0){
                                Console.WriteLine("NOTICE: Nothing update - {0}.", titleOs_res);
                            }else{
                                Console.WriteLine("SUCCESS: Inserted {0} news in {1}.", count, titleOs_res);
                            }
                        }
                        catch(Exception ex){
                            Console.WriteLine("{0}", ex.ToString());
                        }
                    }
                });
            }
        }

        //
        // Drop Target News table
        //
        static public void delete(News_Source newsdata)
        {
            var connection = SqliteHelper.DBContext("NewsFeeds_Main.db");

            using (connection)
            {
                connection.Open();
                var titleOs_res = newsdata.title.Replace(" ", "_").Replace("-", "_").Replace("|","").Replace("__","_");
                var delTableCmd = connection.CreateCommand();
                delTableCmd.CommandText = String.Format(@"DROP TABLE IF EXISTS {0}", titleOs_res);
                delTableCmd.ExecuteNonQuery();
            }
        }
        
        // 
        // Get All Feeds Table Name
        // 
        static public List<News_Source> get()
        {
            var connection = SqliteHelper.DBContext("NewsFeeds_Main.db");

            using (connection)
            {
                connection.Open();

                //Read the newly inserted data:
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT name FROM sqlite_master WHERE type ='table' AND name NOT LIKE 'sqlite_%'";

                List<News_Source> result = new List<News_Source>();
                using (var reader = selectCmd.ExecuteReader())
                {
                    var i = 1;
                    while (reader.Read())
                    {
                        var message = String.Format("{0})", i) + reader["name"];
                        Console.WriteLine(message);
                        i++;

                        result.Add(new News_Source(){
                            title = reader["name"].ToString()
                        });
                    }
                }
                return result;
            }
        }

        //
        // Search news detail
        //
        static public void get(News_Source newsdata)
        {
            var connection = SqliteHelper.DBContext("NewsFeeds_Main.db");

            var titleOs = YCLib.stringHandler(newsdata.title);
            using (connection)
            {
                connection.Open();

                //Read the newly inserted data:
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = String.Format("SELECT * FROM {0} ORDER BY pubDt DESC LIMIT 20", titleOs);

                using (var reader = selectCmd.ExecuteReader())
                {
                    var count = 1;
                    Console.Clear();
                    while (reader.Read())
                    {
                        var message = string.Format("{0})",count) + reader["pubDt"] + "  " + reader["title"] + "\n";// + "summary:" + reader["summary"];
                        count++;
                        Console.WriteLine(message);
                    }
                }
            }
        }

        //
        // Get News Feeds List
        //
        static public void getFeeds()
        {
            var connection = SqliteHelper.DBContext("NewsRss_URL.db");

            using (connection)
            {
                connection.Open();

                //Read the newly inserted data:
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM NewsRssURL";

                using (var reader = selectCmd.ExecuteReader())
                {
                    var i = 1;
                    while (reader.Read())
                    {
                        var message = String.Format("{0})", i) + reader["title"] + "\n  URL:" + reader["rssURL"];
                        Console.WriteLine(message);
                        i++;
                    }
                }
            }
        }

        //
        // Get NewsFeeds Lastest Update Date
        //
        static public string getLstDt(News_Source newsdata)
        {
            var connection = SqliteHelper.DBContext("NewsFeeds_Main.db");

            var result = string.Empty;
            var titleOs = YCLib.stringHandler(newsdata.title);
            using (connection)
            {
                connection.Open();

                //Read the newly inserted data:
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = String.Format("SELECT * FROM {0} ORDER BY pubDt Desc LIMIT 1", titleOs);

                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader["pubDt"].ToString();
                    }
                }
            }
            return result;
        }
    }
}
