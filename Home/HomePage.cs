using BaseDriver;
using BasePage;

namespace HomeTests
{
    public class HomePage : BasePage<HomePageElementMap, HomePageValidator>
    {
        public HomePage(Driver mainDriver)
            : base("Home/Index", "", mainDriver)
        {

        }

        public void SignOut()
        {
            Page.WebDriver.WaitForReadyState(refreshable: true);
            Page.UserMenu.Click2();
            Page.SignOut.Click2();
            Page.SignInFormReady();
        }
    }
}
