using BaseDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TestTools;

namespace BasePage
{
    public class BasePageValidator<M>
        where M : BasePageElementMap
    {
        protected IWebDriver WebDriver => Page.WebDriver;
        protected WebDriverWait DriverWait => Page.DriverWait;

        public BasePageValidator()
        {
            Page = null;
        }

        public M Page { get; private set; }

        public void SetPage(M page)
        {
            Page = page;
        }

        public void AssertEntityDoesNotExistInGrid(string[] columnNames, string[][] cellValues, bool backToFirst = true)
        {
            try
            {
                var msgLine = "Assert entity not exists in table by several column values: ";
                for (var i = 0; i < columnNames.Length; ++i)
                {
                    msgLine = $"{msgLine}{cellValues[i]}({columnNames[i]})";
                    if (i < columnNames.Length - 1) msgLine += ", ";
                }
                WebDriver.WaitForReadyState();
                Page.SetLanguage();
                var j = Page.SelectRowBySeveralValues(columnNames, cellValues, backToFirst);
                if (j < cellValues.Length)
                {
                    Assert.Fail(
                        $"{cellValues.Length - j} value{(cellValues.Length - j == 1 ? " was" : "s where")} found in table!");
                }

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void AssertEntityDoesNotExistInGrid(string[] columnNames, string[] cellValues, bool backToFirst = true)
        {
            try
            {
                var msgLine = "Assert entity not exists in table by several column values: ";
                for (var i = 0; i < columnNames.Length; ++i)
                {
                    msgLine = $"{msgLine}{cellValues[i]}({columnNames[i]})";
                    if (i < columnNames.Length - 1) msgLine += ", ";
                }
                WebDriver.WaitForReadyState();
                Page.SetLanguage();
                var j = Page.GetElementRowNumberBySeveralValues(columnNames, cellValues, backToFirst);
                if (j > -1)
                {
                    Assert.Fail("Values were found in table!");
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void AssertEntityDoesNotExistByContext(string columnName, string cellValue, IWebElement container = null)
        {
            Page.ContextSearchInGrid(cellValue, columnName);
            var i = Page.CountTableRows(container);
            if (i > 0)
            {
                Assert.Fail($"Value '{cellValue}' was found in column '{columnName}'!");
            }

        }

        public void AssertEntityDoesNotExistInGrid(string columnName, string cellValue, bool backToFirst = true)
        {
            try
            {
                //WebDriver.WaitForReadyState();
                Page.SetLanguage();
                if (Page.IsElementExistsInGrid(cellValue))
                {
                    Assert.Fail($"Entity '{cellValue}' was found in grid!");
                }
                if (!backToFirst)
                {
                    return;
                }
                Page.First();
            }
            catch (Exception e)
            {
                throw;    // it is for the test failed, without it test continues execution after error
            }
        }

        public int AssertEntityExistsInGrid(string[] columnNames, string[][] cellValues, bool backToFirst = false,
            GetElementBy getTableBy = GetElementBy.ClassName, string getTableByValue = null, IWebElement container = null, string[] groupElementName = null)
        {
            try
            {
                var msgLine = "Assert entity exists in table by several column values: ";
                for (var p = 0; p < cellValues.Length; ++p)
                {
                    for (var i = 0; i < columnNames.Length; ++i)
                    {
                        msgLine = $"{msgLine}{cellValues[p][i]}({columnNames[i]})";
                        if (i < columnNames.Length - 1) msgLine += ", ";
                    }
                    if (p < cellValues.Length - 1)
                        msgLine += "; ";
                }
                WebDriver.WaitForReadyState();
                Page.SetLanguage();
                var j = Page.SelectRowBySeveralValues(columnNames, cellValues, backToFirst, getTableBy, getTableByValue, container);
                if (j > 0)
                {
                    Assert.Fail($"{j} value{(j == 1 ? " was" : "s were")} not found in table!");
                }
                return j;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public int AssertEntityExistsInGrid(string[] columnNames, string[] cellValues, bool backToFirst = false,
            bool checkTheCheckbox = false, GetElementBy getTableBy = GetElementBy.ClassName, string getTableByValue = null, IWebElement container = null, string[] groupElementName = null)
        {
            try
            {
                var msgLine = "Assert entity exists in table by several column values: ";
                for (var i = 0; i < columnNames.Length; ++i)
                {
                    msgLine = $"{msgLine}{cellValues[i]}({columnNames[i]})";
                    if (i < columnNames.Length - 1) msgLine += ", ";
                }
                WebDriver.WaitForReadyState();
                Page.SetLanguage();
                var j = Page.GetElementRowNumberBySeveralValues(columnNames, cellValues, backToFirst, checkTheCheckbox,
                    getTableBy, getTableByValue, container, groupElementName);
                if (j < 0)
                {
                    Assert.Fail("Values were not found in table!");
                }

                return j;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void AssertEntitiesExistInGrid(string columnName, string[] cellValues, bool backToFirst = false, bool checkTheCheckbox = true,
                                             IWebElement container = null, string filter = "DEFFLTR", bool force = false, bool applyDefaultConfiguration = true)
        {
            try
            {
                var names = $"'{cellValues[0]}'";
                for (var i = 1; i < cellValues.Length; ++i)
                    names += $", '{cellValues[i]}'";
                if (applyDefaultConfiguration)
                {
                    Page.SetLanguage();
                    Page.CreateNewOrSelectExistingConfiguration(filter, ConfigurationType.Filter, force: force);
                    WebDriver.WaitForReadyState();
                }
                Page.SetLanguage();
                container ??= WebDriver.GetTable().GetParent();
                Page.LookForValuesInGrid(container, cellValues, false, checkTheCheckbox);

                if (backToFirst)
                    Page.First(container);

                //WebDriver.WaitForReadyState();
            }
            catch (Exception e)
            {
                throw;
            }
        }


        public int AssertEntityExistsInGrid(string columnName, string cellValue, bool backToFirst = false,
                       bool checkTheCheckbox = false, GetElementBy getTableBy = GetElementBy.ClassName, string getTableByValue = null,
                                                     IWebElement container = null, string groupElementName = "", string filter = "DEFFLTR", bool force = false, bool applyDefaultConfiguration = true)
        {
            try
            {
                if (applyDefaultConfiguration)
                {
                    Page.SetLanguage();
                    Page.CreateNewOrSelectExistingConfiguration(filter, ConfigurationType.Filter, force: force);
                    WebDriver.WaitForReadyState();
                }
                Page.SetLanguage();
                var i = -1;
                if (!checkTheCheckbox)
                    try
                    {
                        Page.ContextSearchInGrid(cellValue, columnName);
                        i = Page.CountTableRows(container);
                    }
                    catch (Exception e) 
                    {
                        if (e.Message.Contains("was not found"))
                            checkTheCheckbox = !checkTheCheckbox;
                        else
                            throw;
                    }
                if (checkTheCheckbox)
                    i = Page.GetElementRowNumber(getTableBy, getTableByValue, columnName, cellValue, backToFirst,
                        true, container, groupElementName);

                if (i < (checkTheCheckbox ? 0 : 1))
                {
                    Assert.Fail($"Value '{cellValue}' was not found in column '{columnName}' (check={checkTheCheckbox})!");
                }

                return i - (checkTheCheckbox ? 0 : 1);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void AssertValueInLineByColumnName(int irow, string value, string columnName, IWebElement table)
        {
            // Assert that Task status is Finished in the row
            var state = Page.GetTableCellText(irow, columnName, table);
            Assert.AreEqual(value, state, $"Expected '{value}' but actual is '{state}' value in column '{columnName}'");

        }

        public void AssertNoAlertPresent()
        {
            var alarmMessages = WebDriver.AlertMessagesCheck();
            if (alarmMessages == null || alarmMessages.Length == 0) return;
            var alarmMessage = "Alert is present:";
            var n = 0;
            foreach (var alarm in alarmMessages)
            {
                alarmMessage += $"\r\nMessage {++n}: {alarm}";
            }
            Assert.Fail(alarmMessage);
        }

        public void AssertThatMainValidationErrorMessageIsShown()
        {
            if (WebDriver.CheckErrorMessage())
            {
                return;
            }
            Assert.Fail("Error container was not showed!");
        }

        public void AssertErrorMessage(string expectedMessage)
        {
            var errmsg = Page.AllertChecker(true).Trim('\n', '\r', ' ', '×');
            if (string.IsNullOrEmpty(errmsg))
                Assert.Fail("Error message not shown");
            Assert.AreEqual(expectedMessage, errmsg,
                $"Expected '{expectedMessage}' but actual is '{errmsg}'!");
        }

        public void AssertThatValidationErrorHintsAreShown(string[] errorMessages)
        {
            foreach (var message in errorMessages)
            {
                var errorHint = Page.GetErrorMessage(message);
                if (errorHint == null || errorHint.Count == 0)
                    Assert.Fail($"Error hint '{message}' was not showed!");
            }
        }

        public void AssertThatValidationErrorMessagesAreShown(string[] errorMessages, string entityName/**/ = ""/**/)
        {
            foreach (var message in errorMessages)
            {
                string expectedText = null;
                var msgStruct = message.Split('|');
                var titleOnly = false;
                var included = false;
                if (msgStruct.Length > 2)
                {
                    if (msgStruct[1].Contains("T"))
                        titleOnly = true;
                    if (msgStruct[1].Contains("I"))
                        included = true;
                    switch (msgStruct[2])
                    {
                        case "FIXED":
                        case "F":
                            expectedText = msgStruct[0];
                            break;
                        case "REQUIRED":
                        case "R":
                            expectedText = $"The {msgStruct[0]} field is required.";
                            break;
                        case "REQUIRED2":
                        case "R2":
                            expectedText = $"The {msgStruct[0]} field is required";
                            break;
                        case "MANDATORY":
                        case "M":
                            expectedText = $"The {msgStruct[0]} of the {entityName} is mandatory";
                            break;
                        case "MANDATORY2":
                        case "M2":
                            expectedText = $"The {msgStruct[0]} field is mandatory";
                            break;
                        default:
                            Assert.Fail("Unexpected type!");
                            break;
                    }
                }
                if (expectedText == null)
                    expectedText = msgStruct[0];
                var errorItem = Page.GetErrorMessageItem(expectedText, !included);
                if (errorItem == null || errorItem.Count == 0)
                    Assert.Fail($"Validation summary item '{expectedText}' was not showed!");
                if (!titleOnly)
                {
                    var errorHint = Page.GetErrorMessage(expectedText, "span", !included);
                    if (errorHint == null || errorHint.Count == 0)
                        Assert.Fail($"Error point '{expectedText}' was not showed!");
                }
            }
        }

        public void AssertCurrentBrowserWindowTitle(string titleExpected)
        {
            var titleActual = WebDriver.Title;
            Assert.IsTrue(titleActual.Contains(titleExpected),
                $"Expected browser window title is '{titleExpected}', but actual is '{titleActual}'.");
        }

        public void AssertRequestVerificationToken()
        {
            var token = WebDriver.GetElementByQuery(GetElementBy.Tagname, "input[name='__RequestVerificationToken']");
            Assert.IsNotNull(token, "Input with name '__RequestVerificationToken' is not present on the card!");
        }

        public void AssertTextBoxValue(string expectedValue, IWebElement element)
        {
            if (element == null)
            {
                Assert.Fail($"Text box element is not found, intended text to verify is: '{expectedValue}'.");
            }
            DriverWait.WaitForElementIsAvailable(element);
            var value = element.GetElementValue();
            try
            {
                if (string.IsNullOrEmpty(expectedValue))
                    Assert.AreEqual(string.IsNullOrEmpty(expectedValue), string.IsNullOrEmpty(value));
                else
                    Assert.AreEqual(expectedValue, value.RemoveDoubleSpacing());
            }
            catch
            {
                element.ScrollIntoView();
                throw;
            }

        }

        public void AssertTextBoxValue(string expectedValue, string elementId)
        {
            try
            {
                var singleField = WebDriver.GetElementById(elementId);
                //singleField.SetFocus();
                AssertTextBoxValue(expectedValue, singleField);
            }
            finally
            {

            }
        }

        public void AssertListBoxValueSelected(string expectedValue, string entityName, bool editMode = true, bool withInput = true, string inputId = "Id")
        {
            var additionalString = (withInput) ? $"_{inputId}" : "";
            WebDriver.WaitForReadyState();
            try
            {
                if (!editMode)
                {
                    var listBoxParent = WebDriver.GetElementById($"{entityName}{additionalString}");
                    var selection = listBoxParent.GetElementByQuery(GetElementBy.AttributeWithValue, "selected=selected").GetText(true) ??
                        listBoxParent.GetAttrib("value");
                    try
                    {
                        Assert.AreEqual(expectedValue, selection, $"'{expectedValue}' value was expected, but '{selection}' was found!");
                    }
                    catch
                    {
                        listBoxParent.ScrollIntoView();
                        throw;
                    }
                    return;
                }
                var mainCard = WebDriver.GetElementById("mainCardForm");
                var lstBxs = mainCard.GetElementsByOneAttribute($"{entityName}{additionalString}_listbox", "span", "aria-controls");
                var lstBx = (lstBxs.Count == 1)
                    ? lstBxs[0]
                    : lstBxs?.FirstOrDefault(lb => lb != null && string.IsNullOrEmpty(lb.GetElementByTagName("select").GetAttrib("class").Split(' ').FirstOrDefault(s => s.Equals("ignore"))));
                var element = lstBx/*mainCard.GetElementByAttributeWithValue($"aria-controls={entityName}{additionalString}_listbox")*/ ??
                                              //mainCard.GetElementById(entityName)?.GetParent() ??
                                              //mainCard
                                              //.GetElementByAttributeWithValue($"data-valmsg-for='{entityName.IdToNameConvert()}'")
                                              //.GetParent()
                                              //.GetElementByAttributeWithValue("role=listbox");
                              mainCard.GetElementByAttributeWithValue($"data-valmsg-for='{entityName.Replace('_', '.')}'")
                                      .GetParent().GetElementByAttributeWithValue("role=listbox");

                DriverWait.WaitForElementIsVisible(element);
                var span = element.GetElementByClassName("k-input-value-text").WaitForElementIsClickable();

                Assert.AreEqual(expectedValue, span.Text, true,
                    $"Expected value is: '{expectedValue}', but value in list box is: '{span.Text}'");
            }
            finally
            {
            }
        }

        public void AssertTreeListBoxValueSelected(string expectedValue, string entityName)
        {
            var expected = expectedValue.Split('|')[^1];
            var inputElement = WebDriver.GetElementById(entityName).GetParent().GetElementByQuery(GetElementBy.ClassName, "k-input-value-text");
            var selection = inputElement.GetElementValue();
            Assert.AreEqual(expected, selection, $"'{expected}' value was expected, but '{selection}' was found!");
        }

        public void AssertDateValue(string expectedNumberOfDaysFromToday, IWebElement dateEdit)
        {
            if (dateEdit == null)
            {
                Assert.Fail(
                    $"Calendar text box element is not found, intended number of days from today to verify: '{expectedNumberOfDaysFromToday}'.");
            }
            var dateExpected = Page.GetCalendarDate(expectedNumberOfDaysFromToday, expect: true).ToString(BasePageElementMap.GetCurrentCultureInfo).ConvertToRegionalData(ConfigSettingsReader.RegionalSettings);
            //if (dateExpected.Length != 10)
            //    dateExpected = dateExpected.ConvertToRegionalData(ConfigSettingsReader.RegionalSettings);
            //AssertTextBoxValue(dateExpected, dateEdit);
            var format = dateEdit.GetAttrib("format");
            var actualValue = dateEdit.GetElementValue();
            try
            {
                if (actualValue.Contains('#'))// actualDate = actualValue.Split('#')[1];
                    AssertFixedSlidingDate(dateExpected, dateEdit);
                else
                {
                    var actualDate = (!string.IsNullOrEmpty(format)
                        ? DateTime.ParseExact(dateEdit.GetElementValue(), format, BasePageElementMap.GetCurrentCultureInfo)
                        : Page.GetCalendarDate(actualValue, expect: false)
                        ).ToString(BasePageElementMap.GetCurrentCultureInfo).ConvertToRegionalData(ConfigSettingsReader.RegionalSettings);

                    Assert.AreEqual(dateExpected, actualDate, $"Expected value is '{dateExpected}', but actual data '{actualDate}' (text box value is '{actualValue}')");
                }
            }
            catch
            {
                dateEdit.ScrollIntoView();
                throw;
            }
        }

        public void AssertFixedSlidingDate(string expectedValue, IWebElement dateEdit)
        {
            var recDateEditParent = dateEdit.GetParent();
            recDateEditParent.SetFocus();
            var valueInput = recDateEditParent.GetElementByClassName("k-input");
            var actualValue = valueInput.GetElementValue();
            Assert.AreEqual(expectedValue, actualValue.RemoveDoubleSpacing(), $"Expected value is '{expectedValue}', but actual data '{actualValue}'");
        }


        public void AssertSingleSelectTextBoxValue(string value, string idEntityName, bool editMode = true, bool trnCode = false)
        {
            string idStr;
            IWebElement lookUpInput;
            WebDriver.WaitForReadyState();
            if (trnCode && !editMode)
            {
                idStr = $"{idEntityName.Replace('_', '.')}{(editMode ? "_SelectedCode" : "")}";
                lookUpInput = WebDriver.GetElementByOneAttribute(idStr, "label", "for")?.GetParent()?.GetParent()?.
                    FindElements(By.XPath("following-sibling::*/div"))[0];
            }
            else
            {
                idStr = $"{idEntityName}{(editMode ? "_SelectedCode" : "")}";
                lookUpInput = WebDriver.GetElementById(idStr);
            }
            Assert.IsNotNull(lookUpInput, $"Control with id='{idStr}' is absent! (editmode={editMode}, trncode={trnCode})");
                
            string actual;
            if (editMode)
            {
                actual = lookUpInput.GetAttribute("data-old-value").RemoveDoubleSpacing();
            }
            else
            {
                actual = lookUpInput.GetElementValue().RemoveDoubleSpacing();
            }
            try
            {
                Assert.AreEqual(value, actual, $"Control with id='{idStr}' has unexpected value!");
            }
            catch
            {
                lookUpInput.ScrollIntoView();
                throw;
            }
        }

        public void AssertNumericTextBoxValue(string expectedValue, IWebElement control, bool editMode = true)
        {
            if (control == null)
            {
                Assert.Fail($"Numeric text box element is not found, intended number to verify: '{expectedValue}'.");
            }
            control.SetFocus();

            if (editMode)
                control.GetParent().WaitForElementIsClickable().SetFocus();
            var expected = expectedValue.ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings);
            //var splittedValue = expectedValue.Split(',');
            //var expected = splittedValue[0];
            //if (splittedValue.Length > 1)
            //{
            //    var afterpoint = splittedValue[1].TrimEnd('0');
            //    if (!string.IsNullOrEmpty(afterpoint)) expected += $".{afterpoint}";
            //}
            AssertTextBoxValue(expected, control);
        }

        public void AssertDetailTable(string entityName, int rowCount, object value, DetailTableType tableType)
        {
            Assert.AreNotEqual(null, value);
            var valueArray = value as List<Dictionary<string, string>>;
            if (null == valueArray) Assert.Fail("Value is not present");
            var minCount = Math.Min(valueArray.Count, rowCount);
            var divGrid = WebDriver.GetElementById($"{entityName}_table");
            var ttype = divGrid.GetAttribute("class").Contains("detail-table-widget");
            for (var i = 0; i < minCount; ++i)
            {
                var scriptBegin = $@"
                    var elem1 = document.getElementById('{entityName}_table');
                    var elem2 = elem1.getElementsByClassName('k-grid-content')[0];
                    var lines = elem2.getElementsByTagName('tr');
                    ";
                if (ttype)
                {
                    if (!valueArray[i].TryGetValue("Text", out var sTry)) sTry = valueArray[i]["Iban"];
                    var row = Convert.ToInt32(((long)WebDriver.Execute(scriptBegin + @"
                for(var i = 0; i < lines.length; i++)
                {
                    var cols = lines[i].getElementsByTagName('td');
                    if(cols[2].innerText.trim() == arguments[0].trim())
                        return i;
                } return -1;
                ", sTry)).ToString());
                    if (row < 0) Assert.Fail($"Line with '{sTry}' value not found in '{entityName}' table");
                    var script = scriptBegin + $"var cols = lines[{row}].getElementsByTagName('td');";
                    var main = (bool)WebDriver.Execute($"{script}\nreturn !!(cols[0].getElementsByClassName('selected')[0]);");
                    Assert.AreEqual(valueArray[i]["Main"].ToLower(), main.ToString().ToLower(),
                        $@"Main check box has wrong value at line {i} of {entityName} detail table!" +
                        $@"Expected '{valueArray[i]["Main"].ToLower()}' but '{main.ToString().ToLower()}' is present!");
                    var column1 = (string)WebDriver.Execute(script + "return cols[1].innerText.trim();");
                    if (!valueArray[i].TryGetValue("Type", out sTry)) sTry = valueArray[i]["Local"];
                    Assert.AreEqual(sTry, column1,
                        $@"Second colunm has wrong value at line {i} of {entityName} detail table!" +
                        $@"Expected '{sTry}' but '{column1}' is present!");
                    var column2 = (string)WebDriver.Execute(script + "return cols[2].innerText.trim();");
                    if (!valueArray[i].TryGetValue("Text", out sTry)) sTry = valueArray[i]["Iban"];
                    Assert.AreEqual(sTry, column2,
                        $@"Third column has wrong value at line {i} of {entityName} detail table!" +
                        $@"Expected '{sTry}' but '{column2}' is present!");
                    if (!entityName.Contains("NetAddresses")) return;
                    var column3 = (string)WebDriver.Execute(script + "return cols[3].innerText.trim();");
                    Assert.AreEqual(valueArray[i]["Comment"], column3,
                        $@"Fourth column has wrong value at line {i} of {entityName} detail table!" +
                        $@"Expected '{valueArray[i]["Comment"]}' but '{column3}' is present!");
                }
                else
                {
                    string col1;
                    string col2;
                    var col3 = "";
                    var col4 = "";
                    var col5 = "";
                    switch (tableType)
                    {
                        case DetailTableType.FixedCost:
                        case DetailTableType.UnitCost:
                            {
                                col1 = "StepFee";
                                col2 = "Ceiling";
                                break;
                            }
                        case DetailTableType.Amount:
                            {
                                col1 = "Rate";
                                col2 = "Ceiling";
                                break;
                            }
                        case DetailTableType.Steps:
                            {
                                col1 = "FixedAmmount";
                                col2 = "MarkupPercentage";
                                col3 = "RateIndex";
                                col4 = "RateIndexPart";
                                col5 = "Ceiling";
                                break;
                            }
                        case DetailTableType.CalculationType:
                            {
                                col1 = "Amount";
                                col2 = "Percentage";
                                col3 = "Ceiling";
                                break;
                            }
                        case DetailTableType.AccountIdentifiers:
                        case DetailTableType.Identifiers:
                        case DetailTableType.Phones:
                        case DetailTableType.NetAddresses:
                        default:
                            throw new ArgumentOutOfRangeException(nameof(tableType), tableType, null);
                    }
                    for (var j = 0; j < rowCount; ++j)
                    {
                        var rate = WebDriver.Execute(scriptBegin + @"
                            var cols = lines[" + j + @"].getElementsByTagName('td');
                            return cols[2].innerText.trim()");
                        AssertAreEqual(valueArray[j][col1].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings), rate.ToString());
                        var ceiling = WebDriver.Execute(scriptBegin + @"
                            var cols = lines[" + j + @"].getElementsByTagName('td');
                            return cols[3].innerText.trim()");
                        AssertAreEqual(valueArray[j][col2].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings), ceiling.ToString());
                        if (col3 != "" && (valueArray[j][col3] != null))
                        {
                            var cell3 = WebDriver.Execute(scriptBegin + @"
                            var cols = lines[" + j + @"].getElementsByTagName('td');
                            return cols[4].innerText.trim()");
                            AssertAreEqual(valueArray[j][col3].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings), cell3.ToString());
                        }
                        if (col4 != "" && (valueArray[j][col4] != null))
                        {
                            var cell4 = WebDriver.Execute(scriptBegin + @"
                            var cols = lines[" + j + @"].getElementsByTagName('td');
                            return cols[6].innerText.trim()");
                            AssertAreEqual(valueArray[j][col4].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings), cell4.ToString());
                        }
                        if (col5 != "" && (valueArray[j][col5] != null))
                        {
                            var cell5 = WebDriver.Execute(scriptBegin + @"
                            var cols = lines[" + j + @"].getElementsByTagName('td');
                            return cols[7].innerText.trim()");
                            AssertAreEqual(valueArray[j][col5].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings), cell5.ToString());
                        }
                    }
                }
            }
        }

        private void AssertAreEqual(string expected, string actual)
        {
            Assert.AreEqual(expected, actual, $@"Expected '{expected}' but '{actual}' is present!");

        }

        private void AssertAddressesSection(int index, IReadOnlyDictionary<string, string> values, bool editMode = true)
        {
            var main = values["AddressesMain"];
            if (main != null)
            {
                AssertToggleControlValue(main, $"Entity_Addresses_{index}__Main");
            }
            var type = values["Type"];
            if (type != null)
            {
                AssertListBoxValueSelected(type, $"Entity_Addresses_{index}__Addr_Type", editMode);
            }
            //values[3]; - Country description for validate
            AssertSingleSelectTextBoxValue(values["CountryDescription"], $"Entity_Addresses_{index}__Addr_Country", editMode);
            AssertTextBoxValue(values["Zip"], WebDriver.GetElementById($"Entity_Addresses_{index}__Addr_Zip"));
            AssertTextBoxValue(values["Province"], WebDriver.GetElementById($"Entity_Addresses_{index}__Addr_Province"));
            AssertTextBoxValue(values["City"], WebDriver.GetElementById($"Entity_Addresses_{index}__Addr_City"));
            AssertTextBoxValue(values["Street"], WebDriver.GetElementById($"Entity_Addresses_{index}__Addr_Street"));
            AssertTextBoxValue(values["Building"], WebDriver.GetElementById($"Entity_Addresses_{index}__Addr_Building"));
            AssertTextBoxValue(values["Apartment"], WebDriver.GetElementById($"Entity_Addresses_{index}__Addr_Appartment"));
            AssertTextBoxValue(values["AddressLine1_"], WebDriver.GetElementById($"Entity_Addresses_{index}__Addr_Line1"));
            AssertTextBoxValue(values["AddressLine2_"], WebDriver.GetElementById($"Entity_Addresses_{index}__Addr_Line2"));
            AssertTextBoxValue(values["Comment"], WebDriver.GetElementById($"Entity_Addresses_{index}__Comment"));
        }

        private void AssertOperationsSection(int index, IReadOnlyDictionary<string, string> values,
            bool editMode = true)
        {
            AssertSingleSelectTextBoxValue(values["OperationTypeDescription"],
                $"Entity_Types_{index}__OperationType", editMode); //values[1] - Operation Type description
            AssertSingleSelectTextBoxValue(values["SignatureTypeDescription"],
                $"Entity_Types_{index}__Signature", editMode); //values[3] - Signature description

            if (!string.IsNullOrEmpty(values["AmountLimitMin"]))
            {
                var limitMin = WebDriver.GetElementById($"Entity_Types_{index}__LimitMin");
                AssertNumericTextBoxValue(values["AmountLimitMin"], limitMin);
            }

            if (!string.IsNullOrEmpty(values["AmountLimitMax"]))
            {
                var limitMax = WebDriver.GetElementById($"Entity_Types_{index}__LimitMax");
                AssertNumericTextBoxValue(values["AmountLimitMax"], limitMax);
            }

            AssertSingleSelectTextBoxValue(values["CurrencyCode"], $"Entity_Types_{index}__Currency",
                editMode); //values[7] - Currency description
            var peroid = values["Period"];
            if (peroid != null)
            {
                AssertListBoxValueSelected(peroid, $"Entity_Types_{index}__Period", editMode);
            }
        }

        public void AssertLettersSection(int index, Dictionary<string, string> values, bool editMode = true)
        {
            if (!string.IsNullOrEmpty(values["CreationDate"]))
            {
                var calendarEdit = WebDriver.GetElementByName($"Entity.Letters[{index}].CreationDate");
                AssertDateValue(values["CreationDate"], calendarEdit);
            }

            AssertSingleSelectTextBoxValue(values["ContactDescription"], $"Entity_Letters_{index}__Contact", editMode);
            //values[2] - Contact description
            AssertTextBoxValue(values["Comment"], WebDriver.GetElementById($"Entity_Letters_{index}__Comment"));
        }

        public void AssertRulesSection(int index, Dictionary<string, string> values, bool editMode, string entityName)
        {
            // entityName : RulesO or RulesR
            // Assert Priority
            AssertNumericTextBoxValue(values["Priority"], WebDriver.GetElementById($"Entity_{entityName}_{index}__Prio"));

            // Assert toggle ExcludeFromProcess
            AssertToggleControlValue(values.ContainsKey("ExcludeFromProcess") ? values["ExcludeFromProcess"] : "false", $"Entity_{entityName}_{index}__ExcludeFromProcess");

            // Assert Rule
            AssertSingleSelectTextBoxValue(values["RuleDescription"], $"Entity_{entityName}_{index}__Rule", editMode);

            // Assert toggle RedefineOptions
            AssertToggleControlValue(values.ContainsKey("RedefineOptions") ? values["RedefineOptions"] : "false", $"Entity_{entityName}_{index}__RedefineOptions");
        }

        public void AssertRadioButtonValue(string id, string expectedValue = "")
        {
            var value = "";
            switch (expectedValue.ToLower())
            {
                case "yes":
                case "true":
                    value = "true";
                    break;
                case "no":
                case "false":
                    value = "false";
                    break;
                case "not set":
                    break;
                default:
                    value = expectedValue;
                    break;
            }
            var input = WebDriver.GetElementById($"{id}_{value}");
            Assert.IsNotNull(input, "Select radio as button verification failed: input checkbox was not found.");
            var checkedValue = input.GetAttribute("checked") ?? "false";
            var expected = new[] { "false", "unchecked" };
            Assert.IsTrue(!expected.Any(checkedValue.Contains),
                $"Expected '{(expectedValue != "" ? expectedValue : $"{id}_")}' radiobutton should be 'checked' but actual it is '{checkedValue}'");
        }

        private void AssertLevelSection(int index, IReadOnlyList<string> values)
        {
            AssertRadioButtonValue($"Entity_AaLevels_{index}__ValidationRule", values[0]);
            //TODO: realize user list processing
            AssertInMultiselectGrid($"Entity_AaLevels_{index}__Users", new[] { values[1] });
        }

        public void AssertArrayListView(string entityName, int rowCount, IReadOnlyList<string[]> values)
        {
            if (values == null)
                Assert.Fail($"'{entityName}' values are not present");
            var arrayNotEmpty = values.Count > 0;
            var count = (rowCount == 0 && arrayNotEmpty) ? 1 : rowCount > 0 ? rowCount : 0;
            for (var i = 0; i < count; ++i)
            {
                switch (entityName)
                {
                    case "AaLevels":
                        AssertLevelSection(count - i - 1, values[i]);
                        break;
                    default:
                        Assert.Fail($"Not implemented filling of list view for '{entityName}' Entity name");
                        break;
                }
            }

        }

        private void AssertIdentifierSection(int index, IReadOnlyDictionary<string, string> values)
        {
            var main = values["Main"];
            if (!string.IsNullOrEmpty(main))
            {
                AssertToggleControlValue(main, $"Entity_IdentifiersGroups_{index}__Main");
            }
            var local = values["Local"];
            if (!string.IsNullOrEmpty(local))
                AssertTextBoxValue(local,
                    WebDriver.GetElementById($"Entity_IdentifiersGroups_{index}__Local"));
            var iban = values["Iban"];
            if (!string.IsNullOrEmpty(iban))
                AssertTextBoxValue(iban, WebDriver.GetElementById($"Entity_IdentifiersGroups_{index}__Iban"));
            var freeform = values["FreeForm"];
            if (!string.IsNullOrEmpty(freeform))
                AssertTextBoxValue(freeform,
                    WebDriver.GetElementById($"Entity_IdentifiersGroups_{index}__FreeForm"));
        }

        public void AssertFile(string value)
        {
            //file-upload-container
            var uploadContainer = WebDriver.GetElementByClassName("file-upload-container");
            //k-file-name-size-wrapper
            var fileContainer = uploadContainer.GetElementByClassName("k-file-name-size-wrapper");
            var fileNameContainer = fileContainer.GetElementByClassName("k-file-name").Text;
            var expect = value;
            if (expect.Contains('\\'))
            {
                var sp = expect.Split('\\');
                expect = sp[^1];
            }
            Assert.AreEqual(expect, fileNameContainer);
        }

        private void AssertFilesSection(int index, IReadOnlyDictionary<string, string> values, bool editMode)
        {
            AssertSingleSelectTextBoxValue(values["AttachmentTypeName"], $"Entity_Files_{index}__FileStore_Type",
                editMode);
            if (values["AttachmentRoleCode"] != null)
                AssertSingleSelectTextBoxValue(values["AttachmentRoleCode"], $"Entity_Files_{index}__FileStore_Role",
                    editMode);
            if (values["FileToAttach"] == null) return;
            var fileContainer = WebDriver.GetElementByClassName("file-upload-container");
            var fileProperties = fileContainer.GetElementsByClassName("k-file-name");
            var fileName = values["FileToAttach"].Split('\\')[^1];
            if (fileProperties.Count != 3 || fileProperties[0].Text != fileName ||
                (values["FileDescription"] != null && fileProperties[1].Text != values["FileDescription"]) || fileProperties[2].Text != "")
                Assert.Fail(
                    $@"File information is wrong{
                        (fileProperties.Count < 1
                            ? "!"
                            : $@": file name '{fileProperties[0].Text}' not equal '{fileName}', file description '{fileProperties
                                    [1].Text}' not equal '{values["FileDescription"]}'{
                                    (fileProperties[2].Text != ""
                                        ? $" and has an error: '{fileProperties[2].Text}'!"
                                        : "!")}")}");
        }

        private void AssertRollingStepsSection(int index, IReadOnlyDictionary<string, string> values)
        {
            AssertTextBoxValue(values["Description"], WebDriver.GetElementById($"Entity_RlStepsVM_{index}__Description"));
            var slDateInput = WebDriver.GetElementById($"Entity_RlStepsVM_{index}__Formula");
            var editDateInput = slDateInput.GetParent().GetElementByClassName("sliding-date-input");
            AssertTextBoxValue(values["Formula"], editDateInput);
        }

        private void AssertSpecialDaysSection(int index, IReadOnlyDictionary<string, string> values)
        {
            AssertTextBoxValue(values["RuleDescription"], WebDriver.GetElementById($"Entity_SpecialDays_{index}__Description"));
            AssertListBoxValueSelected(values["RuleDay"], $"Entity_SpecialDays_{index}__CalculatedDateDay", false, false);
            if (values["RuleDay"] == "Calendar Day")
                AssertTextBoxValue(values["RuleDate"], WebDriver.GetElementByName($"Entity.SpecialDays[{index}].Date"));
            AssertListBoxValueSelected(values["RuleType"], $"Entity_SpecialDays_{index}__DayType", false, false);
        }

        private void AssertGroupingSection(int index, IReadOnlyDictionary<string, string> values, string entity)
        {
            var sufix = "";
            var entityName = entity;
            if (entity == "BalanceRowsAF")
            {
                sufix = "AF";
                entityName = "BalanceRows";
            }
            AssertNumericTextBoxValue(values["Order"], WebDriver.GetElementById($"Entity_{entityName}_{index}__Order"));
            AssertListBoxValueSelected(values["Type"], $"Entity_{entityName}_{index}__{sufix}Type", false, false);
        }

        private void AssertProcessesSection(int index, IReadOnlyDictionary<string, string> values, string entity,
            bool editMode = true)
        {
            var editField = WebDriver.GetElementById($"Entity_{entity}_{index}__Prio");
            AssertNumericTextBoxValue(values["Priority"], editField, editMode);
            AssertSingleSelectTextBoxValue(values["ProcessDescription"], $"Entity_{entity}_{index}__Process", editMode);
        }

        private void AssertCorrespondenceSection(int index, IReadOnlyDictionary<string, string> values, string entity = "Correspondences")
        {
            //left
            AssertInMultiselectGrid($"Entity_{entity}_{index}__LeftMap", new[] { values[$"LeftMap{index + 1}"] });
            if (values.ContainsKey($"LeftGroupingType{index + 1}"))
                AssertRadioButtonValue($"Entity_{entity}_{index}__LeftGroupingType",
                    values[$"LeftGroupingType{index + 1}"]);

            //right
            AssertInMultiselectGrid($"Entity_{entity}_{index}__RightMap", new[] { values[$"RightMap{index + 1}"] });
            if (values.ContainsKey($"RightGroupingType{index + 1}"))
                AssertRadioButtonValue($"Entity_{entity}_{index}__RightGroupingType",
                    values[$"RightGroupingType{index + 1}"]);
        }


        public void AssertListView(string entityName, int rowCount, object values, bool editMode = false)
        {
            if (entityName == "AaLevels")
            {
                AssertArrayListView(entityName, rowCount, (string[][])values);
                return;
            }
            var listOfDictionaries = values as List<Dictionary<string, string>>;
            if (listOfDictionaries == null)
            {
                Assert.Fail($"Values for '{entityName}' entity not found!");
            }
            var listCount = listOfDictionaries.Count;
            for (var i = 0; i < listCount; ++i)
            {
                switch (entityName)
                {
                    case "Addresses":
                        AssertAddressesSection(i, listOfDictionaries[i], editMode);
                        break;
                    case "Operations":
                    case "Types":
                        AssertOperationsSection(i, listOfDictionaries[i], editMode);
                        break;
                    case "Files":
                        AssertFilesSection(i, listOfDictionaries[i], editMode);
                        break;
                    case "Letters":
                        AssertLettersSection(i, listOfDictionaries[i], editMode);
                        break;
                    //case "RulesO":
                    case "RulesC":
                    case "RulesG":
                    case "RulesR":
                        AssertRulesSection(i, listOfDictionaries[i], editMode, entityName);
                        break;
                    case "Criterias1":
                    case "Criterias2":
                    case "Filters1":
                    case "Filters2":
                        AssertRecRuleListView(entityName, i, rowCount, listOfDictionaries[i]);
                        break;
                    case "CriteriaDependencies":
                        AssertCriteriaDependencies(i, listOfDictionaries[i]);
                        break;
                    case "IdentifiersGroups":
                        AssertIdentifierSection(i, listOfDictionaries[i]);
                        break;
                    case "RlStepsVM":
                        AssertRollingStepsSection(i, listOfDictionaries[i]);
                        break;
                    case "SpecialDays":
                        AssertSpecialDaysSection(i, listOfDictionaries[listCount - i - 1]);
                        break;
                    case "Groupings":
                    case "AccountsGroupings":
                    case "BalanceRows":
                    case "BalanceRowsAF":
                        AssertGroupingSection(i, listOfDictionaries[i], entityName);
                        break;
                    case "ProcessesO":
                        AssertProcessesSection(i, listOfDictionaries[i], entityName, editMode);
                        break;
                    case "Correspondences1":
                    case "Correspondences2":
                        AssertCorrespondenceSection(i, listOfDictionaries[i], entityName);
                        break;
                    default:
                        Assert.Fail($"Method to verify '{entityName}' section not found!");
                        break;
                }
            }
        }

        public void AssertValuesInKendoMultiSelect(string mainIdPart, string[] values)
        {
            var itemList = WebDriver.GetElementById(mainIdPart + "_KendoMultiSelect").GetElementsByQuery(GetElementBy.Tagname, "div.k-multiselect-wrap.k-floatwrap ul li");
            foreach (var value in values)
            {
                Assert.IsNotNull(itemList.FirstOrDefault(e => e.GetAttrib("innerText").Equals(value)), $"'{value}' was not selected!");
            }
        }

        public void AssertInMultiselectGrid(string entityName, string[] values, GetElementBy getTableBy = GetElementBy.ClassName, string getTableByValue = null,
            bool verifySeveralSameValues = true)
        {
            var passedFlags = new bool[values.Length];
            var divMultiSelectTable = WebDriver.GetElementById($"propertygridbox-{entityName}");
            WebDriver.WaitForReadyState();
            do
            {
                var rows = Page.GetTableBodyRows(WebDriver.GetTable(getTableBy, getTableByValue, divMultiSelectTable));
                foreach (var row in rows)
                {
                    for (var i = 0; i < values.Length; ++i)
                    {
                        if (!verifySeveralSameValues && passedFlags[i]) continue;
                        if (row.GetElementByOneAttribute(values[i], "span") != null)
                            passedFlags[i] = true;
                    }
                }
            } while ((passedFlags.AllTrue() && !verifySeveralSameValues) || Page.Next(divMultiSelectTable));
            if (passedFlags.AllTrue()) return;
            for (var i = 0; i < passedFlags.Length; ++i)
            {
                Assert.IsTrue(passedFlags[i], $"Value '{values[i]}' was not found");
            }
        }


        /// <summary>
        /// Method asserts checkbox value
        /// </summary>
        /// <param name="expectedValue">Expected value - should be 'true' or 'false' value</param>
        /// <param name="checkBox">Checkbox control</param>
        public void AssertCheckboxValue(string expectedValue, IWebElement checkBox)
        {
            // Verify conversion succeeded
            Assert.IsTrue(bool.TryParse(expectedValue, out var parsedValue), $"Convertion from string to bool value failed, value is: '{expectedValue}'");


            Assert.AreEqual(parsedValue, checkBox.Selected,
                $"Checkbox expected value is '{parsedValue}', but actual is '{checkBox.Selected}'.");
        }

        /// <summary>
        /// Method asserts Extender status (checked or unchecked)
        /// </summary>
        /// <param name="entityName">Entity name, for ex. 'Company'.</param>
        /// <param name="moduleName">Module name, for ex. 'Bank Delegations' or 'Reconciliation'</param>
        /// <param name="expectedCheckBoxValue">Expected checkbox value - 'true' for checked or 'false' - for unchecked</param>
        public void AssertExtenderStatus(string entityName, string moduleName, string expectedCheckBoxValue)
        {
            // Get id
            var labelText = $"Use this {entityName} in {moduleName}";
            var label = Page.GetLabelByText(labelText);
            Assert.IsNotNull(label, $"Label with text '{labelText}' was not found.");
            var id = label.GetAttribute("for");

            // Get Extender checkbox
            var checkbox = WebDriver.GetElementById(id);
            AssertCheckboxValue(expectedCheckBoxValue, checkbox);
        }


        public void AssertTableCellText(int row, int col, string expectedText, IWebElement table)
        {
            var cell = Page.GetTableCell(row, col, table);
            Assert.IsNotNull(cell, $"Cell was not found in table row {row}, column {col}, expected text to find is '{expectedText}'");
            var actualText = cell.Text;

            Assert.AreEqual(expectedText, actualText,
                $"Expected text is: {expectedText}, but actual text in cell is: {actualText}");
        }

        public void AssertRecRuleListView(string entityName, int index, int maxNumberOfListViews, Dictionary<string, string> values)
        {
            string bankOrGlStmtId;
            string leftOrRight;
            var ruleType = WebDriver.GetElementById("Entity_RuleType").GetAttribute("value");
            var isFilter = entityName.Contains("Filter");
            if (entityName == "Criterias1" || entityName == "Filters1")
            {
                bankOrGlStmtId = ruleType.Contains("Ent2") ? "Ent2" : "Ent1";
                leftOrRight = "Left";
            }
            else
            {
                bankOrGlStmtId = ruleType.Contains("Ent1") ? "Ent1" : "Ent2";
                leftOrRight = "Right";
            }

            // Data prefix written in data file
            var dataPrefix = isFilter ? "Filter" + leftOrRight : leftOrRight;

            // This is because criterias are not in the way they were created - start search from the first one
            index = 0;

            // Find list view index by Name field for Criteria section or by Property path for Filter section
            var findIndex = true;
            do
            {
                string expected;
                string actual;
                if (!isFilter)
                {
                    var examElement = WebDriver.GetElementById($"Entity_{entityName}_{index}__Name");
                    //examElement.SetFocus();
                    // This is also verification of text value selected in Name
                    actual = examElement.GetAttribute("value").Trim();
                    expected = values[dataPrefix + "Name"];
                }
                else
                {
                    var examElement = WebDriver.GetElementById($"Entity_{entityName}_{index}__{bankOrGlStmtId}PropertyPath");
                    //examElement.SetFocus();
                    // This is also verification of text value selected in Property path
                    actual = examElement.GetParent().GetElementByClassName("property-path-name").Text.Trim();
                    expected = values[dataPrefix + "Description"];
                }

                if (expected != actual)
                {
                    ++index;
                }
                else
                {
                    findIndex = false;
                }
            } while (findIndex && index < maxNumberOfListViews);

            // If Property text was not found and thus findIndex is true - test fails
            if (findIndex)
            {
                Assert.Fail($"Property field in list view of {entityName} section with text {values[dataPrefix + (isFilter ? "Description" : "Name")]} was not found.");
            }

            if (!isFilter)
            {
                // Verify PropertyPath
                var textPropertyPath = WebDriver
                    .GetElementById($"Entity_{entityName}_{index}__{bankOrGlStmtId}PropertyPath").GetParent()
                    .GetElementByClassName("property-path-name").Text.Trim();
                Assert.AreEqual(values[dataPrefix + "Description"], textPropertyPath);
            }

            // If use F()
            var f = values.ContainsKey(dataPrefix + "F") ? values[dataPrefix + "F"] : null;
            if (f != null && bool.Parse(f))
            {
                var funcPrefix = $"Entity_{entityName}_{index}__Func_";
                // Switch F toggle
                var toggleF = WebDriver.GetElementByXpath(
                    $"//label[@for='{funcPrefix}Code']/following-sibling::div");
                if (toggleF == null)
                    Assert.Fail($"Toggle control '{entityName}_{index}' not found.");
                try
                {
                    AssertToggleValue(values[dataPrefix + "F"], toggleF);
                }
                catch
                {
                    toggleF.ScrollIntoView();
                    throw;
                }
                // Verify function
                AssertListBoxValueSelected(values[dataPrefix + "FType"], $"{funcPrefix}Code", withInput: false);

                var criteriaType = values[dataPrefix + "CriteriaType"].ToLower();
                var funcType = values[dataPrefix + "FType"].ToLower();
                var substrPrefix = funcPrefix + funcPrefix;
                switch (criteriaType)
                {
                    case "string":
                        if (funcType == "substring") // substring
                        {
                            // Verify Start position
                            var startPositionEdit = WebDriver.GetElementById($"{substrPrefix}Params_0__Value_Int32");
                            AssertNumericTextBoxValue(values[dataPrefix + "StartPosition"], startPositionEdit);

                            // Verify Length
                            var lengthEdit = WebDriver.GetElementById($"{substrPrefix}Params_1__Value_Int32_1");
                            AssertNumericTextBoxValue(values[dataPrefix + "Length"], lengthEdit);
                        }
                        else // trim
                        {
                            // Verify triming symbol
                            var edit = WebDriver.GetElementById($"{funcPrefix}Params_0__Value_Char");
                            AssertTextBoxValue(values[dataPrefix + "TrimingSymbol"], edit);

                            // Verify Option
                            AssertListBoxValueSelected(values[dataPrefix + "Option"], $"{funcPrefix}Params_1__Value_StrTrimOption", withInput: false);
                        }
                        break;
                    case "number":
                        if (funcType == "round")
                        {
                            var vInd = entityName.Contains("Filters") ? "3" : "2";
                            // Verify Length
                            var lengthEdit = WebDriver.GetElementById($"{substrPrefix}Params_0__Value_Int32_{vInd}");
                            AssertNumericTextBoxValue(values[dataPrefix + "Length"], lengthEdit, false);
                        }
                        break;
                    case "date":
                        // Verify Date part
                        AssertListBoxValueSelected(values[dataPrefix + "DatePart"],
                            $"{funcPrefix}Params_0__Value_Date{(funcType == "datediff" ? "Diff" : "")}PartOption",
                            withInput: false);

                        if (funcType == "datediff") // datediff
                        {
                            // Verify End Date
                            var calendarEdit = WebDriver.GetElementByName($"Entity.{entityName}[{index}].Func.Params[1].Value");
                            AssertDateValue(values[dataPrefix + "EndDate"], calendarEdit);
                        }
                        break;
                    default:
                        Assert.Fail($"Getting criterias data: wrong criteria type - '{criteriaType}'");
                        break;
                }
            }

            if (isFilter)
            {
                // Verify Operator
                AssertListBoxValueSelected(values[dataPrefix + "Operator"], $"Entity_{entityName}_{index}__Operator", withInput: false);

                // if use criterion - verify it
                var isUserCriterion = values[dataPrefix + "UseCriterion"];
                if (isUserCriterion != null && bool.Parse(isUserCriterion))
                {
                    AssertCheckboxValue(values[dataPrefix + "UseCriterion"], WebDriver.GetElementById($"Entity_{entityName}_{index}__Criterion_Code_CheckBox"));
                    AssertListBoxValueSelected(values[dataPrefix + "FltrValue"], $"Entity_{entityName}_{index}__Criterion", inputId: "Code");
                }
                else
                {
                    var criteriaType = values[dataPrefix + "CriteriaType"].ToLower();
                    switch (criteriaType)
                    {
                        case "string":
                            AssertTextBoxValue(values[dataPrefix + "FltrValue"], WebDriver.GetElementById($"Entity_{entityName}_{index}__RightHandValue_String"));
                            break;
                        case "number":
                            var numEdit = WebDriver.GetElementById($"Entity_{entityName}_{index}__RightHandValue_Decimal");
                            AssertNumericTextBoxValue(values[dataPrefix + "FltrValue"], numEdit);
                            break;
                        case "date":
                            var calendarEdit = WebDriver.GetElementByName($"Entity.{entityName}[{index}].RightHandValue");
                            AssertDateValue(values[dataPrefix + "FltrValue"], calendarEdit);
                            break;
                    }
                }
            }
        }

        public void AssertEntityPropertiesTreeValue(string id, string expectedText)
        {
            var parentDiv = WebDriver.GetElementById(id).GetParent();
            var actualText = parentDiv.GetElementByClassName("property-path-name").Text;

            Assert.AreEqual(expectedText.ToLower(), actualText.ToLower(),
                $"Properties tree text verification - Expected text is: '{expectedText}', but actual text in cell is '{actualText}'");
        }

        public void AssertToggleControlValue(string sValue, string sEntityName)
        {
            if (string.IsNullOrEmpty(sValue))
                return;
            var toggle = WebDriver.GetElementById(sEntityName).GetParent();
            if (toggle == null)
                Assert.Fail($"Toggle control with id='{sEntityName}' not found.");
            //toggle.SetFocus();
            AssertToggleValue(sValue, toggle);
        }

        public void AssertToggleValue(string sValue, IWebElement toggleControl)
        {
            bool? value = null;
            try
            {
                switch (sValue.ToLower())
                {
                    case "yes":
                    case "true":
                        value = true;
                        break;
                    case "no":
                    case "false":
                        value = false;
                        break;
                    default:
                        Assert.Fail(
                            $"Wrong value for verification of toggle control - '{sValue}' should be 'true' or 'false'");
                        break;
                }

                if ((bool)value && toggleControl.GetAttribute("class").Contains("off"))
                {
                    toggleControl.ScrollIntoView();
                    Assert.Fail("Toggle control is switched off, but should be switched on.");
                }

                if (!(bool)value && toggleControl.GetAttribute("class").Contains("primary"))
                {
                    toggleControl.ScrollIntoView();
                    Assert.Fail("Toggle control is switched on, but should be switched off.");
                }

            }
            finally
            {
            }
        }

        public void AssertCriteriaDependencies(int index, Dictionary<string, string> values)
        {
            AssertListBoxValueSelected(values["DependencyLeft"], $"Entity_CriteriaDependencies_{index}__Dependency1", inputId: "Code");
            AssertListBoxValueSelected(values["Operation"], $"Entity_CriteriaDependencies_{index}__DependencyOperation", withInput: false);
            AssertListBoxValueSelected(values["DependencyRight"], $"Entity_CriteriaDependencies_{index}__Dependency2", inputId: "Code");
            AssertToggleControlValue(values["Optional"], $"Entity_CriteriaDependencies_{index}__IsOptional");
        }

        /// <summary>
        /// Assertion of value selected in "select" html control
        /// </summary>
        public void AssertSelectControlValue(string id, string valueOfOptionSelected)
        {
            var selectControl = WebDriver.GetElementById(id);
            Assert.IsNotNull(selectControl, "Select HTML control was not found by id" + id);
            var actualValue = selectControl.GetElementByAttributeWithValue("selected=selected").GetAttribute("innerHTML");
            Assert.AreEqual(valueOfOptionSelected.ToLower(), actualValue.ToLower(),
               $"Select HTML control option value selected - Expected value is: '{valueOfOptionSelected}', but actual is '{actualValue}'");
        }

        public void AssertFilterExists(string filterName = "NewFilterName")
        {
            Assert.IsTrue(Page.CheckFilterOrLayoutExists(filterName, ConfigurationType.Filter));
        }

        public void AssertLayoutExists(string filterName = "NewViewName")
        {
            Assert.IsTrue(Page.CheckFilterOrLayoutExists(filterName, ConfigurationType.Layout));
        }

        public void AssertFilterNotExists(string filterName = "NewFilterName")
        {
            Assert.IsFalse(Page.CheckFilterOrLayoutExists(filterName, ConfigurationType.Filter));
        }

        public void AssertLayoutNotExists(string filterName = "NewViewName")
        {
            Assert.IsFalse(Page.CheckFilterOrLayoutExists(filterName, ConfigurationType.Layout));
        }

        public void AssertCardElementInRightMode(CardItem element, bool viewMode)
        {
            string examinatedClass;
            IWebElement control;
            switch (element.FieldType)
            {
                case FieldType.Text:
                    control = WebDriver.GetElementById(element.NameOrId);
                    examinatedClass = control.GetAttrib("class");
                    break;
                case FieldType.Single:
                    control = WebDriver.GetElementById($"EntityPicker_wrapper_{element.NameOrId}").GetParent();
                    examinatedClass = control.GetAttrib("class");
                    break;
                default:
                    throw new NotImplementedException();
            }

            Assert.IsTrue(examinatedClass.Contains("disabled") == viewMode,
                $"Card is not in {(viewMode ? "view" : "edit")} mode");
        }

        public void AssertTableExistsAndVisible(string entityName)
        {
            var divGrid = WebDriver.GetElementById("Entity_" + entityName + "_table");
            var table = WebDriver.GetElementByClassName("k-grid-content", divGrid).GetElementByTagName("table");
            Assert.IsTrue(table != null && table.Enabled);
        }

    }
}
