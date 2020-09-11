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
        
        public async Task<List<string>> GenerateLinks(int count, bool cacheMode = false)
        {
            if (count <= 0 || count > 20)
            {
                throw new ArgumentException("Argument out of range", nameof(count));
            }

            var links = cacheMode ? new List<string>() : await _awwAppLinkDal.Collect(count);

            var mutex = new SemaphoreSlim(1);
            
            var tasks = Enumerable.Range(0, count - links.Count).Select(async _ =>
            {
                await mutex.WaitAsync();

                using var driver = GetWebDriver();

                driver.Manage().Cookies.DeleteAllCookies();

                var link = await ResolveUniqueUrl(driver);

                driver.Close();

                Thread.Sleep(Random.Next(50, 1000));

                mutex.Release();

                return link;
            }).ToList();

            await Task.WhenAll(tasks);

            return tasks.Select(x => x.Result).ToHashSet().ToList();
        }

        private static IWebDriver GetWebDriver()
        {
            var googleChromeShim = Environment.GetEnvironmentVariable("GOOGLE_CHROME_SHIM");

            return !string.IsNullOrWhiteSpace(googleChromeShim)
                ? new ChromeDriver(new ChromeOptions
                {
                    BinaryLocation = googleChromeShim
                })
                : new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
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
    }
}
