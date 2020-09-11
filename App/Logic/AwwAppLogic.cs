using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using App.Logic.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace App.Logic
{
    public class AwwAppLogic : IAwwAppLogic
    {
        public async IAsyncEnumerable<string> GenerateLinks(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentException("Argument out of range", nameof(count));
            }

            var random = new Random();
            var mutex = new SemaphoreSlim(1);

            var path = Environment.GetEnvironmentVariable("WD_CHROME_PATH");

            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            
            using var driver = new ChromeDriver(path);

            while (count-- > 0)
            {
                await mutex.WaitAsync();
                
                driver.Manage().Cookies.DeleteAllCookies();

                driver.Navigate().GoToUrl("https://awwapp.com/");

                driver.FindElement(By.LinkText("Start drawing")).Click();
                driver.FindElement(By.Id("collaborate-button")).Click();

                var link = driver.FindElement(By.ClassName("js-board-link")).GetAttribute("value");
                
                Thread.Sleep(random.Next(50, 1000));

                mutex.Release();
                
                yield return link;
            }
        }
    }
}
