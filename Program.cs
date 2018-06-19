using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

using HtmlAgilityPack;

namespace WuxiaScraper
{

    class Program
    {
        static String baseUrl = "http://www.wuxiaworld.com";

        static void Main(string[] args)
        {
            var novelUrl = "/novel/";
            Console.WriteLine("Enter the novel url you want (i.e ancient-strengthening-technique): ");
            novelUrl += Console.ReadLine().Trim();

            var web = new HtmlWeb();
            var indexPage = web.Load(baseUrl + novelUrl);
            var chapters = indexPage.DocumentNode.SelectNodes("//li[@class='chapter-item']//a");
            String storyName = indexPage.DocumentNode.SelectSingleNode("//div[@class='section-content']//div[@class='p-15']//h4").InnerText.Trim();
            String tocHeader = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"></head><body><h1>" + storyName + "</h1><ul>";
            TextWriter toc = new StreamWriter(storyName + ".html", true);

            toc.WriteLine(tocHeader);

            Console.WriteLine("Fetching: " + storyName);

            Int32 chapNum = 1;
            foreach (var chap in chapters)
            {
                //System.Threading.Thread.Sleep(1000);
                var title = chap.InnerText.Trim();

                var chapUrl = chap.Attributes["href"].Value.Trim();

                var chapterPage = web.Load(baseUrl + chapUrl);
                var chapterText = chapterPage.DocumentNode.SelectSingleNode("//div[@class='fr-view']").InnerHtml;
                Console.WriteLine("Added chapter" + chapNum.ToString() + " to database: " + title);
                chapterText = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"></head>\n<body><a href=Chapter-" + (chapNum - 1).ToString().PadLeft(5, '0') + ".html>Prev</a><a href=Chapter-" + (chapNum + 1).ToString().PadLeft(5, '0') + ".html>Next</a>" + chapterText + "</body></html>";
                var trueChapNum = chapNum.ToString().PadLeft(5, '0');
                var chapFilename = "Chapter-" + trueChapNum + ".html";
                File.WriteAllText(chapFilename, chapterText);
                Console.WriteLine(chapFilename);
                toc.WriteLine("<li><a href=" + chapFilename + ">Chapter " + chapNum + "</a></li>");


                chapNum++;
            }

            toc.WriteLine("</ul></body></html>");
            toc.Close();

            Console.WriteLine("Do you want to convert to epub? (Requires Calibre) (Y/N): ");
            var shouldMakeEpub = Console.ReadKey().ToString().ToUpper() == "Y";
            if (shouldMakeEpub)
            {
                System.Diagnostics.Process.Start("ebook-convert", "\"" + storyName + ".html\" " + storyName + ".epub --max-levels 1 --max-toc-links 0");
            }

        }
    }
}
