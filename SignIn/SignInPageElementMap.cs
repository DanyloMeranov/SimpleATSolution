using BaseDriver;
using BasePage;
using OpenQA.Selenium;

namespace SignInTests
{
    public class SignInPageElementMap : BasePageElementMap
    {
        public IWebElement SignInButton => WebDriver.GetElementByClassName("btn-sign-in");
    }
}
