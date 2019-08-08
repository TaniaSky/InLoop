using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium.Support;
using System.Collections;

namespace InLooP
{
    //here list of browsers can be added
    [TestFixture(typeof(ChromeDriver))]
    public partial class SetupBase<TWebDriver>  where TWebDriver : IWebDriver, new()
    {
        public IWebDriver _driver;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Init();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (this._driver != null)
            {
                _driver.Close();
                _driver.Quit();
            }
        }
        public void Init()
        {
            var type = typeof(TWebDriver);

            if (type == typeof(ChromeDriver))
                _driver = InitializeChromeDriver();
            else
                _driver = InitOtherDriver();

            _driver.Manage().Window.Position = new Point(0, 0);
            _driver.Manage().Window.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width,
               Screen.PrimaryScreen.WorkingArea.Height);
        }

        private ChromeDriver InitializeChromeDriver()
        {
            ChromeOptions options = new ChromeOptions();

            options.AddArguments("chrome.switches", "--disable-extensions");
            options.AddArguments("disable-infobars");
            options.AddArguments("start-maximized");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(150));
            

            return driver;
        }

        private IWebDriver InitOtherDriver()
        {
            //Just mock
            return new ChromeDriver();
        }

        public void SwitchToLastTab()
        {
            List<String> browserTabs = new List<string>(_driver.WindowHandles);
            _driver.SwitchTo().Window(browserTabs.Last());
        }

        public List<string> GetTopNewsletters()
        {
            var res = new List<string>();
            var date = DateTime.Now;
            if (date.Hour < 17)  //here need to know exact condition
                date = date.AddDays(-1);

            while (res.Count != 5)
            {
                if (date.DayOfWeek == DayOfWeek.Monday || date.DayOfWeek == DayOfWeek.Thursday)
                    res.Add(String.Format("Newsletter {0}", date.ToString("d. MMMM. yyyy")));
                date = date.AddDays(-1);
            }

            return res;
        }

        public string GetLastMondayNewsLetterTitle()
        {
            var res = String.Empty;
            var date = DateTime.Now;
            if (date.Hour < 17)  //here need to know exact condition
                date = date.AddDays(-1);

            while (res == String.Empty)
            {
                if (date.DayOfWeek == DayOfWeek.Monday)
                    res = String.Format("Newsletter {0}", date.ToString("d. MMMM. yyyy"));
                date = date.AddDays(-1);
            }

            return res;
        }

        public void SwitchToBaseTab()
        {
            List<String> browserTabs = new List<string>(_driver.WindowHandles);
            while (browserTabs.Count > 1)
            {
                _driver.SwitchTo().Window(browserTabs.Last());
                _driver.Close();
                _driver.SwitchTo().Window(browserTabs[0]);
                browserTabs = new List<string>(_driver.WindowHandles);
            }
           
        }
    }

    [Parallelizable]
    public class _001_GeneralTests_Newsletter<TWebDriver> : SetupBase<TWebDriver> where TWebDriver : IWebDriver, new()
    {
        public string MainSiteUrl = "https://athletictrainers.inloop.com/en/news"; //Can be moved to config if have few test environments 
        [SetUp]
        public void OpenMainPage()
        {
            _driver.Navigate().GoToUrl(MainSiteUrl);
            var _page = new RecentNewsPage(_driver); 
            Assert.IsTrue(_page.IsDisplayed());
        }

        [Test]
        public void _001_VerifyListIsCorrect()
        {
            Assert.Multiple(() =>
            {
                var _expecteNewsletterCount = 5;

                var _page = new RecentNewsPage(_driver);
                Assert.IsTrue(_page.IsDisplayed());

                Assert.AreEqual(_expecteNewsletterCount, _page.NewslettersItems.Count);
           
                var _actualList = new List<string>();
                foreach (var el in _page.NewslettersItems)
                    _actualList.Add(el.Text);

                var _expectedList = GetTopNewsletters();
                CollectionAssert.AreEqual(_expectedList, _actualList);
            });
        }

        [Test]
        public void _002_VerifyLetterOpens()
        {
            var _page = new RecentNewsPage(_driver);
            Assert.IsTrue(_page.IsDisplayed());

            _page.OpenNewsLetter(GetLastMondayNewsLetterTitle());
            SwitchToLastTab();

            var _newsLetterPage = new MondayNewsLetterPage(_driver);
            Assert.IsTrue(_newsLetterPage.IsDisplayed());
        }

        

        
    }

    [Parallelizable]
    public class _002_Monday_Newsletter<TWebDriver> : SetupBase<TWebDriver> where TWebDriver : IWebDriver, new()
    {
        public string MainSiteUrl = "https://athletictrainers.inloop.com/en/news"; //Can be moved to config if have few test environments 
        public string NewsLetterPageUrl = String.Empty;

        [OneTimeSetUp]
        public void GetNewsletterPage()
        {
            _driver.Navigate().GoToUrl(MainSiteUrl);
            var _page = new RecentNewsPage(_driver);
            Assert.IsTrue(_page.IsDisplayed());

            _page.OpenNewsLetter(GetLastMondayNewsLetterTitle());
            SwitchToLastTab();

            var _newsLetterPage = new MondayNewsLetterPage(_driver);
            Assert.IsTrue(_newsLetterPage.IsDisplayed());

            NewsLetterPageUrl = _driver.Url;
        }

        [SetUp]
        public void Setup()
        {
            _driver.Navigate().GoToUrl(NewsLetterPageUrl);
            var _newsLetterPage = new MondayNewsLetterPage(_driver);
            Assert.IsTrue(_newsLetterPage.IsDisplayed());
        }

        [TearDown]
        public void Teardown()
        {
            SwitchToBaseTab();
        }

        public static IList<string> TagsInLetter = new List<string>()
        {
            "Injuries",
            "Football",
            "Head Athletic Trainers",
            "Head Injuries",
            "NATA",
            "Basketball"
        };        

        public static IEnumerable TagsInLetter_TestCaseSource
        {
            get
            {
                foreach (var el in TagsInLetter)
                {
                    yield return new TestCaseData(el)
                        .SetName(String.Format("_003_TagInLetter_{0}",  el));
                }
            }
        }


        [Test, TestCaseSource(nameof(TagsInLetter_TestCaseSource))]
        public void _001_VerifyTagOpensCorrectPage(String tag)
        {
            var _newsLetterPage = new MondayNewsLetterPage(_driver);
            Assert.IsTrue(_newsLetterPage.IsDisplayed());

            _newsLetterPage.OpenTag(tag);

            SwitchToLastTab();

            var _taggedPage = new SelectedTagPage(_driver);
            Assert.IsTrue(_taggedPage.IsDisplayed());

            Assert.AreEqual(tag.ToUpper(), _taggedPage.Title.Text.ToUpper());

        }

        



    }
}
