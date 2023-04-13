using BaseDriver;
using OpenQA.Selenium;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using OpenQA.Selenium.Support.UI;
using TestTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Sockets;

namespace BasePage
{
    public class BasePageElementMap
    {
        public IWebDriver WebDriver;
        public WebDriverWait DriverWait;
        public string BrowserName;
        public string Description;

        public string CurrentUser;
        public string CurrentPass;

        public Driver MainDriver;

        public string AppUrl;
        public string EntityUrl;
        public string EntityMenuPoint;

        public string currentLanguage = "en-US";

        public BasePageElementMap()
        {
            WebDriver = Driver.Instance.Browser;
            DriverWait = Driver.Instance.BrowserWait;
            BrowserName = Driver.Instance.BrowserName;
        }

        public BasePageElementMap(Driver mainDriver)
        {
            MainDriver = mainDriver;
            WebDriver = MainDriver.InstDriver.Browser;
            DriverWait = MainDriver.InstDriver.BrowserWait;
            BrowserName = MainDriver.InstDriver.BrowserName;
        }

        public void SignInLinkReady() => WebDriver.WaitForReadyStateByLink(new[] { "Change or Reset Password" , "Modifier ou Réinitialiser Mot de Passe" });
        public void SignInFormReady() => DriverWait.Until(d => WebDriver.GetElementByTagName($"form.form-vertical[action='{ConfigSettingsReader.Tenant}useraccount/logon']") != null || WebDriver.GetElementByTagName($"form.form-vertical[action='{ConfigSettingsReader.Tenant}']") != null);

        public bool ElmahAdded
        {
            get => Driver.IsElmahAdded;
            set => Driver.ElmahAdded(value);
        }

        // ReSharper disable once InconsistentNaming
        public static CultureInfo GetCurrentCultureInfo => CultureInfo.GetCultureInfo(ConfigSettingsReader.RegionalSettings.GetRegionalName());

        public IWebElement NextGridLink => WebDriver.GetElementByOneAttribute("Go to the next page");

        public IWebElement FirstGridLink => WebDriver.GetElementByOneAttribute("Go to the first page");

        public IWebElement MainEntityTableDiv => WebDriver.GetElementByClassName("gridArea");//gridArea is allways div

        public IWebElement MainEntityTable => WebDriver.GetElementByClassName("k-selectable");//k-selectable item is allways table

        public IWebElement CreateButton => GetButtonLinkByTitle("Create".Localize(currentLanguage));

        public IWebElement GetButtonLinkByTitle(string title) => WebDriver.GetElementByOneAttribute(title);

        public IWebElement EditButton => WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a[title='{"Edit".Localize(currentLanguage)}'][role='menuitem']");

        public IWebElement DuplicateButton => WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a[title='{"Duplicate".Localize(currentLanguage)}'][role='menuitem']");

        public IWebElement DeleteButton(string actionType = "delete") => WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a[action-type={actionType}]");//\").find(it=>getComputedStyle(it).display!=\"none");

        public IWebElement ViewButton => WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a[title='{"View".Localize(currentLanguage)}'][role='menuitem']");

        public IWebElement AuditCheckedButton => WebDriver.GetElementByQuery(GetElementBy.Tagname, "a.action-selection-multiple[title='Audit checked']");

        public IWebElement AuditDeletedButton => WebDriver.GetElementByQuery(GetElementBy.Tagname, "a.action-selection-none[title='Audit deleted']");

        public IWebElement SaveAndEditButton => WebDriver.GetElementByOneAttribute("Apply".Localize(currentLanguage), "input", "value");

        public IWebElement SaveButton => WebDriver.GetElementByQuery(GetElementBy.Tagname, $"input[value='{"Save".Localize(currentLanguage)}']");

        public void AddElmahDetail(string additional = "", bool elmah = false)
        {
            return; // for debug
        }

        public void ClickSaveButtonWithjQ(bool emptyform = false, bool checkAfter = true)
        {
            try
            {
                if (DriverWait.Until(d => WebDriver.IsAvailableElementByjQ($"input[value='{"Save".Localize(currentLanguage)}']")))
                {
                    //WebDriver.WaitForjQueryReady();
                    WebDriver.Execute($"document.querySelector('input[value=\"{"Save".Localize(currentLanguage)}\"]').click();");
                    WebDriver.Pause();
                }
                else
                    Assert.Fail("Card is not fully loaded");
                if (checkAfter)
                    WebDriver.WaitForReadyState(emptyform);
                else
                    WebDriver.WaitForReadyState(emptyform, false);
                WebDriver.Wait4Ajax();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                throw;
            }
        }

        public IWebElement CancelButton => WebDriver.GetElementByClassName("card-cancel-btn");

        public IWebElement ReturnButton => WebDriver.GetElementByClassName("btn.btn-default");
        public IWebElement AlertReturnButton => WebDriver.GetElementByTextWithTag("Return", "a.btn.btn-secondary");

        public IWebElement EditCode => WebDriver.GetElementById("Entity_Code");

        public IWebElement ChangeByBatch => WebDriver.GetElementByOneAttribute("Change by batch");

        public IWebElement EditGridIcon(IWebElement container) => container.GetElementByQuery(GetElementBy.Tagname, $"span[title='{"Edit".Localize(currentLanguage)}']");

        public IWebElement DeleteGridIcon(IWebElement container)
        {
            SetLanguage(currentLanguage.Substring(0,2));
            return container.GetElementByQuery(GetElementBy.Tagname, $"span[title='{"Delete".Localize(currentLanguage)}']");
        }

        public IWebElement ViewGridIcon(IWebElement container)
        {
            SetLanguage(currentLanguage.Substring(0, 2));
            return container.GetElementByQuery(GetElementBy.Tagname, $"span[title='{"View".Localize(currentLanguage)}']");
        }

        public IWebElement ShowCommentsGridIcon(IWebElement container) => container.GetElementByQuery(GetElementBy.Tagname, "span[class='comment-link']");

        public IWebElement CheckBoxGridItem(IWebElement container) => container.GetElementByQuery(GetElementBy.Tagname, "input[type='checkbox']");

        public IWebElement GetCommentsPopUpDialogAndWaitForItsVisibility
            => DriverWait.WaitForElementIsVisible(GetCommentsPopUpDialog) ? GetCommentsPopUpDialog : null;

        public IWebElement GetCommentsPopUpDialog => WebDriver.GetElementByClassName("comments-slider");

        public IWebElement DeleteAllButton => WebDriver.GetElementByQuery(GetElementBy.Tagname, "a[title='Delete All'][role=menuitem]");

        public void ActionList()
        {
            var action = WebDriver.GetElementsByText(null, GetElementBy.Tagname, "button", "Actions");
            if (action != null && action.Count > 0)
                action[0].Click2();
        }

        public IWebElement GetLabelByText(string sValue) => WebDriver.GetElementByTextWithTag(sValue, "label");

        public ReadOnlyCollection<IWebElement> GetErrorMessageItem(string message, bool strong = true) => WebDriver.GetElementsByTextWithTag(message, "li", strong);

        public ReadOnlyCollection<IWebElement> GetErrorMessage(string message, string tag = "span", bool strong = true) => WebDriver.GetElementsByTextWithTag(message, string.IsNullOrEmpty(tag) ? "span" : tag, strong);

        #region Methods use javascript

