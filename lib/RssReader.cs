using System;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Collections.Generic;
/*
https://stackoverflow.com/questions/10399400/best-way-to-read-rss-feed-in-net-using-c-sharp
*/
namespace lib
{
    public class RssReader
    {
        public List<News_Source> getNews(string URLString)
        {
            XmlTextReader reader = new XmlTextReader (URLString);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            List<News> result = new List<News>();
            List<News_Source> Source = new List<News_Source>();
            string upDate = "1911-10-10 00:00";
            // Get each news
            foreach (SyndicationItem item in feed.Items)
            {
                String title = item.Title.Text;    
                String summary = item.Summary == null ? "" : item.Summary.Text;
                String pubDate = item.PublishDate.ToString("yyyy-MM-dd HH:mm");
                String url = item.Links[0].Uri.ToString();
                result.Add(new News(){
                    title = title, 
                    summary = summary, 
                    pubDate = pubDate,
                    url = url
                    });
                upDate = DateTime.Compare(Convert.ToDateTime(pubDate), Convert.ToDateTime(upDate)) > 0 ? pubDate : upDate;
                
            }
            Source.Add(new News_Source(){
                title = feed.Title.Text,
                rssURL = URLString,
                LstMntDt = upDate,
                newsList = result
                });
            reader.Close();
            return Source;
        }
    }
    public class News_Source
    {
        public string title{get; set;}
        public string rssURL{get; set;}
        public string LstMntDt{get; set;}
        public List<News> newsList{get; set;}
    }
    public class News
    {
        public string title{get; set;}
        public string summary {get; set;}
        public string pubDate {get; set;}
        public string url {get; set;}
    }
}
