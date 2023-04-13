using System;
using BaseDriver;
using BaseTests;
using HomeTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestTools;

//[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]
namespace SignInTests
{
    [TestClass]
    public class SignInTests : BaseTest
    {
        private const string DataFile = "SignInTestData.xml";
        private const string ConnectionString = RelativePathForDataFiles + DataFile;

        private HomePage _homePage;

        public void InitTests(string username, string password)
        {
            //if (!ConfigSettingsReader.Tenant.Equals("/") && ConfigSettingsReader.Tenant.Length > 2)
            //{ 

            //}
            var page = new SignInPage(MainDriver);
            _homePage = new HomePage(MainDriver);
            try
            {
                StartStep("Initializing tests");

                var tries = 0;

                while (true)
                {
                    try
                    {
                        StartStep("Filling Sign In form");
                        page.UserUpdate(username);
                        page.Navigate();
                        var tenant = ConfigSettingsReader.Tenant.Trim('/');
                        if (!page.Page.WebDriver.GetCurrentUrl().Contains(tenant))
                            page.Page.WebDriver.GetElementByCustomScript($"return Array.from(document.querySelectorAll('a.btn-secondary')).find(el => el.textContent.includes('{tenant}'))").Click();
                        if (page.Page.WebDriver.GetElementById("loginPage") == null)
                        {
                            page.Navigate("useraccount/logoff");
                            if (!page.Page.WebDriver.GetCurrentUrl().Contains(tenant))
                                page.Page.WebDriver.GetElementByCustomScript($"return Array.from(document.querySelectorAll('a.btn-secondary')).find(el => el.textContent.includes('{tenant}'))").Click();
                        }

                        //var currentUser = page.Page.WebDriver.GetCurrentUser();
                        //if (page.Page.WebDriver.GetElementById("loginPage") == null &&
                        //    currentUser != null && !currentUser.ToLower().Contains(username.ToLower()))
                        //{
                        //    _homePage.UserUpdate(currentUser);
                        //    _homePage.SignOut();
                        //}

                        if (page.Page.WebDriver.GetElementById("loginPage") != null)
                            page.FillSignInForm(username, password);

                        FinishStep();//Filling

                        StartStep($"Verifying that user '{username}' signed in");
                        _homePage = new HomePage(MainDriver);
                        _homePage.UserUpdate(username);
                        _homePage.Page.WebDriver.WaitForReadyState();
                        if (_homePage.Page.WebDriver.GetElementById("userMenu") == null)
                            if (_homePage.Page.WebDriver.GetCurrentUrl().Contains(ConfigSettingsReader.Tenant))
                                Assert.Fail("Log in is failed");
                            else
                                Assert.Fail("Redirect to root index page");
                        _homePage.Page.SetLanguage();
                        _homePage.Validate().AssertLanguageIsEnglish();
                        _homePage.Page.WebDriver.WaitForReadyStateByLink("Contact Us");
                        _homePage.Validate().AssertUserSignedIn(username);

                        return;
                    }
                    catch (Exception e)
                    {
                        if ((!(page.IsRedirect() ||
                            page.Page.WebDriver.CheckErrors() ||
                            page.Page.WebDriver.CheckValidation500Error() ||
                            page.CheckErrorRequest())) &&
                            !e.Message.Contains("This page isn’t working"))
                        {
                            Assert.Fail($"Initialization error: {e.Message}");
                        }
                        if (tries >= 5)
                        {
                            if (e.Message.Contains("This page isn’t working"))
                                Assert.Fail($"Initialization error: {e.Message}");
                            if (page.CheckErrorRequest())
                                Assert.Fail("Connection error!");
                            if (page.Page.WebDriver.CheckErrors())
                                throw;
                        }
                        page.Page.AddElmahDetail();
                        if (tries < 5)
                        {
                            Driver.ElmahAdded(false);
                            page = new SignInPage(MainDriver);
                            _homePage = new HomePage(MainDriver);
                        }
                    }
                    finally
                    {
                        FinishStep();//Verifying or Filling
                    }
                    if (tries >= 5)
                        Assert.Fail(!_homePage.Page.WebDriver.IsReadyStateComplete()
                            ? "Browser is not answered!"
                            : "Redirect is happened! :(");
                    StartStep($"{(!_homePage.Page.WebDriver.IsReadyStateComplete() ? "Browser is not answered" : "Redirected")}. Try again ({++tries})");
                    FinishStep();//try again
                }
            }
            catch (Exception e)
            {
                page.Page.AddElmahDetail();
                throw;
            }
            finally
            {
                FinishStep();//Initializing
            }

        }

        public void SignOutUser()
        {
            _homePage = new HomePage(MainDriver);
            var username = _homePage.Page.WebDriver.GetCurrentUser();
            StartStep($"Sign out user '{username}'");
            _homePage.SignOut();
            FinishStep();//Sign out
        }

        [TestMethod]
        [TestCategory("QUICK")]
        [TestCategory("VICASH")]
        public void SignInCorrect()
        {
            var i = GetDataDictionary(ConnectionString, "CorrectCredentials");
            InitTests(GetDataValue("UserName"), GetDataValue("Password"));
            SignOutUser();
            while (i > 1)
            {
                RestartDriver();
                GetDataDictionary(--i);
                InitTests(GetDataValue("UserName"), GetDataValue("Password"));
                SignOutUser();
            }
        }

    }
}
