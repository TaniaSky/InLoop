using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InLooP
{
    public class PageBase
    {
        public IWebDriver _driver;
        public WebDriverWait _wait;

        public PageBase(IWebDriver driver)
        {
            this._driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(60));

            WaitForPageLoad();
            CloseCookieBanner();
            PageFactory.InitElements(_driver, this);

        }
        
        public void WaitForPageLoad()
        {
            Console.WriteLine("Waiting");
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            wait.Until((wdriver) =>
               (wdriver as IJavaScriptExecutor).ExecuteScript("return document.readyState").Equals("complete")

            );
            wait.Until(wdriver => Int32.Parse((wdriver as IJavaScriptExecutor).ExecuteScript("if (window.jQuery) {return jQuery.active;  } else { return -1;}").ToString()) <= 0);
            
            // here need to check that angular loaded
            // I tried few solutions, but they didn't work good
            // need to prepare good javascript 

            (_driver as IJavaScriptExecutor).ExecuteScript("window.scrollTo(0,document.body.scrollHeight);");
            (_driver as IJavaScriptExecutor).ExecuteScript("window.scrollTo(0,document.body.scrollHeight);");

            Thread.Sleep(5000);

          
        }

        public void CloseCookieBanner()
        {
            if (_driver.FindElements(By.XPath("*//div[@aria-label='cookieconsent']")).Count>0 && _driver.FindElement(By.XPath("*//div[@aria-label='cookieconsent']")).Displayed)
            {
                _driver.FindElement(By.XPath("*//a[text() = 'I accept']")).Click();
                WaitForPageLoad();
            }
        }
        public virtual bool IsDisplayed()
        {
            return false;
        }
    }

    public class RecentNewsPage : PageBase
    {
        [FindsBy(How = How.XPath, Using = "*//div/ul[not (contains (@class, 'widget'))][contains (@class, 'tag-list')]/li[@class='ng-scope']")]
        public IList<IWebElement> MainMenuItems { get; set; }

        [FindsBy(How = How.XPath, Using = "*//div[@id='newsletters-archive']/div/article/header/h3/a")]
        public IList<IWebElement> NewslettersItems { get; set; }

        [FindsBy(How = How.TagName, Using = "body")]
        public IWebElement Body { get; set; }

        public void OpenNewsLetter(string label)
        {
            OpenQA.Selenium.Interactions.Actions actions = new OpenQA.Selenium.Interactions.Actions(_driver);
            actions.MoveToElement(_driver.FindElement(By.XPath(String.Format("*//a[text()='{0}']", label)))).Build().Perform();
            _driver.FindElement(By.XPath(String.Format("*//a[text()='{0}']", label))).Click();
        }

        public RecentNewsPage(IWebDriver driver) : base(driver) { }
        public override bool IsDisplayed()
        {
            return MainMenuItems.Count>0;
        }
    }

    public class MondayNewsLetterPage : PageBase
    {
        [FindsBy(How = How.XPath, Using = "*//span[text()='TOP INDUSTRY NEWS']")]
        public IWebElement TopIndustryNewsLabel { get; set; }

        public void OpenTag (string label)
        {
            _driver.FindElement(By.XPath(String.Format("*//*//a[@class='mcnButton'][text()='{0}']", label))).Click();
        }
        public MondayNewsLetterPage(IWebDriver driver) : base(driver) { }
        public override bool IsDisplayed()
        {
            return _wait.Until(drv => TopIndustryNewsLabel.Displayed);
        }
    }

    public class SelectedTagPage : PageBase
    {
        [FindsBy(How = How.XPath, Using = "*//h3[contains(@class, 'title')]")]
        public IWebElement Title { get; set; }

       
        public SelectedTagPage(IWebDriver driver) : base(driver) { }
        public override bool IsDisplayed()
        {
            return _wait.Until(drv => Title.Displayed);
        }
    }
}
