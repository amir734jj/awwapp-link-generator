using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using App.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace App.Logic
{
    public class AwwAppLogic : IAwwAppLogic
    {
        private static readonly Random Random = new Random();

        private readonly IAwwAppLinkDal _awwAppLinkDal;
        
        public AwwAppLogic(IAwwAppLinkDal awwAppLinkDal)
        {
            _awwAppLinkDal = awwAppLinkDal;
        }

        private async Task<List<string>> GenerateLinks(int count, bool cacheMode)
        {
            if (count <= 0 || count > 20)
            {
                throw new ArgumentException("Argument out of range", nameof(count));
            }

            var links = cacheMode ? new List<string>() : await _awwAppLinkDal.Collect(count);

            foreach (var _ in Enumerable.Range(0, count - links.Count))
            {
                using var driver = GetWebDriver();

                driver.Manage().Cookies.DeleteAllCookies();

                var link = await ResolveUniqueUrl(driver);

                if (!cacheMode)
                {
                    await _awwAppLinkDal.MarkUsed(link);
                }

                driver.Close();

                Thread.Sleep(Random.Next(50, 2000));

                links.Add(link);
            }
            
            return links.ToHashSet().ToList();
        }

        private static IWebDriver GetWebDriver()
        {
            var googleChromeShim = Environment.GetEnvironmentVariable("GOOGLE_CHROME_SHIM");

            if (!string.IsNullOrWhiteSpace(googleChromeShim))
            {
                return new ChromeDriver(new ChromeOptions
                {
                    BinaryLocation = googleChromeShim
                });
            }

            return new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        private async Task<string> ResolveUniqueUrl(IWebDriver driver)
        {
            driver.Navigate().GoToUrl("https://awwapp.com/");

            driver.FindElement(By.LinkText("Start drawing")).Click();

            driver.FindElement(By.Id("collaborate-button")).Click();

            var link = driver.FindElement(By.ClassName("js-board-link")).GetAttribute("value");

            await _awwAppLinkDal.Insert(link);

            return link;
        }

        public async Task<List<string>> GenerateLinks(int count)
        {
            return await GenerateLinks(count, false);
        }

        public async Task CacheLinks(int count)
        {
            await GenerateLinks(count, true);
        }
    }
}
