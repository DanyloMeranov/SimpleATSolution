using System;
using System.Collections.ObjectModel;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Threading;
using TestTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaseDriver
{
    public static class Extentions
    {
        private static IWebDriver Browser => Driver.Instance.Browser;
        private static WebDriverWait BrowserWait => Driver.Instance.BrowserWait;
        private static string BrowserName => Driver.Instance.BrowserName;

        public static void Pause(this object _, int millisecons = 500)
        {
            //Driver.Report.StartStep($"Paused for {millisecons} ms");
            Thread.Sleep(millisecons);
            //AllureNextReport.FinishStep();
        }

        public static void CloseJavaAlert(this IWebDriver driver)
        {
            try
            {
                Thread.Sleep(500);
                driver.SwitchTo().Alert().Dismiss();
            }
            catch (NoAlertPresentException) { }
        }

        public static void CloseModalDialogs(this IWebDriver _)
        {
            try
            {
                WindowsDialog.CancelButonClick();
            }
            catch (Exception) { /*ignored*/ }
        }

        public static object Execute(this object driver, string script, params object[] args)
        {
            try
            {
                return ((IJavaScriptExecutor)driver).ExecuteScript(script, args);
            }
            catch (UnhandledAlertException e)
            {
                Browser.CloseJavaAlert();
                Browser.CloseModalDialogs();
                var msg = e.Message.Contains('{') ? e.Message.Split('{', '}')[1] : e.Message;
                var reason = msg.Split(':')[1].Trim();
                Assert.Fail(reason != ""
                    ? $"Javascript on page causes popup window: {reason}"
                    : "Something wrong! Open dialog window is not closed yet");
            }
            catch (Exception e)
            {
                if (e.Message.Contains("The HTTP request to the remote WebDriver server for"))
                    Assert.Fail($"Connection trouble: {e.Message}");
                if(e.Message.Contains("Timed out receiving message"))
                    Assert.Fail(e.Message);
                if (e.Message.Contains("javascript error"))
                    _ = new JavaScriptException($"javascript error: script \n{{\n{script}\n}}\n returns error message:\n {e.Message}");
                if (!(e.Message.Contains("Timed out after 60 seconds")
                    || e.Message.Contains("ConnectException")
                    || e.Message.Contains("chrome not reachable")
                    || e.SkipableFail())
                   )
                    Browser.Assert500();
                throw;
            }
            return null;
        }

        public static IWebElement GetElementByCustomScript(this object driver, string script, params object[] args)
        {
            return (IWebElement)Execute(driver, script, args);
        }

        public static string GetCurrentUser(this IWebDriver driver)
        {
            return (string)driver.Execute("return document.querySelector('#userMenu')?.innerText");
        }

        //public static string GetCookie(this object obj) => (string)Browser.Execute("return document.cookie;");

        public static string GetCookies(this object obj)
        {
            var cookies = Browser.Manage().Cookies.AllCookies;
            if (cookies.Count == 0) return "";
            var ret = "";
            var i = 0;
            foreach (var cookie in cookies)
            {
                ret += $"{cookie.Name}={cookie.Value}{(++i == cookies.Count ? "" : "; ")}";
            }
            return ret;
        }

        public static string GetCookieLang(this string cookie)
        {
            var cookieArray = cookie.Split(';');
            return cookieArray.FirstOrDefault(d => d.Contains(".AspNetCore.Culture="))?.Split('=')[1];
        }

        public static bool IsVisiblbe(this IWebElement element)
        {
            Browser.Assert500();
            return (bool)Browser.Execute("return arguments[0].offsetWidth > 0 && arguments[0].offsetHeight > 0", element);
        }

        public static bool SkipableFail(this Exception e) => e.StackTrace.Contains("elmah");

        public static IWebElement JsExecute(this IWebDriver driver, string script, params object[] args) => (IWebElement)driver.Execute(script, args);

        public static IWebElement JsExecute(this IWebElement container, string script, params object[] args) => Browser.JsExecute(script, container, args);

        public static ReadOnlyCollection<IWebElement> JsExecute2(this IWebDriver driver, string script, params object[] args) => (ReadOnlyCollection<IWebElement>)driver.Execute(script, args);

        public static ReadOnlyCollection<IWebElement> JsExecute2(this IWebElement container, string script, params object[] args) => Browser.JsExecute2(script, container, args);

        public static bool IsReadyStateComplete(this IWebDriver driver)
        => (bool)driver.Execute("return 'undefined' != typeof document && document != null && document.readyState === 'complete'");

        public static bool CheckErrorMessage(this IWebDriver driver)
        {
            var div = driver.GetElementByClassName("validation-summary-errors");
            div?.ScrollIntoView();
            return div != null;
        }

        public static string[] GetValidationErrorStrings(this IWebDriver driver)
        {
            var div = driver.GetElementByClassName("validation-summary-errors");
            var errList = div.GetElementsByTagName("li");
            if (errList == null || errList.Count == 0)
                return null;
            var errMsgs = (from errLi in errList where !string.IsNullOrEmpty(errLi.Text) select errLi.Text).ToList();
            return errMsgs.ToArray();
        }

        public static bool CheckValidation500Error(this IWebDriver driver)
        {
            var div = driver.GetElementByClassName("validation-summary-errors");
            var errList = div?.GetElementsByTagName("li");
            if (errList == null || errList.Count == 0) return false;
            return errList.Any(errElement => errElement != null && errElement.Text == "500 - Internal server error.");
        }

        public static bool CheckErrorRequest(this object _)
        {
            var dialog = Browser.GetElementById("popupDialog");
            var subHeaderText = "";
            try
            {
                subHeaderText = dialog?.GetElementByClassName("subheader")?.Text;
            }
            catch (Exception) { }
            return subHeaderText == "We're sorry, but the server was unable to complete your request";
        }

        private static bool MyAny(this ReadOnlyCollection<object> lines, string[] errMessages)
        {
            if (lines == null) return false;
            foreach (var line in lines)
            {
                foreach (var errMsg in errMessages)
                    if (line.ToString().Equals(errMsg))
                        return true;
            }
            return false;
        }

        public static bool CheckError404(this ReadOnlyCollection<object> err404)
        => err404 != null && err404.MyAny(new[] { "Error 404 occured" });

        public static bool CheckError500(this ReadOnlyCollection<object> err500)
        => err500 != null && err500.MyAny(new[] { "500 - Internal server error." , "Error 500 occured" });
                                                                                                          

        public static bool CheckError401(this ReadOnlyCollection<object> err401)
        => err401 != null && err401.MyAny(new[] { "401 - Unauthorized: Access is denied due to invalid credentials." });

        public static bool CheckErrors(this IWebDriver driver)
        {
            var err500 = driver.GetElementTextsByTagName("h3");
            return CheckError500(err500) || CheckError401(err500);
        }

        public static bool CheckError401(this IWebDriver driver)
        {
            var err401 = driver.GetElementTextsByTagName("h3");
            return CheckError401(err401);
        }

        private static string GetErrorContent(this IWebDriver driver)
        {
            var content = driver.GetElementById("content");
            return content?.GetElementByTagName("p")?.GetAttrib("innerText");
        }

        private static string _title = null;
        private static string Title
        {
            get
            {
                if (string.IsNullOrEmpty(_title))
                    _title = Browser.GetElementById("actiontitle")?.GetAttrib("innerText");
                return _title;
            }
        }

        public static string GetErrorFromTitle(this IWebDriver driver)
        {
            var title = Title;
            if(title != null && title.Contains("Error"))
                return title.Split(' ')[3];
            else
                return "";
        }

        public static bool Assert500(this IWebDriver driver)
        {
            //#controllerName Split('\n')[^1] == Error - Error NNN occured
            //#actiontitle innerText =  Error - Error NNN occured
            //#content contains h3 and p
            var h3S = driver.GetElementTextsByTagName("h3");
            var h2S = driver.GetElementTextsByTagName("h2");
            if (h2S.CheckError500() || h3S.CheckError500() || driver.GetErrorFromTitle().Equals("500"))
            {
                var errorMessage = driver.GetErrorContent();
                Assert.Fail($"Error 500{(!string.IsNullOrEmpty(errorMessage) ? $" with message: {errorMessage}" : "!")}");
            }
            if (h2S.CheckError401() || h3S.CheckError401() || driver.GetErrorFromTitle().Equals("401"))
            {
                var errorMessage = driver.GetErrorContent();
                Assert.Fail($"Error 401{(!string.IsNullOrEmpty(errorMessage) ? $" with message: {errorMessage}" : "!")}");
            }
            if (h2S.CheckError404() || h3S.CheckError404() || driver.GetErrorFromTitle().Equals("404"))
            {
                var errorMessage = driver.GetErrorContent();
                Assert.Fail($"Error 404{(!string.IsNullOrEmpty(errorMessage) ? $" with message: {errorMessage}" : "!")}");
            }
            if (h2S.CheckErrorRequest())
                Assert.Fail("Request error");
            var h4s = driver.GetElementTextsByTagName("h4");
            if (h4s.MyAny(new[] { "Access Denied" }))
                Assert.Fail($"Access Denied: '{driver.GetElementByTagName("h5").GetText()}'");
            if (driver.GetElementByClassName("neterror") != null)
                Assert.Fail("This page isn’t working");
            return false;
        }

        public static IWebElement GetTabElement(this IWebDriver driver, string tabName)
        {
            var select = driver.GetElementsByText(null, GetElementBy.Tagname, "a[data-toggle='tab']", tabName);
            if (select == null) return null;
            foreach (var item in select)
            {
                if (item.GetElementValue().ToLower() == tabName.ToLower())
                    return item;
            }
            return null;
        }

        public static string GoToTab(this IWebDriver driver, string tabName, bool withoutValidation = false)
        {
            var divId = "";
            //Driver.Report.StartStep($"Going on tab '{tabName}'");
            try
            {
                var tab = driver.GetTabElement(tabName);
                if (tab == null)
                    Assert.Fail($"Tab '{tabName}' was not found");
                if (withoutValidation)
                {
                    tab.JsClick();
                    Pause(null, 1000);
                }
                else
                    tab.Click2();
                var alert = driver.GetAlertTitle();
                if (alert != null && alert.Equals("Warning"))
                {
                    driver.GetAlertPopUp().AlertCloseButton().JsClick();
                    if (withoutValidation)
                    {
                        tab.JsClick();
                        Pause(null, 1000);
                    }
                    else
                        tab.Click2();
                }
                divId = tab.GetAttribute("aria-controls");
                BrowserWait.WaitForElementIsVisible(driver.GetElementById(divId));
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
            return divId;
        }

        private static void ScreeningTab(this IWebDriver driver, string tabName)
        {
            driver.GoToTab(tabName, true);
            //Driver.Report.TakeScreenShot(tabName + " tab");
            driver.Pause(1000);
        }

        private static string TabScreeningErrorMessage(this IWebDriver driver, string errMessage)
        {
            if (errMessage.EndsWith("SEPA Info Extender is mandatory"))
            {
                driver.ScreeningTab("SEPA info");
            }
            if (errMessage.EndsWith("DI is mandatory"))
            {
                driver.ScreeningTab("Debts & Investments");
            }
            if (errMessage.EndsWith("Reconciliation is mandatory"))
            {
                driver.ScreeningTab("Reconciliation");
            }
            if (errMessage.EndsWith("BD is mandatory"))
            {
                driver.ScreeningTab("Bank Delegation of Authorities");
            }
            if (errMessage.EndsWith("Cash is mandatory"))
            {
                driver.ScreeningTab("Cash");
            }
            return errMessage;
        }

        private static void AssertValidateErrorMessages(this IWebDriver driver)
        {
            if (driver.CheckErrorMessage())
            {
                var errList = driver.GetValidationErrorStrings();
                if (errList == null || errList.Length == 0)
                    Assert.Fail("Validation error without details");
                var message = "Validation error(s): " + driver.TabScreeningErrorMessage(errList[0]);
                for (var i = 1; i < errList.Length; ++i)
                {
                    message += $", {driver.TabScreeningErrorMessage(errList[i])}";
                }
                Assert.Fail(message);
            }
        }

        private static bool _errorCheck(this IWebDriver driver, bool emptyform)
        {
            var ret = driver.CheckErrorMessage();
            if (!ret) return false;
            if (!emptyform)
                driver.AssertValidateErrorMessages();
            else if (driver.CheckValidation500Error())
                Assert.Fail("Validation error: '500 - Internal server error.'");
            return true;
        }

        public static void WaitForReadyStateByClass(this IWebDriver driver, string classname = "", bool jQ = true)
        {
            driver.WaitForReadyState(jQ: jQ);
            BrowserWait.WaitForElementByClass(classname, null);
        }

        public static void WaitForReadyStateByLink(this IWebDriver driver, string linkText, string tag = "a", bool jQ = true)
        {
            driver.WaitForReadyState(jQ: jQ);
            BrowserWait.WaitForLinkReady(linkText, tag);
        }

        public static void WaitForReadyStateByLink(this IWebDriver driver, string[] linkText, string tag = "a", bool jQ = true)
        {
            driver.WaitForReadyState(jQ: jQ);
            BrowserWait.WaitForLinkReady(linkText, tag);
        }

        public static string GetAttrib(this IWebElement element, string attribute)
        {
            //*DEBUG*/try
            //*DEBUG*/{
            //*DEBUG*/    Driver.Report.StartStep($"Geting '{attribute}' attribute of {(element == null ? "null" :"")} element");
            if (element == null) return "";
            var script = $"return arguments[0].{(attribute != "innerText" ? (attribute != "tag" ? $"getAttribute('{attribute}')" : "tagName") : attribute)}";
            return (string)Browser.Execute(script, element) ?? "";
            //*DEBUG*/}
            //*DEBUG*/finally
            //*DEBUG*/{
            //*DEBUG*/    Driver.Report.FinishStep();
            //*DEBUG*/}
        }

        public static string GetText(this IWebElement element, bool returnNull = false)
        {
            if (element == null) return returnNull ? null : "";
            return (string)Browser.Execute("return arguments[0].innerText;", element) ?? "";
        }

        public static void Wait4ReadyState(this IWebDriver driver) => BrowserWait.Until(d => driver.IsReadyStateComplete());

        public static void WaitForReadyState(this IWebDriver driver, bool emptyform = false, bool jQ = true, bool refreshable = false)
        {
            try
            {
                var err500Counter = 5;
                while (true) // Handle timeout somewhere
                {
                    driver.Wait4ReadyState();

                    if (!(jQ && driver.JQueryReady()))
                    {
                        var h2S = driver.GetElementTextsByTagName("h4");
                        if (refreshable && (h2S.CheckError500() || h2S.CheckError401() || h2S.CheckErrorRequest()))
                        {
                            if (err500Counter == 0)
                            {
                                driver.Assert500();
                            }

                            --err500Counter;
                            driver.Navigate().Refresh();
                            continue;
                        }
                        if (!refreshable && (h2S.CheckError500() || h2S.CheckError401() || h2S.CheckErrorRequest()))
                            driver.Assert500();
                    }

                    if (ConfigSettingsReader.DebugLvl == 1 || ConfigSettingsReader.DebugLvl == 3)
                    {
                        var jsError = driver.GetJavaScriptErrors();
                        if (jsError.Length > 0)
                            Assert.Fail(
                                $"JavaScript error(s):{Environment.NewLine}{jsError.Aggregate("", (s, entry) => s + entry + Environment.NewLine)}");
                    }

                    if (jQ && driver.JQueryReady())
                    {
                        try
                        {
                            driver.Wait4Ajax();
                        }
                        catch (Exception e)
                        {
                            if (e.Message.Contains("Timed out after 60 seconds") || e.Message.Contains("Timed out receiving message"))
                            {
                                var ajaxActivity = (long)driver.Execute("return 'undefined' != typeof jQuery ? jQuery.active : -1");
                                if (ajaxActivity < 1)
                                    throw;
                                Assert.Fail($"Ajax did not complete {ajaxActivity} process{(ajaxActivity > 1 ? "es" : "")} in 60 waiting minutes");
                            }
                            throw;
                        }
                    }
                    driver.Assert500();
                    if (!emptyform)
                    {
                        var alarmMessages = driver.AlertMessagesCheck();
                        if (alarmMessages != null)
                        {
                            var alarmMessage = "Alert is present:";
                            foreach (var alarm in alarmMessages)
                            {
                                alarmMessage += $"\r\n-> {alarm}";
                            }
                            Assert.Fail(alarmMessage);
                        }
                    }

                    if (!driver._errorCheck(emptyform))
                    {
                        if (!emptyform)
                            driver.Assert500();
                    }
                    break;
                }
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                try
                {
                    if (ConfigSettingsReader.DebugLvl == 1 || ConfigSettingsReader.DebugLvl == 3)
                    {
                        var jsError = driver.GetJavaScriptErrors();
                        if (jsError.Length > 0)
                            Assert.Fail(
                                $"JavaScript error(s):{Environment.NewLine}{jsError.Aggregate("", (s, entry) => s + entry + Environment.NewLine)}");
                    }
                }
                catch (Exception)
                {
                    //ignore
                }
                if (!driver.IsReadyStateComplete()) Assert.Fail($"{BrowserName} page did not answer!");
                throw;
            }
        }

        public static void WaitForGridReady(this IWebDriver driver)
        {
            var t = 0;
            while (driver.GetElementById("general-search-dispay-button") == null && t < 10)
            {
                t++;
                driver.Pause();
            }
            if (driver.GetElementById("general-search-dispay-button") == null)
                driver.WaitForReadyState();
            else
                driver.GetElementById("general-search-dispay-button").WaitForElementIsClickable();
        }

        public static void WaitForPortletIsReady(this IWebDriver driver)
        {
            var t = 0;
            while (driver.GetElementByClassName("btn-save-as") == null && t < 10)
            {
                ++t;
                driver.Pause();
            }
            driver.WaitForReadyState();
        }

        public static IWebElement GetAlertPopUp(this IWebDriver driver) => driver.GetElementByQuery(GetElementBy.Tagname, "#popupDialog.fade.in");

        public static IWebElement AlertCloseButton(this IWebElement alert) => alert.GetElementByQuery(GetElementBy.ClassName, "btn.btn-secondary");

        public static string GetAlertTitle(this IWebDriver driver)
        {
            var popupDialog = driver.GetAlertPopUp();
            return popupDialog?.GetAlertTitle();
        }

        public static string GetAlertTitle(this IWebElement container)
        {
            return container.GetElementByClassName("modal-title")?.GetAttrib("innerText");
        }


        public static string[] AlertMessagesCheck(this IWebDriver driver)
        {
            var popupDialog = driver.GetAlertPopUp();
            if (popupDialog != null)
            {
                var h4T = popupDialog.GetAlertTitle();
                if (!string.IsNullOrEmpty(h4T) && h4T.Equals("An error occurred"))
                {
                    return popupDialog.GetElementsByClassName("modal-body")?.Select(alert => alert.GetElementValue()).ToArray();
                }
            }

            var alarmContainer = driver.GetElementByQuery(GetElementBy.Tagname, "div.alert.alert-warning.alert-dismissible");
            var alarms = alarmContainer?
            .JsExecute2(@"
let ret = Array.from(arguments[0].querySelectorAll('h4.alert-heading')).filter(item => getComputedStyle(item).display != 'none');
return ret != null && ret.length > 0 ? ret : null");
            return alarms?.Select(alert => alert.GetElementValue()).ToArray();
        }

        public static void Wait4Ajax(this IWebDriver driver)
            => BrowserWait.Until(d => (bool)driver.Execute("return 'undefined' != typeof jQuery && jQuery.active == 0"));
    
        public static string[] GetJavaScriptErrors(this IWebDriver driver)
        {
            var errLogs =
                driver.Manage().Logs.GetLog(LogType.Browser).Where(m => m.Level == LogLevel.Severe);
            return errLogs.Select(errLog => $"{errLog.Level}: {errLog.Message}").ToArray();
        }

        public static bool ElementAvailable(this IWebDriver driver, IWebElement element)
        {
            if (element == null) return false;
            return (bool)driver.Execute(@"
            return (document != null && document.readyState === 'complete' && arguments[0] != null);", element);
        }

        public static bool ElementByClassIsAvailable(this IWebElement element, string className)
        {
            Browser.WaitForjQueryReady();
            return
                (bool)
                    Browser.Execute(
                        $@"
                if(document == null || document.readyState != 'complete') return false;
                let element = {GetPointNode(element != null)}.getElementsByClassName(arguments[1])[0];
                return !!element;",
                        element, className);
        }

        public static bool ElementVisiblityCheckUsedByDisplay(this IWebElement element, string displayState = "block")
        {
            if (element == null) return displayState == "none";
            return GetVisibilityState(element).Contains(displayState);
        }

        public static string GetVisibilityState(this IWebElement element)
        {
            Browser.WaitForjQueryReady();
            return (string)Browser.Execute(@"return getComputedStyle(arguments[0]).display", element); 
        }

        private static string GetElementsScriptByQuery(this string prefix, GetElementBy getTableBy, string value)
        {
            var script = prefix;
            switch (getTableBy)
            {
                case GetElementBy.ClassName:
                    script += "\"." + value + "\"";
                    break;
                case GetElementBy.AttributeWithValue:
                    script += "\"[" + value + "]\"";
                    break;
                case GetElementBy.Tagname:
                    script += "\"" + value + "\"";
                    break;
                case GetElementBy.Id:
                    script += "\"#" + value + "\"";
                    break;
            }
            script += ")";
            return script;
        }

        public static string QuerySelector(this string starter, GetElementBy getTableBy, string value, bool single = true)
        => single ? starter + ".querySelector(".GetElementsScriptByQuery(getTableBy, value)
              : $"Array.from({starter}.querySelectorAll(".GetElementsScriptByQuery(getTableBy, value) + ")";

        public static string GetPointNode(bool isContainer = false, int index = 0)
        => (isContainer ? $"arguments[{index}]" : "document");

        public static string GetQuerySript(this string starter, GetElementBy getTableBy, string value, bool isContainer = false, bool single = true)
        => starter + GetPointNode(isContainer).QuerySelector(getTableBy, value, single);

        public static IWebElement GetElementByQuery(this IWebDriver driver, GetElementBy getTableBy, string value, IWebElement container = null)
        {
            var script = "return ".GetQuerySript(getTableBy, value, container != null, true);
            return (IWebElement)driver.Execute(script, container);
        }

        public static IWebElement GetElementByQuery(this IWebElement container, GetElementBy getTableBy, string value)
        => Browser.GetElementByQuery(getTableBy, value, container);

        public static ReadOnlyCollection<IWebElement> GetElementsByQuery(this IWebDriver driver, GetElementBy getTableBy, string value, IWebElement container = null)
        {
            var script = "let items = ".GetQuerySript(getTableBy, value, container != null, false);
            return (ReadOnlyCollection<IWebElement>)driver.Execute(script + $@"
            return (items.length == 0) ? null : items;", container);
        }

        public static ReadOnlyCollection<IWebElement> GetElementsByQuery(this IWebElement container, GetElementBy getTableBy, string value)
        => Browser.GetElementsByQuery(getTableBy, value, container);

        public static IWebElement GetRowByCell(this IWebElement container)
        {
            var script = "return arguments[0].closest('tr');";
            return (IWebElement)Browser.Execute(script, container);
        }

        public static IWebElement GetRowChkBxByCell(this IWebElement container)
        {
            var script = "return arguments[0].closest('tr').querySelector('input.batchCheckbox');";
            return (IWebElement)Browser.Execute(script, container);
        }

        public static IWebElement WaitForElementIsReady(this IWebDriver driver, GetElementBy getTableBy, string value, IWebElement clickable = null, IWebElement container = null)
        {
            var script = "return ".GetQuerySript(getTableBy, value, container != null, true);
            var ret = BrowserWait.Until(e =>
            {
                if (clickable != null)
                    clickable.ClickUnderRedis();
                else
                    driver.WaitForReadyState();
                return (IWebElement)driver.Execute(script, container);
            });
            return ret;
        }

        public static IWebElement WaitForElementIsReady(this IWebElement container, GetElementBy getTableBy, string value, IWebElement clickable = null)
        => Browser.WaitForElementIsReady(getTableBy, value, clickable, container);

        public static IWebElement WaitUntilElementByClassIsAvailable(this IWebDriver driver, string className,
            IWebElement container = null, int secondsTimeOut = 20)
        => (IWebElement)driver.Execute($@"
            let doc = {GetPointNode(container != null)};
            for(let i = {secondsTimeOut * 5}; i > 0; i--)
            {{
              let b = false;
              setTimeout(function(){{
                b = true;
              }}, 200);
              let element = doc.querySelector('.{className}');
              if(element != null) return element;
            }}
            return null;", container);

        public static bool WaitForElementIsAvailable(this WebDriverWait driverWait, IWebElement element)
        => driverWait.Until(d => Browser.ElementAvailable(element));

        public static IWebElement WaitForElementByClass(this WebDriverWait driverWait, string className, IWebElement container, bool isAvailable = true)
        {
            if (isAvailable)
                driverWait.Until(d => container.ElementByClassIsAvailable(className));
            else
                driverWait.Until(d => !container.ElementByClassIsAvailable(className));
            var ret = container.GetElementByClassName(className);//for debug
            return ret;
        }

        public static bool WaitForElementIsNotAvailable(this WebDriverWait driverWait, IWebElement element)
        => driverWait.Until(d => !Browser.ElementAvailable(element));

        public static bool WaitForVisible(this WebDriverWait driverWait, IWebElement element, string displayState = "block")
        => (element != null) && driverWait.Until(d => element.ElementVisiblityCheckUsedByDisplay(displayState));

        public static bool WaitForInVisible(this WebDriverWait driverWait, IWebElement element)
        => (element == null) || driverWait.Until(d => element.ElementVisiblityCheckUsedByDisplay("none"));

        public static bool WaitForElementIsVisible(this WebDriverWait driverWait, IWebElement element)
        => (element != null) && driverWait.Until(d => element.IsVisiblbe());

        public static IWebElement WaitForElementIsClickable(this WebDriverWait driverWait, IWebElement element)
        => driverWait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(element));

        public static IWebElement WaitForElementIsClickable(this IWebElement element)
        => BrowserWait.WaitForElementIsClickable(element);


        public static bool WaitForElementIsNotVisible(this WebDriverWait driverWait, IWebElement element)
        => driverWait.Until(d => (element == null) || !element.IsVisiblbe());

        public static IWebElement GetElementByOneAttribute(this IWebDriver driver, string attrValue, string tag = "a", string attr = "title", IWebElement element = null)
        => driver.GetElementByQuery(GetElementBy.Tagname, $"{tag}[{attr}='{attrValue}']", element);

        public static IWebElement GetElementByOneAttribute(this IWebElement element, string attrValue, string tag = "a", string attr = "title")
        => Browser.GetElementByOneAttribute(attrValue, tag, attr, element);

        public static ReadOnlyCollection<IWebElement> GetElementsByOneAttribute(this IWebDriver driver, string attrValue, string tag = "a", string attr = "title", IWebElement element = null)
        => driver.GetElementsByQuery(GetElementBy.Tagname, $"{tag}[{attr}='{attrValue}']", element);

        public static ReadOnlyCollection<IWebElement> GetElementsByOneAttribute(this IWebElement element, string attrValue, string tag = "a", string attr = "title")
        => Browser.GetElementsByOneAttribute(attrValue, tag, attr, element);

        //private static ReadOnlyCollection<IWebElement>FindElementsByXpathForIe(this IWebDriver driver, string xPath, IWebElement section = null)
        //{
        //    var ret = new List<IWebElement>();
        //    if (xPath == "..")
        //    { 
        //        if(section == null)
        //            Assert.Fail("Section was not defined");
        //        var el = (IWebElement)driver.Execute("return arguments[0].parentElement;", section);
        //        ret.Add(el);
        //        return ret.AsReadOnly();
        //    }
        //    var root = xPath.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
        //    if (root.Length > 1 && section == null)
        //        Assert.Fail("Section was not defined");

        //    if (root.Length <= 1) section = null;

        //    var jscrpt = @"
        //    var root = arguments[0].split('//');
        //    var queueList = root[root.length - 1].split(/[\[\]]+/);
        //    var ret = [];
        //    var tag = queueList[0];
        //    var elements = ";
        //    jscrpt += section == null ? "document" : "arguments[1]";
        //    jscrpt += @".getElementsByTagName(tag);
        //    var vi = 0;
        //    var el = elements.item(vi);
        //    while (el)
        //    {
        //        var approve = false;
        //        for (var j = 1; j < queueList.length; ++j)
        //        {
        //            var item = el;
        //            var param = queueList[j];
        //            if (param.indexOf('contains') == 0)
        //            {
        //                var newparam = param.split(/[,()]+/);
        //                var field = (newparam[1] == '.')
        //                    ? item.innerText
        //                    : item.getAttribute(newparam[1].replace('@', '').trim());
        //				var sp = newparam[2].split(/[']+/);
        //        		if (field==null || field.indexOf(sp[1])<0) { approve = false; break; }
        //            }else
        //            if (param.indexOf('@') == 0)
        //            {
        //                var pare = param.replace('@', '').split(/[=']+/);

        //                var att = item.getAttribute(pare[0]);
        //                if (att == null || att != pare[1]) { approve = false; break; } 
        //            }
        //            approve = true;
        //        }
        //        if (approve) ret.push(el);
        //        vi++;
        //        el = elements.item(vi);
        //    }
        //    return (ret.length==0)?null:ret; ";
        //    var arrret = (ReadOnlyCollection<IWebElement>) driver.Execute(jscrpt, xPath, section);
        //    return arrret;
        //}

        public static IWebElement GetElementByXpath(this IWebDriver driver, string xPath, IWebElement section = null)
        {
            var ret = section != null
                ? section.FindElements(By.XPath(xPath))
                : driver.FindElements(By.XPath(xPath));
            return ret.Count > 0 ? ret[0] : null;
        }

        public static ReadOnlyCollection<object> GetElementTextsByTagName(this IWebDriver driver, string tagName,
            IWebElement section = null)
        {
            return (ReadOnlyCollection<object>) driver.Execute($@"
                let arrayResult = {GetPointNode(section != null)}.getElementsByTagName('{tagName}');"+ @"
                if(!arrayResult) return null;
                let array = Array.from(arrayResult).filter(value => value != null).map(value => value.innerText);
                return (array.length == 0) ? null : array;", section);
        }

        public static IWebElement GetNextSibling(this IWebElement elem, string tag = "" )
        {
            return string.IsNullOrEmpty(tag)
                ? (IWebElement)Browser.Execute("return arguments[0].nextElementSibling;", elem)
                : (IWebElement)Browser.Execute($@"
            let el = arguments[0].nextElementSibling;
            while(el && el.tagName != '{tag.ToUpper()}'){{
                el = el.nextElementSibling;
            }}
            return el;", elem);
        }

        public static IWebElement GetParent(this IWebElement node)
        {
            if (node == null)
                Assert.Fail("Can not get parrent for null item");
            return Browser.JsExecute("return arguments[0].parentElement", node);
        }

        public static bool JQueryReady(this IWebDriver driver) => (bool)driver.Execute("return 'undefined' != typeof window.jQuery");

        public static void WaitForjQueryReady(this IWebDriver driver)
        {
            try
            {
                BrowserWait.Until(d => driver.JQueryReady());
            }
            catch (Exception)
            {
                driver.Assert500();
                Assert.Fail("jQuery is not ready on the page");
            }
        }

        public static bool IsChecked(this IWebElement element)
        {
            if(element==null) Assert.Fail("Element should not be null!");
            return (bool) Browser.Execute("return arguments[0].checked;", element);
        }

        /// <summary>
        /// Gets element by attribute with it's value
        /// </summary>
        /// <param name="node">Section in which to search</param>
        /// <param name="attributeWithValue">String with attribute and value attribute=value, e.g. role=dialog or data-valmsg-for='Entity.Amount'</param>
        /// <returns></returns>
        public static IWebElement GetElementByAttributeWithValue(this IWebElement node, string attributeWithValue)
        => (IWebElement)Browser.Execute("return ".GetQuerySript(GetElementBy.AttributeWithValue, attributeWithValue, node != null, true), node);

        public static IWebElement GetElementByAttributeValueAndTag(this IWebDriver driver, string attributeWithValue, string tag = "a")
        {
            Browser.WaitForjQueryReady();
            var par = attributeWithValue.Split('=');
            return (IWebElement)driver.Execute($@"
let el = document.getElementsByTagName('{tag}');
for(let e of el)
{{
    if(e.getAttribute('{par[0]}')==='{par[1]}')
    return e;
}}");
        }

        public static ReadOnlyCollection<IWebElement> GetElementsByText(this IWebDriver driver, IWebElement node, GetElementBy getTableBy, string sBy, string sValue, bool strong = true)
        {
            var script = "let ret = ".GetQuerySript(getTableBy, sBy, node != null, false);
            var ret = (ReadOnlyCollection<IWebElement>)driver.Execute(script +
            $@".filter(item => item.innerText{(strong ? $" === '{sValue}'" : $".includes('{sValue}')")} 
|| (typeof(item.value) == typeof("""") && item.value{(strong ? $" === '{sValue}'" : $".includes('{sValue}')")})
|| item.innerText{(strong ? $" === '\"{sValue}\"'" : $".includes('\"{sValue}\"')")}
|| (typeof(item.value) == typeof("""") && item.value{(strong ? $" === '\"{sValue}\"'" : $".includes('\"{sValue}\"')")}))
return ret.length > 0 ? ret : null", node);
            return ret != null && ret.Count > 0 ? ret : null;
        }

        public static IWebElement GetElementByText(this IWebDriver driver, IWebElement node, GetElementBy getTableBy, string sBy, string sValue, bool strong = true)
        {
            return (IWebElement)driver.Execute("return ".GetQuerySript(getTableBy, sBy, node != null, false) +
                $".find(item => item.innerText{(strong ? $" === '{sValue}'" : $".includes('{sValue}')")} || (item.value != null && item.value{(strong ? $" === '{sValue}'" : $".includes('{sValue}')")}))", node);
        }

        public static ReadOnlyCollection<IWebElement> GetElementsByText(this IWebElement node, GetElementBy getTableBy, string sBy, string sValue, bool strong = true)
        => Browser.GetElementsByText(node, getTableBy, sBy, sValue, strong);

        public static ReadOnlyCollection<IWebElement> GetElementsByTextWithTag(this IWebDriver driver, string sValue, string sTag = "td", bool strong = true)
        => driver.GetElementsByText(null, GetElementBy.Tagname, sTag, sValue, strong);

        public static IWebElement GetElementByTextWithTag(this IWebDriver driver, string sValue, string sTag = "td", bool strong = true, IWebElement node = null)
        => driver.GetElementByText(node, GetElementBy.Tagname, sTag, sValue, strong);

        public static IWebElement GetElementByTextWithTag(this IWebElement node, string sValue, string sTag = "td", bool strong = true)
        => Browser.GetElementByTextWithTag(sValue, sTag, strong, node);

        public static IWebElement SetFocus(this IWebElement element)
        {
            if (element != null)
                Browser.Execute("arguments[0].focus();", element);
            return element;
        }

        public static void JsClick(this IWebElement element) => Browser.Execute("arguments[0].click();", element);

        public static void Click2(this IWebElement element, bool allign = false, bool emptyform = false, bool focusElement = true, bool jQ = true)
        {
            if (focusElement)
                element.SetFocus();
            element.JsClick();
            element.Pause();
            Browser.WaitForReadyState(emptyform, jQ);
            if (ConfigSettingsReader.DebugLvl == 1)
                try
                {
                    //Driver.Report.StartStep($"After click2 cookie: '{Browser.GetCookies()}'");
                    //AllureNextReport.FinishStep();
                }
                catch { }
        }

        public static Actions MoveToElement(this IWebElement element)
        {
            try
            {
                //Driver.Report.StartStep("Moving screen to the element");
                Assert.IsNotNull(element, "Web element for movings to was not found.");
                var actions = new Actions(Browser);
                var result = actions.MoveToElement(element);
                //AllureNextReport.FinishStep();
                return result;
            }
            catch (Exception e)
            {
                //Driver.Report.LogFailedStepWithFailedTestCase(e);
                throw;
            }
        }

        public static void MoveToElementAndClick(this IWebElement element)
        {
            if (Driver.Instance.BrowserName != "Firefox")
            {
                element.MoveToElement().Click().Perform();
                Browser.WaitForReadyState();
            }
            else
                element.Click2();
        }

        public static int ClickIt(this IWebElement element)
        {
            try
            {
                element.MoveToElement().Click().Perform();
                return 0;
            }
            catch (Exception)
            {
                // ignored
            }
            try
            {
                element.Click();
                return 1;
            }
            catch (Exception)
            {
                // ignored
            }
            try
            {
                element.Click2();
                return 2;
            }
            catch (Exception)
            {
                // ignored
            }
            try
            {
                element.JsClick();
                Browser.WaitForReadyState();
                return 3;
            }
            catch (Exception)
            {
                // ignored
            }
            return -1;
        }

        public static void CloseRedis(this IWebDriver driver, bool emptyform = false, bool jQ = true)
        {
            driver.Execute(@"
function isVisible(item){
    return item != null && item.offsetWidth > 0 && item.offsetHeight > 0
}
let header = document.querySelector('.notification-container-header.opened');
if(header != null){
    document.querySelector('.notification-mark-as-read-all').click();
    document.querySelector('.notification-close-all').click();
}
let bb = document.querySelectorAll('[data-dismiss=toastr]');
if(bb.length>0){
    for(let b of bb){
        if(isVisible(b)) b.click();
    }
}");
            driver.WaitForReadyState(emptyform, jQ: jQ);
        }

        public static void ClickUnderRedis(this IWebElement element, bool emptyformbefore = false, bool emptyformafter = false, bool focused = false, bool jQ = true)
        {
            if (element == null) throw new NullReferenceException();
            var textDescribtion = element.GetElementValue(true);
            //Driver.Report.StartStep($"Close all Redis notification and click on '{textDescribtion}' element");
            Browser.CloseRedis(emptyformbefore, jQ: jQ);
            element.Click2(emptyform: emptyformafter, focusElement: focused, jQ: jQ);
            //AllureNextReport.FinishStep();
        }

        #region Single Get methods
        public static IWebElement GetElementById(this IWebDriver driver, string elementId)
        {
            var script = $"return document.getElementById('{elementId}');";
            return (IWebElement)driver.Execute(script);
        }

        public static IWebElement GetElementById(this IWebElement container, string elementId)
        {
            var script = $"return ".GetQuerySript(GetElementBy.Id, elementId, container != null, true);
            return (IWebElement)Browser.Execute(script, container);
        }

        public static IWebElement GetElementByName(this IWebDriver driver, string elementName)
        {
            var script = $"return document.getElementsByName('{elementName}')[0];";
            return (IWebElement)driver.Execute(script);
        }

        public static IWebElement GetElementByClassName(this IWebDriver driver, string elementClassName,
            IWebElement container = null)
        {
            var script = "return ".GetQuerySript(GetElementBy.ClassName, elementClassName, container != null);
            return (IWebElement) driver.Execute(script, container);
        }

        public static IWebElement GetElementByClassName(this IWebElement container, string elementClassName)
        => Browser.GetElementByClassName(elementClassName, container);

        public static IWebElement GetElementByTagName(this IWebDriver driver, string elementTagName,
            IWebElement container = null, bool containerMandatory = false)
        {
            if (container == null && containerMandatory)
                return null;
            var script = "return ".GetQuerySript(GetElementBy.Tagname, elementTagName, container != null, true);
            return container == null ? (IWebElement)driver.Execute(script) : (IWebElement)driver.Execute(script, container);
        }

        public static IWebElement GetElementByTagName(this IWebElement container, string elementTagName, bool containerMandatory = false)
        => Browser.GetElementByTagName(elementTagName, container, containerMandatory);
        #endregion

        #region Mutly returned Get methods
        public static ReadOnlyCollection<IWebElement> GetElementsByName(this IWebDriver driver, string elementName)
        {
            var script = $"return document.getElementsByName({elementName})";
            return (ReadOnlyCollection<IWebElement>)driver.Execute(script);
        }

        public static ReadOnlyCollection<IWebElement> GetElementsByClassName(this IWebDriver driver, string elementClassName,
            IWebElement container = null)
        {
            var script = $"let ret = ".GetQuerySript(GetElementBy.ClassName, elementClassName, container != null, false) + @"
return ret.length > 0 ? ret : null";
            return driver.Execute(script, container) as ReadOnlyCollection<IWebElement>;
        }

        public static ReadOnlyCollection<IWebElement> GetElementsByClassName(this IWebElement container, string elementClassName)
        => Browser.GetElementsByClassName(elementClassName, container);

        public static ReadOnlyCollection<IWebElement>  GetElementsByTagName(this IWebDriver driver, string elementTagName,
            IWebElement container = null, bool containerMandatory = false)
        {
            if (container == null && containerMandatory)
                return null;
            var script = $"let ret = ".GetQuerySript(GetElementBy.Tagname, elementTagName, container != null, false) + @"
return ret.length > 0 ? ret : null";
            return driver.Execute(script, container) as ReadOnlyCollection<IWebElement>;
        }

        public static ReadOnlyCollection<IWebElement> GetElementsByTagName(this IWebElement container, string elementTagName, bool containerMandatory = false)
        => Browser.GetElementsByTagName(elementTagName, container, containerMandatory);

        #endregion


        /// <summary>
        /// Extention method to get a table (grid) by specified parameters 
        /// </summary>
        /// <param name="driver">WebDriver instance</param>
        /// <param name="getTableBy">GetTableBy enum value to search by</param>
        /// <param name="getTableByValue">Value by which to search, default "null" value is applicable for searching by CSS Class
        /// and Tagname - default values are "k-selectable" and "table" respectively</param>
        /// <param name="container">Container IWebElement element in which to search for table, by default is null </param>
        /// <returns></returns>
        public static IWebElement GetTable(this IWebDriver driver, GetElementBy getTableBy = GetElementBy.ClassName, string getTableByValue = null, IWebElement container = null)
        {
            IWebElement table;

            switch (getTableBy)
            {
                case GetElementBy.ClassName:
                    getTableByValue ??= "k-selectable";    // default value if argument is null
                    //Driver.Report.StartStep(
                    //    $"Looking for table by class name '{getTableByValue}'{(container == null ? "" : " in container")}");
                    table = container == null ?
                        driver.GetElementByClassName(getTableByValue) :
                        container.GetElementByClassName(getTableByValue);
                    //AllureNextReport.FinishStep();
                    break;
                
                case GetElementBy.Tagname:
                    getTableByValue ??= "table";       // default value if argument is null
                    //Driver.Report.StartStep(
                    //    $"Looking for table by tag name '{getTableByValue}'{(container == null ? "" : " in container")}");
                    table = container == null ?
                        driver.GetElementByTagName(getTableByValue) :
                        container.GetElementByTagName(getTableByValue);
                    //AllureNextReport.FinishStep();
                    break;

                case GetElementBy.AttributeWithValue:
                    if (getTableByValue == null || container == null)
                    {
                        Assert.Fail(
                            $"Get table by '{getTableBy}' failed: value '{getTableByValue}' and container should not be null.");
                    }
                    //Driver.Report.StartStep($"Looking for table by attribute '{getTableByValue}' in container");
                    table = GetElementByAttributeWithValue(container, getTableByValue);
                    //AllureNextReport.FinishStep();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(getTableBy), getTableBy, null);
            }

            Assert.IsNotNull(table, $"Table was not found by '{getTableBy}' and its value '{getTableByValue}'{(container == null ? "" : " in container")}.");

            return table;
        }

        public static IWebElement GetTable(this IWebElement container, GetElementBy getTableBy = GetElementBy.ClassName, string getTableByValue = null)
        => Browser.GetTable(getTableBy, getTableByValue, container);

        public static string RemoveDoubleSpacing(this string value)
        {
            if (!value.Contains("  ")) return value;
            var arrValue = value.Split(' ').Where(v => !string.IsNullOrEmpty(v));
            var enumerable = arrValue as string[] ?? arrValue.ToArray();
            if (enumerable.Length == 0) return "";
            return enumerable.Length > 1 ? string.Join(" ", enumerable) : enumerable[0];
        }

        public static void ScrollIntoView(this IWebElement element)
        {
            Browser.Execute("arguments[0].scrollIntoView({block: \"center\", inline: \"nearest\"});", element);
            element.SetFocus();
        }

        public static bool AllTrue(this bool[] arrayBools)
        => arrayBools.Length != 0 && arrayBools.Aggregate(true, (current, boolItem) => current & boolItem);

        public static void SendTextBoxValue(this IWebElement element, string value)
        {
            var id = element.GetAttribute("title");
            if (string.IsNullOrEmpty(id))
                id = element.GetAttribute("id");
            if (string.IsNullOrEmpty(id))
                id = element.TagName;
            //Driver.Report.StartStep($"Set '{value}' value to {(string.IsNullOrEmpty(id) ? $"'{id}'" : "textbox")} element");
            element.SetFocus();
            element.Clear();
            element.SendKeys(value);
            //AllureNextReport.FinishStep();
        }

        public static void SendTextBoxValue(this IWebDriver driver, string elementId, string value)
        {
            var inputName = driver.GetElementById(elementId);
            inputName.SendTextBoxValue(value);
        }

        public static void SetTextToSlidingDateInput(this IWebDriver driver, IWebElement element, string value)
        {
           // Driver.Report.StartStep($"Set '{value}' value to element");
            var ctrlEdit = (IWebElement)driver.Execute(@"
                let sdCtrl = Array.from(arguments[0].parentElement.querySelectorAll('.k-input-inner')).filter(item => item.offsetWidth > 0 && item.offsetHeight > 0)[0];
                sdCtrl.click();
                return sdCtrl;", element);
            ctrlEdit.Clear();
            ctrlEdit.SendKeys(value);
            ctrlEdit.SetTrigger("change");
            ctrlEdit.SetTrigger("focusout");
            Pause(null,550);
            //AllureNextReport.FinishStep();
        }


        public static void SetTextBoxValue(this IWebDriver driver, IWebElement element, string value)
        {
            if (value == null) return;
            //Driver.Report.StartStep($"Set '{value}' value to element");
            try
            {
                driver.Execute("arguments[0].value = arguments[1];", element, value);
            }
            catch
            {
                element.ScrollIntoView();
                throw;
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        public static void SetTextBoxValue(this IWebDriver driver, string elementId, string value, string trigger = "")
        {
            //Driver.Report.StartStep($"Set '{value}' value to '{elementId}' element"); 
            try
            {
                var element = driver.GetElementById(elementId);
                driver.SetTextBoxValue(element, value);
                if (trigger != "")
                    element.SetTrigger(trigger);
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        public static void SetTrigger(this IWebElement element, string trigger = "change")
        {
            //Driver.Report.StartStep($"Set element with trigger '{trigger}'");
            Browser.Execute($@"$(arguments[0]).trigger('{trigger}');", element);
            //AllureNextReport.FinishStep();
        }

        public static void SetTextBoxValueWithInputTrigger(this IWebDriver driver, string elementId, string value)
        {
            //Driver.Report.StartStep($"Set '{value}' value to '{elementId}' element with trigger 'input'");
            driver.Execute($"$('#{elementId}').val('{value}').trigger('input');");
            //AllureNextReport.FinishStep();
        }

        /// <summary>
        /// Drag source and drop it on target 
        /// </summary>
        /// <param name="driver">Current driver</param>
        /// <param name="source">Source element</param>
        /// <param name="target">Target element</param>
        public static void DragAndDrop(this IWebDriver driver, IWebElement source, IWebElement target)
        {
            if (source == null) throw new NoSuchElementException("Drag&Drop issue: Source item is null!");
            if (target == null) throw new NoSuchElementException("Drag&Drop issue: Target item is null!");
            source.SetTrigger("mousedown");
            new Actions(driver).DragAndDrop(source, target).Perform();
        }

        public static void DragAndDrop1(this IWebDriver driver, IWebElement source, IWebElement target)
        {
            if (source == null) throw new NoSuchElementException("Drag&Drop issue: Source item is null!");
            if (target == null) throw new NoSuchElementException("Drag&Drop issue: Target item is null!");
            source.SetTrigger("mousedown");
            var builder = new Actions(driver);
            var dragAndDrop = builder.ClickAndHold(source)./*MoveToElement(target).*/Release(target).Build();
            dragAndDrop.Perform();
        }

        public static void DragAndDrop2(this IWebDriver driver, IWebElement source, IWebElement target)
        {
            if (source == null) throw new NoSuchElementException("Drag&Drop issue: Source item is null!");
            if (target == null) throw new NoSuchElementException("Drag&Drop issue: Target item is null!");
            const string script = @"
    function createEvent(typeOfEvent) {
        let event = document.createEvent(""CustomEvent"");
        event.initCustomEvent(typeOfEvent, true, true, null);
        event.dataTransfer = {
            data: { },
        setData: function(key, value) {
                this.data[key] = value;
            },
        getData: function(key) {
                return this.data[key];
            }
        };
        return event;
    }
    function dispatchEvent(element, event, transferData) {
        if (transferData !== undefined)
        {
            event.dataTransfer = transferData;
        }
        if (element.dispatchEvent) {
            element.dispatchEvent(event);
        } else if (element.fireEvent) {
            element.fireEvent(""on"" + event.type, event);
        }
    }
    function simulateHTML5DragAndDrop(element, target)
    {
        let dragStartEvent = createEvent('dragstart');
        dispatchEvent(element, dragStartEvent);
        let dropEvent = createEvent('drop');
        dispatchEvent(target, dropEvent, dragStartEvent.dataTransfer);
        let dragEndEvent = createEvent('dragend');
        dispatchEvent(element, dragEndEvent, dropEvent.dataTransfer);
    }
    simulateHTML5DragAndDrop(arguments[0], arguments[1]);
    ";
            driver.Execute(script, source, target);
        }


        public static string GetDownloadPath(this IWebDriver driver)
        {
            var browserName = ConfigSettingsReader.BrowserName;
            var caps = Driver.Instance.RemoteDriver.Capabilities;
            var downloadPath = "";
            switch (browserName)
            {
                case "Firefox":
                    downloadPath = caps.GetCapability("browser.download.dir").ToString();
                    break;
                case "InternetExplorer":
                    break;
                case "Chrome":
                    downloadPath = (caps.GetCapability("download.default_directory") != null) ? caps.GetCapability("download.default_directory").ToString() : Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Downloads");
                    break;
                default:
                    Assert.Fail("Unknown browser setting: " + browserName);

                    break;
            }
            if (!downloadPath.EndsWith(@"\")) downloadPath += @"\";
            return downloadPath;
        }

        public static void WaitForLinkReady(this WebDriverWait driverWait, string linkString, string tag = "a")
        {
            var errMessage = "";
            var error = false;
            try
            {
                error = !driverWait.Until(d => Browser.GetElementsByTextWithTag(linkString, tag).Count > 0);
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                error = true;
            }
            finally
            {
                if (error)
                {
                    Browser.AssertValidateErrorMessages();
                    if (!Browser.Assert500())
                        Assert.Fail($"Link with text '{linkString}' not found!{(string.IsNullOrEmpty(errMessage) ? "" : " Javascript returns message: " + errMessage)}");
                }
            }
        }

        public static void WaitForLinkReady(this WebDriverWait driverWait, string[] linkStrings, string tag = "a")
        {
            var errMessage = "";
            var error = false;
            try
            {
                error = !driverWait.Until(d => linkStrings.Any(ls =>
                {
                    var elements = Browser.GetElementsByTextWithTag(ls, tag);
                    return elements != null && elements.Count > 0;
                }));
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                error = true;
            }
            finally
            {
                if (error)
                {
                    Browser.AssertValidateErrorMessages();
                    if (!Browser.Assert500())
                        Assert.Fail($"Link with text(s) '{string.Join("', '", linkStrings)}' not found!{(string.IsNullOrEmpty(errMessage) ? "" : " Javascript returns message: " + errMessage)}");
                }
            }
        }

        public static IWebElement AssertElementByIdAvailability(this IWebDriver driver, string stringId)
        {
            var element = driver.GetElementById(stringId);
            if (element == null) Assert.Fail($"Element with Id='{stringId}' not found!");
            return element;
        }

        public static bool IsAvailableElementByjQ(this object container, string linkString)
        {
            if(container is IWebDriver driver)
                return Browser.ElementAvailable(driver.GetElementByQuery(GetElementBy.Tagname, linkString));
            return Browser.ElementAvailable(((IWebElement)container).GetElementByQuery(GetElementBy.Tagname, linkString));
        }

        //public static bool IsAvailableElementByXpath(this IWebDriver driver, string linkString, bool jQ = true)
        //{
        //    var link = driver.GetElementByXpath(linkString, jQ: jQ);
        //    return driver.ElementAvailable(link);
        //}

        public static string GetElementTitle(this IWebElement element)
        {
            var script = @"
            let returnValue = '';
            let textContainer = arguments[0];
            returnValue = textContainer.getAttribute('title');
            if(returnValue == null || returnValue == '')
                returnValue = textContainer.innerText;
            if(returnValue == null || returnValue == '')
                returnValue = textContainer.textContent;
            if(returnValue == null || returnValue == '')
                returnValue = textContainer.value;
            return returnValue;";
            var value = (string)Browser.Execute(script, element);
            //Driver.Report.StartStep($"DEBUG_INFO: Element description is '{value}'");
            //AllureNextReport.FinishStep();
            return value.Trim();
        }

        public static string GetElementValue(this IWebElement element, bool title = false)
        {
            var script = @"
let returnValue = '';
let textContainer = arguments[0].getElementsByTagName('textarea')[0];
if(textContainer == null) textContainer = arguments[0].getElementsByClassName('modal-body')[0];
if(textContainer == null) textContainer = arguments[0];
";
            if (title)
                script += "returnValue = textContainer.title;";
            else
                script += "returnValue = textContainer.value;";
            script += @"
if (returnValue == null || returnValue == '')
    returnValue = textContainer.textContent;
if(returnValue == null || returnValue == '')
    returnValue = textContainer.innerText;
return returnValue;";
            var value = (string)Browser.Execute(script, element);
            //Driver.Report.StartStep($"DEBUG_INFO: Check with '{value}'");
            //Driver.Report.FinishStep();
            return value.Trim();
        }

        public static string GetElementValue(this object element, bool title = false)
        {
            return ((IWebElement)element).GetElementValue(title);
        }

        #region Security Policy methods

        public static IWebElement GetItemFromTreeBranch(this IWebElement treeBranch, string itemName)
        {
            var ret = treeBranch.GetElementByTextWithTag(itemName, "span.k-in", false);
            //(IWebElement)Browser.Execute($"return $($(arguments[0]).find(\"div.k-top:contains('{itemName}')\")[0]).parent()[0];", treeBranch);
            return ret.GetParent().GetParent();
        }

        public static IWebElement GetSecurityItemByPath(this IWebElement currentItem, string path)
        {
            var previousItem = currentItem;
            if (string.IsNullOrEmpty(path) || path.ToLower() == "root") return previousItem;
            var arrPath = path.Split('|');
            var newItem = previousItem.GetItemFromTreeBranch(arrPath[0]);
            var newPath = "";
            for (var i = 1; i < arrPath.Length; ++i)
            {
                newPath += arrPath[i] + "|";
            }
            return GetSecurityItemByPath(newItem, newPath.Trim('|'));
        }

        public static string GetStateString(this string value)
        {
            string stVal = value.ToLower() switch
            {
                "ignore" => "unknown",
                "allow" => "checked",
                "deny" => "unchecked",
                _ => value.ToLower(),
            };
            return stVal;
        }

        public static bool SetSecurityToItem(this IWebElement item, string stateValue)
        {
            var multibox = item.GetElementByClassName("k-multibox");
            var state = multibox.GetAttribute("class").Split(' ').FirstOrDefault(c=>c.Contains("k-state-"))?.Split('-')[2];
            if (state == stateValue) return true;
            var tim = 60;
            while (state != stateValue)
            {
                if (state == null)
                    Assert.Fail("Wrong multibox control has found");
                if (tim-- == 0) break;
                multibox.Click2();
                state = multibox.GetAttribute("class").Split(' ').FirstOrDefault(c => c.Contains("k-state-"))?.Split('-')[2];
            }
            return (state == stateValue);
        }

        public static string GetItemSecurity(this IWebElement item)
        {
            var multibox = item.GetElementByClassName("k-multibox");
            var state = multibox.GetAttribute("class").Split(' ').FirstOrDefault(c => c.Contains("k-state-"))?.Split('-')[2];
            return state;
        }

        #endregion

        #region Layout methods

        public static void ClearSelectedItems(this IWebElement areaControl, string tab)
        {
            var selectedItemList = areaControl.GetElementByClassName($"layout-{tab}-grid") ?? areaControl.GetElementByClassName("selectedColumns");
            if (selectedItemList == null)
                Assert.Fail($"Area with Items to clear not found on {tab} tab");
            var removeButtonClass = "column-remove-btn";
            var deleteButton = selectedItemList.GetElementByClassName(removeButtonClass);
            if(deleteButton==null)
                removeButtonClass = "layout-settings-remove-btn";
            deleteButton = selectedItemList.GetElementByClassName(removeButtonClass);
            while (deleteButton != null)
            {
                deleteButton.Click2();
                deleteButton = selectedItemList.GetElementByClassName(removeButtonClass);
            }
        }

        public static void CheckAgregateBoxes(this IWebDriver driver)
        {
            var script = @"
            let aggr = document.getElementsByClassName('container-aggregate');
            let i = 0;
            while(i < aggr.length){
                let chbs = aggr[i++].getElementsByClassName('aggregate-checkbox');
                let j = 0;
                while(j < chbs.length){
                chbs[j++].click();
                }
            }";
            driver.Execute(script);
        }

        private static int _sggIndex;

        public static void CheckCollapsedGroup(this IWebDriver driver, int [] expectedCount, string groupClass = "server-grouping-grid")
        {
            var tab = driver.GetTable(container:driver.GetElementByClassName("gridArea")).GetParent();
            _sggIndex = 0;
            tab.CheckCollapsedGroup(expectedCount, groupClass);
        }

        public static void CheckCollapsedGroup(this IWebElement container, int [] expectedCount, string groupClass = "server-grouping-grid")
        {
            var accCount = Browser.CountTableRows(container);
            Assert.AreEqual(expectedCount[_sggIndex], accCount, $"Expected {expectedCount[_sggIndex]} records but actual is {accCount}");
            var btExpand = container.GetElementByClassName("k-i-expand");
            if (btExpand == null) return;
            while (btExpand != null)
            {
                if (expectedCount.Length == 1)
                    Assert.Fail("Wrong usage CheckCollapsedGroup() method: expected more Count variables");
                btExpand.Click2();
                var sgg = BrowserWait.Until(d => Browser.GetElementsByClassName(groupClass)[_sggIndex++]);
                BrowserWait.Until(d => sgg.IsAvailableElementByjQ("table"));
                var table = sgg.GetElementByQuery(GetElementBy.Tagname, "table");
                BrowserWait.Until(d => table.IsVisiblbe());
                if (sgg.GetElementByQuery(GetElementBy.Tagname, "span.k-pager-info") != null)
                    BrowserWait.Until(d => sgg.IsAvailableElementByjQ("span.k-pager-info.k-label"));
                sgg.CheckCollapsedGroup(expectedCount);
                btExpand = container.GetElementByClassName("k-i-expand");
            }
        }

        #endregion

        public static int CountTableRows(this IWebElement section, bool all = true) => Browser.CountTableRows(section, all);

        public static int CountTableRows(this IWebDriver driver, IWebElement section = null, bool all = true)
        {
            //Driver.Report.StartStep("Counting table rows");
            try
            {
                var pagerInfo = section.GetElementByQuery(GetElementBy.Tagname, "span.k-pager-info.k-label");
                if (pagerInfo == null)
                {
                    var rows = section.GetTableBodyRows();
                    var rowcount = rows.Count;
                    if (rowcount == 0) return 0;
                    foreach (var row in rows)
                    {
                        if (row == null || row.GetAttribute("class").Contains("k-grouping-row") || row.GetAttribute("class").Contains("k-detail-row") || row.GetAttribute("class").Contains("k-no-data"))
                            rowcount--;
                    }
                    return rowcount;
                }
                var infoItems = pagerInfo.Text.Split(' ');
                
                if (infoItems.Length < 2) return -1;
                if (infoItems.Length < 6) return 0;
                var i = 0;
                while (infoItems[i++].ToLower() != "from")
                {
                }
                return Convert.ToInt32(infoItems[all ? i : i - 2]);
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        public static ReadOnlyCollection<IWebElement> GetTableBodyRows(this IWebElement table, bool body = true)
        {
            //Driver.Report.StartStep("Getting table row array");
            var script = (table == null)
                ? "let table = document.getElementsByClassName('k-selectable')[0];"
                : "let table = arguments[0];";
            script += @$"
if(table.tagName!='TABLE') 
    table = table.getElementsByClassName('k-selectable')[0];
let rows = ((table == null) ? null : table{(body  ? ".tBodies[0]" : "")}.rows); 
if(rows != null && rows.length == 0) 
    rows = null; 
return rows;";
            var rows = (ReadOnlyCollection<IWebElement>)((table == null) ? Browser.Execute(script) : Browser.Execute(script, table));
            //AllureNextReport.FinishStep();
            return rows ?? new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
        }

        public static int[] ConvertDataToInt(this string[] income)
        {
            if (income.Length == 0) return null;
            var ret = new int[income.Length];
            for (var i = 0; i < income.Length; ++i)
            {
                ret[i] = Convert.ToInt32(income[i]);
            }
            return ret;
        }

        public static int ConvertDataToInt(this string income) => income == null ? 0 : Convert.ToInt32(income);


        public static bool[] ConvertDataToBool(this string[] income)
        {
            if (income.Length == 0) return null;
            var ret = new bool[income.Length];
            for (var i = 0; i < income.Length; ++i)
            {
                ret[i] = Convert.ToBoolean(income[i]);
            }
            return ret;
        }

        public static bool ConvertDataToBool(this string income) => income != null && Convert.ToBoolean(income);

        /// <summary>
        /// Wait until file with file name is created
        /// </summary>
        /// <param name="driver">current Web Driver</param>
        /// <param name="fileName">File name</param>
        /// <param name="timeout">Time out in milliseconds</param>
        public static void WaitForFile(this IWebDriver driver, string fileName, long timeout)
        {
            new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout))
                .Until(d => FileProcessor.FileExists(fileName));
        }

        public static bool IsDropDownActive(this IWebElement dropDown)
        {
            var activeClasses = dropDown.GetAttribute("class").Split(' ');
            return activeClasses.FirstOrDefault(a => a == "k-state-active" || a == "k-state-border-down" || a == "k-state-border-up") != null;
        }

        public static string GetBorderState(this IWebElement control, bool invert = true, bool except = true)
        {
            var activeClasses = control.GetAttribute("class").Split(' ');
            var borderState = activeClasses.FirstOrDefault(a => a.Contains("k-state-border-"));
            if (borderState == null)
            {
                var activeControl = control.GetElementByClassName("k-state-border-up") ?? control.GetElementByClassName("k-state-border-down");
                activeClasses = activeControl?.GetAttribute("class").Split(' ');
                borderState = activeClasses?.FirstOrDefault(a => a.Contains("k-state-border-"));
            }
            if (borderState == null && except)
                Assert.Fail("Active drop down list was not found");
            return borderState == null ? null : (!invert ? borderState.Split('-')[3] : borderState.Split('-')[3] == "up" ? "down" : "up");
        }

        public static string GetCurrentUrl(this IWebDriver driver)
        {
            var retUrl = driver.Url;
            var tmpStrs = retUrl.Split('#');
            //if (tmpStrs.Length > 1 && string.IsNullOrEmpty(tmpStrs[1]))
                retUrl = tmpStrs[0];
            return retUrl;
        }

        public static string Localize(this string toTranslate, string lang = "en-US")
        {
            switch (lang)
            {
                case "fr":
                case "fr-FR":
                    return toTranslate switch
                    {
                        "Create" => "Créer",
                        "Edit" => "Modifier",
                        "Delete" => "Supprimer",
                        "View" => "Afficher",
                        "Save and Edit" => "Enregistrer et Modifier",
                        "Save" => "Enregistrer",
                        "Cancel" => "Annuler",
                        "Return" => "Retour",
                        "More" => "Plus",
                        _ => toTranslate,
                    };
                default:
                    return toTranslate;
            }
        }

        public static string GetLanguageValue(this IWebDriver driver)
        => ((IWebElement)driver.Execute("return ".GetQuerySript(GetElementBy.ClassName, "language .k-input-value-text .country-description"))).GetElementValue();

        public static string GetListItemByLang(this IWebDriver driver, string lang = "en")
        {
            var dsc = driver.GetElementByTextWithTag(lang.GetCoreLanguageItem(), "span");
            return dsc.GetElementValue();
        }

        public static string GetCoreLanguageItem(this string lang)
        {
            return lang switch
            {
                "1033" or "en" or "en-US" or "en-GB" or "c%3Den-US%7Cuic%3Den-US" => "English - EN",
                "1036" or "fr" or "fr-FR" or "c%3Dfr-FR%7Cuic%3Dfr-FR" => "Français - FR",
                "ca-ES" or "ca" or "1027" => "Catalan - CA",
                "es-ES" or "es" or "1034" => "Español - ES",
                "it-IT" or "it" or "1040" => "Italiano - IT",
                "pt-PT" or "pt" or "2070" => "Português - PT",
                _ => ""
            };
        }

        public static string WhatIsLang(this string checkLang)
        {
            return checkLang switch
            {
                "1033" or "en" or "en-US" or "en-GB" or "c%3Den-US%7Cuic%3Den-US" or "English - EN" => "en",
                "1036" or "fr" or "fr-FR" or "c%3Dfr-FR%7Cuic%3Dfr-FR" or "Français - FR" => "fr",
                "ca-ES" or "ca" or "1027" or "Catalan - CA" => "ca",
                "es-ES" or "es" or "1034" or "Español - ES" => "es",
                "it-IT" or "it" or "1040" or "Italiano - IT" => "it",
                "pt-PT" or "pt" or "2070" or "Português - PT" => "pt",
                _ => checkLang
            };
        }

        public static string IdToNameConvert(this string income)
        {
            var tmp = income.Split('_');
            var outcome = tmp[0];
            for (var p = 1; p < tmp.Length; ++p)
            {
                if (string.IsNullOrEmpty(tmp[p])) continue;
                if (int.TryParse(tmp[p], out var i) && i.ToString().Equals(tmp[p]))
                {
                    outcome += $"[{i}]";
                    continue;
                }
                outcome += "." + tmp[p];
            }
            return outcome;
        }

        public static string ClassTreePathToSymbolicString(this string income)
        {
            var cur = false;
            var ret = "(P) ⊙";
            var treeLst = income.Split('#')[1].Split('.');
            for (var i = 0; i < treeLst.Length; ++i)
            {
                switch (treeLst[i])
                {
                    case "Id":
                        if (cur)
                            ret += "𝅘Code";
                        break;
                    case "Country":
                    case "Currency":
                        cur = true;
                        ret += $"ᗒ{treeLst[i]}";
                        break;
                    case "AccOwner":
                        ret += $"ᗒCompany";
                        break;
                    default:
                        if (i < treeLst.Length - 1)
                            ret += $"ᗒ{treeLst[i]}";
                        else
                            ret += $"𝅘{treeLst[i]}";
                        break;
                }
            }
            return ret;
        }

        public static string ConvertToRegionalData(this string value, string rset = "")
        {
            var ret = value;
            var vSplit = ret.Split(' ');
            var atFormat = vSplit[0].Contains('.');
            var dSplit = atFormat ? vSplit[0].Split('.') : vSplit[0].Split('/');
            var cset = ConfigSettingsReader.RegionalSettings;
            if (string.IsNullOrEmpty(rset))
                rset = ConfigSettingsReader.RegionalSettings;

            switch (rset)
            {
                case "Spain":
                case "Catalan":
                case "France":
                case "Italy":
                case "Portuguese":
                    for (var i = 0; i < dSplit.Length; ++i)
                        dSplit[i] = (i < 2) ? $"{dSplit[i]:00}" : $"{dSplit[i]:0000}";
                    if (!atFormat && cset == "Great Britain")
                        (dSplit[1], dSplit[0]) = (dSplit[0], dSplit[1]);
                    break;
                case "Great Britain":
                    for (var i = 0; i < dSplit.Length; ++i)
                        dSplit[i] = Convert.ToInt32(dSplit[i]).ToString();
                    if (atFormat || cset != "Great Britain")
                        (dSplit[1], dSplit[0]) = (dSplit[0], dSplit[1]);
                    break;
                default:
                    break;
            }
            vSplit[0] = string.Join('/', dSplit);
            ret = string.Join(' ', vSplit);
            return ret;
        }

        public static string ConvertToRegionalNumeric(this string value, string rset = "Great Britain")
        {
            if (string.IsNullOrEmpty(value)) return value;
            var ret = value;
            var vSplit = ret.Split('.');
            switch (rset)
            {
                case "Italy":
                case "Spain":
                    vSplit[0] = vSplit[0].Replace(',', '.');
                    ret = string.Join(',', vSplit);
                    break;
                case "Catalan":
                case "France":
                case "Portuguese":
                    vSplit[0] = vSplit[0].Replace(',', ' ');
                    ret = string.Join(',', vSplit);
                    break;
                default:
                    break;
            }
            return ret;
        }

        public static string[] ConvertToRegionalNumeric(this string[] value, string rset = "Great Britain")
        {
            var ret = new string[value.Length];
            for (var i = 0; i < value.Length; ++i)
            {
                ret[i] = value[i].ConvertToRegionalNumeric(rset);
            }
            return ret;
        }

        public static string GetRegionalName(this string value)
        {
            return value switch
            {
                "es-ES" => "es-ES",
                "Spain" => "es-ES",
                "ca-ES" => "ca-ES",
                "Catalan" => "ca-ES",
                "fr-FR" => "fr-FR",
                "France" => "fr-FR",
                "it-IT" => "it-IT",
                "Italy" => "it-IT",
                "pt-PT" => "pt-PT",
                "Portuguese" => "pt-PT",
                "en-US" => "en-US",
                "en-GB" => "en-US",
                "Great Britain" => "en-US",
                _ => "en-US",
            };
        }

        public static IWebElement RefreshButton(this IWebDriver driver)
        {
            return driver.GetElementByClassName("k-i-reload");
        }

        public static IWebElement RefreshButton(this IWebElement container)
        {
            return (container != null) ? container.GetElementByClassName("k-i-reload") : Browser.RefreshButton();
        }
    }
}
