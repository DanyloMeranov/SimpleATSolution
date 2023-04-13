using BaseDriver;
using BasePage;

namespace SignInTests
{
    public class SignInPage : BasePage<SignInPageElementMap>
    {
        public SignInPage(Driver mainDriver)
            : base("", "")
        {
            MainDriver = mainDriver;
            Page.MainDriver = mainDriver;
        }

        public void FillSignInForm(string userName, string password)
        {
            Page.SignInFormReady();
            var language = WebDriver.GetElementById("login-select-language").GetElementValue();//GetAttribute("value");
            //if (language != "English" && language != "en")
            //{
                var langdd = WebDriver.GetElementByClassName("k-dropdownlist");
            var englishCaption = WebDriver.GetListItemByLang();
            StartStep($"Change language from '{language}' to '{englishCaption}'");
            Page.SelectValueInDropDownList(langdd, englishCaption);
            FinishStep();
            WebDriver.Pause();
            WebDriver.WaitForReadyState();
            Page.SignInFormReady();
            //}
            WebDriver.SetTextBoxValue("UserName", userName);
            WebDriver.SetTextBoxValue("Password", password);
            StartStep($"Selected language is '{WebDriver.GetElementById("login-select-language").GetElementValue()}'");
            FinishStep();
            try
            {
                StartStep("Click on SignIn button");
                Page.SignInButton.Click2();
            }
            finally
            {
                FinishStep();
            }
        }

        public bool IsRedirect()
        {
            return Page.WebDriver.GetCurrentUrl().Contains("ReturnUrl=%2FError%2FHttpError") || Page.WebDriver.GetElementByClassName("btn-sign-in") != null;
        }
    }
}