        public int GetColumnSortNumber(string columnName, IWebElement table)
        {
            var colIndex = (string)WebDriver.Execute(@"
                let headerCells = arguments[0].rows[0].cells;
                for (let i = 0; i < headerCells.length; i++) {
                  let a = headerCells[i].getElementsByClassName('k-column-title') [0];
                  if (a != null && a.innerHTML.indexOf(arguments[1]) >= 0) {
                    let st = a.innerHTML;
                    if(st.indexOf('<abbr') == 0) {
                        let sp = st.split('>');
                        st = sp[sp.length - 1];
                    }
                    if(st != arguments[1]) continue;
                    return a.querySelector('sup').innerText;
                  }
                } return '';", table, columnName);
            return string.IsNullOrEmpty(colIndex) ? 0 : Convert.ToInt32(colIndex);
        }


        /// <summary>
        /// Get column number by column name.
        /// Attention! For simple Entity names only!
        /// </summary>
        /// <param name="columnName">Columns name</param>
        /// <param name="table">Table web element</param>
        /// <returns>Returns column number else -1</returns>
        public int GetColumnNumber(string columnName, IWebElement table)
        {
            var colNumber = (long)WebDriver.Execute(
                @"
                let headerCells = arguments[0].rows[0].cells;
                for (let i = 0; i < headerCells.length; i++) {
                  let a = headerCells[i].getElementsByClassName('k-link') [0];
                  if (a == null) a = headerCells[i];
                  if (a != null && a.innerHTML.indexOf(arguments[1]) >= 0) {
                    let st = a.querySelector('span')?.innerHTML;
                    if(st == null) st = a.innerHTML;
                    if(st.indexOf('<abbr') == 0){
                        let sp = st.split('>');
                        let ci = sp[sp.length - 1];
                        st = ci.replace(/['""]+/,'');
                    }
                    if(st != arguments[1]) continue;
                    return i;
                  }
                } return -1;", table, columnName);
            return Convert.ToInt32(colNumber.ToString());
        }

        public void ExportToExcel()
        {
            var container = WebDriver.GetElementByQuery(GetElementBy.Tagname, "a[action-type=getgridexcel]");//GetElementByXpath("//a[@action-type='getgridexcel']");
            container.Click2();
        }

        public int GetRowNumberByColumnName(string columnName, string value, IWebElement table, bool islocked = false)
        {
            var script = $@"
            let headerCells = {(islocked ? "document.querySelector('div.k-grid-header-locked table')" : "arguments[0].querySelector('.k-grid-header')")};
            if(headerCells.rows) 
                headerCells = headerCells.rows[0].cells;
            else
                return -1;
            //    headerCells = headerCells[1].rows[0].cells;
            let a, col;
            let prefix = {(islocked ? "'-locked'" : "''")};
            while(true){{
                for (let i = 0; i < headerCells.length; i++) {{
                    a = headerCells[i].getElementsByClassName('k-link') [0];
                    if (a != null && a.innerHTML.indexOf(arguments[1]) >= 0) {{
                        let st = a.innerHTML;
                        if(st.indexOf('<span') == 0){{
                            let sp = st.split('>');
                            let ci = 0;
                            for(let j = 0; j < sp.length; ++j){{
                                if(sp[j].indexOf(arguments[1]) < 0) continue;
                                ci = j
                                break
                            }}
                            st = sp[ci].split('<')[0].replace(/['""]+/,'');
                        }}
                        if(st.indexOf(arguments[1]) != 0) continue;
                        col = i;
                        break;
                    }}
                }}
                if(col>-1) break;
                if(prefix === '') return -1;
                prefix = '';
                headerCells = document.querySelector('div.k-grid-header-wrap table')[0].rows[0].cells;
            }}
            let tbodyRows = {(islocked ? "document.querySelector('div.k-grid-content'+prefix+' table')" : "arguments[0]")}.tBodies[0].rows;
            for (let i = 0; i < tbodyRows.length; i++) {{
                if (tbodyRows[i].cells.length == 1) continue;
                if (tbodyRows[i].getAttribute('class') === 'k-grouping-row') continue;
                if (getComputedStyle(tbodyRows[i]).display != 'none' && tbodyRows[i].cells[col] != null ) {{
                    let spancell = tbodyRows[i].cells[col].querySelector('span');
                    let inrtxt = spancell == null ? tbodyRows[i].cells[col].innerText : spancell.title;
                    if (inrtxt == arguments[2]) {{
                        return i;
                    }}
                }} 
                else
                {{
                    if (getComputedStyle(tbodyRows[i]).display != 'none' && tbodyRows[i].cells[col] != null && tbodyRows[i].cells[col].title == arguments[2]) {{
                        return i; 
                    }}
                }}
            }} 
            return -1; ";
            var cellNumber = (long)WebDriver.Execute(script, table, columnName, value);
            return Convert.ToInt32(cellNumber.ToString());
        }


        public IWebElement GetTableHeaderCell(string columnName, IWebElement table)
        {
            var cell = (IWebElement)WebDriver.Execute(
                @"
            let headerCells = arguments[0].rows[0].cells;
            for (let i = 0; i < headerCells.length; i++) {
              let a = headerCells[i].getElementsByClassName('k-link') [0];
              if (a != null && String(a.innerHTML).indexOf(arguments[1]) >= 0) {
                return headerCells[i];
              }
            }
            return null;
            ", table, columnName);
            return cell;
        }

        public int GetRowNumberBySeveralValues(string[] columnNames, string[] values, IWebElement table)
        {
            var cellNumber = (long)WebDriver.Execute(@"
            let tbodyRows = arguments[0].tBodies[0].rows;
            for (let i = 0; i < tbodyRows.length; i++){
                if(tbodyRows[i].getAttribute('class') === 'k-grouping-row') continue;
                if(tbodyRows[i].getAttribute('class') === 'k-no-data') continue;
                let ok = 0;
                for (let j = 0; j < arguments[1].length; j++)
                {
                    let ind = arguments[1][j];
                    let text = arguments[2][j];
                    if(tbodyRows[i].cells[ind].getElementsByTagName('span')[0].innerText !== text) break;
                    ok++;
                }
                if (ok == arguments[1].length) return i;
            } return -1; ", table, columnNames.Select(t => GetColumnNumber(t, table)).ToArray(), values);
            return Convert.ToInt32(cellNumber.ToString());
        }

        public string GetTableCellText(int irow, string columnName, IWebElement table)
        {
            var resultsTable = table.GetElementByTagName("tbody");
            var rows = resultsTable.GetElementsByTagName("tr", true);
            if (rows == null)
                return null;
            var colunms = rows[irow].GetElementsByTagName("td");
            // Assert that Task status is Finished in the row
            return colunms[GetColumnNumber(columnName, table)].Text;
        }

        public IWebElement GetTableCell(string columnName, string value, IWebElement table = null)
        {
            // ReSharper disable once InconsistentNaming
            var _table = table ?? WebDriver.GetTable();
            var col = GetColumnNumber(columnName, _table);
            var row = GetRowNumberByColumnName(columnName, value, _table);
            if (col == -1 || row == -1)
            {
                return null;
            }

            var cell = GetTableCell(row, col, _table);
            return cell;
        }

        public IWebElement GetTableCell(int row, int col, IWebElement table)
        {
            if (col == -1 || row == -1)
            {
                return null;
            }
            var cell = (IWebElement)WebDriver.Execute(
                "return arguments[0].tBodies[0].rows[arguments[1]].cells[arguments[2]];", table, row, col
                );
            return cell;
        }

        public ReadOnlyCollection<IWebElement> GetTableColumnCells(int col, IWebElement table)
        {
            if (col == -1)
            {
                return null;
            }
            var cells = (ReadOnlyCollection<IWebElement>)WebDriver.Execute(
@"
let rows = arguments[0].tBodies[0].rows;
let array = [];
for(let i = 0; i < rows.length; i++)
{
  array.push(rows[i].cells[arguments[1]]);
}
return (array.length==0)?null:array;
", table, col);
            return cells;
        }

        public IWebElement GetTableRow(int rowNumber, IWebElement table)
        {
            var cell = (IWebElement)WebDriver.Execute(
                "return arguments[0].tBodies[0].rows[arguments[1]];", table, rowNumber);
            return cell;
        }

        public ReadOnlyCollection<IWebElement> GetTableBodyRows(IWebElement table, int tindex = 0)
        {
            var script = (table == null)
                ? $"let table = document.getElementsByClassName('k-selectable')[{tindex}];"
                : "let table = arguments[0];";
            script += $@"
let rows = (table == null) ? null : table.tBodies[0].rows; 
if(rows != null && rows.length == 0) rows = null; 
return rows;";

            var rows = (ReadOnlyCollection<IWebElement>)((table == null) ? WebDriver.Execute(script) : WebDriver.Execute(script, table));
            return rows ?? new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
        }

        /// <summary>
        /// Find row element in table and return its line number
        /// </summary>
        /// <param name="tableParent">Tables parent element</param>
        /// <param name="value">Looking value</param>
        /// <param name="getTableBy">How to get table</param>
        /// <param name="getTableByValue">Value by which to find the table</param>
        /// <param name="startRow">Start line to search or -1</param>
        /// <returns>Returns found element line number or -1</returns>
        public int FindRow(IWebElement tableParent, string value, GetElementBy getTableBy, string getTableByValue = null, int startRow = -1)
        {
            var returnRow = startRow;
            do
            {
                WebDriver.WaitForReadyState();
                var currentTable = WebDriver.GetTable(getTableBy, getTableByValue, tableParent);
                var rows = GetTableBodyRows(currentTable);
                for (var i = (returnRow < 0) ? 0 : returnRow; i < rows.Count; ++i)
                {
                    if (rows[i].GetElementByOneAttribute(value, "span") == null) continue;
                    return i;
                }
                returnRow = -1;
            } while ((returnRow == -1) && Next(tableParent));
            First(tableParent);
            return returnRow;
        }

        public void FillDetailTableLine(string tableName, Dictionary<string, string> values, DetailTableType tableType)
        {
            var line = "tr";
            var block1 = "";
            string block2;
            string block3;
            var block4 = "";
            var block5 = "";
            var gridview = false;
            switch (tableType)
            {
                case DetailTableType.Identifiers:
                case DetailTableType.Phones:
                    block1 = $@"
            let cell1 = cols[0].querySelector('input');
            let main = cols[0].querySelector('.selected');
            if({((values["Main"].ToLower() == "true") ? "main==null || !main" : "main!=null && main")}) cell1.click();
            cols = line.getElementsByTagName('td');";
                    block2 = $@"
            let drpdwn = cell2.querySelector('.k-input') ?? cell2.querySelector('.k-input-value-text')
            drpdwn.click();
            let it = Array.from(document.querySelectorAll('li')).find(it=>it.innerText.includes('{values["Type"]}'));
            it.click();";
                    block3 = $@"
            let txt = cell3.querySelector('input.k-input-inner');
            $(txt).val(""{values["Text"]}"").trigger('change');
            cell2.click();";
                    break;
                case DetailTableType.NetAddresses:
                    block1 = $@"
            let cell1 = cols[0].querySelector('input');
            let main = cols[0].querySelector('.selected');
            if({(values["Main"].ToLower() == "true" ? "!main" : "main")}) cell1.click();
            cols = line.getElementsByTagName('td');";
                    block2 = $@"
            let drpdwn = cell2.querySelector('.k-input') ?? cell2.querySelector('.k-input-value-text')
            drpdwn.click();
            let it = Array.from(document.querySelectorAll('li')).find(it=>it.innerText.includes('{values["Type"]}'));
            it.click();";
                    block3 = $@"
            let txt = cell3.querySelector('input.k-input-inner');
            $(txt).val(""{values["Text"]}"").trigger('change');
            cell2.click();";
                    block4 = $@"
            cols = line.getElementsByTagName('td');
            let cell4 = cols[3];
            cell4.click();
            let txt2 = cell4.querySelector('input.k-input-inner');
            $(txt2).val(""{values["Comment"]}"").trigger('change');
            cell2.click();";
                    break;
                case DetailTableType.Amount:
                    gridview = true;
                    line = ".k-grid-edit-row";
                    //Name('Rate')[0]
                    block2 = $@"
            let number1 = cell2.getElementsByClassName('k-input-inner')[1] ?? cell2.querySelector('.k-input-value-text')[1];
            $(number1).val(""{values["Rate"].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings)}"").trigger('focusout');";
                    //Name('Ceiling')[0]
                    block3 = $@"
            let number2 = cell3.getElementsByClassName('k-input-inner')[1] ?? cell3.querySelector('.k-input-value-text')[1];
            $(number2).val(""{(values["Ceiling"].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings) ?? "")}"").trigger('focusout');";
                    block5 = @"
            let accept = line.querySelector('.grid-view-accept');
            accept.click();";
                    break;
                case DetailTableType.Steps:
                    gridview = true;
                    line = ".k-grid-edit-row";
                    //Name('Rate')[0]
                    block2 = $@"
            let number1 = cell2.getElementsByClassName('k-input-inner')[1] ?? cell2.querySelector('.k-input-value-text')[1];
            $(number1).val(""{values["FixedAmmount"].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings)}"").trigger('focusout');";
                    //Name('Ceiling')[0]
                    block3 = $@"
            let number2 = cell3.getElementsByClassName('k-input-inner')[1] ?? cell3.querySelector('.k-input-value-text')[1];
            $(number2).val(""{(values["MarkupPercentage"].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings) ?? "")}"").trigger('focusout');";
                    if (values["RateIndexPart"] != null)
                    {
                        block3 += $@"
            cols = line.getElementsByTagName('td');
            let cell4 = cols[6];
            cell4.click();
            let number3 = cell4.getElementsByClassName('k-input-inner')[1] ?? cell4.querySelector('.k-input-value-text')[1];
            $(number3).val(""{(values["RateIndexPart"].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings) ?? "")}"").trigger('focusout');";
                    }
                    if (values["Ceiling"] != null)
                    {
                        block3 += $@"cols = line.getElementsByTagName('td');
            let cell5 = cols[7];
            cell5.click();
            let number4 = cell5.getElementsByClassName('k-input-inner')[1] ?? cell5.querySelector('.k-input-value-text')[1];
            $(number4).val(""{(values["Ceiling"].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings) ?? "")}"").trigger('focusout'); ";
                    }
                    block5 = @"
            let accept = line.querySelector('.grid-view-accept');
            accept.click();";
                    break;
                case DetailTableType.FixedCost:
                case DetailTableType.UnitCost:
                    gridview = true;
                    line = ".k-grid-edit-row";
                    //Name('Value')[0]
                    block2 = $@"
            let number1 = cell2.getElementsByClassName('k-input-inner')[1] ?? cell2.querySelector('.k-input-value-text')[1];
            $(number1).val(""{values["StepFee"].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings)}"").trigger('focusout');";
                    //Name('Ceiling')[0]
                    block3 = $@"
            let number2 = cell3.getElementsByClassName('k-input-inner')[1] ?? cell3.querySelector('.k-input-value-text')[1];
            $(number2).val(""{values["Ceiling"].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings)}"").trigger('focusout');";
                    block5 = @"
            let accept = line.querySelector('.grid-view-accept');
            accept.click();";
                    break;
                case DetailTableType.CalculationType:
                    line = ".k-grid-edit-row";
                    //Name('Rate')[0]
                    block2 = $@"
            let number1 = cell2.getElementsByClassName('k-input-inner')[1] ?? cell2.querySelector('.k-input-value-text')[1];
            $(number1).val(""{values["Rate"].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings)}"").trigger('focusout');";
                    //Name('Percentage')[0]
                    block3 = $@"
            let number2 = cell3.getElementsByClassName('k-input-inner')[1] ?? cell3.querySelector('.k-input-value-text')[1];
            $(number2).val(""{values["Percentage"].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings)}"").trigger('focusout');";
                    //Name('Ceiling')[0]
                    block4 = $@"
            cols = line.getElementsByTagName('td');
            let cell4 = cols[3];
            cell4.click();
            let number3 = cell4.getElementsByClassName('k-input-inner')[1] ?? cell4.querySelector('.k-input-value-text')[1];
            $(number3).val(""{values["Ceiling"].ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings)}"").trigger('focusout');";
                    block5 = @"
            let accept = line.querySelector('.grid-view-accept');
            accept.click();";
                    break;
                case DetailTableType.ZBASettings:
                    block2 = $@"
            let drpdwn = cell2.querySelector('.k-input-inner') ?? cell2.querySelector('.k-input-value-text')
            drpdwn.click();
            let it = Array.from(document.querySelectorAll('li')).find(it=>it.innerText.includes('{values["Counterparty"]}'));
            it.click();";
                    block3 = $@"
            let txt = cell3.querySelector('input.k-input-inner');
            $(txt).val(""{values["Text"]}"").trigger('change');
            cell2.click();";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tableType), tableType, null);
            }

            WebDriver.WaitForReadyState();
            WebDriver.WaitForjQueryReady();
            //allways first line!
            var script = $@"
            let elem1 = document.getElementById('{tableName}_table');
            let elem2 = elem1.querySelector('.k-grid-content');
            let line = elem2.querySelector('{line}');
            let cols = line.getElementsByTagName('td');
            {block1}
            let cell2 = cols[{(gridview ? "2" : "1")}];
            cell2.click();
            {block2}
            cols = line.getElementsByTagName('td');
            let cell3 = cols[{(gridview ? "3" : "2")}];
            cell3.click();
            {block3}";
            if (block4 != "")
                script += block4;
            if (block5 != "")
                script += block5;
            WebDriver.Execute(script);
        }

        #endregion

        public bool PagerClick(string title, IWebElement defbutton, IWebElement section = null, bool isclick = true)
        {
            WebDriver.WaitForReadyState();
            var pagerGridLink = (section == null) ? defbutton : section.GetElementByOneAttribute(title);
            if (pagerGridLink == null || pagerGridLink.GetAttribute("aria-disabled").Equals("true"))
                return false;
            if (isclick) pagerGridLink.Click2(focusElement: false);

            return true;
        }

        /// <summary>
        /// Goto next page by pager
        /// </summary>
        /// <param name="section">Pager container (if it is present, otherwise null)</param>
        /// <param name="clickNext">Neends to click Next or no (by default is 'true')</param>
        /// <returns></returns>
        public bool Next(IWebElement section = null, bool clickNext = true)
        {
            return PagerClick("Go to the next page", NextGridLink, section, clickNext);
        }

        /// <summary>
        /// Goto first page by pager
        /// </summary>
        /// <param name="section">Pager container (if it is present, otherwise null)</param>
        /// <returns></returns>
        public bool First(IWebElement section = null)
        {
            return PagerClick("Go to the first page", FirstGridLink, section);
        }

        /// <summary>
        /// Count table rows
        /// </summary>
        /// <param name="section">Section with table (if it is present, otherwise null)</param>
        /// <returns></returns>
        public int CountTableRows(IWebElement section = null)
        {
            try
            {
                var pagerInfo = section.GetElementByQuery(GetElementBy.Tagname, "span.k-pager-info-2.k-label");
                pagerInfo ??= section.GetElementByQuery(GetElementBy.Tagname, "span.k-pager-info.k-label");

                if (pagerInfo == null)
                {
                    var table = (section == null || section.GetAttrib("tag") == "table")
                        ? section
                        : section.GetElementByQuery(GetElementBy.Tagname, "table");
                    var rows = GetTableBodyRows(table);
                    var rowcount = rows.Count;
                    foreach (var row in rows)
                    {
                        if (row.GetAttribute("class").Contains("k-grouping-row") ||
                            row.GetAttribute("class").Contains("k-detail-row") ||
                            row.GetAttribute("class").Contains("k-no-data"))
                            rowcount--;
                    }

                    return rowcount;
                }

                if (pagerInfo.Text == "No items to display")
                    return 0;
                var infoItems = pagerInfo.Text.Split(' ');
                if (infoItems.Length < 4) return 0;
                var i = 0;
                while (i < infoItems.Length && (infoItems[i].ToLower() != "from") && (infoItems[i].ToLower() != "loaded"))
                {
                    i++;
                }
                if (i >= infoItems.Length) 
                    i = infoItems.Length - 2;
                return Convert.ToInt32(infoItems[i + 1]);
            }
            finally
            {
            }
        }

        public int CountTableRows(string value, string columnName, IWebElement section = null)
        {
            int colNumber;
            IWebElement rows;
            //k-grid-header-wrap.k-auto-scrollable - right grid part (with delete icon)
            //k-grid-header-locked - left grid part (with edit/view icons)
            //k-grid-header - header container (div or thead type)
            var headerContainer = section.GetElementByClassName("k-grid-header");
            var isCompositeTable = headerContainer.GetAttrib("tag") == "div";
            if (isCompositeTable)
            {
                var container = WebDriver.GetElementByQuery(GetElementBy.ClassName, "k-grid-header-locked");
                colNumber = GetColumnNumber(columnName, container);
                if (colNumber < 0)
                {
                    container = WebDriver.GetElementByQuery(GetElementBy.ClassName, "k-grid-header-wrap.k-auto-scrollable");
                    colNumber = GetColumnNumber(columnName, container);
                    if (colNumber < 0)
                        Assert.Fail($"Column name '{columnName}' was not found");
                    rows = WebDriver.GetElementByQuery(GetElementBy.ClassName, "k-grid-content.k-auto-scrollable").GetTable();
                }
                else
                {
                    rows = WebDriver.GetElementByQuery(GetElementBy.ClassName, "k-grid-content-locked").GetTable();
                }
            }
            else
            {
                colNumber = GetColumnNumber(columnName, WebDriver.GetTable());
                rows = WebDriver.GetTable();
            }

            var sel = GetTableColumnCells(colNumber, rows);
            if (sel.Count == 0) return 0;
            var coun = sel.ToArray().Count(cell => cell.GetElementByQuery(GetElementBy.Tagname, "span").GetAttrib("title") == value);
            return coun;
        }

        /// <summary>
        /// Check or uncheck checkox
        /// </summary>
        /// <param name="sValue">"TRUE" or "FALSE" value</param>
        /// <param name="chkBox">Checkbox control</param>
        public void CheckOrUncheckTheCheckbox(string sValue, IWebElement chkBox, bool focus = false)
        {
            if (chkBox == null)
                Assert.Fail("Check box element was not found!");
            if (!bool.TryParse(sValue, out var parseResult))
            {
                throw new ArgumentException($"Value parameter should be true or false values, now it's '{sValue}'.");
            }
            //Report.StartStep(((parseResult) ? "C" : "Unc") + "hecking checkbox");

            if ((parseResult && !chkBox.Selected) || (!parseResult && chkBox.Selected))
            {
                for (var i = 40; i > 0 && chkBox.Selected != parseResult; --i)
                {
                    chkBox.Click2(focusElement: focus);
                    if (chkBox.Selected != parseResult)
                    {
                        chkBox.SetTrigger();
                        WebDriver.WaitForReadyState();
                    }

                    if (chkBox.Selected == parseResult) break;

                }
                if (chkBox.Selected != parseResult)
                {
                    chkBox.ScrollIntoView();
                    Assert.Fail(
                        $"Value '{sValue}' is not applied to check box ({chkBox.GetAttribute("id") ?? "unknown"}).");
                }
            }
            //AllureNextReport.FinishStep();
        }

        public void SwitchToggleControl(string sValue, IWebElement toggleControl)
        {
            if (string.IsNullOrEmpty(sValue))
                return;
            bool? value = null;
            if (sValue.ToLower() == "yes" || sValue.ToLower() == "true") value = true;
            if (sValue.ToLower() == "no" || sValue.ToLower() == "false") value = false;
            if (value == null) 
                Assert.Fail($"Wrong value for verification of toggle control - '{sValue}' should be 'true' or 'false'");
            SwitchToggleControl((bool)value, toggleControl);
        }

        public void SwitchToggleControl(bool sValue, IWebElement toggleControl)
        {
            var value = sValue;
            //Report.StartStep($"Switching toggle value to '{sValue}'");
            if ((value && toggleControl.GetAttribute("class").Contains("off")) || (!value && !toggleControl.GetAttribute("class").Contains("off")))
            {
                try
                {
                    toggleControl.Click2(focusElement: false);
                }
                catch
                {
                    toggleControl.ScrollIntoView();
                    throw;
                }
            }
            //AllureNextReport.FinishStep();
        }

        public void SwitchToggleControl(string sValue, string sEntityName)
        {
            var toggle = WebDriver.GetElementById(sEntityName).GetParent();
            SwitchToggleControl(sValue, toggle);
        }

        public void ModalButtonClick(string popupTypeOrId = "Dialog", string buttonText = "Yes", bool popup = true, bool errorexpected = false)
        {
            //Report.StartStep($"Clicking '{buttonText}' button on '{popupTypeOrId}' modal window");
            try
            {
                WebDriver.WaitForReadyState();
                var popUpDialog = WebDriver.GetElementByQuery(GetElementBy.Tagname, $"#{(popup ? "popup" : "") + popupTypeOrId}.in");
                var t = 5;
                while (popUpDialog == null && t-- > 0)
                {
                    WebDriver.Pause(1000);
                    WebDriver.WaitForReadyState();
                    popUpDialog = WebDriver.GetElementByQuery(GetElementBy.Tagname, $"#{(popup ? "popup" : "") + popupTypeOrId}.in");
                }
                if (popUpDialog == null) return;
                var button = DriverWait.Until(d => popUpDialog.GetElementByTextWithTag(buttonText, "button"));
                button.Click2(focusElement: false, emptyform: errorexpected);
                popUpDialog = WebDriver.GetElementByQuery(GetElementBy.Tagname, $"#{(popup ? "popup" : "") + popupTypeOrId}.in");
                t = 5;
                while (popUpDialog != null && t-- > 0)
                {
                    WebDriver.Pause(1000);
                    WebDriver.WaitForReadyState(emptyform: errorexpected);
                    popUpDialog = WebDriver.GetElementByQuery(GetElementBy.Tagname, $"#{(popup ? "popup" : "") + popupTypeOrId}.in");
                }
                WebDriver.Pause();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                ////Report.LogFailedStepWithFailedTestCase(e);
                if (e.Message.Contains("500") || e.Message.Contains("401")) throw;
                WebDriver.Assert500();
                throw;
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        /// <summary>
        /// Clicks on popup dialog's button
        /// </summary>
        /// <param name="buttonText">text of button to click, by default is "Yes"</param>
        /// <param name="errorexpected">Are we expecting an error?</param>
        public void PopupDialogClick(string buttonText = "Yes", bool errorexpected = false) => ModalButtonClick("Dialog", buttonText, errorexpected: errorexpected);

        public void SelectInSingleSelectGrid(IWebElement container, string sValue, string filter = "DEFFLTR", string layout = "DEFVIEW")
        {
            //Report.StartStep($"Selecting '{sValue}' item in single select lookup table");
            try
            {
                WebDriver.WaitForReadyState();
                if (container == null)
                    Assert.Fail("Container is not available!");

                IWebElement dialog = null;
                try
                {
                    dialog = container.WaitForElementIsReady(GetElementBy.ClassName, "fade.in[role=dialog]");
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Timed out after"))
                        Assert.Fail("Lookup dialog was not shown!");
                    throw;
                }

                if (!DriverWait.WaitForVisible(dialog))
                    Assert.Fail("Single select dialog was not shown");

                //Check default Filter and View and set it if it needs
                if (dialog.GetElementByClassName("lookupGridConfigPanel") != null)
                {
                    if (dialog.GetElementById("filters-Section") != null)
                        CreateNewOrSelectExistingConfiguration(filter, ConfigurationType.Filter, container: dialog);
                    if (dialog.GetElementById("layouts-Section") != null)
                        CreateNewOrSelectExistingConfiguration(layout, ConfigurationType.Layout, container: dialog);
                }

                IWebElement selectTable = null;
                try
                {
                    selectTable = container.WaitForElementIsReady(GetElementBy.ClassName, "k-selectable");
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Timed out after"))
                        Assert.Fail("Modal grid was not shown!");
                    throw;
                }

                do
                {
                    var cellText = selectTable.GetElementByOneAttribute(sValue, "span") ??
                                   selectTable.GetElementByOneAttribute($"FRP_{sValue.ToUpper()}", "span");

                    //MoveToElementAndClick(cellText);//
                    if (cellText != null)
                    {
                        cellText.Click2(focusElement: false);
                        DriverWait.WaitForInVisible(dialog);
                        break;
                    }

                    if (!Next(container, false))
                        Assert.Fail($"Element {sValue} was not found on Lookup dialog");

                } while (Next(container));
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }


        //for xeye testing
        public bool FindInSingleSelectGrid(IWebElement container, string sValue, string filter = "DEFFLTR", string layout = "DEFVIEW")
        {
            bool isFound = false;
            //Report.StartStep($"Selecting '{sValue}' item in single select lookup table");
            try
            {
                WebDriver.WaitForReadyState();
                if (container == null)
                    Assert.Fail("Container is not available!");

                var dialog = container.WaitForElementIsReady(GetElementBy.ClassName, "fade.in[role=dialog]");
                if (!DriverWait.WaitForVisible(dialog))
                    Assert.Fail("Single select dialog was not shown");

                //Check default Filter and View and set it if it needs
                if (dialog.GetElementByClassName("lookupGridConfigPanel") != null)
                {
                    if (dialog.GetElementById("filters-Section") != null)
                        CreateNewOrSelectExistingConfiguration(filter, ConfigurationType.Filter, container: dialog);
                    if (dialog.GetElementById("layouts-Section") != null)
                        CreateNewOrSelectExistingConfiguration(layout, ConfigurationType.Layout, container: dialog);
                }

                var selectTable = container.WaitForElementIsReady(GetElementBy.ClassName, "k-selectable");
                do
                {
                    var cellText = selectTable.GetElementByOneAttribute(sValue, "span") ??
                                   selectTable.GetElementByOneAttribute($"FRP_{sValue.ToUpper()}", "span");
                    if (cellText != null)
                    {

                        isFound = true;
                        break;
                    }

                    if (!Next(container, false))
                        break;

                } while (Next(container));
            }
            finally
            {
                //AllureNextReport.FinishStep();
                
            }
            return isFound;
        }

        public void FindInSingleSelectField(string idEntityName, string sValue, bool isPresent = true)
        {
           
            //Report.StartStep($"Filling '{idEntityName}' single select field with '{sValue}' value");
            try
            {
                var divContainer = WebDriver.AssertElementByIdAvailability($"EntityPicker_wrapper_{idEntityName}");
                var selectButton = WebDriver.JsExecute("return Array.from(arguments[0].querySelectorAll('button')).find(item => item.title.includes('Select'))", divContainer);
                selectButton.Click2(focusElement: false);
                Assert.IsTrue(isPresent == FindInSingleSelectGrid(divContainer, sValue),$"Entity {idEntityName} is {(isPresent?"not":"")} found in lookup");
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        public void FillSingleSelectField(string idEntityName, string sValue)
        {
            //Report.StartStep($"Filling '{idEntityName}' single select field with '{sValue}' value");
            try
            {
                var divContainer = WebDriver.AssertElementByIdAvailability($"EntityPicker_wrapper_{idEntityName}");
                var selectButton = WebDriver.JsExecute("return Array.from(arguments[0].querySelectorAll('button')).find(item => item.title.includes('Select'))", divContainer);
                selectButton.Click2(focusElement: false);
                SelectInSingleSelectGrid(divContainer, sValue);
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        /// <summary>
        /// Fill SpecialDays listview section
        /// </summary>
        /// <param name="index">Current index</param>
        /// <param name="values">Value dictionary</param>
        private void FillAddressesSection(int index, IReadOnlyDictionary<string, string> values)
        {
            //Report.StartStep($"Filling {index} Address section");
            try
            {
                var main = values["AddressesMain"];
                if (main != null)
                {
                    SwitchToggleControl(main, $"Entity_Addresses_{index}__Main");
                }
                var type = values["Type"];
                if (type != null)
                {
                    SelectValueInListBox(type, $"Entity_Addresses_{index}__Addr_Type");
                }
                var country = values["CountryCode"];
                FillSingleSelectField($"Entity_Addresses_{index}__Addr_Country", country);
                //values[3]; - Country description for validate

                WebDriver.SetTextBoxValue($"Entity_Addresses_{index}__Addr_Zip", values["Zip"]);
                WebDriver.SetTextBoxValue($"Entity_Addresses_{index}__Addr_Province", values["Province"]);
                WebDriver.SetTextBoxValue($"Entity_Addresses_{index}__Addr_City", values["City"]);
                WebDriver.SetTextBoxValue($"Entity_Addresses_{index}__Addr_Street", values["Street"]);
                WebDriver.SetTextBoxValue($"Entity_Addresses_{index}__Addr_Building", values["Building"]);
                WebDriver.SetTextBoxValue($"Entity_Addresses_{index}__Addr_Appartment", values["Apartment"]);
                WebDriver.SetTextBoxValue($"Entity_Addresses_{index}__Addr_Line1", values["AddressLine1_"]);
                WebDriver.SetTextBoxValue($"Entity_Addresses_{index}__Addr_Line2", values["AddressLine2_"]);
                WebDriver.SetTextBoxValue($"Entity_Addresses_{index}__Comment", values["Comment"]);
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        /// <summary>
        /// Fill SpecialDays listview section
        /// </summary>
        /// <param name="index">Current index</param>
        /// <param name="values">Value dictionary</param>
        private void FillOperationsSection(int index, IReadOnlyDictionary<string, string> values)
        {
            //Report.StartStep($"Filling {index} Operation section");
            try
            {
                FillSingleSelectField($"Entity_Types_{index}__OperationType", values["OperationTypeCode"]);
                //values[1] - description
                FillSingleSelectField($"Entity_Types_{index}__Signature", values["SignatureTypeCode"]);
                //values[3] - description
                if (!string.IsNullOrEmpty(values["AmountLimitMin"]))
                {
                    var limitMin = WebDriver.GetElementById($"Entity_Types_{index}__LimitMin");
                    SetNumberInNumericTextBox(values["AmountLimitMin"], limitMin);
                }
                if (!string.IsNullOrEmpty(values["AmountLimitMax"]))
                {
                    var limitMax = WebDriver.GetElementById($"Entity_Types_{index}__LimitMax");
                    SetNumberInNumericTextBox(values["AmountLimitMax"], limitMax);
                }
                FillSingleSelectField($"Entity_Types_{index}__Currency", values["CurrencyCode"]);
                //values[7] - description
                var peroid = values["Period"];
                if (peroid != null)
                {
                    SelectValueInListBox(peroid, $"Entity_Types_{index}__Period");
                }
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        public void FillFile(string value)
        {
            //Report.StartStep($"Insert '{value}' file name without dialog window");
            try
            {
                WebDriver.GetElementById("single-file").SendKeys(value);
                var dialogTitle = BrowserName == "Firefox" ? "File Upload" :
                    BrowserName == "InternetExplorer" ? "Choose File to Upload" : "Open";
                WindowsDialog.IsOpenDialogActive(dialogTitle);
                WebDriver.WaitForReadyState();
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        /// <summary>
        /// Fill Files listview section
        /// </summary>
        /// <param name="index">Current index</param>
        /// <param name="values">Value dictionary</param>
        private void FillFilesSection(int index, IReadOnlyDictionary<string, string> values)
        {
            //Report.StartStep($"Filling {index} File section");
            try
            {
                FillSingleSelectField($"Entity_Files_{index}__FileStore_Type", values["AttachmentTypeName"]);
                if (values["AttachmentRoleCode"] != null)
                    FillSingleSelectField($"Entity_Files_{index}__File_Role", values["AttachmentRoleCode"]);
                var uploadFileSection =
                    WebDriver.GetElementByOneAttribute($"Entity.Files[{index}].FileStore", "div", "data-filepropertyname");
                WebDriver.WaitForReadyState();
                var addButton = uploadFileSection.GetElementByAttributeWithValue("name='files[]'");
                addButton.SendKeys(values["FileToAttach"]);
                /* addButton.ScrollIntoView();
                addButton.MoveToElementAndClick(); //"Add file" button
                var dialogTitle = BrowserName == "Firefox" ? "File Upload" : "Open";
                var t = 0;
                while (!WindowsDialog.IsOpenDialogActive(dialogTitle) && t++ < 20)
                {
                    WebDriver.WaitForReadyState();
                }
                if (!WindowsDialog.IsOpenDialogActive(dialogTitle))
                    Assert.Fail("Something wrong! Open dialog window was not opened");
                var filename = values["FileToAttach"];
                //Report.StartStep($"Insert '{filename}' file name into dialog window");
                if (filename.LastIndexOf('\\') > -1)
                    WindowsDialog.SetTextToOpenDialog(filename.Substring(0, filename.LastIndexOf('\\') + 1));
                WebDriver.WaitForReadyState();
                WindowsDialog.SetTextToOpenDialog(filename);
                t = 0;
                while (WindowsDialog.IsOpenDialogActive(dialogTitle) && t++ < 20)
                {
                    WebDriver.WaitForReadyState();
                }
                if (WindowsDialog.IsOpenDialogActive(dialogTitle))
                    Assert.Fail("Something wrong! Open dialog window is not closed yet");
                Report.FinishStep();*/
                IWebElement decs = null;
                var t = 0;
                while (decs == null && t++ < 1000)
                    decs = WebDriver.GetElementByClassName("description-input", uploadFileSection);
                decs = DriverWait.WaitForElementIsClickable(decs);
                WebDriver.SetTextBoxValue(decs, values["FileDescription"]);
                var buttonStart = DriverWait.WaitForElementIsClickable(WebDriver.GetElementByClassName("k-upload-selected", uploadFileSection));
                buttonStart.Click2(); //"Start" button
                t = 0;
                while (WebDriver.GetElementByClassName("file-uploaded", uploadFileSection) == null && t < 20)
                {
                    if (WebDriver.ElementAvailable(WebDriver.GetElementByOneAttribute("Bad Request", "span")))
                        Assert.Fail("Bad Request! File was not uploaded");
                    WebDriver.WaitForReadyState();
                    t++;
                }
                if (!WebDriver.ElementAvailable(WebDriver.GetElementByClassName("file-uploaded", uploadFileSection)) && t == 20)
                {
                    Assert.Fail("Something wrong! File was not uploaded");
                }
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        /// <summary>
        /// Fill Level listview section
        /// </summary>
        /// <param name="index">Current index</param>
        /// <param name="values">Value list</param>
        /// <param name="testPurpose">Test purpose</param>
        private void FillLevelSection(int index, IReadOnlyList<string> values, TestPurpose testPurpose)
        {
            //Report.StartStep($"Filling {index} Level section");
            try
            {
                var radioButtonLabel = WebDriver.GetElementByOneAttribute($"Entity_AaLevels_{index}__ValidationRule_{values[0]}", "label", "for");
                radioButtonLabel.Click2();
                //TODO: realise user list processing
                SelectInMultiselectGrid($"Entity_AaLevels_{index}__Users", new[] { values[1] }, testPurpose);
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        /// <summary>
        /// Fill Letters listview section
        /// </summary>
        /// <param name="index">Current index</param>
        /// <param name="values">Value dictionary</param>
        private void FillLettersSection(int index, IReadOnlyDictionary<string, string> values)
        {
            //Report.StartStep($"Filling {index} Letter section");
            try
            {
                var calendarEdit = WebDriver.GetElementByName($"Entity.Letters[{index}].CreationDate");
                SetCalendarDate(calendarEdit, values["CreationDate"], false);
                FillSingleSelectField($"Entity_Letters_{index}__Contact", values["Contact"]);
                WebDriver.SetTextBoxValue($"Entity_Letters_{index}__Comment", values["Comment"]);
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        /// <summary>
        /// Fill Rules listview section
        /// </summary>
        /// <param name="index">Current index</param>
        /// <param name="values">Value dictionary</param>
        private void FillRulesSection(int index, IReadOnlyDictionary<string, string> values, string entityName)
        {
            //entityName : RulesO or RulesR
            //Report.StartStep($"Filling {index} Rule section");
            try
            {
                // Set Priority
                SetNumberInNumericTextBox(values["Priority"], WebDriver.GetElementById($"Entity_{entityName}_{index}__Prio"));

                // Set toggle ExcludeFromProcess
                SwitchToggleControl(values.ContainsKey("ExcludeFromProcess") ? values["ExcludeFromProcess"] : "false", $"Entity_{entityName}_{index}__ExcludeFromProcess");

                // Set Rule
                FillSingleSelectField($"Entity_{entityName}_{index}__Rule", values["RuleCode"]);

                // Set toggle RedefineOptions
                SwitchToggleControl(values.ContainsKey("RedefineOptions") ? values["RedefineOptions"] : "false", $"Entity_{entityName}_{index}__RedefineOptions");
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        /// <summary>
        /// Fill RecRule listview section
        /// </summary>
        /// <param name="entityName">Entity name</param>
        /// <param name="index">Current index</param>
        /// <param name="values">Value dictionary</param>
        private void FillRecRuleSection(string entityName, int index, IReadOnlyDictionary<string, string> values)
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

            //Report.StartStep($"Filling list view of {entityName} section for {bankOrGlStmtId} statement (index={index})");
            try
            {
                if (!isFilter)
                {
                    // Fill Name field
                    WebDriver.SetTextBoxValue($"Entity_{entityName}_{index}__Name", values[dataPrefix + "Name"], "blur");
                }

                // Select Property field
                SelectValueInPropertiesTree($"Entity_{entityName}_{index}__{bankOrGlStmtId}PropertyPath", values[dataPrefix + "Property"]);

                // If use F()
                var f = values.ContainsKey(dataPrefix + "F") ? values[dataPrefix + "F"] : null;
                if (f != null && bool.Parse(f))
                {
                    var funcPrefix = $"Entity_{entityName}_{index}__Func_";
                    // Switch F toggle
                    var toggleF = WebDriver.GetElementByOneAttribute($"{funcPrefix}Code", "label", "for").GetNextSibling("div");
                    SwitchToggleControl(values[dataPrefix + "F"], toggleF);

                    // Select function
                    SelectValueInListBox(values[dataPrefix + "FType"], $"{funcPrefix}Code", false);

                    var criteriaType = values[dataPrefix + "CriteriaType"].ToLower();
                    var funcType = values[dataPrefix + "FType"].ToLower();

                    switch (criteriaType)
                    {
                        case "string":
                            if (funcType == "substring") // substring
                            {
                                var idPrefix = funcPrefix;//+funcPrefix; // only substring has this duplication!
                                                          // Set Start position
                                var startPositionEdit = WebDriver.GetElementById($"{idPrefix}Int32");
                                SetNumberInNumericTextBox(values[dataPrefix + "StartPosition"], startPositionEdit);

                                // Set Length
                                var lengthEdit = WebDriver.GetElementById($"{idPrefix}Int32_1");
                                SetNumberInNumericTextBox(values[dataPrefix + "Length"], lengthEdit);
                            }
                            else // trim
                            {
                                // Set triming symbol
                                WebDriver.SetTextBoxValue($"{funcPrefix}Params_0__Value_Char", values[dataPrefix + "TrimingSymbol"]);

                                // Select Option
                                SelectValueInListBox(values[dataPrefix + "Option"], $"{funcPrefix}Params_1__Value_StrTrimOption", false);
                            }
                            break;
                        case "number":
                            if (funcType == "round")
                            {
                                var vInd = entityName.Contains("Filters") ? "3" : "2";
                                // Set Length
                                var lengthEdit = WebDriver.GetElementById($"{funcPrefix}Int32_{vInd}");
                                SetNumberInNumericTextBox(values[dataPrefix + "Length"], lengthEdit);
                            }
                            break;
                        case "date":
                            // Select Date part
                            SelectValueInListBox(values[dataPrefix + "DatePart"], $"{funcPrefix}Params_0__Value_Date{(funcType == "datediff" ? "Diff" : "")}PartOption", false);

                            if (funcType == "datediff") // datediff
                            {
                                // Set End Date
                                var calendarEdit = WebDriver.GetElementByName($"Entity.{entityName}[{index}].Func.Params[1].Value");
                                SetCalendarDate(calendarEdit, values[dataPrefix + "EndDate"], false);
                            }
                            break;
                        default:
                            Assert.Fail($"Getting criterias data: wrong criteria type - '{criteriaType}'");
                            break;
                    }
                }

                if (isFilter)
                {
                    // Select Operator
                    SelectValueInListBox(values[dataPrefix + "Operator"], $"Entity_{entityName}_{index}__Operator", false);

                    // if use criterion - select it from list box
                    var isUseCriterion = values[dataPrefix + "UseCriterion"];
                    if (isUseCriterion != null && bool.Parse(isUseCriterion))
                    {
                        CheckOrUncheckTheCheckbox(values[dataPrefix + "UseCriterion"], WebDriver.GetElementById($"Entity_{entityName}_{index}__Criterion_Code_CheckBox"));
                        SelectValueInListBox(values[dataPrefix + "FltrValue"], $"Entity_{entityName}_{index}__Criterion", inputId: "Code");
                    }
                    else
                    {
                        var criteriaType = values[dataPrefix + "CriteriaType"].ToLower();
                        switch (criteriaType)
                        {
                            case "string":
                                WebDriver.SetTextBoxValue($"Entity_{entityName}_{index}__RightHandValue_String", values[dataPrefix + "FltrValue"]);
                                break;
                            case "number":
                                var numEdit = WebDriver.GetElementById($"Entity_{entityName}_{index}__RightHandValue_Decimal");
                                SetNumberInNumericTextBox(values[dataPrefix + "FltrValue"], numEdit);
                                break;
                            case "date":
                                var calendarEdit = WebDriver.GetElementByName($"Entity.{entityName}[{index}].RightHandValue");
                                SetCalendarDate(calendarEdit, values[dataPrefix + "FltrValue"]);
                                break;
                            default:
                                Assert.Fail($"Error criterias data: unknown type - '{criteriaType}'");
                                break;
                        }
                    }
                }
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        internal bool CheckFilterOrLayoutExists(string sName, ConfigurationType tType)
        {
            WebDriver.Assert500();
            var configName = (tType == ConfigurationType.Filter) ? "filter" : "layout";

            var section = WebDriver.GetElementById($"{configName}s-Section");
            var link = section.GetElementByClassName("dropdown-toggle"); // GetElementByAttributeWithValue("data-toggle=dropdown");
            Assert.IsNotNull(link);
            link.ClickUnderRedis();
            var hlItem = section.GetElementByAttributeWithValue($"data-{configName}-name='{sName}'");
            if (hlItem == null) return false;
            link.ClickUnderRedis();
            return true;
        }

        /// <summary>
        /// Fill CriteriaDependencies listview section
        /// </summary>
        /// <param name="index">Current index</param>
        /// <param name="values">Value dictionary</param>
        private void FillCriteriaDependenciesSection(int index, IReadOnlyDictionary<string, string> values)
        {
            SelectValueInListBox(values["DependencyLeft"], $"Entity_CriteriaDependencies_{index}__Dependency1", inputId: "Code");
            SelectValueInListBox(values["Operation"], $"Entity_CriteriaDependencies_{index}__DependencyOperation", false);
            SelectValueInListBox(values["DependencyRight"], $"Entity_CriteriaDependencies_{index}__Dependency2", inputId: "Code");
            SwitchToggleControl(values["Optional"], $"Entity_CriteriaDependencies_{index}__IsOptional");
        }

        /// <summary>
        /// Fill Identifier listview section
        /// </summary>
        /// <param name="index">Current index</param>
        /// <param name="values">Value dictionary</param>
        private void FillIdentifierSection(int index, IReadOnlyDictionary<string, string> values)
        {
            //Report.StartStep($"Filling {index} Identifiers section");
            try
            {
                var main = values["Main"];
                if (main != null)
                {
                    SwitchToggleControl(main, $"Entity_IdentifiersGroups_{index}__Main");
                } //Entity_IdentifiersGroups_0__Local
                //WebDriver.SetTextBoxValueWithInputTrigger($"Entity_IdentifiersGroups_{index}__Local", values["Local"]);
                //WebDriver.SetTextBoxValueWithInputTrigger($"Entity_IdentifiersGroups_{index}__Iban", values["Iban"].Replace(" ", ""));
                var identLocal = WebDriver.GetElementById($"Entity_IdentifiersGroups_{index}__Local");
                identLocal.Clear();
                if (!string.IsNullOrEmpty(values["Local"]))
                {
                    identLocal.SendKeys(Keys.Home);
                    identLocal.SendKeys(values["Local"]);
                }
                var identIban = WebDriver.GetElementById($"Entity_IdentifiersGroups_{index}__Iban");
                identIban.Clear();
                if (!string.IsNullOrEmpty(values["Iban"]))
                {
                    identIban.SendKeys(Keys.Home);
                    identIban.SendKeys(values["Iban"].Replace(" ", ""));
                }
                if (!string.IsNullOrEmpty(values["FreeForm"]))
                    WebDriver.SetTextBoxValueWithInputTrigger($"Entity_IdentifiersGroups_{index}__FreeForm", values["FreeForm"]);
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        /// <summary>
        /// Fill RollingSteps listview section
        /// </summary>
        /// <param name="index">Current index</param>
        /// <param name="values">Value dictionary</param>
        private void FillRollingStepsSection(int index, IReadOnlyDictionary<string, string> values)
        {
            var slDateInput = WebDriver.GetElementById($"Entity_RlStepsVM_{index}__Formula");
            var editDateInput = slDateInput.GetParent().GetElementByClassName("sliding-date-input");
            editDateInput.Clear();
            editDateInput.SendKeys(values["Formula"]);
            //WebDriver.SetTextBoxValue(editDateInput, values["Formula"]);
            editDateInput.SetTrigger();
            var tDescription = WebDriver.GetElementById($"Entity_RlStepsVM_{index}__Description");
            tDescription.Clear();
            tDescription.SendKeys(values["Description"]);
            if(values.ContainsKey("Percentage"))
            {
                var inputPerc = WebDriver.GetElementById($"Entity_RlStepsVM_{index}__Percentage");
                //inputPerc.GetParent().GetElementByClassName("k-formatted-value").Click2();
                SetNumberInNumericTextBox(values["Percentage"], inputPerc);
            }
        }

        /// <summary>
        /// Fill SpecialDays listview section
        /// </summary>
        /// <param name="index">Current index</param>
        /// <param name="values">Value dictionary</param>
        private void FillSpecialDaysSection(int index, IReadOnlyDictionary<string, string> values)
        {
            WebDriver.SetTextBoxValue($"Entity_SpecialDays_{index}__Description", values["RuleDescription"]);
            SelectValueInListBox(values["RuleDay"], $"Entity_SpecialDays_{index}__CalculatedDateDay", false, true);
            if (values["RuleDay"] == "Calendar Day")
                WebDriver.SetTextBoxValue(WebDriver.GetElementByName($"Entity.SpecialDays[{index}].Date"), values["RuleDate"]);
            SelectValueInListBox(values["RuleType"], $"Entity_SpecialDays_{index}__DayType", false, true);
        }

        /// <summary>
        /// Fill Grouping listview section
        /// </summary>
        /// <param name="index">Current index</param>
        /// <param name="values">Value dictionary</param>
        /// <param name="entity">Entity name</param>
        private void FillGroupingSection(int index, IReadOnlyDictionary<string, string> values, string entity)
        {
            var sufix = "";
            var entityName = entity;
            if (entity == "BalanceRowsAF")
            {
                sufix = "AF";
                entityName = "BalanceRows";
            }
            SetNumberInNumericTextBox(values["Order"], WebDriver.GetElementById($"Entity_{entityName}_{index}__Order"));
            var chkBox = WebDriver.GetElementById($"Entity_{entityName}_{index}__{sufix}Type").GetParent();
            chkBox.Click2();
            SelectValueInDropDownList(chkBox, values["Type"]);
        }

        private void FillProcessesSection(int index, IReadOnlyDictionary<string, string> values, string entity)
        {
            SetNumberInNumericTextBox(values["Priority"], WebDriver.GetElementById($"Entity_{entity}_{index}__Prio"));
            FillSingleSelectField($"Entity_{entity}_{index}__Process", values["Process"]);
        }

        private void FillCorrespondenceSection(int index, IReadOnlyDictionary<string, string> values, TestPurpose testPurpose, string entity = "Correspondences")
        {
            //left
            SelectInMultiselectGrid($"Entity_{entity}_{index}__LeftMap", new[] { values[$"LeftMap{index + 1}"] },
                testPurpose);
            if (values.ContainsKey($"LeftGroupingType{index + 1}"))
                SelectRadioAsButton($"Entity_{entity}_{index}__LeftGroupingType",
                    values[$"LeftGroupingType{index + 1}"]);

            //right
            SelectInMultiselectGrid($"Entity_{entity}_{index}__RightMap", new[] { values[$"RightMap{index + 1}"] },
                testPurpose);
            if (values.ContainsKey($"RightGroupingType{index + 1}"))
                SelectRadioAsButton($"Entity_{entity}_{index}__RightGroupingType",
                    values[$"RightGroupingType{index + 1}"]);
        }

        /// <summary>
        /// Fill ListView section with dictionary data array
        /// </summary>
        /// <param name="entityName">Entity name</param>
        /// <param name="rowCount">Row count</param>
        /// <param name="values">Value dictionary array</param>
        /// <param name="buttonAdd">Add button control</param>
        /// <param name="testPurpose"></param>
        private void FillListView(string entityName, int rowCount, IReadOnlyList<Dictionary<string, string>> values, IWebElement buttonAdd, TestPurpose testPurpose)
        {
            var arrayNotEmpty = values.Count > 0;
            var count = (rowCount == 0 && arrayNotEmpty) ? 1 : rowCount > 0 ? rowCount : 0;
            for (var i = 0; i < count; ++i)
            {
                switch (entityName)
                {
                    case "Addresses":
                        FillAddressesSection(i, values[i]);
                        break;
                    case "Operations":
                    case "Types":
                        FillOperationsSection(i, values[i]);
                        break;
                    case "Files":
                        FillFilesSection(i, values[i]);
                        break;
                    case "Letters":
                        FillLettersSection(i, values[i]);
                        break;
                    //case "RulesO":
                    case "RulesC":
                    case "RulesG":
                    case "RulesR":
                        FillRulesSection(i, values[i], entityName);
                        break;
                    case "Criterias1":
                    case "Criterias2":
                    case "Filters1":
                    case "Filters2":
                        FillRecRuleSection(entityName, i, values[i]);
                        break;
                    case "CriteriaDependencies":
                        FillCriteriaDependenciesSection(i, values[i]);
                        break;
                    case "IdentifiersGroups":
                        FillIdentifierSection(i, values[i]);
                        break;
                    case "RlStepsVM":
                        FillRollingStepsSection(i, values[i]);
                        break;
                    case "SpecialDays":
                        FillSpecialDaysSection(i, values[i]);
                        break;
                    case "Groupings":
                    case "AccountsGroupings":
                    case "BalanceRows":
                    case "BalanceRowsAF":
                        FillGroupingSection(i, values[i], entityName);
                        break;
                    case "ProcessesO":
                        FillProcessesSection(i, values[i], entityName);
                        break;
                    case "Correspondences1":
                    case "Correspondences2":
                        FillCorrespondenceSection(i, values[i], testPurpose, entityName);
                        break;
                    default:
                        Assert.Fail($"Not implemented filling of list view for '{entityName}' Entity name");
                        break;
                }
                if (i + 1 < rowCount)
                    buttonAdd.Click2();
            }
        }

        /// <summary>
        /// Fill ListView section with string data array
        /// </summary>
        /// <param name="entityName">Entity name</param>
        /// <param name="rowCount">Row count</param>
        /// <param name="values">String value array</param>
        /// <param name="testPurpose">Test purpose</param>
        /// <param name="buttonAdd">Add button control</param>
        private void FillListView(string entityName, int rowCount, IReadOnlyList<string[]> values, IWebElement buttonAdd, TestPurpose testPurpose)
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
                        FillLevelSection(i, values[i], testPurpose);
                        break;
                    default:
                        Assert.Fail($"Not implemented filling of list view for '{entityName}' Entity name");
                        break;
                }
                if (i + 1 < rowCount)
                    buttonAdd.Click2();
            }
        }

        /// <summary>
        /// Fill ListView section (global method)
        /// </summary>
        /// <param name="entityName">Entity name</param>
        /// <param name="rowCount">Row count</param>
        /// <param name="values">Values object</param>
        /// <param name="testPurpose">Test purpose</param>
        public void FillListView(string entityName, int rowCount, object values, TestPurpose testPurpose)
        {
            //Report.StartStep($"Filling '{entityName}' ListView (rowCount={rowCount})");
            try
            {
                var divListSection = WebDriver.GetElementByName($"Entity.{(entityName == "BalanceRowsAF" ? "BalanceRows" : entityName)}").GetParent();
                var buttonAdd = divListSection.GetElementByOneAttribute("Add", "input", "value");
                var delButtons = divListSection.GetElementsByOneAttribute("Delete item");
                var delCount = delButtons?.Count ?? 0;
                if (delCount == 0)
                {
                    buttonAdd.Click2();
                }

                for (var j = delCount; j > (testPurpose == TestPurpose.ClearBeforeFill ? 0 : 1); --j)
                {
                    delButtons[0].WaitForElementIsClickable().Click2();
                    PopupDialogClick();
                    delButtons = divListSection.GetElementsByOneAttribute("Delete item");
                    if (j == delButtons?.Count) //??? delButtons== null ||
                        Assert.Fail("Section was not deleted");

                }

                if (testPurpose == TestPurpose.ClearBeforeFill)
                    return;
                if (entityName == "Files" && delCount > 0)
                {
                    var buttonDel = divListSection.GetElementByOneAttribute("Delete item");
                    buttonDel.WaitForElementIsClickable().Click2();
                    PopupDialogClick();
                    buttonAdd.Click2();
                }

                if (values is List<Dictionary<string, string>> listOfDictionaries)
                {
                    FillListView(entityName, rowCount, listOfDictionaries, buttonAdd, testPurpose);
                }
                else
                {
                    var valueArray = (string[][])values;
                    FillListView(entityName, rowCount, valueArray, buttonAdd, testPurpose);
                }
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        public void FillDetailTable(string entityName, int rowCount, object value, TestPurpose testPurpose, DetailTableType tableType)
        {
            if (rowCount == 0) return;
            //Report.StartStep($"Filling '{entityName}' Detail table (rowRount={rowCount})");
            var divGrid = WebDriver.GetElementById($"{entityName}_table") ?? WebDriver.GetElementById(entityName);
            var ttype = divGrid.GetAttribute("class").Contains("detail-table-widget");
            var buttonAdd = WebDriver.GetElementByClassName(ttype ? "k-grid-add" : "add-new-row-description", divGrid);
            var divContent = WebDriver.GetElementByClassName("k-grid-content", divGrid);
            if (testPurpose != TestPurpose.Create)
            {
                var lines = GetTableBodyRows(divContent.GetTable(GetElementBy.Tagname));
                while (lines != null && lines.Count > 1)
                {
                    var btnDel = lines[^1].GetElementByOneAttribute("Delete", ttype ? "span" : "a");
                    btnDel.WaitForElementIsClickable().Click2();
                    ModalButtonClick();
                    lines = GetTableBodyRows(divContent.GetTable(GetElementBy.Tagname));
                }
                //Report.StartStep("Wait for button Add is clickable and press it");
                if ((lines == null || lines.Count == 0) && rowCount > 0)
                    buttonAdd.WaitForElementIsClickable().Click2();
                //AllureNextReport.FinishStep();
                if (lines != null && lines.Count == 1 && rowCount > 0)
                {
                    var additionalGrid = divGrid.GetElementByClassName("grid-view-edit");
                    if (additionalGrid != null)
                        additionalGrid.WaitForElementIsClickable().Click2();
                }
            }
            else
            {
                buttonAdd.WaitForElementIsClickable().Click2(true);
            }

            var valueArray = value as List<Dictionary<string, string>>;
            if (valueArray == null) Assert.Fail($"Values for '{entityName}' detail table are missing");
            for (var i = 0; i < rowCount; ++i)
            {
                var s = valueArray[i].Aggregate("", (current, item) => current + $" '{item.Value}'({item.Key})");
                //Report.StartStep($"Filling {i} row of '{entityName}' detail table:{s}");
                FillDetailTableLine(entityName, valueArray[i], tableType);
                if (i + 1 < rowCount) buttonAdd.WaitForElementIsClickable().Click2();
                //AllureNextReport.FinishStep();
            }
            //AllureNextReport.FinishStep();
        }

        /// <summary>
        /// Expand row line by click on expand icon
        /// </summary>
        /// <param name="itemName">Row description</param>
        /// <param name="container">Row container</param>
        private static void ExpandRowItem(string itemName, IWebElement container)
        {
            var expandItem =
                container.GetElementByTextWithTag(itemName, "div")
                    .GetParent()
                    .GetElementByClassName("k-icon.k-i-expand");
            expandItem.Click2();
        }

        /// <summary>
        /// Marks the row checkboxes, according to the value string array
        /// </summary>
        /// <param name="containerId">Cintainer ID</param>
        /// <param name="values">String array of values (description)</param>
        internal void FillRowsTree(string containerId, string[] values)
        {
            var divRows = WebDriver.GetElementById(containerId);
            foreach (var value in values)
            {
                var splitted = value.Split('.');
                for (var i = 0; i < splitted.Length - 1; ++i)
                    ExpandRowItem(splitted[i], divRows);
                //Report.StartStep($"Checking {value} item");
                IWebElement chkBoxContainer = (IWebElement)WebDriver.Execute($"return Array.from(document.querySelector('#{containerId}').querySelectorAll('.k-treeview-item')).find(el=>el.innerText == '{splitted[^1]}')");
                CheckOrUncheckTheCheckbox("true",
                    chkBoxContainer.GetElementByTagName("input"), true);
                //AllureNextReport.FinishStep();
            }

        }

        public void FillStructureFieldParameters(string entityName, Dictionary<string, string> values, int index)
        {
            //Report.StartStep($"Filling {index} Structure field parameter row");
            var type = values["FieldType"]; //Fixed/Variable
            var length = string.IsNullOrEmpty(values["FieldLength"]) ? "0" : values["FieldLength"]; //number; used for Fixed only
            var mandatory = string.IsNullOrEmpty(values["FieldMandatory"]) ? "false" : values["FieldMandatory"]; //true/false
            var included = string.IsNullOrEmpty(values["FieldIncluded"]) ? "true" : values["FieldIncluded"]; //true/false
            var format = string.IsNullOrEmpty(values["FieldFormat"]) ? null : values["FieldFormat"];
            var defvalue = string.IsNullOrEmpty(values["FieldDefValue"]) ? null : values["FieldDefValue"];
            //            var converters = values[7].Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);//converter list
            var converters = values["FieldConverter"]?.Split('|') ?? Array.Empty<string>(); //converter list

            var typeInput = WebDriver.GetElementByName($"Entity.Fields[{index}].RowType");
            if (string.IsNullOrEmpty(type))
                type = typeInput.GetAttribute("value");
            else
            {
                try
                {
                    //Report.StartStep($"Select '{type}' value from 'Entity.Fields[{index}].RowType' dropdown list");
                    if (typeInput.GetAttribute("value") != type)
                    {
                        var typeControl = typeInput.GetParent();
                        typeControl.MoveToElement().Perform();
                        SelectValueInDropDownList(typeControl, type);
                    }
                }
                finally
                {
                    //AllureNextReport.FinishStep();
                }
            }

            if (type == "Fixed")
            {
                try
                {
                    //Report.StartStep($"Set '{length}' value to 'Entity.Fields[{index}].Length' field");
                    var numberControl = WebDriver.GetElementByName($"Entity.Fields[{index}].Length");
                    SetNumberInNumericTextBox(length, numberControl);
                }
                finally
                {
                    //AllureNextReport.FinishStep();
                }
            }
            if (values["UsedIn"].ToLower() != "export")
            {
                try
                {
                    //Report.StartStep($"Set '{mandatory}' value to 'Entity.Fields[{index}].IsMandatory' field");
                    var toggle = WebDriver.GetElementByName($"Entity.Fields[{index}].IsMandatory").GetParent();
                    SwitchToggleControl(mandatory, toggle);
                }
                finally
                {
                    //AllureNextReport.FinishStep();
                }
            }
            if (values["UsedIn"].ToLower() != "import")
            {
                try
                {
                    //Report.StartStep($"Set '{included}' value to 'Entity.Fields[{index}].IsIncluded' field");
                    var toggle = WebDriver.GetElementByName($"Entity.Fields[{index}].IsIncluded").GetParent();
                    SwitchToggleControl(included, toggle);
                }
                finally
                {
                    //AllureNextReport.FinishStep();
                }
            }
            WebDriver.SetTextBoxValue(WebDriver.GetElementByName($"Entity.Fields[{index}].Format"), format);
            WebDriver.SetTextBoxValue(WebDriver.GetElementByName($"Entity.Fields[{index}].DefaultValue"), defvalue);
            if (converters.Length == 0)
            {
                //AllureNextReport.FinishStep();
                return;
            }
            var convertersControl = WebDriver.GetElementByName($"Entity.Fields[{index}].Converters").GetParent().GetParent().GetParent();
            convertersControl.GetElementByClassName("glyphicon").Click2();
            GetElementRowNumberBySeveralValues(new[] { "Reference", "Used In", "External Value From", "Internal Value" }, converters, true, true, container: convertersControl);
            convertersControl.GetElementByTextWithTag("Accept", "a").WaitForElementIsClickable().Click2();
            DriverWait.WaitForElementIsNotVisible(convertersControl.GetElementByTextWithTag("Accept", "a"));
            //AllureNextReport.FinishStep();
        }

        private string _addDefTimeToDate(string date)
        {
            var ret = date;
            var spld = date.Split(' ');
            switch (spld.Length)
            {
                case 1:
                    ret = date + " 00:00:00 PM";
                    break;
                case 2:
                    int.TryParse(spld[1].Split(':')[0], out var hr);
                    if (hr > 11)
                        ret = date + " AM";
                    else
                        ret = date + " PM";
                    break;
                case 3:
                    break;
                default:
                    Assert.Fail($"Something wrong with date value: '{date}'");
                    break;
            }
            return ret;
        }

        public void SetCalendarDate(IWebElement editControl, string numberOfDaysToAddFromTodayDate, bool slidingdate = true, bool dateWithTime = false)
        {
            var editField = editControl;
            //Report.StartStep($"Setting calendar date ({numberOfDaysToAddFromTodayDate} day(s) from current day)");
            if (editField == null)
            {
                Assert.Fail($"Calendar text box element is not found, intended number of days to enter from today: '{numberOfDaysToAddFromTodayDate}'.");
            }

            var calcDateStr = _addDefTimeToDate(
                GetCalendarDate(numberOfDaysToAddFromTodayDate, dateWithTime, false).ToString(BasePageElementMap.GetCurrentCultureInfo))
                .ConvertToRegionalData(ConfigSettingsReader.RegionalSettings);
            if (slidingdate)
                WebDriver.SetTextToSlidingDateInput(editField, calcDateStr);
            else
            {
                var efType = editField.GetAttrib("type");
                if (efType == "hidden")
                    editField = editControl.GetParent().GetElementByQuery(GetElementBy.Tagname, "input.k-input");
                WebDriver.SetTextBoxValue(editField, calcDateStr);
                editField.SetTrigger("change");
                editField.SetTrigger("focusout");
            }

            //AllureNextReport.FinishStep();
        }

        public DateTime GetCalendarDate(string numberOfDaysToAddFromTodayDate, bool time = false, bool toview = true, bool expect = true)
        {
            var numberToWork = numberOfDaysToAddFromTodayDate;
            if (numberToWork != null)
            {
                var slidingDate = numberToWork.Split('#');
                if (slidingDate.Length > 1)
                    numberToWork = slidingDate[1];
            }
            //var dtformat = (toview ? "dd/MM/yyyy" : "M/d/yyyy") + (time ? " HH:mm:ss" : "");
            //Report.StartStep($"Convert '{numberToWork}' to date");// to '{dtformat}' format");
            try
            {
                return DateTime.TryParse(numberToWork, BasePageElementMap.GetCurrentCultureInfo, DateTimeStyles.None, out var date) ?
                date//.ToString(dtformat, GetCurrentCultureInfo)
                : SetDateFromToday(numberToWork ?? "0");//, time);//.ToString(dtformat, GetCurrentCultureInfo);
            }
            catch
            {
                return DateTime.TryParse(numberToWork, CultureInfo.CreateSpecificCulture(expect ? "fr-FR" : "en-US"), DateTimeStyles.None, out var date) ?
                date//.ToString(dtformat, GetCurrentCultureInfo)
                : SetDateFromToday(numberToWork ?? "0");//, time);//.ToString(dtformat, GetCurrentCultureInfo);
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        /// <summary>
        /// Method returns string representation of date by adding the specified number of days from today's date in format M/d/yyyy
        /// </summary>
        /// <param name="numberOfDaysToAddFromTodayDate">Number of days we add to today</param>
        /// <param name="time">Are we use time or not?</param>
        /// <returns>System.String</returns>
        public DateTime SetDateFromToday(string numberOfDaysToAddFromTodayDate)//, bool time)
        {
            if (!double.TryParse(numberOfDaysToAddFromTodayDate, out var numOfDays))
            {
                throw new InvalidCastException($"Value '{numberOfDaysToAddFromTodayDate}' could not be converted to double");
            }

            return DateTime.Today.AddDays(numOfDays);//.ToString("M/d/yyyy" + (time ? " HH:mm:ss" : ""), GetEnUSCultureInfo);
        }

        public void SetColorHexValue(IWebElement inputWebElement, string valueColorInHex)
        {
            var hexBox = inputWebElement.GetParent();
            var selector = hexBox.GetElementByClassName("k-input-button");
            selector.Click2();
            var elementBorderState = GetBorderState(hexBox);
            var element = WebDriver.WaitForElementIsReady(GetElementBy.ClassName, $"k-colorpicker-popup.k-state-border-{elementBorderState}");
            var clickable = element.GetElementByClassName("k-input.k-textbox");
            var error = false;
            var errMsg = "";
            var ti = 5;
            do
            {
                var input = clickable.GetElementByClassName("k-input-inner");
                try
                {
                    //input.Clear(); //not worked
                    //Report.StartStep("Clear HEX-edit field");
                    input.SendKeys(Keys.Control + "a" + Keys.Delete);//use this instead of .Clear()
                    WebDriver.Pause();
                    var tries = 5;
                    while (WebDriver.GetElementsByQuery(GetElementBy.ClassName, $"k-colorpicker-popup.k-state-border-{elementBorderState}") == null && tries-- > 0)
                    {
                        selector.Click2();
                        WebDriver.Pause();
                        elementBorderState = GetBorderState(hexBox);
                        element = WebDriver.WaitForElementIsReady(GetElementBy.ClassName, $"k-colorpicker-popup.k-state-border-{elementBorderState}");
                        clickable = element.GetElementByClassName("k-input.k-textbox");
                        input = clickable.GetElementByClassName("k-input-inner");
                        input.SendKeys(Keys.Control + "a" + Keys.Delete);//use this instead of .Clear()
                    }
                }
                catch (Exception e)
                {
                    error = true;
                    errMsg = "(Clear) " + e.Message;
                    if (ti > 1)
                    {
                        //Driver.//Report.LogFailedStepWithFailedTestCase(e, errMsg);
                        AddElmahDetail();
                        //AllureNextReport.ResetErrorFlag();
                        Driver.ElmahAdded(false);
                    }
                    selector.SetFocus();
                    selector.Click2();
                    continue;
                }
                finally
                {
                    //AllureNextReport.FinishStep();
                }
                try
                {
                    //Report.StartStep($"Fill HEX-edit field with '{valueColorInHex}'");
                    input.SendKeys(valueColorInHex);
                }
                catch (Exception e)
                {
                    error = true;
                    errMsg = "(Fill) " + e.Message;
                    if (ti > 1)
                    {
                        //Driver.//Report.LogFailedStepWithFailedTestCase(e, errMsg);
                        AddElmahDetail();
                        //AllureNextReport.ResetErrorFlag();
                        Driver.ElmahAdded(false);
                    }
                    selector.SetFocus();
                    selector.Click2();
                    continue;
                }
                finally
                {
                    //AllureNextReport.FinishStep();
                }
                try
                {
                    input.SetTrigger("change");
                }
                catch (Exception e)
                {
                    error = true;
                    errMsg = "(Set trigger) " + e.Message;
                    if (ti > 1)
                    {
                        //Driver.//Report.LogFailedStepWithFailedTestCase(e, errMsg);
                        AddElmahDetail();
                        //AllureNextReport.ResetErrorFlag();
                        Driver.ElmahAdded(false);
                    }
                    selector.SetFocus();
                    selector.Click2();
                    continue;
                }
            } while (error && --ti > 0);
            if (error)
                Assert.Fail($"Dialog with HEX value was not interactable. Error with message: {errMsg}");
            var timer = 0;
            while (timer++ < 10 && WebDriver.GetElementByQuery(GetElementBy.ClassName, $"k-state-border-{elementBorderState}") != null)
            {
                selector.Click2();
                WebDriver.WaitForReadyState();
            }

            static string GetBorderState(IWebElement hexBox)
            {
                var activeClasses = hexBox.GetAttribute("class").Split(' ');
                var borderState = activeClasses.FirstOrDefault(a => a.Contains("k-state-border-"));
                if (borderState == null)
                    Assert.Fail("Active drop down list was not found");
                return borderState.Split('-')[3] == "up" ? "down" : "up";
            }
        }

        private IWebElement GetActiveList(string borderState, int index = 0)
        {
            var elements = WebDriver.GetElementsByQuery(GetElementBy.ClassName, $"k-state-border-{borderState}");
            if (elements == null || elements.Count <= index) return null;
            var element = elements[index];
            if (!string.IsNullOrEmpty(element.GetAttrib("calss").Split(' ').FirstOrDefault(a => a.Equals("k-menu-group"))))
            {
                if (elements.Count <= index + 1) return null;
                element = elements[index + 1];
            }
            return element;
        }

        public void SelectUnselectKendoMultiSelectItem(IWebElement item, string[] values, bool unselect = false)
        {
            if ((values == null || values.Length == 0) && unselect)
            {
                if (item.GetAttrib("class").Contains("k-state-selected"))
                    item.Click2(focusElement: false);
                return;
            }
            //if (values.Length == 0 && !unselect) return;
            var itemName = item.GetAttrib("innerText");
            foreach (var value in values)
            {
                if (!value.Equals(itemName)) continue;
                if ((unselect && item.GetAttrib("class").Contains("k-state-selected")) || (!unselect && !item.GetAttrib("class").Contains("k-state-selected")))
                    item.Click2(focusElement: false);
                break;
            }
        }

        public void SelectValuesInKendoMultiSelect(string mainIdPart, string[] values)
        {
            var control = WebDriver.GetElementById(mainIdPart+ "_KendoMultiSelect");
            var clickControl = control.GetElementByQuery(GetElementBy.Tagname, "div.k-multiselect-wrap.k-floatwrap");
            clickControl.Click2();
            var borderState = clickControl.GetParent().GetBorderState();
            var itemList = GetActiveList(borderState).GetElementsByQuery(GetElementBy.Tagname, "ul li");
            foreach (var item in itemList)
            {
                SelectUnselectKendoMultiSelectItem(item, values);
            }
        }

        public void SelectValueInDropDownList(object webElement, string valueToSelect, string secState = null, IWebElement defActiveControl = null)
        {
            SelectValueInDropDownList((IWebElement)webElement, valueToSelect, secState, defActiveControl);
        }

        public void SelectValueInDropDownList(IWebElement webElement, string valueToSelect, string secState = null, IWebElement defActiveControl = null, bool listView = false, bool forced = false)
        {
            if (valueToSelect == null) return;
            var clickableElement = webElement;
            if (!clickableElement.GetAttribute("class").Contains("k-input"))
                clickableElement = webElement.GetElementByClassName("k-input") ?? webElement.GetElementByClassName("k-input-value-text");
            if (clickableElement == null)
                Assert.Fail("DropDown input control was not found");
            var examsValue = clickableElement.GetElementValue();
            if (examsValue != null && (
                (examsValue == "" && valueToSelect == "") || 
                (examsValue != "" && 
                valueToSelect != "" &&
                !forced &&
                valueToSelect.Equals(examsValue.Contains("<abbr>") ? examsValue.Split('>')[^1] : examsValue) ||
                examsValue.ToUpper() == "FRP_" + valueToSelect.ToUpper()))) return;

            var ul = WebDriver.GetElementById(webElement.GetAttrib("aria-controls"));

            if (!forced && (examsValue.WhatIsLang() == valueToSelect.WhatIsLang() || 
                (examsValue.Contains("<abbr>") && examsValue.Split('>')[^1] == valueToSelect) ||
                (valueToSelect.ToLower().Contains("user") &&
                 examsValue.ToUpper() == "FRP_" + valueToSelect.ToUpper())))
            {
                if(!listView && ul.GetAttrib("aria-hidden").ToLower().Contains("false"))
                    clickableElement.ClickUnderRedis();
                return;
            }

            WebDriver.CloseRedis();
            var t = 10;
            var err = false;
            string elementBorderState = null;
            var activeList = GetActiveList(elementBorderState);
            if (!listView && secState == null)
            {
                while (string.IsNullOrEmpty(elementBorderState) && t-- > 0)
                {
                    //Report.StartStep("Try to open dropdown list");
                    clickableElement.ClickUnderRedis();
                    elementBorderState = webElement.GetBorderState(except: false);
                    //AllureNextReport.FinishStep();
                }
                if (t == 0 && !ul.GetAttrib("aria-hidden").ToLower().Contains("true"))
                    err = true;
            }
            else
            {
                while (string.IsNullOrEmpty(elementBorderState) && t-- > 0)
                    try
                    {
                        elementBorderState = webElement.GetBorderState();
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.Contains("Active drop down list was not found"))
                            throw;
                        clickableElement.ClickUnderRedis();
                    }
                activeList = GetActiveList(elementBorderState);
                while (activeList == null && t-- > 0)
                {
                    //Report.StartStep($"Looking for element with 'k-state-border-{elementBorderState}' class");
                    clickableElement.ClickUnderRedis();
                    activeList = GetActiveList(elementBorderState);
                    //AllureNextReport.FinishStep();
                }
                if (t == 0 && activeList == null)
                    err = true;
            }
            activeList ??= GetActiveList(elementBorderState);
            ul = secState == null ? activeList.GetElementByTagName("ul.k-list-ul") : activeList.GetElementByTagName("ul.k-treeview-lines");
            if (err)
                Assert.Fail("Element is not clickable or drop down list can not be opened");
            if (ul == null)
                Assert.Fail("Element has no list or wrong element!");
            if (listView)
            {
                elementBorderState ??= webElement.GetBorderState();
                var script = $@"
let activeEl = document.querySelector('.k-state-border-{elementBorderState}')
let item = Array.from(activeEl.querySelectorAll('li')).find(ll=>ll.innerText === '{valueToSelect}')
item.click()";
                WebDriver.Execute(script);
                WebDriver.WaitForReadyState();
            }
            else
            {
                var lis = ul.GetElementsByTagName("li", true);
                if (lis.Count == 0)
                    throw new NoSuchElementException("Item list is not available!");
                //Report.StartStep($"Looking for '{valueToSelect}' element in list with {lis.Count} items");
                var el = lis.FirstOrDefault(li => valueToSelect.Contains(">") ? li.GetElementValue() == valueToSelect : li.GetElementValue().Split('>')[^1] == valueToSelect);
                if (el == null && valueToSelect.ToLower().Contains("user"))
                    el = lis.FirstOrDefault(li => li.GetElementValue().ToUpper() == "FRP_" + valueToSelect.ToUpper());
                //AllureNextReport.FinishStep();
                if (el == null)
                {
                    el = lis.FirstOrDefault(li => li.GetElementValue() == "All columns");
                    el.Click2(focusElement: false);
                    throw new NoSuchElementException(
                        $"li element with text '{valueToSelect}' was not found!");
                }
                el.Click2(focusElement: false);

                if (secState != null && !el.SetSecurityToItem(secState.GetStateString()))
                {
                    el.ScrollIntoView();
                    Assert.Fail($"Can't set state '{secState}' to '{valueToSelect}' item");
                }
            }
        }

        /// <summary>
        /// Method selects value from the list-box, specify ONLY ONE parameter - either part of id in entityName parameter OR sectionClassName
        /// </summary>
        /// <param name="valueToSelect">Li value (text) to select</param>
        /// <param name="entityName">Entity name - part of id, may contain entity name with index number, 
        /// for ex.: if Id is Entity_Addresses_2__Addr_Type_DDL_Id_listbox, then specify part - Entity_Addresses_2__Addr_Type</param>
        /// <param name="withInput">Input posibility presens</param>
        public void SelectValueInListBox(string valueToSelect, string entityName, bool withInput = true, bool listView = false, string inputId = "Id", bool forced = false)
        {
             //Report.StartStep($"Selecting '{valueToSelect}' value in '{entityName}' listbox");
            var mainCard = WebDriver.AssertElementByIdAvailability("mainCardForm");
            var additionalString = (withInput) ? $"_{inputId}" : "";
            var lstBxs = mainCard.GetElementsByOneAttribute($"{entityName}{additionalString}_listbox", "span", "aria-controls");
            var lstBx = (lstBxs.Count == 1) 
                ? lstBxs[0] 
                : lstBxs?.FirstOrDefault(lb => lb != null && string.IsNullOrEmpty(lb.GetElementByTagName("select").GetAttrib("class").Split(' ').FirstOrDefault(s => s.Equals("ignore"))));
            var element = lstBx ?? 
                          mainCard.GetElementByAttributeWithValue($"data-valmsg-for='{entityName.Replace('_', '.')}'")
                                  .GetParent().GetElementByAttributeWithValue("role=listbox");
            if (element == null) Assert.Fail($"'{entityName}' listbox was not found!");
            element.SetFocus();
            // Perform click to open List-box control
            SelectValueInDropDownList(element, valueToSelect,listView: listView, forced: forced);
            //AllureNextReport.FinishStep();
        }

        public void SelectValueInListBoxTree(string listTree, string entityName)
        {
            var clickableElement = WebDriver.GetElementById(entityName);
            var activeElement = clickableElement.GetParent();
            clickableElement.Click2(focusElement: false);
            while (!activeElement.IsDropDownActive())
            {
                clickableElement.Click2(focusElement: false);
            }
            var borderState = activeElement.GetBorderState();
            var treeListBox = WebDriver.GetElementByQuery(GetElementBy.ClassName,
                $"k-state-border-{borderState}").GetElementByTagName("ul.k-group.k-treeview-group");
            SelectValueInListBoxTree(listTree.Split('|'), treeListBox, 0);
            clickableElement.Click2(focusElement: false);
        }

        public void SelectValueInListBoxTree(string[] listTree, IWebElement entity, int i = 0)
        {
            if (i < listTree.Length - 1)
            {
                //Report.StartStep($"Expand '{listTree[i]}' value in listboxtree");
                var newEntry = entity.GetElementsByText(GetElementBy.ClassName, "k-treeview-leaf-text", listTree[i])?[0].GetParent().GetParent().GetParent();
                var iconSpan = newEntry.GetElementByQuery(GetElementBy.ClassName, "k-icon");
                if (iconSpan.GetAttrib("class").Contains("k-i-expand"))
                    iconSpan.Click2();
                //AllureNextReport.FinishStep();
                SelectValueInListBoxTree(listTree, newEntry.GetElementByQuery(GetElementBy.Tagname, "ul.k-group.k-treeview-group"), ++i);
                return;
            }
            //Report.StartStep($"Selecting '{listTree[i]}' value in listboxtree");
            var element = entity.GetElementsByText(GetElementBy.ClassName, "k-treeview-leaf-text", listTree[i])?[^1];
            if (element == null)
                Assert.Fail($"'{listTree[i]}' element was not found");
            //element.Click();
            //element.SetFocus();
            element.Click2(focusElement: false);
            //AllureNextReport.FinishStep();
        }

        public static void ExpandPropertiesTree(IWebElement modalDialog, string itemToSelect)
        {
            var expanditem = modalDialog;
            var localTree = "";
            var treeList = itemToSelect.Split('.');
            var treeHead = treeList[0].Split('#');
            if (treeHead.Length > 1)
            {
                var item = expanditem.GetElementByAttributeWithValue($"data-value='{StringConst.EntityClassId[treeHead[0]]}'");
                var icon = item.GetElementByQuery(GetElementBy.ClassName, "k-icon");
                if (icon.GetAttrib("class").Contains("k-i-expand"))
                    icon.Click2();
            }
            foreach (var treeItem in treeList)
            {
                localTree = string.IsNullOrEmpty(localTree) ? treeItem : localTree + "." + treeItem;
                var item = expanditem.GetElementByAttributeWithValue($"data-value='{localTree}'");
                if (localTree.Split('.').Length == treeList.Length)
                {
                    return;
                }
                var icon = item.GetElementByQuery(GetElementBy.ClassName, "k-icon");
                if (icon.GetAttrib("class").Contains("k-i-expand"))
                    icon.Click2();
                expanditem = item;
            }
        }

        public void SelectValueInModalTreeBox(IWebElement valueContainer, string treeValues, bool noSave = false, bool mandatory = true)
        {
            var modalDialog = valueContainer.GetElementByQuery(GetElementBy.ClassName, "fieldstreebox.in");
            var timer = 60;
            while (modalDialog == null && timer-- > 0)
            {
                WebDriver.WaitForReadyState();
                modalDialog = valueContainer.GetElementByQuery(GetElementBy.ClassName, "fieldstreebox.in");
            }
            if (modalDialog == null)
                Assert.Fail("Tree modal dialog was not shown!");

            var searchLst = treeValues.Split('|');
            if (searchLst.Length > 1)
                treeValues = searchLst[0];
            var list = treeValues.Split('.');
            if (list.Length == 1)
                list = list[0].Split('#');
            var searchVal = (searchLst.Length > 1) ? searchLst[1] : list.LastOrDefault<string>();
            var searchField = modalDialog.GetElementByClassName("tree-filter-value");
            var oldStyleTree = true;
            if (searchField != null)
            {
                //Report.StartStep($"Filtering tree by: {searchVal}");
                oldStyleTree = false;
                WebDriver.SetTextBoxValue(searchField, searchVal);
                modalDialog.GetElementByClassName("btn-tree-search").Click2();
                //AllureNextReport.FinishStep();
            }
            else
                ExpandPropertiesTree(modalDialog, treeValues);

            var treeItem = valueContainer.GetElementByAttributeWithValue($"data-value='{treeValues}'");
            if (treeItem == null && !mandatory) return;
            if (treeItem == null) Assert.Fail($"Element {treeValues} was not found in the TreeBox.");
            if (oldStyleTree)
            {
                treeItem.GetElementByQuery(GetElementBy.Tagname, "span.tree-node-name").Click2();
                DriverWait.WaitForInVisible(modalDialog);
            }
            else
            {
                var cb = treeItem.GetElementByQuery(GetElementBy.Tagname, "input.columnCheckbox");
                if (cb != null)
                    CheckOrUncheckTheCheckbox("true", cb);
                else
                    treeItem.GetElementByQuery(GetElementBy.Tagname, "span.tree-node-name").Click2();//title=treeValues
                if (!noSave) 
                    modalDialog.GetElementByClassName("cancelColumnsTree").Click2();
            }

        }

        public void SelectValueInPropertiesTree(string entityName, string treeValues)
        {
            //Report.StartStep($"Selecting '{treeValues}' in '{entityName}' Properties tree");
            var valueContainer = WebDriver.GetElementById(entityName).GetParent();
            var btnSelect = valueContainer.GetElementByClassName("glyphicon-pencil").GetParent();

            btnSelect.ClickIt();

            WebDriver.WaitForReadyState();
            SelectValueInModalTreeBox(valueContainer, treeValues);
            //AllureNextReport.FinishStep();
        }

        public void SelectValueInSelectedPropertiesTree(string entityName, string treeValues)
        {
            try
            {
                //Report.StartStep($"Selecting '{treeValues}' value in '{entityName}' selected properties tree");
                var valueContainer = WebDriver.GetElementById(entityName);
                var btnAdd = valueContainer.GetElementByOneAttribute("Add", "input", "value");
                btnAdd.Click2(focusElement: false);
                SelectValueInModalTreeBox(valueContainer, treeValues);
                //AllureNextReport.FinishStep();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                //Report.LogFailedStepWithFailedTestCase(e);
                throw;
            }
        }

        public void SelectValueInEntityPropertiesTree(string entityName, string entityValue, string treeValues)
        {
            try
            {
                //Report.StartStep($"Selecting '{treeValues}' value in '{entityName}' property tree for '{entityValue}' entity");
                var valueContainer = WebDriver.GetElementById(entityName).GetParent();
                SelectValueInDropDownList(valueContainer, entityValue);
                SelectValueInPropertiesTree(entityName, treeValues);
                //AllureNextReport.FinishStep();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                //Report.LogFailedStepWithFailedTestCase(e);
                throw;
            }
        }

        public void SetNumberInNumericTextBox(string number, IWebElement control)
        {
            //Report.StartStep($"Setting {number} value to numeric text box");
            if (control == null)
            {
                Assert.Fail($"Numeric text box element is not found, intended number to input: '{number}'");
            }

            WebDriver.SetTextBoxValue(control, number.ConvertToRegionalNumeric(ConfigSettingsReader.RegionalSettings));
            control.SetTrigger("focusout");
            //AllureNextReport.FinishStep();
        }

        public void LookForValuesInGrid(IWebElement divContainer, string[] values, bool checkSeveralSameValues = false, bool check = true)
        {
            if (values.Length == 0) return;
            //Report.StartStep("Multiselecting in grid");
            var checkedFlags = new bool[values.Length];
            WebDriver.WaitForReadyState();
            do
            {
                var foundIndexes = new int[values.Length];
                var foundCount = 0;
                var table = WebDriver.GetTable(GetElementBy.ClassName, container: divContainer);
                var chkBoxes = new List<IWebElement>();
                for (var i = 0; i < values.Length; ++i)
                {
                    if (!checkSeveralSameValues && checkedFlags[i]) continue;
                    var cells = table.GetElementsByOneAttribute(values[i], "span");
                    if (cells == null || cells.Count == 0)
                        continue;
                    //Report.StartStep($"{(check ? "C" : "Unc")}heck '{values[i]}' item.");
                    foreach (var cell in cells)
                    {
                        //if (check)
                            chkBoxes.Add(cell.GetRowChkBxByCell());
                        if (!checkSeveralSameValues) break;
                    }
                    //AllureNextReport.FinishStep();
                    foundIndexes[foundCount++] = i;
                }

                if (foundCount < 1)
                    continue;

                Array.Resize(ref foundIndexes, foundCount);

                
                foreach (var chB in chkBoxes)
                {
                    if ((!chB.Selected && check) || (chB.Selected && !check))
                        chB.Click2();
                }
                foreach (var ind in foundIndexes)
                {
                    checkedFlags[ind] = true;
                }
            } while (!(checkedFlags.AllTrue() && !checkSeveralSameValues) && Next(divContainer));
            if (checkedFlags.AllTrue())
            {
                //AllureNextReport.FinishStep();
                return;
            }
            for (var i = 0; i < checkedFlags.Length; ++i)
            {
                Assert.IsTrue(checkedFlags[i], $"Value '{values[i]}' was not found in lookup");
            }
            //AllureNextReport.FinishStep();
        }

        public void MultiselectingInGrid(IWebElement divContainer, string[] values, bool checkSeveralSameValues = false)
        {
            LookForValuesInGrid(divContainer, values, checkSeveralSameValues, true);
        }

        public void SelectInMultiselectGrid(string entityName, string[] values, TestPurpose purpose, bool checkSeveralSameValues = false, string filter = "DEFFLTR", string layout = "DEFVIEW")
        {
            if (values.Length == 0) return;
            //Report.StartStep($"Selecting for '{entityName}' multiselect grid");
            //table with final data: id=propertygridbox-<Entity_name>
            if (purpose == TestPurpose.Edit || purpose == TestPurpose.Duplicate)
            {
                var divMultiSelectTable = WebDriver.AssertElementByIdAvailability($"propertygridbox-{entityName}");
                var rowCount = GetTableBodyRows(WebDriver.GetTable(GetElementBy.ClassName, container: divMultiSelectTable)).Count;
                while (rowCount > 0)
                {
                    DeleteGridIcon(divMultiSelectTable).WaitForElementIsClickable().Click2();
                    ModalButtonClick();
                    if (rowCount == GetTableBodyRows(WebDriver.GetTable(GetElementBy.ClassName, container: divMultiSelectTable)).Count)
                        Assert.Fail("Line was not deleted");
                    rowCount--;
                }
            }
            //!! TODO add processing check once value only
            //button Select: class="btn btn-secondary t-lookup-multi-button pull-right" data-entity-property-name=<Entity_name>
            var dialogName = $"dialog{entityName}";
            var btnSelect = WebDriver.GetElementByOneAttribute(dialogName, "div", "data-target-name");
            Assert.IsNotNull(btnSelect, "Select button for multi-select grid was not found.");
            DriverWait.WaitForVisible(btnSelect);
            btnSelect.Click2();
            //lookup: id=dialog<Entity_name>
            var divLookUp = WebDriver.GetElementById(dialogName);
            var timer = 60;
            while (!DriverWait.WaitForVisible(divLookUp) && timer-- > 0)
            {
                WebDriver.WaitForReadyState();
                divLookUp = WebDriver.GetElementById(dialogName);
            }
            if (!DriverWait.WaitForVisible(divLookUp) && timer == 0)
                Assert.Fail($"Dialog '{dialogName}' was not shown");

            //Check default Filter and View and set it if it needs
            CreateNewOrSelectExistingConfiguration(filter, ConfigurationType.Filter);
            CreateNewOrSelectExistingConfiguration(layout, ConfigurationType.Layout);

            MultiselectingInGrid(divLookUp, values, checkSeveralSameValues);

            var accButton = divLookUp.GetElementByClassName("accept-lookup");
            //button Accept: //a[@data-property-name="<Entity_name>"]
            accButton.JsClick();
            //DriverWait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(accButton)).Click();
            WebDriver.WaitForReadyState();
            DriverWait.WaitForInVisible(divLookUp);
            //AllureNextReport.FinishStep();
        }


        /// <summary>
        /// Method chooses whether to use Entity with Extender 
        /// </summary>
        /// <param name="entityName">Entity name, for ex. 'Company'.</param>
        /// <param name="moduleName">Module name, for ex. 'Bank Delegations' or 'Reconciliation'</param>
        /// <param name="useExtender">String value with boolean, should be 'true' or 'false'</param> 
        public void UseEntityWithExtender(string entityName, string moduleName, string useExtender)
        {
            // Assert parse successfull
            Assert.IsTrue(bool.TryParse(useExtender, out var use), $"Convertion from string to bool value failed, value is: '{useExtender}'");

            //Report.StartStep(((use) ? "A" : "Disa") + $"ctivate '{moduleName}' extender for '{entityName}' entity");
            // Get id
            var labelText = $"Use this {entityName} in {moduleName}";
            var label = GetLabelByText(labelText);
            Assert.IsNotNull(label, $"Label with text '{labelText}' was not found.");
            var id = label.GetAttribute("for");

            // Get checkbox
            var checkbox = WebDriver.GetElementById(id);
            DriverWait.WaitForElementIsVisible(checkbox.GetParent());

            // if Extender checkbox is unchecked and do not use Extender - exit
            if (!checkbox.Selected && !use)
            {
                //AllureNextReport.FinishStep();
                return;
            }

            CheckOrUncheckTheCheckbox(useExtender, checkbox);

            // Get div with Extender data
            var divExtender = WebDriver.GetElementById(id.Replace("Input_", ""));

            if (use) // use Extender
            {
                DriverWait.WaitForElementIsVisible(divExtender);
            }
            else // if not use Extender
            {
                PopupDialogClick();
                DriverWait.WaitForElementIsNotVisible(divExtender);
            }
            //AllureNextReport.FinishStep();
        }

        public bool IsElementExistsInGrid(string value, IWebElement container = null)
        {
            //Report.StartStep($"Checking whether '{value}' element exists in grid");
            WebDriver.WaitForReadyState();
            if (container == null)
                container = WebDriver.GetTable();
            do
            {
                if (container.GetElementByOneAttribute(value, "span") == null) continue;
                //AllureNextReport.FinishStep();
                return true;
            } while (Next(container));
            //AllureNextReport.FinishStep();
            return false;
        }

        private string GetDefTableValue(GetElementBy getTableBy)
        {
            return getTableBy switch
            {
                GetElementBy.ClassName => "k-selectable",
                GetElementBy.Tagname => "table",
                _ => "role='grid'",
            };
        }

        public int GetElementRowNumber(GetElementBy getTableBy, string getTableByValue, string columnName, string cellValue, bool backToFirst = false, bool checkTheCheckbox = false, IWebElement container = null, string groupElementName = "")
        {
            //Report.StartStep($"Getting row number of '{cellValue}' element in '{columnName}' column");
            int rowNumber;
            WebDriver.WaitForReadyState();
            do
            {
                var islocked = (WebDriver.GetElementByClassName("k-grid-content-locked") != null);

                if (groupElementName != "" && IsElementExistsInGrid(cellValue))
                {
                    FindGroupAndExpand(groupElementName);
                }

                var valueBy = getTableByValue ?? GetDefTableValue(getTableBy);

                var table = (container != null)
                    ? container.WaitForElementIsReady(getTableBy, valueBy)
                    : WebDriver.WaitForElementIsReady(getTableBy, valueBy);

                if (table == null)
                    Assert.Fail($"Table by {Enum.GetName(typeof(GetElementBy), getTableBy)} '{valueBy}' was not found!");

                rowNumber = GetRowNumberByColumnName(columnName, cellValue, table, islocked);


                if (rowNumber < 0) continue;
                CheckRowNumber(rowNumber, table, checkTheCheckbox);
                if (backToFirst)
                {
                    First(container);
                }
                break;
            } while (Next(container));

            //AllureNextReport.FinishStep();
            return rowNumber;
        }

        public int SelectRowBySeveralValues(string[] columnName, string[][] cellValues, bool backToFirst = false,
            GetElementBy getTableBy = GetElementBy.ClassName, string getTableByValue = null, IWebElement container = null,
            string[] groupElementName = null)
        {
            var cellArrayValues = cellValues.ToList();

            //Report.StartStep("Getting element row number by several values in several columns");
            try
            {
                WebDriver.WaitForReadyState();
                do
                {
                    if (groupElementName != null && groupElementName.Length > 0)
                    {
                        foreach (var group in groupElementName)
                            FindGroupAndExpand(group);
                    }

                    var toDel = new List<string[]>();
                    foreach (var cellValue in cellArrayValues)
                    {
                        if (CheckRowNumber(GetRowNumberBySeveralValues(columnName, cellValue,
                            WebDriver.GetTable(getTableBy, getTableByValue, container)), withCheck: true))
                            toDel.Add(cellValue);
                    }

                    foreach (var td in toDel)
                    {
                        cellArrayValues.Remove(td);
                    }

                    if (cellArrayValues.Count == 0)
                        break;
                } while (Next(container));

                if (backToFirst)
                    First(container);
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }

            return cellArrayValues.Count;
        }

        public bool CheckRowNumber(int rowNumber, IWebElement table = null, bool withCheck = false)
        {
            if (rowNumber < 0 || !withCheck) return false;
            var rowElement = GetTableBodyRows(table)[rowNumber];
            CheckOrUncheckTheCheckbox("TRUE", CheckBoxGridItem(rowElement));
            return true;
        }

        public int GetElementRowNumberBySeveralValues(string[] columnName, string[] cellValue, bool backToFirst = false, bool checkTheCheckbox = false, GetElementBy getTableBy = GetElementBy.ClassName, string getTableByValue = null, IWebElement container = null, string[] groupElementName = null)
        {
            //Report.StartStep("Getting element row number by several values in several columns");
            WebDriver.WaitForReadyState();
            do
            {
                if (groupElementName != null && groupElementName.Length > 0)
                {
                    foreach (var group in groupElementName)
                        FindGroupAndExpand(group);
                }

                var rowNumber = GetRowNumberBySeveralValues(columnName, cellValue, WebDriver.GetTable(getTableBy, getTableByValue, container));
                if (rowNumber < 0) continue;
                CheckRowNumber(rowNumber, withCheck: checkTheCheckbox);
                if (!backToFirst)
                {
                    //AllureNextReport.FinishStep();
                    return rowNumber;
                }
                First(container);
                //AllureNextReport.FinishStep();
                return rowNumber;
            } while (Next(container));

            //AllureNextReport.FinishStep();
            return -1;
        }

        public void ClearAllSingleSelects()
        {
            try
            {
                //Report.StartStep("Clearing all visible sinlge select controlls");
                var btns = WebDriver.GetElementsByClassName("t-lookup-single-clear-button");
                if (btns == null || btns.Count == 0)
                    btns = WebDriver.GetElementsByClassName("glyphicon-remove");
                foreach (var btn in btns)
                {
                    btn.Click2(focusElement: false);
                }
                //AllureNextReport.FinishStep();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                //Report.LogFailedStepWithFailedTestCase(e);
                throw;
            }
        }

        public void SelectRadioAsButton(string id, string buttonValueToSelect)
        {
            try
            {
                var value = "";
                switch (buttonValueToSelect.ToLower())
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
                        value = buttonValueToSelect;
                        break;
                }
                //Report.StartStep($"Selecting '{buttonValueToSelect}' value of '{id}_{value}' RadioButton");
                var inputCheckbox = WebDriver.GetElementById($"{id}_{value}");
                var label = inputCheckbox.GetParent();
                if (label.TagName != "label")
                    label = label.GetElementByTagName("label");
                Assert.IsNotNull(label, "Select radio as button failed: label was not found.");
                if (!label.IsVisiblbe())
                    Assert.Fail("Radio button is not visible.");
                try
                {
                    label.Click2(focusElement: false);
                }
                catch
                {
                    label.ScrollIntoView();
                    throw;
                }
                //CheckOrUncheckTheCheckbox("true", label);
                //AllureNextReport.FinishStep();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                //Report.LogFailedStepWithFailedTestCase(e);
                throw;
            }
        }

        private void _exportTojson(string filename)
        {
            var filePath = ConfigSettingsReader.DownloadPath; //@"C:\Temp\"; //WebDriver.GetDownloadPath();
            if (FileProcessor.FileExists($@"{filePath}{filename}"))
                FileProcessor.DeleteFile($@"{filePath}{filename}");
            var cbSelectAll = WebDriver.GetElementById("checkAllRecords");
            cbSelectAll.Click2();
            var btSelectAll = WebDriver.GetElementByClassName("action-selection-selectall");
            btSelectAll.Click2();
            OpenActionMenu("Export", "grid-actions-one-for-all");
            var container = WebDriver.GetElementByQuery(GetElementBy.Tagname, "a[title='Export Checked (JSON)']");
            container.Click2();
            var popup = WebDriver.GetElementById("popupDialog");
            var timer = 60;
            while (!DriverWait.WaitForVisible(popup) && timer-- > 0)
            {
                WebDriver.WaitForReadyState();
                popup = WebDriver.GetElementById("popupDialog");
            }
            if (!DriverWait.WaitForVisible(popup) && timer == 0)
                Assert.Fail("Dialog 'popupDialog' was not shown");
            popup.GetElementByTextWithTag("Export", "button").Click2();
            WebDriver.WaitForFile($@"{filePath}{filename}", 10000);
            if (!FileProcessor.FileExists($@"{filePath}{filename}"))
                Assert.Fail($@"File '{filePath}{filename}' was not created!");
            if (popup.ElementVisiblityCheckUsedByDisplay())
                popup.GetElementByTextWithTag("Close", "button").Click2();
        }

        public void ExportTojson(string filename)
        {
            try
            {
                //Report.StartStep($"Exporting entities list to json-file ['{filename}']");
                _exportTojson(filename);
                //AllureNextReport.FinishStep();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                //Report.LogFailedStepWithFailedTestCase(e);
                AddElmahDetail();
                throw;
            }
        }

        public void SetFixedSlidingDate(string name, string dateValue)
        {
            var fixedDateEdit = WebDriver.GetElementById($"Formula_{name.Replace('.', '_')}");//("fixeddate-value");
            fixedDateEdit.Clear();
            fixedDateEdit.SendKeys(dateValue.ConvertToRegionalData(ConfigSettingsReader.RegionalSettings));
        }

        public bool FindGroupAndExpand(string groupElementName)
        {
            var groupElement = WebDriver.GetElementByTextWithTag(groupElementName, "span.group-field-value")?.GetParent();
            var arrowElement = groupElement?.GetElementByClassName("k-i-expand");
            if (arrowElement == null) return false;
            arrowElement.Click2();
            return true;
        }

        public void ContextSearchInGrid(string value, string column = "All columns")
        {
            SetLanguage();
            var generalSearchGrids = WebDriver.GetElementsByClassName("general-search-to-grid");
            if (generalSearchGrids == null || generalSearchGrids.Count == 0)
                Assert.Fail("Search grids were not found!");
            foreach (var generalSearchRow in generalSearchGrids)
            {
                if (!generalSearchRow.IsVisiblbe())
                {
                    var searchButton = WebDriver.GetElementById("general-search-dispay-button");
                    searchButton.ClickUnderRedis();
                }

                var columnSpan = generalSearchRow.GetElementByQuery(GetElementBy.ClassName,
                    "k-dropdownlist.general-search-filter-column");
                SelectValueInDropDownList(columnSpan, column);

                var filterText = generalSearchRow.GetElementByQuery(GetElementBy.ClassName, "general-search-to-grid-filter-text");
                filterText.Clear();
                filterText.SendKeys(value);

                var searchGeneralSearch = generalSearchRow.GetElementByQuery(GetElementBy.ClassName, "btn-general-search");
                searchGeneralSearch.ClickUnderRedis();
            }
        }

        public void ClearContextSearchInGrid()
        {
            var generalSearchRow = WebDriver.GetElementByQuery(GetElementBy.ClassName, "general-search-row");
            if (!generalSearchRow.IsVisiblbe())
            {
                var searchButton = WebDriver.GetElementById("general-search-dispay-button");
                searchButton.ClickUnderRedis();
            }
            var clearButton = generalSearchRow.GetElementByQuery(GetElementBy.ClassName, "general-search-clear");
            clearButton.ClickUnderRedis();
        }

        public void ScrollToBottomInsideElement(IWebElement scrollArea)
        {
            var js = ((IJavaScriptExecutor)WebDriver);
            js.ExecuteScript("arguments[0].scrollTo(0, arguments[0].scrollHeight)", scrollArea);
            WebDriver.WaitForReadyState();
        }

        public void OpenActionMenu(string actionButtonName = "More", string actionbar = "grid-actions")
        {
            WebDriver.WaitForReadyState();
            var gridAction = WebDriver.GetElementByClassName(actionbar);
            var buttonMore = gridAction.GetElementByTextWithTag(actionButtonName, "button");
            buttonMore.ClickUnderRedis();
        }

        public string GetEntityLink()
        {
            return AppUrl + EntityUrl;
        }

        /// <summary>
        /// Method creates a new configuration item (filter or layout) with specified name or selects already existing one
        /// </summary>
        /// <param name="sName">Configuration name</param>
        /// <param name="tType">Configuration type (filter or layout)</param>
        /// <param name="isDefaultBased">Use Default configuration or create from Blank</param>
        /// <param name="pagingDisabled">Disabling pager</param>
        /// <param name="columnsWidthBestFit">Best fit for collumns</param>
        /// <param name="allowFrozenColumns">Allow frozen collumns</param>
        /// <param name="container">Control which contain filter/layout</param>
        /// <param name="force">Select filter/layout even selected</param>
        /// <returns>Returns true if element was created or false if it already exists</returns>
        public bool CreateNewOrSelectExistingConfiguration(string sName, ConfigurationType tType,
            bool isDefaultBased = true, bool pagingDisabled = false, bool columnsWidthBestFit = true,
            bool allowFrozenColumns = true, IWebElement container = null, bool force = false)
        {
            try
            {
                WebDriver.Assert500();
                var configName = (tType == ConfigurationType.Filter || tType == ConfigurationType.CPWFilter)
                    ? "filter"
                    : "layout";
                //Report.StartStep($"Try to select '{sName}' {configName}");

                var section = container == null
                    ? WebDriver.GetElementById($"{configName}s-Section")
                    : container.GetElementById($"{configName}s-Section");

                var ul = section.GetElementByClassName("dropdown-menu");
                var defaultElementIcon = ul.GetElementByClassName("glyphicon-predefined-default") ??
                                         ul.GetElementByClassName("glyphicon-predefined") ??
                                         ul.GetElementByClassName("glyphicon-star") ??
                                         ul.GetElementByClassName("glyphicon-star-empty");
                var defConfigName = defaultElementIcon.GetParent().GetAttrib($"data-{configName}-name");

                //var defConfigName = (tType == ConfigurationType.Filter || tType == ConfigurationType.CPWFilter)
                //    ? "DEFFLTR"
                //    : "DEFVIEW";

                var defValue = section.GetElementById($"{configName}sDefault").Text;
                if (defValue == sName && !force)
                {
                    //Report.StartStep("Already selected");
                    //AllureNextReport.FinishStep();
                    return false;
                }

                var link = section.GetElementByClassName("dropdown-toggle");
                Assert.IsNotNull(link);
                link.ClickUnderRedis();

                var hlItem = ul.GetElementByAttributeWithValue($"data-{configName}-name='{sName}'");
                if (hlItem != null)
                {
                    //Report.StartStep($"Selecting {configName} with name {sName}");

                    hlItem.Click2();

                    //AllureNextReport.FinishStep();
                    return false;
                }

                if (sName == defConfigName)
                    Assert.Fail(
                        $"There is no default {(tType == ConfigurationType.Filter || tType == ConfigurationType.CPWFilter ? "filter" : "layout")}!");

                try
                {
                    //Report.StartStep($"Creating {configName} with name {sName}");
                    //Report.StartStep($"Creating copy {defConfigName} {configName}");
                    var confLink = (isDefaultBased)
                        ? WebDriver.GetElementByQuery(GetElementBy.Tagname,
                            $"a[data-{configName}-name='{defConfigName}']").GetNextSibling("a")
                        : WebDriver.GetElementById(
                            $"new{Char.ToUpper(configName[0]) + configName[1..]}Btn"); //"/following-sibling::a");
                    confLink.Click2();
                    //AllureNextReport.FinishStep();

                    //Report.StartStep($"Save this {configName} copy as {sName}");
                    if (tType == ConfigurationType.CPWFilter || tType == ConfigurationType.CPWLayout) return true;
                    IWebElement lnkSaveAs;
                    DriverWait.WaitForElementIsVisible(lnkSaveAs =
                        WebDriver.GetElementById("action-saveas-configuration"));
                    lnkSaveAs.Click2();

                    SwitchToggleControl("false", "IsDefault");
                    //Report.StartStep($"Set name '{sName}'");
                    var btnCode = WebDriver.GetElementById("Code");
                    btnCode.WaitForElementIsClickable().Clear();
                    btnCode.SendKeys(sName);
                    //AllureNextReport.FinishStep();
                    FillSingleSelectField("Profile", "Public");
                    if (tType == ConfigurationType.Layout)
                    {
                        SwitchToggleControl(pagingDisabled.ToString(), "PagingDisabled");
                        if (!isDefaultBased)
                        {
                            SwitchToggleControl(columnsWidthBestFit.ToString(), "ColumnsWidthBestFit");
                            SwitchToggleControl(allowFrozenColumns.ToString(), "AllowFrozenColumns");
                        }
                    }
                    //AllureNextReport.FinishStep();
                }
                finally
                {
                    //AllureNextReport.FinishStep();
                }
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                //Report.LogFailedStepWithFailedTestCase(e);
                throw;
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
            return true;
        }

        public void RunProcess(string value, string TestUserName, ReconciliationModule module, string columnName = "Description")
        {
            var title = "Run Reconciliation Process";
            //Report.StartStep("Looking for AR process to run");
            if (GetElementRowNumber(GetElementBy.ClassName, "k-selectable", columnName, value,
                        checkTheCheckbox: true) == -1)
                Assert.Fail("AR process was not found");
            //Report.StartStep($"Press '{title}' button");
            switch (module)
            {
                case ReconciliationModule.Cash:
                    title += " (Cash)";
                    break;
                case ReconciliationModule.Reconciliation:
                    title += " (Reconciliation)";
                    break;
                default:
                    break;
            }
            title += " (Run In Batch Task)";
            try
            {
                var runButton = WebDriver.GetElementByQuery(GetElementBy.Tagname,
                    $"a[title='{title}']");
                runButton.Click2();
                //AllureNextReport.FinishStep();
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
            //var dialogDiv = WebDriver.GetElementById("popupDialog");
            //Report.StartStep($"Select run as '{TestUserName}'");
            try
            {
                SelectValueInDropDownList(WebDriver.GetElementById("User").GetParent(), TestUserName);
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
            //Report.StartStep("Press dialog 'Yes' button");
            try
            {
                var yesButton = WebDriver.GetElementByQuery(GetElementBy.ClassName, "modal-footer")
                    .GetElementByQuery(GetElementBy.ClassName, "btn.btn-primary");
                yesButton.Click2();
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        public void GetAutoRecStatistic()
        {
            //Report.StartStep("Verify results - State and Status of autorec run");
            try
            {
                var rowElement = GetTableBodyRows(null)[0];
                rowElement.GetElementByClassName("k-hierarchy-cell").GetElementByTagName("a").Click2();

                var status = "Running";
                var inew = 0;
                var tries = 20;
                //Waiting for when status is Running
                while ((status == "Running" || status == "New") && tries > 0)
                {
                    // Find detail status results table
                    var detailCell = DriverWait.Until(d => WebDriver.GetElementByClassName("k-detail-cell"));

                    var subTable = DriverWait.Until(d => detailCell.GetElementByClassName("k-selectable"));

                    status = GetTableCellText(0, "Scheduled Tasks State", subTable);

                    switch (status)
                    {
                        case "Running":
                            //Report.StartStep("Refresh...");
                            try
                            {
                                DriverWait.Until(d => detailCell.RefreshButton()).Click2();
                            }
                            finally
                            {
                                //AllureNextReport.FinishStep();
                            }
                            --tries;
                            break;
                        case "New":
                            if (++inew <= 20) break;
                            //Report.StartStep("Process was not started");
                            //AllureNextReport.FinishStep();
                            //AllureNextReport.FinishStep();
                            return;
                        default:
                            var state = GetTableCellText(0, "Scheduled Tasks Last Run State", subTable);
                            //Report.StartStep(
                                //$@"Task was finished with status '{state}' within {
                                //    GetTableCellText(0, "Scheduled Tasks Last Run Duration", subTable)}");
                            //AllureNextReport.FinishStep();
                            if (state == "Error")
                            {
                                var result = GetTableCell(0, GetColumnNumber("Scheduled Tasks Result", subTable),
                                        subTable)
                                    .GetElementByQuery(GetElementBy.Tagname, "span")
                                    .GetAttrib("innerText");
                                //AllureNextReport.FinishStep();
                                //Report.StartStep($"Process was finished with follow error message:\n{result}");
                                //AllureNextReport.FinishStep();
                                Assert.Fail("Autoreconciliation was failed!");
                            }
                            --tries;
                            break;
                    }

                    WebDriver.WaitForReadyState();
                }
            }
            finally
            {
                //AllureNextReport.FinishStep();
            }
        }

        public string AllertChecker(bool errorexpected = false)
        {
            WebDriver.WaitForReadyState(errorexpected);
            WebDriver.CloseRedis(emptyform: errorexpected);
            var gridArea = WebDriver.GetElementByClassName("gridArea");
            var alertHeader = gridArea.GetElementByClassName("alert-heading") ?? WebDriver.GetElementByClassName("alert-error");
            if (alertHeader == null) return "";
            return alertHeader.GetElementValue();
        }

        public void OpenCloseCommentSlider(bool flag = true)
        {
            var commentsLinks = WebDriver.FindElements(By.LinkText("Comments"));
            var firstOrDefault = commentsLinks.FirstOrDefault(el => el.IsVisiblbe());
            if (firstOrDefault == null) Assert.Fail("Comment link was not found");
            firstOrDefault.Click2();
            try
            {
                if (flag)
                    DriverWait.WaitForElementIsVisible(WebDriver.GetElementByClassName("comments-slider"));
                else
                    DriverWait.WaitForElementIsNotVisible(WebDriver.GetElementByClassName("comments-slider"));
            }
            catch (Exception)
            {
                if ((flag && !WebDriver.GetElementByClassName("comments-slider").IsVisiblbe()) || (!flag && WebDriver.GetElementByClassName("comments-slider").IsVisiblbe()))
                    Assert.Fail($"Slider is{(flag ? "" : " not")} shown, but should be{(!flag ? "" : " not")}");
                throw;
            }
        }

        private void _switchLang(string lang, IWebElement langInput)
        {
            var langCaption = WebDriver.GetListItemByLang(lang.WhatIsLang());
            SelectValueInDropDownList(langInput, langCaption);
            WebDriver.Pause();
            WebDriver.WaitForReadyState();
        }

        public void SetLanguage(string lang = "en", bool force = false)
        {
            var langInput = WebDriver.GetElementByClassName("language .k-input-value-text");
            var language = WebDriver.GetLanguageValue();//langInput.GetElementValue();//GetAttribute("value");

            #region cookie language check
            var cookielang = WebDriver.GetCookies().GetCookieLang();
            if ((cookielang != null && !language.WhatIsLang().Equals(cookielang.WhatIsLang())) || !language.WhatIsLang().Equals(lang))
            {
                _switchLang(cookielang, langInput);
                langInput = WebDriver.GetElementById("login-select-language");
                language = WebDriver.GetLanguageValue();//langInput.GetElementValue();//GetAttribute("value");
            }
            #endregion

            if (force || !language.WhatIsLang().Equals(lang))
            {
                //Report.StartStep($"Switch language from '{language}' to '{lang}'");
                try
                {
                    _switchLang(lang, langInput);
                }
                catch
                { throw; }
                finally
                {
                    //AllureNextReport.FinishStep();
                }
            }
            else
            {
                //Report.StartStep($"Language is already set to '{language}'");
                //AllureNextReport.FinishStep();
            }
        }

        public void SelectInCheckBoxList(string treeId, string[] treeValues)
        {
            var treeContainer = WebDriver.GetElementById(treeId);
            foreach(var item in treeValues)
            {
                var value = item.Split(";");
                var chkBox = treeContainer.GetElementByOneAttribute(value[0], "input", "aria-label");
                var val = value.Length < 2 || string.IsNullOrEmpty(value[1]) ? "true" : value[1];
                CheckOrUncheckTheCheckbox(val, chkBox);
            }
        }
    }
}
