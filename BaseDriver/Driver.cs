using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
//using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;
using TestTools;

namespace BaseDriver
{
    public class DriverFactory
    {
        private static ThreadLocal<DriverFactory> _threadDriver;

        public Guid Identifier { get; private set; }

        public bool ElmahAdded { get; set; }

        public const int Width = 1920;
        public const int Height = 1080;

        public static DriverFactory GetInstance() => _threadDriver?.Value;

        public static DriverFactory CreateInstance(Guid identifier)
        {
            _threadDriver = new ThreadLocal<DriverFactory>(() => new DriverFactory(), true)
            {
                Value = {Identifier = identifier}
            };
            return _threadDriver.Value;
        }

        public RemoteWebDriver RemoteDriver { get; private set; }
        private IWebDriver _browser;

        public string BrowserName;// => ConfigSettingsReader.BrowserName;

        public int ThreadId { get; }

        public IWebDriver Browser
        {
            get
            {
                if (_browser == null)
                {
                    throw new NullReferenceException(
                        "The WebDriver browser instance was not initialized. You should first call the method Start.");
                }
                return _browser;
            }
            private set { _browser = value; }
        }

        private WebDriverWait _browserWait;

        public WebDriverWait BrowserWait
        {
            get
            {
                if (_browserWait == null || _browser == null)
                {
                    throw new NullReferenceException(
                        "The WebDriver browser wait instance was not initialized. You should first call the method Start.");
                }
                return _browserWait;
            }
            private set { _browserWait = value; }
        }

        public DriverFactory()
        {
            BrowserName = ConfigSettingsReader.BrowserName;
            ThreadId = Environment.CurrentManagedThreadId;
        }

        private DriverOptions GetCapabilities()
        {
            string downloadPath = ConfigSettingsReader.DownloadPath; //.DownloadPath.; @"C:\Temp";
            DriverOptions capabilities = null;
            switch (BrowserName)
            {
                //Mozilla Firefox
                case "Firefox":
                    var profile = new FirefoxProfile
                    {
                        //AcceptUntrustedCertificates = false,
                        //AssumeUntrustedCertificateIssuer = true
                    };
                    profile.SetPreference("browser.download.folderList", 2);
                    //0-desktop,1-file download folder,2-specified location

                    //Set downloadPath
                    profile.SetPreference("browser.download.dir", downloadPath);

                    //Set File Open & Save preferences
                    profile.SetPreference("browser.download.lastDir", downloadPath);

                    profile.SetPreference("browser.helperApps.neverAsk.openFile",// "JSON File");
                    "text/csv,text/plain,text/html,text/anytext,text/comma-separated-values,application/x-msexcel,application/excel,application/x-excel,application/vnd.ms-excel,image/png,image/jpeg,application/msword,application/xml,application/octet-stream,application/plain");
                    //profile.SetPreference("browser.helperApps.neverAsk.saveToDisk", "JSON File");
                    //"text/csv,application/x-msexcel,application/excel,application/x-excel,application/vnd.ms-excel,image/png,image/jpeg,text/html,text/plain,application/msword,application/xml,text/comma-separated-values,application/octet-stream");
                    profile.SetPreference("browser.helperApps.alwaysAsk.force", false);
                    profile.SetPreference("pdfjs.previousHandler.alwaysAskBeforeHandling", false);
                    profile.SetPreference("browser.safebrowsing.downloads.enabled", true);
                    profile.SetPreference("browser.download.manager.focusWhenStarting", false);
                    profile.SetPreference("browser.download.panel.shown", false);
                    profile.SetPreference("general.useragent.locale", "en-GB");
                    var mfOptions = new FirefoxOptions { Profile = profile };
                    //mfOptions.LogLevel = FirefoxDriverLogLevel.Trace;
                    capabilities = mfOptions;
                    break;
                //Internet Explorer
                //case "InternetExplorer":
                //    var ieOptions = new InternetExplorerOptions();

                //    capabilities = ieOptions;
                //    break;
                //Edge
                case "Edge":
                    var edgeOptions = new EdgeOptions();
                    //edgeOptions.AddAdditionalCapability("acceptSslCerts", "true");
                    capabilities = edgeOptions;
                    break;
                //Google Chrome
                case "Chrome":
                    var gcOptions = new ChromeOptions();

                    gcOptions.AddArguments("--enable-logging --v=1");
                    gcOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);

                    // disable saving passwords notification
                    gcOptions.AddUserProfilePreference("credentials_enable_service", false);

                    // set default download directory
                    //Map<String, Object> prefs = new HashMap<String, Object>(); //Lucent? Nhibernate?
                    //prefs.put("download.default_directory", downloadPath);
                    //gcOptions.setExperimentalOption("prefs", prefs);
                    gcOptions.AddUserProfilePreference("download.default_directory", downloadPath);

                    // disable prompt dialog to save file (save to default download directory in silence mode)
                    gcOptions.AddUserProfilePreference("download.prompt_for_download", false);

                    // disable browser graphical interface
                    if (ConfigSettingsReader.HeadLess)
                        gcOptions.AddArgument("--headless");
                    gcOptions.AddArgument($"--window-size={Width},{Height}");
                    capabilities = gcOptions;
                    break;

                default:
                    Assert.Fail("Unknown browser setting: " + BrowserName);

                    break;
            }
            return capabilities;
        }

