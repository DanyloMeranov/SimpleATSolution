using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Events;

namespace BaseDriver
{
    /// <summary>
    /// Override selenium methods to add event logs
    /// </summary>
    public class NextEventFiringWebDriver : EventFiringWebDriver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NextEventFiringWebDriver"/> class.
        /// </summary>
        /// <param name="parentDriver">The parent driver.</param>
        public NextEventFiringWebDriver(IWebDriver parentDriver)
            : base(parentDriver)
        {
        }

        /// <summary>
        /// Raises the <see cref="E:Navigating" /> event.
        /// </summary>
        /// <param name="e">The <see cref="WebDriverNavigationEventArgs"/> instance containing the event data.</param>
        protected override void OnNavigating(WebDriverNavigationEventArgs e)
        {
            //Driver.Report.StartStep("Navigating to: " + e.Url);
            base.OnNavigating(e);
            //AllureNextReport.FinishStep();
        }

        /// <summary>
        /// Raises the <see cref="E:ElementClicked" /> event.
        /// </summary>
        /// <param name="e">The <see cref="WebElementEventArgs"/> instance containing the event data.</param>
        protected override void OnElementClicking(WebElementEventArgs e)
        {
            //Driver.Report.StartStep("Clicking on: " + ToStringElement(e));//'" + e.Element.GetElementTitle() + "'");
            base.OnElementClicking(e);
            //AllureNextReport.FinishStep();
        }

        /// <summary>
        /// Raises the <see cref="E:ElementValueChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="WebElementValueEventArgs"/> instance containing the event data.</param>
        protected override void OnElementValueChanged(WebElementValueEventArgs e)
        {
            //Driver.Report.StartStep($"Element '{ToStringElement(e)}' value changed to: " + e.Value);
            base.OnElementValueChanged(e);
            //AllureNextReport.FinishStep();
        }

        /// <summary>
        /// Raises the <see cref="E:FindingElement" /> event.
        /// </summary>
        /// <param name="e">The <see cref="FindElementEventArgs"/> instance containing the event data.</param>
        protected override void OnFindingElement(FindElementEventArgs e)
        {
            //Driver.Report.StartStep("Finding element: " + e.FindMethod);
            base.OnFindingElement(e);
            //AllureNextReport.FinishStep();
        }

        /// <summary>
        /// Raises the <see cref="E:ExceptionThrown" /> event.
        /// </summary>
        /// <param name="e">The <see cref="WebDriverExceptionEventArgs"/> instance containing the event data.</param>
        protected override void OnException(WebDriverExceptionEventArgs e)
        {
            //Driver.Report.StartStep("There was an exception: " + e.ThrownException.Message);
            //AllureNextReport.FinishStep();
                //LogFailedStepWithFailedTestCase(e.ThrownException);
            base.OnException(e);
        }
        
        /// <summary>
        /// To the string element.
        /// </summary>
        /// <param name="e">The <see cref="WebElementEventArgs"/> instance containing the event data.</param>
        /// <returns>Formated issue</returns>
        private static string ToStringElement(WebElementEventArgs e)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "{0} {{{1}{2}{3}{4}{5}{6}{7}{8}{9}}}",
                e.Element.TagName,
                AppendAttribute(e, "id"),
                AppendAttribute(e, "name"),
                AppendAttribute(e, "value"),
                AppendAttribute(e, "class"),
                AppendAttribute(e, "type"),
                AppendAttribute(e, "role"),
                AppendAttribute(e, "title"),
                AppendAttribute(e, "href"),
                AppendAttribute(e, "data-entity"));
        }

        /// <summary>
        /// Appends the attribute.
        /// </summary>
        /// <param name="e">The <see cref="WebElementEventArgs"/> instance containing the event data.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>Atribute and value</returns>
        private static string AppendAttribute(WebElementEventArgs e, string attribute)
        {
            var attrValue = attribute == "text" ? e.Element.Text : e.Element.GetAttribute(attribute);
            return string.IsNullOrEmpty(attrValue) ? string.Empty : string.Format(CultureInfo.CurrentCulture, " {0}='{1}' ", attribute, attrValue);
        }
    }
}
