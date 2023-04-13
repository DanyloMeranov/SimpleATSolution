using BaseDriver;
using BasePage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HomeTests
{
    public class HomePageValidator : BasePageValidator<HomePageElementMap>
    {
        public void AssertUserSignedIn(string userName)
        {
            Assert.IsTrue(WebDriver.GetCurrentUser().ToLower().Contains(userName.ToLower()), $"User '{userName}' is not signed in.");
        }

        public bool AssertLanguageIsEnglish()
        {
            var currentLang = WebDriver.GetElementById("login-select-language").GetElementValue();
            Assert.IsTrue("en".Equals(currentLang), $"Expected 'en' but '{currentLang}' is present");
            return false;
        }
    }
}