        public void Init()
        {
            ElmahAdded = false;
            var capabilities = GetCapabilities();

            var defaultTimeOut = ConfigSettingsReader.DefaultTimeOut();
            var halfDefTimeOut = defaultTimeOut / 2;

            var tries = 9;
            while (true)
                try
                {
                    RemoteDriver = new RemoteWebDriver(new Uri(ConfigSettingsReader.HubUrl), capabilities.ToCapabilities(), TimeSpan.FromSeconds(defaultTimeOut));
                    if (RemoteDriver == null)
                        if (tries-- > 0) continue;
                        else Assert.Fail($"Remote driver was not initialized (hub='{ConfigSettingsReader.HubUrl}')");

                    Browser = new NextEventFiringWebDriver(RemoteDriver);
                    if (Browser == null)
                        if (tries-- > 0)
                        {
                            RemoteDriver.Quit();
                            RemoteDriver = null;
                            continue;
                        }
                        else Assert.Fail("Browser driver was not initialized");

                    BrowserWait = new WebDriverWait(Browser, TimeSpan.FromSeconds(halfDefTimeOut));
                    //BrowserWait.PollingInterval = TimeSpan.FromMilliseconds(50);
                    break;
                }
                catch (Exception)
                {
                    if (tries-- > 0)
                    {
                        if (RemoteDriver != null)
                        {
                            RemoteDriver.Quit();
                            RemoteDriver = null;
                        }
                        continue;
                    }
                    throw;
                }
            //Use Chrome Option instead Manage().Window
            //RemoteDriver.Manage().Window.Maximize();
            //Browser.Manage().Window.Size = new System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width,
            //        Screen.PrimaryScreen.Bounds.Height);
            //RemoteDriver.Manage().Window.Size = new System.Drawing.Size/*(640, 480);//*/(Width, Height);

            Browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(halfDefTimeOut);
            Browser.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(defaultTimeOut - 10);
            Browser.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(halfDefTimeOut + 30);
        }

        public void CloseWebDriver()
        {
            if (RemoteDriver != null)
            {
                RemoteDriver.Quit();
                RemoteDriver = null;
            }
            _threadDriver.Dispose();
            _threadDriver = null;
            ElmahAdded = false;
        }
    }

    public class Driver
    {
        //private static AllureNextReport _report;

        private static DriverFactory _instance;
        public static DriverFactory Instance => _instance ??= DriverFactory.GetInstance();
        public DriverFactory InstDriver => _instance ??= DriverFactory.GetInstance();

        //public static AllureNextReport Report => _report ??= AllureNextReport.Instance;

        public static bool IsElmahAdded => Instance.ElmahAdded;

        public static void ElmahAdded(bool value)
        { Instance.ElmahAdded = value; }

        public int StartBrowser(Guid identifier)
        {
            if (_instance == null || _instance.Identifier != identifier)
            {
                _instance = DriverFactory.CreateInstance(identifier);
            }
            _instance.Init();
            return _instance.ThreadId;
        }

        public void StopBrowser()
        {
            //if (_report != null)
            //    AllureNextReport.StopInstance();
            //_report = null;
            if (_instance != null)
                _instance.CloseWebDriver();
            _instance = null;
        }

        /// <summary>
        /// Take ScreenShot of current driver window and save it to png-file
        /// </summary>
        public static byte[] TakeScreenshot()
        {
            try
            {
                var screenshot = ((ITakesScreenshot)Instance.Browser).GetScreenshot();
                return screenshot.AsByteArray;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
