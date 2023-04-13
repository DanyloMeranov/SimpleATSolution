using BaseDriver;
using BasePage;
using OpenQA.Selenium;

namespace HomeTests
{
    public class HomePageElementMap : BasePageElementMap
    {

        public IWebElement UserMenu => WebDriver.GetElementById("userMenu");

        public IWebElement SignOut => WebDriver.GetElementByClassName("lnk-signout");
    }
}
