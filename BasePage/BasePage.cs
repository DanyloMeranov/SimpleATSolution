using System.Collections.Generic;
using BaseDriver;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TestTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BasePage
{
    public class BasePage<M>
        where M : BasePageElementMap, new()
    {
        public readonly string _appUrl = $"http://{ConfigSettingsReader.AppSite}:{ConfigSettingsReader.AppPort}";
        private readonly string _entityUrl;
        private readonly string _entityMenuPoint;
        private List<CardItem> _cardItems;
        private M _page;

        public string TestUserName { get; private set; }

        public string DownloadPath => ConfigSettingsReader.DownloadPath;

        public List<CardItem> CardItems
        {
            get
            {
                return _cardItems ??= new List<CardItem>();
            }
            set
            {
                _cardItems = value;
            }
        }

        protected IWebDriver WebDriver => _page != null ? _page.WebDriver : Page.WebDriver;

        protected WebDriverWait DriverWait => _page.DriverWait;

        protected Driver MainDriver;

        public BasePage(string entityUrl, string menuPoint)
        {
            _entityUrl = entityUrl;
            _entityMenuPoint = menuPoint;
            if (string.IsNullOrEmpty(TestUserName))
                TestUserName = ConfigSettingsReader.TestUserName;
            _page = new M { CurrentUser = TestUserName, CurrentPass = ConfigSettingsReader.TestUserPass, AppUrl = _appUrl, EntityUrl = _entityUrl, EntityMenuPoint = _entityMenuPoint };
        }

        public M Page => _page ??= new M { CurrentUser = TestUserName, CurrentPass = ConfigSettingsReader.TestUserPass, AppUrl = _appUrl, EntityUrl = _entityUrl, EntityMenuPoint = _entityMenuPoint };

        public void PageUpdate()
        {
            _page = new M { CurrentUser = TestUserName, CurrentPass = ConfigSettingsReader.TestUserPass, AppUrl = _appUrl, EntityUrl = _entityUrl, EntityMenuPoint = _entityMenuPoint }; // { MainDriver = MainDriver };
        }

        public void UserUpdate(string newUser)
        {
            TestUserName = newUser;
            _page.CurrentUser = TestUserName;
        }

        public void NavigateNew(string entityName = "", bool external = false)
        {
            if (!string.IsNullOrEmpty(entityName) && entityName.Equals("Elmah"))
            {
                Navigate(withoutjQ: true, external: external);
                return;
            }
            Navigate(external: external);
            return;
        }

        public void Navigate(string shortUrl = null, bool withoutjQ = false, bool external = false)
        {
            shortUrl ??= _entityUrl;
            var _tries = 5;
            while (true)
            {
                try
                {
                    if (shortUrl.Contains("http://") || shortUrl.Contains("https://"))
                        WebDriver.Navigate().GoToUrl(shortUrl);
                    else
                        WebDriver.Navigate().GoToUrl(_appUrl + (shortUrl.ToLower().Contains("elmah")? "": ConfigSettingsReader.Tenant) + shortUrl);
                    WebDriver.WaitForReadyState(jQ: !withoutjQ, refreshable: true);
                    if (ConfigSettingsReader.DebugLvl == 1)
                        try
                        {
                            StartStep($"After navigate cookie: '{WebDriver.GetCookies()}'");
                            FinishStep();
                        }
                        catch { }
                    if (string.IsNullOrEmpty(shortUrl) || shortUrl.Contains("useraccount/logoff") || shortUrl.Contains("elmah")) return;
                    Page.SetLanguage();
                }
                catch (Exception e)
                {
                    if (Page.WebDriver.CheckError401())
                        throw;
                    if (_tries-- > 0)
                    {
                        Refresh(external);
                        continue;
                    }
                    if (e.SkipableFail()) throw;
                    //Driver.Report.LogFailedStepWithFailedTestCase(e);
                    if (external)
                        Page.AddElmahDetail();
                    throw;
                }
                break;
            }
        }

        public void Refresh(bool external = false)
        {
            try
            {
                WebDriver.Navigate().Refresh();
                WebDriver.WaitForReadyState(refreshable: true);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                if (external)
                    Page.AddElmahDetail();
                throw;
            }
        }

        public void Backward()
        {
            WebDriver.Navigate().Back();
        }

        /// <summary>
        /// Method creates a new filter with specified name or selects an already existing one
        /// </summary>
        /// <param name="fName">Filter name (by default it is "NewFilterName")</param>
        /// <param name="isDefaultBased"></param>
        /// <param name="tType"></param>
        /// <returns>Returns true if filter was created or false if it already exists</returns>
        public bool CreateNewOrSelectFilter(string fName = "NewFilterName", bool isDefaultBased = true, ConfigurationType tType = ConfigurationType.Filter)
        {
            return Page.CreateNewOrSelectExistingConfiguration(fName, tType, isDefaultBased);
        }

        /// <summary>
        /// Method creates a new layout with specified name or selects an already existing one
        /// </summary>
        /// <param name="vName">Layout name (by default it is "NewViewName")</param>
        /// <param name="isDefaultBased"></param>
        /// <param name="pagingDisabled"></param>
        /// <param name="columnsWidthBestFit"></param>
        /// <param name="allowFrozenColumns"></param>
        /// <param name="tType"></param>
        /// <returns>Returns true if layout was created or false if it already exists</returns>
        public bool CreateNewOrSelectLayout(string vName = "NewViewName", bool isDefaultBased = true, bool pagingDisabled = false, bool columnsWidthBestFit = true, bool allowFrozenColumns = true, ConfigurationType tType = ConfigurationType.Layout)
        {
            return Page.CreateNewOrSelectExistingConfiguration(vName, tType, isDefaultBased, pagingDisabled,
                columnsWidthBestFit, allowFrozenColumns);
        }
        public bool DeleteFilter(BasePage<BasePageElementMap, BasePageValidator<BasePageElementMap>> basePage, string sName) =>
            DeleteFilter(basePage, new[] { sName });

        public bool DeleteFilter(BasePage<BasePageElementMap, BasePageValidator<BasePageElementMap>> basePage, string[] sNames)
        {
            basePage?.Navigate();
            return DeleteExistingConfiguration(sNames);
        }

        public bool DeleteView(BasePage<BasePageElementMap, BasePageValidator<BasePageElementMap>> basePage, string sName) =>
            DeleteView(basePage, new[] { sName });
        public bool DeleteView(BasePage<BasePageElementMap, BasePageValidator<BasePageElementMap>> basePage, string[] sNames)
        {
            basePage?.Navigate();
            return DeleteExistingConfiguration(sNames, ConfigurationType.Layout);
        }

        public bool DeleteExistingConfiguration(string[] sNames, ConfigurationType tType = ConfigurationType.Filter)
        {
            WebDriver.Assert500();
            var configName = (tType == ConfigurationType.Filter) ? "filter" : "layout";
            StartStep($"Trying to delete {configName}{(sNames.Length > 1 ? "s" : "")}");
            foreach (var sName in sNames)
            {
                StartStep($"Loocking for {configName} with name {sName}");
                var confLink = WebDriver.GetElementByQuery(GetElementBy.Tagname,
                        $"a[data-{configName}-name='{sName}']");
                if (confLink == null)
                    Assert.Fail($"{configName} with name {sName} was not found.");
                FinishStep();

                var opened = WebDriver.GetElementByClassName("open");
                if (opened == null)
                {
                    StartStep($"Opening {configName} list");
                    confLink.GetParent().GetParent().ClickUnderRedis();
                    FinishStep();
                }
                StartStep($"Opening {configName} with name {sName}");
                confLink.GetNextSibling("a").Click2();
                FinishStep();

                StartStep($"Deleting {configName} with name {sName}");
                IWebElement fDelete;
                DriverWait.WaitForElementIsVisible(fDelete = WebDriver.GetElementById("action-delete-configuration"));
                fDelete.Click2();
                FinishStep();

                Page.ModalButtonClick($"confirmDelete_{configName}", "Delete", false);

            }
            FinishStep();
            return true;
        }

        private static string ConditionToString(string denial, string condition)
        {
            var ret = (denial.ToLower() == "true" ? "not " : "") + condition;
            return ret;
        }

        public void FillFilterSection(string[] filterLines)
        {
            try
            {
                WebDriver.WaitForReadyState();
                WebDriver.WaitForjQueryReady();

                if (filterLines.Length == 0) return;
                var filterTable = WebDriver.GetElementByClassName("filter-config-table");
                var buttonAdd = filterTable.GetElementByClassName("filter-add-row");
                foreach (var fLine in filterLines)
                {
                    var columns = fLine.Split(';');
                    if (!string.IsNullOrEmpty(columns[3]) && columns[3].Contains("User")) columns[3] = TestUserName;

                    StartStep(
                        $"Add row: '{columns[0]}' {ConditionToString(columns[1], columns[2])} '{columns[3]}'");
                    buttonAdd.Click2();
                    Page.SelectValueInModalTreeBox(WebDriver.GetElementById("filterConfig"),
                        columns[0]);
                    var tableLine =
                        filterTable.GetElementByQuery(GetElementBy.AttributeWithValue, $"data-column='{columns[0].Split('|')[0]}'");
                    if (!string.IsNullOrEmpty(columns[1]))
                        Page.SwitchToggleControl(columns[1], tableLine.GetElementByClassName("toggle.btn"));
                    if (!string.IsNullOrEmpty(columns[2]))
                        Page.SelectValueInDropDownList(tableLine.GetElementByQuery(GetElementBy.ClassName, "k-dropdownlist.configPanel-item"),
                            columns[2]);
                    if (string.IsNullOrEmpty(columns[3]))
                    {
                        FinishStep();
                        return;
                    }
                    tableLine = tableLine.GetElementByClassName("val-cell");
                    var input = tableLine.GetElementByClassName("t-filter-lookup-display.k-input-inner") ??
                                tableLine.GetElementByClassName("configPanel-item.k-input") ??
                                tableLine.GetElementByClassName("ui-calc.k-input") ??
                                tableLine.GetElementByClassName("k-formatted-value.k-input") ??
                                tableLine.GetElementByClassName("configPanel-item.k-textbox") ??
                                tableLine.GetElementsByClassName("k-dropdownlist.configPanel-item")?[1];

                    if (input == null)
                    {
                        Page.CheckOrUncheckTheCheckbox("true", tableLine
                            .GetElementsByText(GetElementBy.Tagname, "label", columns[3])[0]
                            .GetParent()
                            .GetElementByQuery(GetElementBy.Tagname, "input.filterCheckbox"));
                    }
                    else
                    {
                        //should be changed to single-select scheme!
                        if (input.GetAttribute("class").Contains("lookup"))
                        {
                            var inputContainer = tableLine.GetElementByClassName("t-single-lookup-input-container").GetParent();
                            inputContainer.GetElementByQuery(GetElementBy.Tagname, "button.btn.filter-lookup").Click2();
                            Page.SelectInSingleSelectGrid(inputContainer, columns[3]);
                        }
                        else
                        {
                            if (input.GetAttribute("class").Contains("k-dropdownlist"))
                                Page.SelectValueInDropDownList(input, columns[3]);
                            else
                            {
                                if (input.GetAttribute("class").Contains("ui-calc"))
                                {
                                    var numerInput = tableLine.GetElementsByClassName("ui-calc.k-input-inner")[1];
                                    Page.SetNumberInNumericTextBox(columns[3], numerInput);
                                    WebDriver.WaitForReadyState();
                                }
                                else
                                {
                                    input.Click2();
                                    input.Clear();
                                    input.SendKeys(columns[3]);
                                    WebDriver.WaitForReadyState();
                                }
                            }
                        }
                    }
                    FinishStep();
                }
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                if (Page.CheckErrorRequest()) throw;
                Page.AddElmahDetail("filterconfigurationtab");
                throw;
            }
        }

        public void CreateFilter(string[] filterLines, string filterName = "NewFilterName", bool isDefaultBased = true, bool empty = false)
        {
            var done = false;
            var tries = 3;
            do
            {
                StartStep($"Create or select {filterName} filter");
                try
                {
                    if (!CreateNewOrSelectFilter(filterName, isDefaultBased))
                        return;
                }
                finally
                {
                    FinishStep();
                }

                StartStep($"Fill {filterName} filter with {filterLines.Length} filter setting lines");
                try
                {
                    FillFilterSection(filterLines);
                    done = true;
                }
                catch 
                {
                    if (--tries < 0)
                    {
                        throw;
                    }
                    Navigate();
                    continue;
                }
                finally
                {
                    FinishStep();
                }
            } while (!done);

            ClickSaveConfigurationButton(/*empty*/);
        }

        public void CreateLayout(string[] layoutLines, string layoutName = "NewViewName", bool empty = false)
        {
            if (!CreateNewOrSelectLayout(layoutName))
                return;

            WebDriver.WaitForjQueryReady();
            //TODO: set Layout options
            ClickSaveConfigurationButton(empty);
        }

        public void CreateViewWithCustomColumnByFullname(string viewName = "DefView")
        {
            if (!CreateNewOrSelectLayout(viewName))
                return;

            WebDriver.WaitForReadyState();
            WebDriver.WaitForjQueryReady();

            ClickSaveConfigurationButton();
        }

        public void CreateViewWithCustomColumnByFullname(string[] fullNames, string viewName = "ViewWithDesc", bool isDefaultBased = true, bool pagingDisabled = false, bool columnsWidthBestFit = true, bool allowFrozenColumns = true)
        {
            if (!CreateNewOrSelectLayout(viewName, isDefaultBased, pagingDisabled, columnsWidthBestFit, allowFrozenColumns))
                return;
            try
            {
                if (fullNames != null && fullNames.Count() != 0)
                    CheckElementsInLayoutTree(WebDriver.GetElementByQuery(GetElementBy.ClassName, "layout-column-grid"), fullNames);
                ClickSaveConfigurationButton();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail("layoutconfigurationtab");
                throw;
            }
        }

        public void ClickSaveConfigurationButton(bool empty = false)
        {
            StartStep("Save configuration");
            WebDriver.GetElementById("action-save-configuration").Click2(emptyform: empty);
            FinishStep();
        }

        public void ClickSaveReportButton()
        {
            StartStep("Save report");
            WebDriver.GetElementByClassName("save-portlet-filter").Click2();
            FinishStep();
        }

        public void ClickButtonByTagName(string tagname)
        {
            WebDriver.GetElementByQuery(getTableBy: GetElementBy.Tagname, value: tagname).Click2();
        }

        public void ClickRefreshGrid()
        {
            WebDriver.RefreshButton().Click2();
        }

        /// <summary>
        /// Method creates or selects view "ViewWithDesc"
        /// </summary>
        public void CreateViewWithDescriptionColumn(string liEntityName)
        {
            if (!CreateNewOrSelectLayout("ViewWithDesc"))
                return;

            WebDriver.WaitForReadyState();

            CheckElementsInLayoutTree(WebDriver.GetElementByQuery(GetElementBy.ClassName, "layout-column-grid"), new[] { liEntityName + ".Description", liEntityName + ".Description 2" }, mandatory: false);

            ClickSaveConfigurationButton();
        }

        private string GetTabNameByEnum(LayoutTabs layoutTabs)
        {
            return layoutTabs switch
            {
                LayoutTabs.Columns => "Columns",
                LayoutTabs.Grouping => "Grouping",
                LayoutTabs.Sorting => "Columns",
                LayoutTabs.Detail => "Detail",
                LayoutTabs.Aggregate => "Aggregate",
                LayoutTabs.GroupingAggregate => "Grouping",
                LayoutTabs.Chart => "Chart",
                _ => throw new ArgumentOutOfRangeException(nameof(layoutTabs), layoutTabs, null),
            };
        }

        private void CheckElementsInLayoutTree(IWebElement areaControl, IEnumerable<string> layoutItems, string tab = "column", bool mandatory = true)
        {
            if (!mandatory && (layoutItems == null || layoutItems.Count() == 0)) return;
            if (layoutItems == null || layoutItems.Count() == 0)
                Assert.Fail("List of items to select is empty!");
            StartStep("Opening Properties tree");
            var manageButton = areaControl.GetElementByQuery(GetElementBy.ClassName, "btn-sm.btn-primary");
            manageButton.Click2();
            WebDriver.WaitForReadyState();
            WebDriver.WaitForjQueryReady();
            FinishStep();

            var treeModalContainer = WebDriver.GetElementById($"layout-{tab}-tree-modal_entityPropertiesTree");
            foreach (var treeValues in layoutItems)
            {
                StartStep($"Choose '{treeValues}' element");
                Page.SelectValueInModalTreeBox(WebDriver.GetElementById("layoutConfig"), treeValues, true, mandatory: mandatory);
                FinishStep();
            }
            treeModalContainer.GetParent().GetElementByClassName("cancelColumnsTree").Click2();
        }

        private string GetLayoutClassSubName(LayoutTabs layoutSection)
        {
            return layoutSection switch
            {
                LayoutTabs.Columns => "column",
                LayoutTabs.Grouping => "grouping",
                LayoutTabs.Sorting => "sorting",
                LayoutTabs.Detail => "detail",
                LayoutTabs.GroupingAggregate => "grouping-aggregate",
                _ => "",
            };
        }

        public void FillTabLayout(string[] layoutItems, LayoutTabs layoutTab = LayoutTabs.Columns, bool goTo = true, bool clearExists = false, bool[] toggles = null, string[] layoutItemsDirection = null)
        {
            try
            {
                var lcp = WebDriver.GetElementById("LayoutConfigPanel");
                var tab = lcp.GetElementByTextWithTag(GetTabNameByEnum(layoutTab), "a") ??
                    lcp.GetElementByTextWithTag(GetTabNameByEnum(LayoutTabs.Columns), "a");
                if (goTo)
                {
                    StartStep($"Go to {GetTabNameByEnum(layoutTab)} tab");
                    tab.Click2();
                    FinishStep();
                }
                else
                {
                    WebDriver.WaitForReadyState();
                    WebDriver.WaitForjQueryReady();

                }

                var areaId = tab.GetAttribute("aria-controls");
                IWebElement areaControl = layoutTab switch
                {
                    LayoutTabs.Chart => WebDriver.GetElementById("chartConfigPanel"),
                    LayoutTabs.Detail => WebDriver.GetElementById(areaId).GetElementByClassName("k-treeview"),
                    _ => WebDriver.GetElementById(areaId).GetElementByQuery(GetElementBy.ClassName, $"layout-{GetLayoutClassSubName(layoutTab)}-grid") ??
                         WebDriver.GetElementById(areaId).GetElementByQuery(GetElementBy.ClassName, $"layout-{GetLayoutClassSubName(LayoutTabs.Columns)}-grid"),
                };
                DriverWait.WaitForElementIsAvailable(areaControl);
                switch (layoutTab)
                {
                    case LayoutTabs.Grouping:
                        if (clearExists)
                            areaControl.GetParent().ClearSelectedItems(GetLayoutClassSubName(layoutTab));
                        if (toggles != null && toggles.Length > 0)
                        {
                            //RememberCollapsedGroups - toggle 1
                            Page.SwitchToggleControl(toggles[0].ToString(),
                                WebDriver.GetElementById("RememberCollapsedGroups").GetParent());
                            //ServerGrouping - toggle 2
                            Page.SwitchToggleControl(toggles[1].ToString(),
                                WebDriver.GetElementById("ServerGrouping").GetParent());
                        }
                        CheckElementsInLayoutTree(areaControl, layoutItems, GetLayoutClassSubName(layoutTab));
                        break;
                    case LayoutTabs.Sorting:
                        CheckSortingItemsInLayoutGrid(areaControl, layoutItems, layoutItemsDirection, clearExists);
                        break;
                    case LayoutTabs.Detail:
                        foreach (var layItem in layoutItems)
                        {
                            StartStep($"Expanding {layItem} path.");
                            BasePageElementMap.ExpandPropertiesTree(areaControl, layItem);
                            FinishStep();

                            StartStep($"Selecting {layItem} item.");
                            var treeItem = areaControl.GetElementByAttributeWithValue($"data-value='{layItem}'");
                            Page.CheckOrUncheckTheCheckbox("true", treeItem.GetElementByClassName("columnCheckbox"));
                            FinishStep();
                        }
                        break;
                    case LayoutTabs.Aggregate:
                    case LayoutTabs.GroupingAggregate:
                    case LayoutTabs.Columns:
                        if (clearExists)
                            areaControl.ClearSelectedItems(GetLayoutClassSubName(layoutTab));
                        CheckElementsInLayoutTree(areaControl, layoutItems, GetLayoutClassSubName(layoutTab));
                        break;
                    case LayoutTabs.Chart:
                        if (!string.IsNullOrEmpty(layoutItems[0]))
                            Page.SelectValueInDropDownList(
                                WebDriver.GetElementByQuery(GetElementBy.AttributeWithValue, "aria-controls=Chart_ChartDisplayType_listbox"), layoutItems[0]); //value1
                        if (!string.IsNullOrEmpty(layoutItems[1]))
                            Page.SelectValueInDropDownList(WebDriver.GetElementByQuery(GetElementBy.AttributeWithValue, "aria-controls=Chart_ChartType_listbox"),
                                layoutItems[1]); //value2
                        if (!string.IsNullOrEmpty(layoutItems[2]))
                            Page.SelectValueInDropDownList(WebDriver.GetElementByQuery(GetElementBy.AttributeWithValue, "aria-controls=Chart_CategoryAxis_listbox"),
                                layoutItems[2]); //value3
                        var gridOfCharts = WebDriver.GetElementById("gridOfCharts");
                        gridOfCharts.GetElementByClassName("add-new-row-description").Click2();
                        if (!string.IsNullOrEmpty(layoutItems[3]))
                        {
                            Page.SelectValueInDropDownList(gridOfCharts.GetElementsByClassName("k-picker.k-dropdownlist")[0], layoutItems[3]); //value4
                            Page.SelectValueInDropDownList(gridOfCharts.GetElementsByClassName("k-picker.k-dropdownlist")[2], layoutItems[4]); //value5
                            gridOfCharts.GetElementByClassName("glyphicon-save").Click2();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(layoutTab), layoutTab, null);
                }
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                if (Page.CheckErrorRequest()) throw;
                Page.AddElmahDetail("layoutconfigurationtab");
                throw;
            }
        }

        private void CheckSortingItemsInLayoutGrid(IWebElement areaControl, string[] layoutPropertyItems, string[] layoutItemsDirections, bool clearExists)
        {
            if (layoutItemsDirections == null || (layoutItemsDirections.Length != layoutPropertyItems.Length))
                Assert.Fail("Incorrect data in properties Direction");
            var tableHeader = areaControl.GetElementByClassName("k-grid-header").GetTable(GetElementBy.Tagname);
            var colNumSort = Page.GetColumnNumber("Sort", tableHeader);
            var table = areaControl.GetElementByClassName("k-grid-content").GetTable(GetElementBy.Tagname);
            if (clearExists)
            {
                StartStep("Clearing sorting directions in all columns");
                for (int i = 0; i < table.GetTableBodyRows().Count; i++)
                {
                    var currentCell = Page.GetTableCell(i, colNumSort, table).GetElementByClassName("sorting-direction");
                    SetSortingDirection(currentCell, "None");
                }
                FinishStep();
            }
            int index = 0;
            var colNumProperty = Page.GetColumnNumber("Property", tableHeader);
            bool isExist;
            foreach (var layoutItem in layoutPropertyItems)
            {
                isExist = false;
                for (int i = 0; i < table.GetTableBodyRows().Count; i++)
                {
                    var propertyCell = Page.GetTableCell(i, colNumProperty, table).GetElementByQuery(GetElementBy.Tagname, "span");
                    var sortingCell = Page.GetTableCell(i, colNumSort, table).GetElementByClassName("sorting-direction");
                    if (propertyCell.GetElementTitle() == layoutItem.ClassTreePathToSymbolicString())
                    {
                        StartStep($"Setting {layoutItemsDirections[index]} direction for {propertyCell.Text} property");
                        SetSortingDirection(sortingCell, layoutItemsDirections[index]);
                        index++;
                        isExist = true;
                        FinishStep();
                        break;
                    }
                }
                if (!isExist)
                    Assert.Fail($"Entity {layoutItem} not found");
            }
        }

        public void SetSortingDirection(IWebElement element, string direction)
        {
            int attempts = 0;
            while (element.GetAttribute("title") != direction)
            {
                element.Click2();
                attempts++;
                if (attempts > 2)
                    Assert.Fail($"Direction {direction} not found");
            }
        }

        public void VerifyCommentExistsInTheList(string expectedText)
        {
            var dialog = Page.GetCommentsPopUpDialogAndWaitForItsVisibility;
            var actualComment = dialog.GetElementsByClassName("comment-text").FirstOrDefault(el => el.Text == expectedText);
            Assert.IsNotNull(actualComment, $"Comment with text '{expectedText}' was not found!");

            // Close comments form
            Page.OpenCloseCommentSlider(false);
        }

        /// <summary>
        /// Open action menu
        /// </summary>
        /// <param name="actionButtonName">Expanding button text</param>
        /// <param name="actionbar">Action bar class</param>
        public void OpenActionMenu(string actionButtonName = "More", string actionbar = "grid-actions")
        {
            Page.OpenActionMenu(actionButtonName, actionbar);
        }

        public IWebElement GetFirstRowIfGroup(ReadOnlyCollection<IWebElement> rows, string checkValue)
        {
            var i = 0;
            var rowElement = rows[i];
            while (rowElement.GetAttrib("class").Contains("k-grouping-row"))
            {
                if (rows.Count < i + 2) Assert.Fail($"Element '{checkValue}' was not found!");
                rowElement.GetElementByClassName("k-i-expand")?.Click2();
                rowElement = rows[++i];
            }
            return rowElement;
        }

        protected void StartStep(string stepDescription)
        {
            //Driver.Report.StartStep(stepDescription);
        }

        protected void FinishStep()
        {
            //AllureNextReport.FinishStep();
        }

        private string CheckAuditId(string prefix, int revNumber, string sufix, bool isId = true, bool isSingle = false)
        {
            var additional = "_AuditedEntity";
            if (isId)
                if (revNumber < 0)
                {
                    if (isSingle && WebDriver.GetElementById($"{prefix}{sufix}") != null)
                        return $"{prefix}{sufix}";
                    else if (isSingle && WebDriver.GetElementById($"{prefix}{additional}{sufix}") != null)
                        return $"{prefix}{additional}{sufix}";
                }
                else if (WebDriver.GetElementById($"{prefix}{revNumber}{sufix}") != null)
                    return $"{prefix}{revNumber}{sufix}";
                else if (WebDriver.GetElementById($"{prefix}{revNumber}{additional}{sufix}") != null)
                    return $"{prefix}{revNumber}{additional}{sufix}";
                else if (WebDriver.GetElementById($"{prefix}{(revNumber == 1 ? 2 : 1)}{sufix}") != null)
                    return $"{prefix}{(revNumber == 1 ? 2 : 1)}{sufix}";
                else if (WebDriver.GetElementById($"{prefix}{(revNumber == 1 ? 2 : 1)}{additional}{sufix}") != null)
                    return $"{prefix}{(revNumber == 1 ? 2 : 1)}{additional}{sufix}";
                else Assert.Fail($"Id '{prefix}{revNumber}{sufix}', '{prefix}{revNumber}{additional}{sufix}', '{prefix}{(revNumber == 1 ? 2 : 1)}{sufix}' or '{prefix}{(revNumber == 1 ? 2 : 1)}{additional}{sufix}' was not found!");
            else
            {
                if (WebDriver.GetElementByName($"{prefix}{revNumber}{sufix}") != null)
                    return $"{prefix}{revNumber}{sufix}";
                else if (WebDriver.GetElementByName($"{prefix}{revNumber}{sufix}") != null)
                    return $"{prefix}{(revNumber == 1 ? 2 : 1)}{sufix}";
                else Assert.Fail($"Name '{prefix}{revNumber}{sufix}' or '{prefix}{(revNumber == 1 ? 2 : 1)}{sufix}' was not found!");
            }
            return "";
        }

        public List<CardItem> RecreateCardItemsForAudit(int revNumber, string serialNumber = "2", string revisionType = "Added")
        {
            var oldCardItems = CardItems.ToList();
            CardItems.Clear();

            if (!string.IsNullOrEmpty(serialNumber) || !string.IsNullOrEmpty(revisionType))
            {
                CardItems.Add(new CardItem
                {
                    NameOrId = $"Revision_{revNumber}_RevisionInfo_SerialNumber",
                    FieldType = FieldType.Text,
                    Value = serialNumber
                });
                var revUser = (WebDriver.GetElementById($"Revision_{revNumber}_RevisionInfo_UserCode") != null)
                              ? $"Revision_{revNumber}_RevisionInfo_UserCode"
                              : (WebDriver.GetElementById($"Revision_1_RevisionInfo_UserCode") != null)
                                ? $"Revision_1_RevisionInfo_UserCode"
                                : "";
                if (!string.IsNullOrEmpty(revUser))
                    CardItems.Add(new CardItem
                    {
                        NameOrId = revUser,
                        FieldType = FieldType.Text,
                        Value = WebDriver.GetCurrentUser()
                    });
                CardItems.Add(new CardItem
                {
                    NameOrId = $"Revision_{revNumber}_RevisionInfo_RevisionType",
                    FieldType = FieldType.Text,
                    Value = revisionType
                });
            }

            foreach (var item in oldCardItems)
            {
                var ci = item;
                if (!string.IsNullOrEmpty(item.AuditId))
                {
                    if (item.AuditId.Equals("NONE"))
                        continue;
                    ci.NameOrId = item.AuditId.StartsWith('_') || item.AuditId.StartsWith('.') ? $"Revision_{revNumber}{item.AuditId}" : item.AuditId;
                    CardItems.Add(ci);
                    continue;
                }
                switch (item.FieldType)
                {
                    case FieldType.Date:
                        var dateId = WebDriver.GetElementByOneAttribute($"Revision_{revNumber}_{item.NameOrId.Replace('.', '_')}", "label", "for").GetAttrib("id");
                        ci.NameOrId = dateId;
                        CardItems.Add(ci);
                        break;
                    case FieldType.DetailTable:
                    case FieldType.ListView:
                    case FieldType.EntityPropertiesTree:
                    case FieldType.SelectedPropertiesTree:
                    case FieldType.FieldsParameter:
                    case FieldType.Multi:
                    case FieldType.Extender:
                        break;
                    case FieldType.Text:
                    case FieldType.Single:
                        if (item.NameOrId == "Entity_Notes") continue;
                        ci.NameOrId = item.NameOrId.Equals("Entity_Id")
                            ? CheckAuditId($"Revision_", revNumber, $"_{item.NameOrId}")
                            : item.NameOrId.Contains("Master_Currency")
                            ? CheckAuditId("EntityPicker_wrapper_", -1, item.NameOrId, true, item.FieldType == FieldType.Single)
                            : CheckAuditId($"Revision_", revNumber, $"_{item.NameOrId}", true, item.FieldType == FieldType.Single);
                        CardItems.Add(ci);
                        break;
                    default:
                        ci.NameOrId = CheckAuditId($"Revision_", revNumber, $"_{item.NameOrId}");
                        CardItems.Add(ci);
                        break;
                }
            }
            return oldCardItems;
        }
    }


    public class BasePage<M, V> : BasePage<M>
        where M : BasePageElementMap, new()
        where V : BasePageValidator<M>, new()
    {
        public BasePage(string entityUrl, string menuPoint = "") : base(entityUrl, menuPoint)
        {
            _validator = null;
        }

        public BasePage(string entityUrl, string menuPoint, Driver mainDriver) : base(entityUrl, menuPoint)
        {
            Page.MainDriver = mainDriver;
            _validator = null;
        }

        private V _validator;
        private V Validator => _validator ??= new V();

        public V Validate()
        {
            //            if (Validator.Page == null)
            Validator.SetPage(Page);
            return Validator;
        }

        public void TestFilter(string[] filterLines, string filterName = "NewFilterName", bool isDefaultBased = true, bool empty = false)
        {
            try
            {
                StartStep($"Creating {filterName} filter");
                CreateFilter(filterLines, filterName, isDefaultBased, empty);
                FinishStep();
                /*
                if (empty)
                {
                    WebDriver.Pause();
                    Assert.AreEqual("The invalid filter can not be saved!", (string)WebDriver.Execute("return $('#popupDialog.fade.in').find('.modal-body')[0]?.innerText"));
                }
                else
                {
                //*/
                Validate().AssertFilterExists(filterName);
                StartStep($"Deleting {filterName} filter");
                DeleteFilter(null, filterName);
                FinishStep();
                //}
            }
            catch 
            {
                Page.AddElmahDetail("filterconfigurationtab");
                throw;
            }
        }

        public void TestView(string[] fullNames, string viewName = "ViewWithDesc", bool isDefaultBased = true, bool pagingDisabled = false, bool columnsWidthBestFit = true, bool allowFrozenColumns = true, bool empty = false)
        {
            try
            {
                CreateViewWithCustomColumnByFullname(fullNames, viewName, isDefaultBased, pagingDisabled, columnsWidthBestFit, allowFrozenColumns);
                if (empty)
                    Assert.AreEqual("Columns is required", ((string)WebDriver.Execute("return document.querySelector('#Columns_validationMessage').innerText")).Trim());
                else
                {
                    Validate().AssertLayoutExists(viewName);
                    DeleteView(null, viewName);
                }
            }
            catch {
                //Driver.Report.LogFailedStepWithFailedTestCase(e);
                Page.AddElmahDetail("layoutconfigurationtab");
                throw;
            }
        }

        /// <summary>
        /// Create entity card with empty form
        /// </summary>
        /// <param name="spnLst">Error span list</param>
        /// <param name="buttonCreate">Create button element</param>
        private void _createEntityWithEmptyForm(string[] spnLst,string entityName, string buttonCreate, string dialogTextAfterSave)
        {
            WebDriver.WaitForReadyState(refreshable: true);
            Page.GetButtonLinkByTitle(buttonCreate.Localize(Page.currentLanguage)).ClickUnderRedis();
            //BeforeSaveAction();
            Page.SetLanguage();
            if (string.IsNullOrEmpty(dialogTextAfterSave))
                Page.ClickSaveButtonWithjQ(true);
            else
            {
                Page.ClickSaveButtonWithjQ();
                Page.PopupDialogClick(dialogTextAfterSave, true);
            }
            //AfterSaveAction();

            Validate().AssertNoAlertPresent();
            Validate().AssertThatMainValidationErrorMessageIsShown();
            Validate().AssertThatValidationErrorMessagesAreShown(spnLst, entityName);
            //Validate().AssertThatValidationErrorHintsAreShown(spnLst, entityName, required);

            Page.CancelButton.ClickUnderRedis(emptyformbefore: true);
            WebDriver.WaitForGridReady();
        }

        public void CreateEntityWithEmptyForm(string[] spnLst, string entityName = "", string buttonCreate = "", string dialogTextAfterSave = "")
        {
            try
            {
                var button = buttonCreate != "" ? buttonCreate : "Create";
                _createEntityWithEmptyForm(spnLst, entityName, button, dialogTextAfterSave);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        private void _createEntity(string[] checkBeforeValues, string[] columnNames)
        {
            Validate().AssertEntityDoesNotExistInGrid(columnNames, checkBeforeValues);
            var tries = 3;
            do
            {
                Page.CreateButton.ClickUnderRedis();
            }
            while (FillEntity(tries: --tries));

            //BeforeSaveAction();
            Page.ClickSaveButtonWithjQ();
            //AfterSaveAction();
            WebDriver.WaitForGridReady();

            Navigate();
            //Page.First();
            var row = Validate().AssertEntityExistsInGrid(columnNames, checkBeforeValues);

            var rowElement = Page.GetTableBodyRows(null)[row];

            Page.EditGridIcon(rowElement).Click2();

            VerifyEntityCard();

            Page.CancelButton.ClickUnderRedis();
            //WebDriver.WaitForReadyState(/*"Filtering row"*/);
            WebDriver.WaitForGridReady();

            Navigate();
            //Page.First();
            Validate().AssertEntityExistsInGrid(columnNames, checkBeforeValues);
        }

        public void CreateEntity(string[] checkBeforeValues, string[] columnNames)
        {
            try
            {
                _createEntity(checkBeforeValues, columnNames);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        public void EntityCreation(string buttonCreate = "", bool nosave = false)
        {
            var button = buttonCreate != "" ? buttonCreate : "Create";
            var tries = 3;
            do
            {
                //Page.SetLanguage();
                Page.GetButtonLinkByTitle(button).ClickUnderRedis();
            }
            while (FillEntity(tries: --tries));

            if (nosave) return;

            Page.ClickSaveButtonWithjQ();
            WebDriver.WaitForGridReady();
        }

        private void EntityVerification(string checkValue, string columnName, string groupName = "", bool byIcon = true, string filter = "DEFFLTR", bool force = false)
        {
            var tries = 5;
            while (true)
            {
                try
                {
                    //Page.First();
                    Navigate();
                    Validate().AssertEntityExistsInGrid(columnName, checkValue, groupElementName: groupName, filter: filter, force: force);

                    var rowElement = GetFirstRowIfGroup(Page.GetTableBodyRows(null), checkValue);
                    if (byIcon)
                    {
                        Page.EditGridIcon(rowElement).Click2();
                    }
                    else
                    {
                        Page.CheckBoxGridItem(rowElement).Click2();
                        //Page.ActionList();
                        OpenActionMenu();
                        Page.EditButton.ClickUnderRedis();
                    }

                    VerifyEntityCard();

                    Page.CancelButton.ClickUnderRedis();

                    WebDriver.WaitForGridReady();

                    //Page.First();
                    Navigate();
                    Validate().AssertEntityExistsInGrid(columnName, checkValue, groupElementName: groupName);
                    return;
                }
                catch
                {
                    if (tries-- < 1) throw;
                    Driver.ElmahAdded(false);
                }
            }
        }

        /// <summary>
        /// Create entity
        /// </summary>
        /// <param name="checkBeforeValue">Entity's value to create</param>
        /// <param name="columnName">Columns name, in which to search for value, by default is 'Code'</param>
        /// <param name="buttonCreate">Create button to click, if not defined - default Create button is used</param>
        /// <param name="groupName">Set group element name. Empty value means not grouping. Default value ""</param>
        /// <param name="nosave">Optional trigger to leave card after filling without save</param>
        private void _createEntity(string checkBeforeValue, string columnName = "Code", string buttonCreate = "", string groupName = "", bool nosave = false)
        {
            StartStep($"Creating entity with {columnName} = '{checkBeforeValue}'");
            try
            {
                Validate().AssertEntityDoesNotExistInGrid(columnName, checkBeforeValue);

                EntityCreation(buttonCreate, nosave);

                if (nosave)
                {
                    FinishStep();
                    return;
                }

                EntityVerification(checkBeforeValue, columnName, groupName);
            }
            finally
            {
                FinishStep();
            }
        }

        public void CreateEntity(string checkBeforeValue, string columnName = "Code", string buttonCreate = "", string groupName = "", bool nosave = false)
        {
            try
            {
                _createEntity(checkBeforeValue, columnName, buttonCreate, groupName, nosave);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        /// <summary>
        /// Edit entity (finding by several values)
        /// </summary>
        /// <param name="checkBeforeValue">Value array looking for to edit</param>
        /// <param name="checkAfterValue">Value array looking for after edit (may be null)</param>
        /// <param name="columnName">Array of column values</param>
        /// <param name="byIcon">Should be icon use or no (<b>true</b>/false)</param>
        private void _editEntity(string[] checkBeforeValue, string[] checkAfterValue, string[] columnName, bool byIcon = true)
        {
            var tries = 3;
            do
            {
                var _row = Validate().AssertEntityExistsInGrid(columnName, checkBeforeValue, checkTheCheckbox: !byIcon);
                var _rowElement = Page.GetTableBodyRows(null)[_row];
                if (byIcon)
                {
                    Page.EditGridIcon(_rowElement).Click2();
                }
                else
                {
                    //Page.ActionList();
                    Page.EditButton.ClickUnderRedis();
                }
            } while (FillEntity(TestPurpose.Edit, tries: --tries));

            //BeforeSaveAction();
            Page.ClickSaveButtonWithjQ();
            //AfterSaveAction();

            WebDriver.WaitForGridReady();

            Navigate();
            var row = Validate().AssertEntityExistsInGrid(columnName, checkAfterValue ?? checkBeforeValue,
                 checkTheCheckbox: !byIcon);
            //Page.SetLanguage();
            var rowElement = Page.GetTableBodyRows(null)[row];
            if (byIcon)
            {
                Page.EditGridIcon(rowElement).Click2();
            }
            else
            {
                //Page.ActionList();
                Page.EditButton.ClickUnderRedis();
            }

            VerifyEntityCard();

            Page.CancelButton.ClickUnderRedis();
            //WebDriver.WaitForReadyState(/*"Filtering row"*/);
            
            WebDriver.WaitForGridReady();

            Navigate();
            //Page.First();
            Validate().AssertEntityExistsInGrid(columnName, checkAfterValue ?? checkBeforeValue);
        }

        public void EditEntity(string[] checkBeforeValue, string[] checkAfterValue, string[] columnName, bool byIcon = true, bool clearBefore = false, bool useReversedCards = false)
        {
            try
            {
                _editEntity(checkBeforeValue, checkAfterValue, columnName, byIcon);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        /// <summary>
        /// Edit entity
        /// </summary>
        /// <param name="checkBeforeValue">Entity's value to find for edit (Code, Description, etc.)</param>
        /// <param name="checkAfterValue">New entity's value</param>
        /// <param name="columnName">Columns name, in which to search for value, by default is 'Code'</param>
        /// <param name="byIcon">Indicates whether to edit entity by icon in grid, by default is 'true'</param>
        /// <param name="groupNameBefore">Set group before element name. Empty value means not grouping. Default value ""</param>
        /// <param name="groupNameAfter">Set group after element name. Empty value means not grouping. Default value ""</param>
        /// <param name="filter"></param>
        private void _editEntity(string checkBeforeValue, string checkAfterValue = "", string columnName = "Code", bool byIcon = true, string groupNameBefore = "", string groupNameAfter = "", string filter = "DEFFLTR", TestPurpose purpose = TestPurpose.Edit, bool useReversedCards = false, bool validate = true)
        {
            var tries = 3;
            do
            {
                Validate().AssertEntityExistsInGrid(columnName, checkBeforeValue, groupElementName: groupNameBefore,
                    filter: filter);
                var row = GetFirstRowIfGroup(Page.GetTableBodyRows(null), checkBeforeValue);
                Page.SetLanguage();
                if (byIcon)
                {
                    Page.EditGridIcon(row).Click2();
                }
                else
                {
                    Page.CheckBoxGridItem(row).Click2();
                    //Page.ActionList();
                    OpenActionMenu();
                    Page.EditButton.ClickUnderRedis();
                }
            }
            while (FillEntity(purpose, (purpose == TestPurpose.ClearBeforeFill) && useReversedCards, tries: --tries));

            //BeforeSaveAction();
            Page.ClickSaveButtonWithjQ();
            //AfterSaveAction();

            WebDriver.WaitForGridReady();

            if (!validate) return;

            checkAfterValue = string.IsNullOrEmpty(checkAfterValue) ? checkBeforeValue : checkAfterValue;
            groupNameAfter = string.IsNullOrEmpty(groupNameAfter) ? groupNameBefore : groupNameAfter;

            EntityVerification(checkAfterValue, columnName, groupNameAfter, byIcon, filter, true);
        }

        public void EditEntity(string checkBeforeValue, string checkAfterValue = "", string columnName = "Code", bool byIcon = true, string groupNameBefore = "", string groupNameAfter = "", string filter = "DEFFLTR", bool clearBefore = false, bool useReversedCards = false, bool validate = true)
        {
            try
            {
                StartStep($"Modifying '{checkBeforeValue}'({columnName}) entity to '{(string.IsNullOrEmpty(checkAfterValue) ? checkBeforeValue : checkAfterValue)}' by {(byIcon ? "icon" : "action")}");

                _editEntity(checkBeforeValue, checkAfterValue, columnName, byIcon, groupNameBefore, groupNameAfter, filter, clearBefore ? TestPurpose.ClearBeforeFill : TestPurpose.Edit, useReversedCards, validate);

                FinishStep();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        /// <summary>
        /// Duplicate entity
        /// </summary>
        /// <param name="checkBeforeValue">Entity's value to find for duplicate</param>
        /// <param name="checkAfterValue">New entity's value</param>
        /// <param name="columnName">Columns name, in which to search for value, by default is 'Code'</param>
        /// <param name="groupNameBefore"></param>
        /// <param name="groupNameAfter"></param>
        private void _duplicateEntity(string checkBeforeValue, string checkAfterValue, string columnName = "Code", string groupNameBefore = "", string groupNameAfter = "")
        {
            Validate().AssertEntityDoesNotExistInGrid(columnName, checkAfterValue);

            var tries = 3;
            do
            {
                Validate().AssertEntityExistsInGrid(columnName, checkBeforeValue, groupElementName: groupNameBefore);
                var _rowElement = GetFirstRowIfGroup(Page.GetTableBodyRows(null), checkBeforeValue);
                Page.SetLanguage();
                Page.CheckBoxGridItem(_rowElement).Click2();
                OpenActionMenu();
                Page.DuplicateButton.ClickUnderRedis();
            }
            while (FillEntity(TestPurpose.Duplicate, tries: --tries));

            Page.ClickSaveButtonWithjQ();
            WebDriver.WaitForGridReady();
            Page.First();
            Validate().AssertEntityExistsInGrid(columnName, checkAfterValue, groupElementName: string.IsNullOrEmpty(groupNameAfter) ? groupNameBefore : groupNameAfter);
            var rowElement = GetFirstRowIfGroup(Page.GetTableBodyRows(null), string.IsNullOrEmpty(checkAfterValue) ? checkBeforeValue : checkAfterValue);
            Page.CheckBoxGridItem(rowElement).Click2();
            OpenActionMenu();
            //Page.SetLanguage();
            Page.EditButton.ClickUnderRedis();
            VerifyEntityCard();
            Page.CancelButton.ClickUnderRedis();
            //WebDriver.WaitForReadyState(/*"Filtering row"*/);
            WebDriver.WaitForGridReady();
            Page.First();
            Validate().AssertEntityExistsInGrid(columnName, checkAfterValue, groupElementName: string.IsNullOrEmpty(groupNameAfter) ? groupNameBefore : groupNameAfter);
        }

        private void _duplicateArrayEntity(string[] checkBeforeValue, string[] checkAfterValue, string[] columnName)
        {
            Validate().AssertEntityDoesNotExistInGrid(columnName, checkAfterValue);

            var tries = 3;
            do
            {
                Validate().AssertEntityExistsInGrid(columnName, checkBeforeValue, checkTheCheckbox: true);
                Page.DuplicateButton.ClickUnderRedis();
            }
            while (FillEntity(TestPurpose.Duplicate, tries: --tries));

            Page.ClickSaveButtonWithjQ();
            WebDriver.WaitForGridReady();

            Navigate();
            //Page.First();
            Validate().AssertEntityExistsInGrid(columnName, checkAfterValue, checkTheCheckbox: true);

            //Page.SetLanguage();
            Page.EditButton.ClickUnderRedis();
            VerifyEntityCard();
            Page.CancelButton.ClickUnderRedis();
            //WebDriver.WaitForReadyState(/*"Filtering row"*/);
            WebDriver.WaitForGridReady();

            Navigate();
            //Page.First();
            Validate().AssertEntityExistsInGrid(columnName, checkAfterValue);
        }

        public void DuplicateEntity(string checkBeforeValue, string checkAfterValue, string columnName = "Code", string groupName = "", string groupNameAfter = "")
        {
            try
            {
                _duplicateEntity(checkBeforeValue, checkAfterValue, columnName, groupName, groupNameAfter);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        public void DuplicateEntity(string[] checkBeforeValue, string[] checkAfterValue, string[] columnName)
        {
            try
            {
                _duplicateArrayEntity(checkBeforeValue, checkAfterValue, columnName);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        private static string NumberPrepare(string number)
        {
            var ret = number;
            if (!float.TryParse(ret, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-GB"), out var res)
            ) return ret;
            if (res.ToString(CultureInfo.InvariantCulture).Length == number.Length)
            {
                ret = res.ToString("#,##0.#0", CultureInfo.CreateSpecificCulture("en-GB"));
            }
            return ret;
        }

        /// <summary>
        /// Deletes one entity by icon in grid
        /// </summary>
        /// <param name="checkValue">Value to find for delete</param>
        /// <param name="columnName">Name of column in which to find the value</param>
        /// <param name="groupName"></param>
        private void _deleteEntityByIcon(string checkValue, string columnName = "Code", string groupName = "", string[] errMessages = null)
        {
            var rowNum = Validate().AssertEntityExistsInGrid(columnName, checkValue, groupElementName: groupName);


            var rowElement = rowNum == 0
                        ? GetFirstRowIfGroup(Page.GetTableBodyRows(
                                WebDriver.GetElementByQuery(GetElementBy.ClassName, "k-grid-content.k-auto-scrollable")?.GetTable()), checkValue)
                        : Page.GetTableBodyRows(null)[rowNum];

            Page.DeleteGridIcon(rowElement).ClickUnderRedis();

            Page.ModalButtonClick(errorexpected: errMessages != null);

            if (errMessages == null)
            {
                var alert = Page.AllertChecker();
                if (alert != "")
                    Assert.Fail($"Deletion failed with message: {alert}");
            }
            else
            {
                foreach (var errMessage in errMessages)
                    Validate().AssertErrorMessage(errMessage);
                var btnClose = Page.AlertReturnButton ?? WebDriver.GetElementByQuery(GetElementBy.ClassName, "alert-error").AlertCloseButton();
                btnClose.ClickUnderRedis(true);
            }

            if (errMessages == null)
            {
                Page.First();
                Validate().AssertEntityDoesNotExistInGrid(columnName, checkValue);
            }
            else
                Validate().AssertEntityExistsInGrid(columnName, checkValue, groupElementName: groupName);
        }

        public void DeleteEntityByIcon(string checkValue, string columnName = "Code", string groupName = "", string[] errMessages = null)
        {
            try
            {
                _deleteEntityByIcon(checkValue, columnName, groupName, errMessages);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        /// <summary>
        /// Deletes several entities by checking the checkboxes and clicking Delete button
        /// </summary>
        /// <param name="checkValues">Values to find for delete</param>
        /// <param name="columnName">Name of column in which to find the value</param>
        /// <param name="groupValues">Groups which can consist of some entities</param>
        private void _deleteEntitiesByBatch(IReadOnlyList<string> checkValues, string columnName = "Code", IReadOnlyList<string> groupValues = null, string[] errMessages = null, string title = "")
        {
            if (groupValues == null || groupValues.Count == 0)
                for (var i = 0; i < checkValues.Count; ++i)
                {
                    var rowNum = Validate().AssertEntityExistsInGrid(columnName, checkValues[i], true);
                    Page.CheckOrUncheckTheCheckbox("true",
                    Page.CheckBoxGridItem(rowNum == 0
                        ? GetFirstRowIfGroup(Page.GetTableBodyRows(null), checkValues[i])
                        : Page.GetTableBodyRows(null)[rowNum]));
                }
            else
                for (var i = 0; i < checkValues.Count; ++i)
                {
                    Validate().AssertEntityExistsInGrid(columnName, checkValues[i], true, true, groupElementName: groupValues[i]);
                }
            if (string.IsNullOrEmpty(title))
                Page.DeleteButton().ClickUnderRedis();
            else
                Page.GetButtonLinkByTitle(title).ClickUnderRedis();

            Page.PopupDialogClick(errorexpected: errMessages != null);

            if (!string.IsNullOrEmpty(title) && errMessages == null)
                Page.Pause(1000);

            if (errMessages == null)
            {
                var alert = Page.AllertChecker();
                if (alert != "")
                    Assert.Fail($"Deletion failed with message: {alert}");
                Navigate();
            }
            else
            {
                foreach (var errMessage in errMessages)
                    Validate().AssertErrorMessage(errMessage);
                Page.AlertReturnButton.ClickUnderRedis(true);
            }

            foreach (var item in checkValues)
            {
                if (errMessages == null)
                {
                    try
                    {
                        Validate().AssertEntityDoesNotExistByContext(columnName, item);
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("was not found"))
                            Validate().AssertEntityDoesNotExistInGrid(columnName, item);
                    }
                }
                else
                    Validate().AssertEntityExistsInGrid(columnName, item);
            }
        }

        public void DeleteEntitiesByBatch(string[] checkValues, string columnName = "Code", string[] groupValues = null, string[] errMessages = null, string title = "")
        {
            try
            {
                _deleteEntitiesByBatch(checkValues, columnName, groupValues, errMessages, title);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        /// <summary>
        /// Changes several entities by checking the checkboxes and clicking ChangeByBatch button
        /// </summary>
        /// <param name="checkValues">Values to find for change</param>
        /// <param name="columnName">Name of column in which to find the value</param>
        /// <param name="groupValues">Groups which can consist of some entities</param>
        private void _changeByBatch(IReadOnlyList<string> checkValues, string columnName = "Code", IReadOnlyList<string> groupValues = null)
        {

            var tries = 3;
            do
            {
                StartStep("Checking existing values in the grid and click them");
                if (groupValues == null || groupValues.Count == 0)
                    Validate().AssertEntitiesExistInGrid(columnName, checkValues.ToArray(), false, true);
                else
                    for (var i = 0; i < checkValues.Count; ++i)
                    {
                        Validate().AssertEntityExistsInGrid(columnName, checkValues[i], true, true, groupElementName: groupValues[i]);
                    }
                FinishStep();

                StartStep("Click 'Change By Batch' button");
                //Page.SetLanguage();
                Page.ChangeByBatch.ClickUnderRedis();
                FinishStep();
            }
            while (FillEntity(TestPurpose.ChangeByBatch, tries: --tries));

            Page.SaveButton.Click2(jQ: false);
            WebDriver.Pause(1500);
            WebDriver.WaitForReadyStateByLink("Return", jQ: false);
            //h4 id=batchUpdateResultForm //Batch updated: 2
            var failResult = WebDriver.GetElementById("batchFailedResultBlock")?.Text.Split(' ')[1];
            if (!string.IsNullOrEmpty(failResult) && Convert.ToInt32(failResult) > 0)
            {
                //id=batchFailedResultBlock //h4 Failed: 1
                //table id=batchUpdateFailed //in line colunm 2 has exception text
                var failTable = WebDriver.GetElementById("batchUpdateFailed");
                var failTableLines = Page.GetTableBodyRows(failTable);
                if (failTableLines == null) Assert.Fail("FailedResult table has no lines");
                var failList = $"\r\n-> '{Page.GetTableCell(0, 2, failTable).Text}'";
                for (var ind = 1; failTableLines.Count > ind; ++ind)
                {
                    failList += $"\r\n-> '{Page.GetTableCell(ind, 2, failTable).Text}'";
                }
                Assert.Fail($"Change by batch was not completed with the following error(s): {failList}");
            }
            Page.ReturnButton.ClickUnderRedis(jQ: false);
            Page.First();
            for (var i = 0; i < checkValues.Count; ++i)
            {
                if (groupValues == null || groupValues.Count == 0)
                    ViewEntity(checkValues[i], columnName);
                else
                    ViewEntity(checkValues[i], columnName, groupName: groupValues[i]);
            }
        }

        private void _changeByBatch(string[][] checkBeforeValues, string[][] checkAfterValues, string[] columnName, string[] groupValues = null)
        {
            var tries = 3;
            do
            {
                StartStep("Checking existing values in the grid and click them");
                if (groupValues == null || groupValues.Length == 0)
                    Validate().AssertEntityExistsInGrid(columnName, checkBeforeValues, true);
                else
                    Validate().AssertEntityExistsInGrid(columnName, checkBeforeValues, true, groupElementName: groupValues);
                FinishStep();

                StartStep("Click 'Change By Batch' button");
                //Page.SetLanguage();
                Page.ChangeByBatch.ClickUnderRedis();
                FinishStep();
            }
            while (FillEntity(TestPurpose.ChangeByBatch, tries: --tries));

            Page.SaveButton.Click2(jQ: false);
            WebDriver.Pause();
            WebDriver.WaitForReadyStateByLink("Return", jQ: false);
            //h4 id=batchUpdateResultForm //Batch updated: 2
            var batchResult = WebDriver.GetElementById("batchUpdateResultForm").Text;
            if (batchResult != "Batch updated: 2")
            {
                //id=batchFailedResultBlock //h4 Failed: 1
                //table id=batchUpdateFailed //in line colunm 2 has exception text
                var failTable = WebDriver.GetElementById("batchUpdateFailed");
                var failTableLines = Page.GetTableBodyRows(failTable);
                if (failTableLines == null) Assert.Fail("FailedResult table has no lines");
                var failList = $"\r\n-> {Page.GetTableCell(0, 2, failTable).Text}";
                for (var ind = 1; failTableLines.Count > ind; ++ind)
                {
                    failList += $"\r\n-> <{Page.GetTableCell(ind, 2, failTable).Text}";
                }
                Assert.Fail($"Change by batch was not completed with the following error(s): {failList}");
            }
            Page.ReturnButton.ClickUnderRedis(jQ: false);
            Page.First();
            foreach (var checkValue in checkAfterValues)
            {
                if (groupValues == null || groupValues.Length == 0)
                    ViewMultiplyColumnEntity(checkValue, columnName);
                else
                    ViewMultiplyColumnEntity(checkValue, columnName, groupNames: groupValues);
            }
        }

        public void ChangeByBatch(string[][] checkBeforeValues, string[][] checkAfterValues, string[] columnName, string[] groupValues = null)
        {
            try
            {
                _changeByBatch(checkBeforeValues, checkAfterValues, columnName, groupValues);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        public void ChangeByBatch(string[] checkValues, string columnName = "Code", string[] groupValues = null)
        {
            try
            {
                _changeByBatch(checkValues, columnName, groupValues);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        private void _checkThePredefined(IEnumerable<string> checkValue, string columnName = "Code", string groupName = "")
        {
            foreach (var item in checkValue)
            {
                Validate().AssertEntityExistsInGrid(columnName, item, groupElementName: groupName);

                var table = WebDriver.GetTable();
                var rowElement = Page.GetTableRow(0, table);
                var columnNumber = Page.GetColumnNumber("Predefined", table);
                if (columnNumber < 0)
                    columnNumber = Page.GetColumnNumber("Is Predefined", table);
                if (columnNumber < 0)
                    Assert.Fail("Predefine column is not available in table");
                var predefCell = Page.GetTableCell(0, columnNumber, table);
                var classIsTrue = predefCell.GetElementByClassName("card-link");

                var classNamePredefined = classIsTrue.GetAttribute("data-bool-val");
                if (classNamePredefined == null)
                    Assert.Fail("Expected 'true' or 'false' value but 'null' was happened");
                var edit = Page.EditGridIcon(rowElement);
                var delete = Page.DeleteGridIcon(rowElement);
                if (classNamePredefined != "true")
                {
                    if (classNamePredefined != "false")
                        Assert.Fail($"Expected 'true' or 'false' value but '{classNamePredefined}' was happened");
                    Assert.IsTrue(edit != null);
                    Assert.IsTrue(delete != null);
                }
                else
                {
                    //Assert.IsTrue(edit == null);
                    Assert.IsTrue(delete == null);
                }
            }
        }

        public void CheckThePredefined(string[] checkValues, string columnName = "Code", string groupName = "")
        {
            try
            {
                _checkThePredefined(checkValues, columnName, groupName);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        private void _deleteEntityThroughTheCard(string checkValue, string columnName = "Code", string groupName = "", string[] errMessages = null)
        {
            Validate().AssertEntityExistsInGrid(columnName, checkValue, groupElementName: groupName);
            StartStep("Enter to edit mode by icon");
            var rowElement = GetFirstRowIfGroup(Page.GetTableBodyRows(null), checkValue);

            Page.SetLanguage();
            Page.EditGridIcon(rowElement).Click2();
            FinishStep();
            StartStep("Click on the 'Delete' button");
            OpenActionMenu("Actions");
            Page.DeleteButton("deletesingle").Click2();//deletesingle
            FinishStep();

            Page.PopupDialogClick(errorexpected: errMessages != null);

            if (errMessages != null)
            {
                Validate().AssertNoAlertPresent();
                Validate().AssertThatMainValidationErrorMessageIsShown();
                Validate().AssertThatValidationErrorMessagesAreShown(errMessages);

                Page.CancelButton.ClickUnderRedis(emptyformbefore: true);
            }
            else
            {
                var alert = Page.AllertChecker();
                if (alert != "")
                    Assert.Fail($"Deletion failed with message: {alert}");
            }

            Page.First();
            if (errMessages == null)
                Validate().AssertEntityDoesNotExistInGrid(columnName, checkValue);
            else
                Validate().AssertEntityExistsInGrid(columnName, checkValue, groupElementName: groupName);
        }

        public void DeleteEntityThroughTheCard(string checkValues, string columnName = "Code", string groupName = "", string[] errMessages = null)
        {
            try
            {
                _deleteEntityThroughTheCard(checkValues, columnName, groupName, errMessages);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        private void _deleteMultiplyColumns(string[][] checkBeforeValue, string[] columnName)
        {
            //need to review: search all pairs per page not pair-by-pair
            Validate().AssertEntityExistsInGrid(columnName, checkBeforeValue, true);
            Page.DeleteButton().ClickUnderRedis();
            Page.PopupDialogClick();

            var alert = Page.AllertChecker();
            if (alert != "")
                Assert.Fail($"Deletion failed with message: {alert}");

            Page.First();
            Validate().AssertEntityDoesNotExistInGrid(columnName, checkBeforeValue);
        }

        private void _deleteMultiplyColumns(string[] checkBeforeValue, string[] columnName, bool action = false,
            bool edit = false)
        {
            var row = Validate().AssertEntityExistsInGrid(columnName, checkBeforeValue, checkTheCheckbox: action);
            var rowElement = Page.GetTableBodyRows(null)[row];
            if (edit)
            {
                if (action)
                {
                    OpenActionMenu();
                    Page.EditButton.ClickUnderRedis();
                }
                else
                    Page.EditGridIcon(rowElement).Click2();
                StartStep("Click on the 'Delete' button");
                OpenActionMenu("Actions");
                Page.DeleteButton("deletesingle").Click2(); //deletesingle
                FinishStep();
            }
            else
            {
                if (action)
                    Page.DeleteButton().ClickUnderRedis();
                else
                    Page.DeleteGridIcon(rowElement).ClickUnderRedis();
            }

            Page.PopupDialogClick();

            var alert = Page.AllertChecker();
            if (alert != "")
                Assert.Fail($"Deletion failed with message: {alert}");

            Page.First();
            Validate().AssertEntityDoesNotExistInGrid(columnName, checkBeforeValue);
        }

        public void DeleteMultiplyColumnEntityByIcon(string[] checkBeforeValue, string[] columnName)
        {
            try
            {
                _deleteMultiplyColumns(checkBeforeValue, columnName);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        public void DeleteMultiplyColumnEntitiesByAction(string[][] checkBeforeValue, string[] columnName)
        {
            try
            {
                _deleteMultiplyColumns(checkBeforeValue, columnName);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        public void DeleteMultiplyColumnsThroughTheCard(string[] checkBeforeValue, string[] columnName)
        {
            try
            {
                _deleteMultiplyColumns(checkBeforeValue, columnName, false, true);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        /// <summary>
        /// View entity
        /// </summary>
        /// <param name="checkValue">Checked value</param>
        /// <param name="columnName">Columns name. Default value is 'Code'</param>
        /// <param name="byIcon">Indicates either view entity by icon in grid</param>
        /// <param name="title">Expected page title</param>
        /// <param name="groupName"></param>
        /// <param name="noReturn">Don't click Return button after verifying</param>
        /// <param name="check">Select row or not. (If you select row selection, entity is searched by column name, otherwise by context search)</param>
        private void _viewEntity(string checkValue, string columnName = "Code", bool byIcon = true, string title = "View", string groupName = "", bool noReturn = false, bool check = false)
        {
            StartStep($"Verifying entity in view mode by {(byIcon ? "icon" : "action")} (Expected: {columnName} = '{checkValue}', title = '{title}')");
            var rowNum = Validate().AssertEntityExistsInGrid(columnName, checkValue, checkTheCheckbox: check, groupElementName: groupName);
            var rowElement = (!check || rowNum == 0)
                ? GetFirstRowIfGroup(Page.GetTableBodyRows(null), checkValue)
                : rowNum < 0
                    ? null
                    : Page.GetTableBodyRows(null)[rowNum];
            if (byIcon)
            {
                Page.ViewGridIcon(rowElement).Click2();
            }
            else
            {
                if (!check)
                    Page.CheckBoxGridItem(rowElement).Click2();
                if (WebDriver.GetElementByClassName("grid-actions").GetElementsByText(GetElementBy.Tagname, "button", "More".Localize(Page.currentLanguage)).Count > 0)
                    OpenActionMenu();
                Page.ViewButton.ClickUnderRedis();
            }
            WebDriver.WaitForReadyStateByLink("Return".Localize(Page.currentLanguage));

            Validate().AssertCurrentBrowserWindowTitle(title.Localize(Page.currentLanguage));

            Validate().AssertRequestVerificationToken();

            VerifyEntityCard(false);
            if (!noReturn)
            {
                //Page.SetLanguage();
                Page.ReturnButton.ClickUnderRedis();
                //WebDriver.WaitForReadyState(/*"Filtering row"*/);
                Page.First();
                Validate().AssertEntityExistsInGrid(columnName, checkValue, checkTheCheckbox: check, groupElementName: groupName);
            }
            FinishStep();
        }

        private void _viewArrayEntity(string[] checkValue, string[] columnName, bool byIcon = true, string title = "View", string[] groupName = null, bool editMode = false)
        {
            StartStep($"Verifying entity in {title.ToLower()} mode by {(byIcon ? "icon" : "action")} (Expected: {columnName} = '{checkValue}', title = '{title}')");
            var row = Validate().AssertEntityExistsInGrid(columnName, checkValue, groupElementName: groupName,
                checkTheCheckbox: !byIcon);
            var rowElement = Page.GetTableBodyRows(null)[row];

            if (byIcon)
            {
                if (editMode)
                    Page.EditGridIcon(rowElement).Click2();
                else
                    Page.ViewGridIcon(rowElement).Click2();
            }
            else
            {
                OpenActionMenu();
                if (editMode)
                    Page.EditButton.ClickUnderRedis();
                else
                    Page.ViewButton.ClickUnderRedis();
            }

            if (editMode)
                WebDriver.WaitForReadyStateByLink("Cancel", "input");
            else
                WebDriver.WaitForReadyStateByLink("Return");

            Validate().AssertCurrentBrowserWindowTitle(title);

            Validate().AssertRequestVerificationToken();

            VerifyEntityCard(editMode);

            if (editMode)
                Page.CancelButton.ClickUnderRedis();
            else
                Page.ReturnButton.ClickUnderRedis();
            //WebDriver.WaitForReadyState(/*"Filtering row"*/);

            Navigate();
            //Page.First();
            Validate().AssertEntityExistsInGrid(columnName, checkValue);
            FinishStep();
        }

        public void ViewEntity(string checkValue, string columnName = "Code", bool byIcon = true, string title = "View", string groupName = "", bool noReturn = false, bool check = false)
        {
            try
            {
                _viewEntity(checkValue, columnName, byIcon, title, groupName, noReturn, check);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        public void ViewMultiplyColumnEntity(string[] checkValue, string[] columnName, bool byIcon = true, string title = "View", string[] groupNames = null, bool editMode = false)
        {
            try
            {
                _viewArrayEntity(checkValue, columnName, byIcon, title, groupNames, editMode);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        private void _viewEditEntity(string checkBeforeValue, string checkAfterValue = "", string columnName = "Code", bool byIcon = true)
        {
            var tries = 3;
            do
            {
                Validate().AssertEntityExistsInGrid(columnName, checkBeforeValue);
                var _rowElement = GetFirstRowIfGroup(Page.GetTableBodyRows(null), checkBeforeValue);

                if (byIcon)
                {
                    Page.ViewGridIcon(_rowElement).Click2();
                }
                else
                {
                    Page.CheckBoxGridItem(_rowElement).Click2();
                    OpenActionMenu();
                    Page.ViewButton.ClickUnderRedis();
                }
                WebDriver.WaitForReadyStateByLink("Return");
                var cardElement = CardItems[0];
                //Validate().AssertCurrentBrowserWindowTitle("View"); //Title is not changed. Uncomment this line after fix
                Validate().AssertCardElementInRightMode(cardElement, true);

                Page.ActionList();
                Page.EditButton.WaitForElementIsClickable().Click2();
                //Validate().AssertCurrentBrowserWindowTitle("Edit"); //Title is not changed. Uncomment this line after fix
                Validate().AssertCardElementInRightMode(cardElement, false);
            }
            while (FillEntity(TestPurpose.Edit, tries: --tries));


            Page.ClickSaveButtonWithjQ();
            WebDriver.WaitForGridReady();
            Page.First();
            Validate().AssertEntityExistsInGrid(columnName, string.IsNullOrEmpty(checkAfterValue) ? checkBeforeValue : checkAfterValue);
            var rowElement = GetFirstRowIfGroup(Page.GetTableBodyRows(null), string.IsNullOrEmpty(checkAfterValue) ? checkBeforeValue : checkAfterValue);

            if (byIcon)
            {
                Page.EditGridIcon(rowElement).Click2();
            }
            else
            {
                Page.CheckBoxGridItem(rowElement).Click2();
                Page.EditButton.ClickUnderRedis();
            }
            VerifyEntityCard();

            Page.CancelButton.ClickUnderRedis();
            //WebDriver.WaitForReadyState(/*"Filtering row"*/);
            WebDriver.WaitForGridReady();
            Page.First();
            Validate().AssertEntityExistsInGrid(columnName, string.IsNullOrEmpty(checkAfterValue) ? checkBeforeValue : checkAfterValue);
        }

        public void ViewEditEntity(string checkBeforeValue, string checkAfterValue = "", string columnName = "Code", bool byIcon = true)
        {
            try
            {
                _viewEditEntity(checkBeforeValue, checkAfterValue, columnName, byIcon);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        private bool CheckCorrectAuditCard(bool repeat, bool both = false)
        {
            WebDriver.WaitForReadyStateByLink("Close");
            var ret = !repeat && (WebDriver.GetElementById("Revision_2_Entity_Id") == null || (both && WebDriver.GetElementById("Revision_1_Entity_Id") == null));
            if (ret)
            {
                StartStep("First audit opening! Try again...");
                FinishStep();
                WebDriver.GetElementById("closeRevisionButton").Click2();
                WebDriver.WaitForReadyStateByLink("Return");
            }
            return ret;
        }

        public void OpenAuditCompare()
        {
            var repeat = false;
            do
            {
                WebDriver.GetElementById("compareRevisionsButton").Click2();
                repeat = CheckCorrectAuditCard(repeat, true);
            } while (repeat);
        }

        private void _auditEntity(int revNumber, string serialNumber = "2", string revisionType = "Added")
        {
            try
            {
                DriverWait.WaitForElementByClass("nav nav-tabs", null);
            }
            catch (Exception e)
            {
                if (WebDriver.GetElementById("Revision_2_RevisionInfo_SerialNumber") == null && WebDriver.GetElementById("Revision_1_RevisionInfo_SerialNumber") == null)
                    Assert.Fail($"Audit card was not shown with an error: {e.Message}");
            }
            var oldCardItems = RecreateCardItemsForAudit(revNumber, serialNumber, revisionType);

            WebDriver.WaitForReadyState();
            VerifyEntityCard(false);

            //restore CardItem array
            CardItems.Clear();
            foreach (var item in oldCardItems)
            {
                CardItems.Add(item);
            }
        }

        public void AuditEntity(int revNumber, string revType = "Added")
        {
            try
            {
                _auditEntity(revNumber, null, revType);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail("RevisionDetails");
                throw;
            }
        }

        private void _auditSelectedDeletedEntity(string checkValue, string columnName = "Code", string buttonCreate = "", string groupName = "", bool nocreate = false)
        {
            var iteration = 1;
            if (!nocreate)
            {
                Validate().AssertEntityDoesNotExistInGrid(columnName, checkValue);

                StartStep($"Creating entity with {columnName} = '{checkValue}'");
                try
                {
                    EntityCreation(buttonCreate);
                }
                finally
                {
                    FinishStep();
                }

                Page.First();
            }

            Validate().AssertEntityExistsInGrid(columnName, checkValue, groupElementName: groupName, checkTheCheckbox: true);

            StartStep($"Audit selected entity with {columnName} = '{checkValue}'");
            try
            {
                Page.OpenActionMenu("More", "grid-actions-one-for-all");
                Page.AuditCheckedButton.Click2();
                WebDriver.WaitForReadyStateByLink("Return");
                var repeat = false;
                do
                {
                    var divTable = Page.WebDriver.GetElementById("auditGridArea");
                    var rows = Page.GetTableBodyRows(WebDriver.GetElementByQuery(GetElementBy.Tagname, "table[role=treegrid]"));
                    var row = rows.First(d => d.GetElementByQuery(GetElementBy.Tagname, $"span[title='{checkValue}']") != null);
                    if (!repeat)
                        row.GetElementByClassName("k-i-expand").Click2();
                    var revId = row.GetElementByQuery(GetElementBy.Tagname, "td[style='display:none']").GetAttrib("innerText");
                    var revIcon = WebDriver.GetElementByQuery(GetElementBy.Tagname, $"div[id=Revisions_{revId}]").GetElementByQuery(GetElementBy.Tagname, "span[id=viewRevisionsButton]");
                    revIcon.Click2();//Maybe it will be repaired someday and it will work!
                    revIcon?.SetTrigger("mouseup");//...but now we call this event to open review
                    repeat = CheckCorrectAuditCard(repeat);
                }
                while (repeat);
                _auditEntity(2, iteration++.ToString(), "Added");
                WebDriver.GetElementById("closeRevisionButton").Click2();
                WebDriver.WaitForReadyStateByLink("Return");
                WebDriver.GetElementById("returnButton").Click2();
            }
            finally
            {
                FinishStep();
            }

            StartStep($"Deleting entity with {columnName} = '{checkValue}'");
            try
            {
                Validate().AssertEntityExistsInGrid(columnName, checkValue, groupElementName: groupName);
                var tabl = WebDriver.GetElementByText(null, GetElementBy.ClassName, "k-grid-content.k-auto-scrollable", NumberPrepare(checkValue)).GetTable();
                var rowElement = GetFirstRowIfGroup(Page.GetTableBodyRows(tabl), checkValue);

                Page.DeleteGridIcon(rowElement).ClickUnderRedis();

                Page.ModalButtonClick();

                var alert = Page.AllertChecker();
                if (alert != "")
                    Assert.Fail($"Deletion failed with message: {alert}");
            }
            finally
            {
                FinishStep();
            }

            Page.First();
            Validate().AssertEntityDoesNotExistInGrid(columnName, checkValue);

            StartStep($"Audit deleted entity with {columnName} = '{checkValue}'");
            try
            {
                Page.OpenActionMenu("More", "grid-actions-one-for-all");
                Page.AuditDeletedButton.Click2();
                WebDriver.WaitForReadyStateByLink("Return");
                var divTable = Page.WebDriver.GetElementById("auditGridArea");
                IWebElement row;
                {
                    var rows = Page.GetTableBodyRows(WebDriver.GetElementByQuery(GetElementBy.Tagname, "table[role=treegrid]"));
                    row = rows.FirstOrDefault(d => d.GetElementByQuery(GetElementBy.Tagname, $"span[title='{checkValue}']") != null);
                } while(row == null && Page.Next());
                if (row == null)
                    Assert.Fail($"Element with value '{checkValue}' was not found in audit grid!");
                row.GetElementByClassName("k-i-expand").Click2();
                var revId = row.GetElementByQuery(GetElementBy.Tagname, "td[style='display:none']").GetAttrib("innerText");
                var revIcon = WebDriver.GetElementByQuery(GetElementBy.Tagname, $"div[id=Revisions_{revId}]").GetElementByQuery(GetElementBy.Tagname, "span[id=viewRevisionsButton]");
                revIcon.Click2();//Maybe it will be repaired someday and it will work!
                revIcon?.SetTrigger("mouseup");//...but now we call this event to open review
                WebDriver.WaitForReadyStateByLink("Close");
                _auditEntity(2, iteration++.ToString(), "Deleted");
                WebDriver.GetElementById("closeRevisionButton").Click2();
                WebDriver.WaitForReadyStateByLink("Return");
                WebDriver.GetElementById("returnButton").Click2();
            }
            finally
            {
                FinishStep();
            }
        }

        public void AuditSelectedDeletedEntity(string checkValue, string columnName = "Code", string buttonCreate = "", string groupName = "", bool nocreate = false)
        {
            try
            {
                _auditSelectedDeletedEntity(checkValue, columnName, buttonCreate, groupName, nocreate);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail("RevisionDetails");
                throw;
            }
        }

        private static string GetItemInformation(CardItem item)
        {
            var stringReturn = $"Data type = '{item.FieldType}', {(item.NameOrId.Contains('.') ? "Name" : "ID")} = '{item.NameOrId + (item.FieldType == FieldType.RowsTree ? item.Description : "")}'";

            switch (item.FieldType)
            {
                case FieldType.Multi:
                    var arrayValues = (string[])item.Value;
                    if (arrayValues.Length > 0) stringReturn += ", Value = '";
                    for (var i = 0; i < arrayValues.Length; ++i)
                    {
                        if (i != 0) stringReturn += " | ";
                        stringReturn += arrayValues[i];
                    }
                    stringReturn += "'";
                    break;
                case FieldType.DetailTable:
                case FieldType.ListView:
                    stringReturn += $", Count = {item.Description}";
                    //var valueArray = (string[][])item.Value;
                    //stringReturn += "/nValue = " + item.Value.ToString();
                    break;
                case FieldType.RowsTree:
                    stringReturn += $", Count = {(item.Value as string[]).Length}";
                    break;
                case FieldType.CheckBox:
                case FieldType.Toggle:
                case FieldType.ListBox:
                case FieldType.Color:
                case FieldType.Text:
                case FieldType.Password:
                case FieldType.Date:
                case FieldType.Single:
                case FieldType.Extender:
                case FieldType.Numeric:
                case FieldType.File:
                    stringReturn += $", Value = '{item.Value}'";
                    break;
                case FieldType.FlagsEnum:
                case FieldType.Radio:
                case FieldType.RadioAsButton:
                case FieldType.SlidingDate:
                case FieldType.EntityPropertiesTree:
                case FieldType.SelectedPropertiesTree:
                case FieldType.TreeListBox:
                case FieldType.FieldsParameter:
                case FieldType.Select:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return stringReturn;
        }

        public void FillFieldProcessing(CardItem item, TestPurpose purpose)
        {
            StartStep($"Processing: {GetItemInformation(item)}");
            try
            {
                switch (item.FieldType)
                {
                    // Filling CheckBox fields (true/false)
                    case FieldType.CheckBox:
                        var chkBox = WebDriver.AssertElementByIdAvailability(item.NameOrId);
                        Page.CheckOrUncheckTheCheckbox(item.Value.ToString(), chkBox);
                        break;
                    case FieldType.ChkBoxList:
                        {
                            var arrayValues = (string[])item.Value;
                            if (arrayValues == null || (arrayValues.Length == 0) || (arrayValues.Length == 1 && string.IsNullOrEmpty(arrayValues[0])))
                                break;
                            Page.SelectInCheckBoxList(item.NameOrId, arrayValues);
                        }
                        break;
                    case FieldType.FlagsEnum:
                        var chkbxs = item.Value as Dictionary<string, string>;
                        if (chkbxs == null)
                            Assert.Fail("No values are specified");
                        foreach (var chkbx in chkbxs)
                        {
                            var chkBx = WebDriver.AssertElementByIdAvailability(chkbx.Key);
                            Page.CheckOrUncheckTheCheckbox(chkbx.Value, chkBx);
                            chkBx.SetTrigger();
                        }
                        break;
                    // Switch toggle button 
                    case FieldType.Toggle:
                        WebDriver.AssertElementByIdAvailability(item.NameOrId);
                        Page.SwitchToggleControl(item.Value.ToString(), item.NameOrId);
                        break;
                    // Select List box item
                    case FieldType.ListBox:
                        if (purpose == TestPurpose.ClearBeforeFill)
                            item.Value = null;
                        Page.SelectValueInListBox(item.Value?.ToString(), item.NameOrId, item.WithInput);
                        break;
                    case FieldType.TreeListBox:
                        Page.SelectValueInListBoxTree(item.Value.ToString(), item.NameOrId);
                        break;
                    case FieldType.Color:
                        Page.SetColorHexValue(WebDriver.GetElementById(item.NameOrId), item.Value.ToString());
                        break;
                    case FieldType.Radio:
                        Page.SelectRadioAsButton(item.NameOrId, item.Value != null ? item.Value.ToString() : "");
                        break;
                    //Filling Password field
                    case FieldType.Password:
                        WebDriver.AssertElementByIdAvailability(item.NameOrId);
                        var h4 = WebDriver.GetElementByTextWithTag("Change password", "a");
                        if (h4 != null)
                        {
                            var clss = h4.GetAttribute("class");
                            if (clss == "collapsed") h4.Click();
                        }
                        WebDriver.SetTextBoxValue(item.NameOrId, item.Value.ToString());
                        break;
                    //Filling Text field
                    case FieldType.Text:
                        WebDriver.AssertElementByIdAvailability(item.NameOrId);
                        WebDriver.SetTextBoxValue(item.NameOrId, item.Value.ToString());
                        break;
                    // Filling Date field
                    case FieldType.Date:
                        var calendarEdit = WebDriver.GetElementByName(item.NameOrId);
                        Page.SetCalendarDate(calendarEdit, item.Value.ToString(), dateWithTime: item.DateWithTime);
                        break;
                    // Filling Sliding Date field
                    case FieldType.SlidingDate:
                        Page.SetFixedSlidingDate(item.NameOrId, item.Value.ToString());
                        break;
                    // Filling Numeric field
                    case FieldType.Numeric:
                        var editField = WebDriver.AssertElementByIdAvailability(item.NameOrId);
                        Page.SetNumberInNumericTextBox(item.Value.ToString(), editField);
                        break;
                    // Filling Kendo Grids: Phones, NetAddresses, Identifiers
                    case FieldType.DetailTable:
                        //Description - is numberOfRows
                        Page.FillDetailTable(item.NameOrId, Convert.ToInt16(string.IsNullOrEmpty(item.Description) ? "0" : item.Description), item.Value, purpose, item.SubType);
                        break;
                    // Filling Multiselect grid
                    case FieldType.Multi:
                        {
                            var arrayValues = (string[])item.Value;
                            if (arrayValues == null || (arrayValues.Length == 0) || (arrayValues.Length == 1 && string.IsNullOrEmpty(arrayValues[0])))
                                break;
                            Page.SelectInMultiselectGrid(item.NameOrId, arrayValues, purpose);
                        }
                        break;
                    case FieldType.KendoMultiSelect:
                        Page.SelectValuesInKendoMultiSelect(item.NameOrId, (string[])item.Value);
                        break;
                    // Filling Sinlge select fields
                    case FieldType.Single:
                        Page.FillSingleSelectField(item.NameOrId, item.Value.ToString());
                        break;
                    // Filling fields area list: Addresses, Operations
                    case FieldType.ListView:
                        if (string.IsNullOrEmpty(item.Description)) break;
                        Page.FillListView(item.NameOrId, Convert.ToInt16(item.Description), item.Value, purpose);
                        break;
                    // Selecting Entity and then EntityPropertiesTree item
                    case FieldType.EntityPropertiesTree:
                        Page.SelectValueInEntityPropertiesTree(item.NameOrId, item.Value.ToString(), item.Description);
                        break;
                    // Selecting SelectedPropertiesTree item
                    case FieldType.SelectedPropertiesTree:
                        Page.SelectValueInSelectedPropertiesTree(item.NameOrId, item.Value.ToString());
                        break;
                    case FieldType.RowsTree:
                        Page.FillRowsTree(item.NameOrId + item.Description, item.Value as string[]);
                        break;
                    case FieldType.FieldsParameter:
                        Page.FillStructureFieldParameters(item.NameOrId, (Dictionary<string, string>)item.Value, Convert.ToInt32(item.Description));
                        break;
                    // Enable/disable Extender
                    case FieldType.Extender:
                        Page.UseEntityWithExtender(item.NameOrId, item.TabName, (string)item.Value);
                        break;
                    case FieldType.File:
                        Page.FillFile((string)item.Value);
                        break;
                }
            }
            finally
            {
                FinishStep();
            }
        }

        public bool FillEntity(TestPurpose purpose = TestPurpose.Create, bool useReversedCard = false, int tries = 3)
        {
            try
            {
                if (purpose == TestPurpose.ClearBeforeFill)
                {
                    FillEntityCard(purpose, useReversedCard);
                    FillEntityCard(TestPurpose.Edit);
                }
                else
                    FillEntityCard(purpose, useReversedCard);
            }
            catch
            {
                if (!Page.CheckErrorRequest()) throw;
                if (tries < 1)
                {
                    throw;
                }
                Page.AddElmahDetail("layoutconfigurationtab");
                Driver.ElmahAdded(false);
                Navigate();
                return true;
            }
            return false;
        }

        public void FillEntityCard(TestPurpose purpose = TestPurpose.Create, bool useReversedCard = false)
        {
            StartStep("Filling the card");
            try
            {
                if (useReversedCard)
                    CardItems.Reverse();
                foreach (var item in CardItems)
                {
                    if (!string.IsNullOrEmpty(item.TabName))
                        Page.WebDriver.GoToTab(item.TabName);

                    // if item is only for verification - skip it
                    if (item.Verify)
                    {
                        continue;
                    }

                    // Clear all Single selects on the card form
                    if (item.ClearSingleSelects)
                    {
                        Page.ClearAllSingleSelects();
                        continue;
                    }
                    if (item.SaveAndEdit)
                    {
                        WebDriver.WaitForReadyState();
                        Page.SaveAndEditButton.ClickUnderRedis(focused: true);
                    }
                    if (item.SaveAs)
                    {
                        WebDriver.WaitForReadyState();
                        IWebElement saveAslnk = WebDriver.GetElementById("action-saveas-configuration") ??
                            WebDriver.GetElementByClassName("btn-save-as");
                        saveAslnk.Click2();
                    }
                    if (item.Value == null)
                    {
                        continue;
                    }

                    if (item.Value is string[] checkVal)
                    {
                        if (checkVal.Length == 0) continue;
                    }

                    try
                    {
                        if (purpose == TestPurpose.ChangeByBatch && item.FieldType != FieldType.ListView && item.FieldType != FieldType.EntityPropertiesTree)
                        {
                            StartStep($"Enable '{item.NameOrId}' item to edit");
                            try
                            {
                                var idName = item.NameOrId +  "_IsToChange";
                                var id = WebDriver.GetElementById(idName.Replace('.', '_'));
                                if (id == null)
                                {
                                    id = WebDriver.GetElementById(item.NameOrId);
                                    if (id == null)
                                        Assert.Fail($"Element {idName} or {item.NameOrId} not found.");
                                    idName = item.NameOrId;
                                }
                                var timer = 30;
                                do
                                {
                                    Page.CheckOrUncheckTheCheckbox("true", id);
                                } while (timer-- > 0 && !id.Selected);
                                if (timer == 0 && !id.Selected)
                                    Assert.Fail($"Checkbox '{idName}' can not be selected!");
                            }
                            finally
                            {
                                FinishStep();
                            }
                        }
                        FillFieldProcessing(item, purpose);
                    }
                    catch (Exception e)
                    {
                        if (e.SkipableFail()) throw;
                        if (Page.CheckErrorRequest()) throw;
                        throw;
                    }
                }
                if (useReversedCard)
                    CardItems.Reverse();
            }
            finally
            {
                FinishStep();
            }
        }

        public void VerifyFieldProcessing(CardItem item, bool editMode)
        {
            StartStep($"Verifying {item.FieldType} '{item.NameOrId}'");
            switch (item.FieldType)
            {
                //Verifying CheckBox fields (true/false)
                case FieldType.CheckBox:
                    var chkBox = WebDriver.GetElementById(item.NameOrId);
                    try
                    {
                        Assert.AreEqual(item.Value.ToString().ToLower(), chkBox.Selected.ToString().ToLower(), $"Wrong value: expected '{item.Value.ToString().ToLower()}' but actualy is '{chkBox.Selected.ToString().ToLower()}'!");
                    }
                    catch
                    {
                        chkBox.ScrollIntoView();
                        throw;
                    }

                    break;
                case FieldType.FlagsEnum:
                    var chkbxs = item.Value as Dictionary<string, string>;
                    if (chkbxs == null)
                        Assert.Fail("No values are specified");
                    foreach (var chkbx in chkbxs)
                    {
                        var chkBx = WebDriver.GetElementById(chkbx.Key);
                        try
                        {
                            Assert.AreEqual(chkbx.Value, chkBx.Selected.ToString().ToLower(), $"Wrong value: expected '{chkbx.Value.ToLower()}' but actualy is '{chkBx.Selected.ToString().ToLower()}'!");
                        }
                        catch
                        {
                            chkBx.ScrollIntoView();
                            throw;
                        }
                    }
                    break;
                // Verify toggle button value
                case FieldType.Toggle:
                    Validate().AssertToggleControlValue(item.Value.ToString(), item.NameOrId);
                    break;
                case FieldType.ListBox:
                    Validate().AssertListBoxValueSelected(item.Value.ToString(), item.NameOrId, editMode, item.WithInput);
                    break;
                case FieldType.TreeListBox:
                    Validate().AssertTreeListBoxValueSelected(item.Value.ToString(), item.NameOrId);
                    break;
                case FieldType.Radio:
                    Validate().AssertRadioButtonValue(item.NameOrId, item.Value != null ? item.Value.ToString() : "");
                    break;
                //Verifying Edit fields (one line)
                case FieldType.Color:
                case FieldType.Text:
                    {
                        Validate().AssertTextBoxValue(item.Value.ToString(), item.NameOrId);
                        break;
                    }
                case FieldType.Label:
                    {
                        var labelCtrl = WebDriver.GetElementByQuery(GetElementBy.Tagname, $"label[for='{item.NameOrId}']");
                        Validate().AssertTextBoxValue(item.Value.ToString(), labelCtrl);
                        break;
                    }
                //Verifying Date field
                case FieldType.Date:
                    var calendarEdit = WebDriver.GetElementByName(item.NameOrId) ??
                                       WebDriver.GetElementByQuery(GetElementBy.Tagname,
                                           $"label[for='{item.NameOrId.Replace('.', '_')}']");
                    //.GetParent().GetParent().GetElementByjQ(GetElementBy.Tagname, "input");
                    //calendarEdit.SetFocus();
                    Validate().AssertDateValue(item.Value.ToString(), calendarEdit);
                    break;
                //Verifying Numeric fields
                case FieldType.Numeric:
                    {
                        var editField = WebDriver.GetElementById(item.NameOrId);
                        //editField.SetFocus();
                        Validate().AssertNumericTextBoxValue(item.Value.ToString(), editField, editMode);
                        break;
                    }
                ////Verifying Kendo Grids: Phones, NetAddresses, Identifiers
                case FieldType.DetailTable:
                    //Description - is numberOfRows
                    Validate().AssertDetailTable(item.NameOrId, Convert.ToInt16(item.Description), item.Value, item.SubType);
                    break;
                //Verifying Multiselect grid
                case FieldType.Multi:
                    var arrayValues = (string[])item.Value;
                    if (arrayValues == null || (arrayValues.Length == 0) || (arrayValues.Length == 1 && string.IsNullOrEmpty(arrayValues[0])))
                        break;
                    Validate().AssertInMultiselectGrid(item.NameOrId, arrayValues);
                    break;
                case FieldType.KendoMultiSelect:
                    Validate().AssertValuesInKendoMultiSelect(item.NameOrId, (string[])item.Value);
                    break;
                //Verifying Sinlge select fields
                case FieldType.Single:
                    Validate().AssertSingleSelectTextBoxValue((item.Description ?? item.Value.ToString()), item.NameOrId, editMode, item.TransactionCode);
                    break;
                // Verifying fields area list: Addresses, Operations
                case FieldType.ListView:
                    Validate().AssertListView(item.NameOrId, Convert.ToInt16(item.Description), item.Value, editMode);
                    break;
                //case FieldType.SelectedPropertiesTree:
                case FieldType.EntityPropertiesTree:
                    if (item.Value.ToString() == "") break;
                    var treeList = item.Description.Split('.');
                    var propertyTreeDescription = treeList[0];
                    for (var p = 1; p < treeList.Length; ++p)
                    {
                        if (treeList[p] == "Account") continue;//Until Account stops fading
                        propertyTreeDescription = $"{propertyTreeDescription} {treeList[p]}";
                    }
                    Validate().AssertEntityPropertiesTreeValue(item.NameOrId, item.DescriptionShort ?? propertyTreeDescription);
                    break;
                case FieldType.RowsTree:
                    break;
                // Verifying Extender status
                case FieldType.Extender:
                    if (editMode)
                        Validate().AssertExtenderStatus(item.NameOrId, item.TabName, (string)item.Value);
                    break;
                case FieldType.Select:
                    Validate().AssertSelectControlValue(item.NameOrId, (string)item.Value);
                    break;
                case FieldType.File:
                    Validate().AssertFile((string)item.Value);
                    break;
            }
            FinishStep();
        }

        public void VerifyEntityCard(bool editMode = true)
        {
            #region Verify section

            StartStep("Verifying the card");
            Page.SetLanguage();
            foreach (var item in CardItems)
            {
                // if item is not for verification - skip it
                if (item.NotVerify)
                {
                    continue;
                }

                if (item.FieldType == FieldType.Extender && item.Value?.ToString().ToLower() == "false" && !editMode)
                {
                    var tabElem = Page.WebDriver.GetTabElement(item.TabName);
                    Assert.IsNull(tabElem, $"Extender '{item.TabName}' should be hidden but tab is present");
                    continue;
                }
                if (!string.IsNullOrEmpty(item.TabName))
                {
                    //Page.SetLanguage();
                    Page.WebDriver.GoToTab(item.TabName);
                }

                if (item.Value == null)
                    continue;

                try
                {
                    VerifyFieldProcessing(item, editMode);
                }
                catch (Exception e)
                {
                    if (e.SkipableFail()) throw;
                    Page.AddElmahDetail();
                    throw;
                }
            }
            FinishStep();
            //Page.SetLanguage();
            #endregion
        }

        public void FillSendAndVerifyComment(string text, bool addCommentToMultipleEntities = false, int index = 0)
        {
            StartStep($"Fill, send and verify comment '{text}'");
            // Fill comment
            var firstOrDefault = Page.GetCommentsPopUpDialogAndWaitForItsVisibility.GetElementsByClassName("comments-body").FirstOrDefault(el => el.IsVisiblbe());
            if (firstOrDefault == null) Assert.Fail("'Comments-body' element was not found in comment slider");
            var input = firstOrDefault.GetElementByQuery(GetElementBy.Tagname, "textarea[placeholder='Enter comment...']");
            WebDriver.SetTextBoxValue(input, text);

            // Send comment
            var webElement = firstOrDefault.GetElementsByTagName("button").FirstOrDefault(elem => elem.Text == "Send");
            if (webElement == null) Assert.Fail("'Send' button was not found");
            webElement.Click2();

            if (!addCommentToMultipleEntities)
            {
                //Verify that comment was added
                VerifyComment(text, index);
            }
            // Close comments form
            Page.OpenCloseCommentSlider(false);
            FinishStep();
        }

        public void VerifyComment(string text, int index = 0)
        {
            try
            {
                PageUpdate();
                StartStep($"Verify comment '{text}'");
                var commentsListContainer = WebDriver.GetElementByClassName("comment-items-cont.list-group");
                var h4Element = WebDriver.WaitUntilElementByClassIsAvailable("media-heading", commentsListContainer);
                if (h4Element == null)
                {
                    WebDriver.WaitForReadyState();
                    PageUpdate();
                    commentsListContainer = WebDriver.GetElementByClassName("comment-items-cont.list-group");
                    h4Element = WebDriver.WaitUntilElementByClassIsAvailable("media-heading", commentsListContainer);
                }
                if (h4Element == null)
                    Assert.Fail("Comment owner not found!");
                var h4Text = h4Element.Text;
                var timeStamp = DateTime.Today.ToString("d", BasePageElementMap.GetCurrentCultureInfo);
                if (!h4Text.Contains(TestUserName) && !h4Text.Contains(timeStamp))
                {
                    Assert.Fail($"User name '{TestUserName}' and date '{timeStamp}' were not found in comment, {h4Text}");
                }
                WebDriver.WaitForReadyState();
                var commentsList = Page.GetCommentsPopUpDialog.GetElementsByClassName("comment-body");
                if (!commentsList[index].Text.EndsWith(text))
                {
                    var found = false;
                    for (var p = 0; p < commentsList.Count; ++p)
                    {
                        if (!commentsList[p].Text.EndsWith(text)) continue;
                        found = true;
                        StartStep($"WARNING! Comment with text '{text}' was found in item with index={p}, although expected in item with index={index}");
                        FinishStep();
                    }
                    if (!found)
                        Assert.Fail($"Could not find comment with text '{text}' (index={index})");
                }
                FinishStep();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        public void EditAndVerifyComment(string text)
        {
            try
            {
                StartStep("Edit and verify comment " + text);
                var lnkEdit = Page.GetCommentsPopUpDialogAndWaitForItsVisibility.GetElementByOneAttribute("Edit");
                lnkEdit.Click2();

                // Verify Cancel button works
                var firstOrDefault = Page.GetCommentsPopUpDialogAndWaitForItsVisibility.GetElementsByTagName("button").FirstOrDefault(elem => elem.Text == "Cancel");
                if (firstOrDefault == null) Assert.Fail("'Cancel' button was not found");
                firstOrDefault.Click2();

                lnkEdit.Click2();

                var editContainer = Page.GetCommentsPopUpDialogAndWaitForItsVisibility.GetElementByClassName("input-group-btn").GetParent();
                var editField = editContainer.GetElementByTagName("textarea");
                WebDriver.SetTextBoxValue(editField, text);

                // Click Save
                var btnSave = Page.GetCommentsPopUpDialogAndWaitForItsVisibility.GetElementsByTagName("button").FirstOrDefault(elem => elem.Text == "Save");
                if (btnSave == null) Assert.Fail("'Save' button was not found");
                btnSave.Click2();
                DriverWait.WaitForElementIsNotVisible(btnSave);

                // Verify comment was added
                VerifyComment(text);
                FinishStep();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        public void DeleteAllComments()
        {
            var delLinks = Page.GetCommentsPopUpDialogAndWaitForItsVisibility.GetElementsByClassName("btn-delete-comment");
            if (delLinks != null)
            {
                var countDelLinks = delLinks.Count;
                StartStep($"Delete all comments ({countDelLinks})");
                var lnkDelete = delLinks[0];
                while (lnkDelete != null)
                {
                    lnkDelete.Click2();
                    Page.ModalButtonClick(buttonText: "No");

                    lnkDelete.Click2();
                    Page.ModalButtonClick();

                    delLinks = Page.GetCommentsPopUpDialogAndWaitForItsVisibility.GetElementsByClassName("btn-delete-comment");
                    if (delLinks != null && countDelLinks == delLinks.Count)
                        Assert.Fail("Comment was not deleted");
                    lnkDelete = delLinks?[0];
                }
            }
            // Close comments form
            Page.OpenCloseCommentSlider(false);
            FinishStep();
        }

        public void DeleteComment()
        {
            StartStep("Delete last comment");
            var delLinks =
                Page.GetCommentsPopUpDialogAndWaitForItsVisibility.GetElementsByClassName("btn-delete-comment");
            var lnkDelete = delLinks[^1];//it is analog [delLinks.Count - 1]
            lnkDelete.Click2();
            Page.ModalButtonClick(buttonText: "No");

            lnkDelete.Click2();
            Page.ModalButtonClick();

            // Close comments form
            Page.OpenCloseCommentSlider(false);
            FinishStep();
        }

        public void CheckTheCheckboxAndClickCommentsLinkFor(IWebElement rowContainer)
        {
            var chkBox = Page.CheckBoxGridItem(rowContainer);
            Page.CheckOrUncheckTheCheckbox("true", chkBox);
            //OpenActionMenu();
            Page.OpenCloseCommentSlider();
        }

        public void CheckTheCheckboxAndClickCommentsLinkFor(int rowNumber)
        {
            var rowContainer = Page.GetTableBodyRows(null)[rowNumber];
            CheckTheCheckboxAndClickCommentsLinkFor(rowContainer);
        }

        private void _testEntityComment(IReadOnlyDictionary<string, string> comments, string[] uniqueName, string[] columnName, string[] groupName)
        {
            var row = Validate().AssertEntityExistsInGrid(columnName, uniqueName, groupElementName: groupName);
            //Add commet(s)

            var i = 0;
            if (comments.ContainsKey("Comment"))
            {
                CheckTheCheckboxAndClickCommentsLinkFor(row);
                FillSendAndVerifyComment(comments["Comment"]);
                ++i;
            }
            else
            {
                while (comments.ContainsKey("Comment" + (i + 1)))
                {
                    CheckTheCheckboxAndClickCommentsLinkFor(row);
                    FillSendAndVerifyComment(comments["Comment" + (i + 1)], index: i);
                    ++i;
                }
            }
            row = Validate().AssertEntityExistsInGrid(columnName, uniqueName);
            CheckTheCheckboxAndClickCommentsLinkFor(row);
            while (i-- > 1)
            {
                DeleteComment();
                CheckTheCheckboxAndClickCommentsLinkFor(row);
            }
            EditAndVerifyComment(comments["CommentUpd"]);
            Page.OpenCloseCommentSlider(false);
            row = Validate().AssertEntityExistsInGrid(columnName, uniqueName);
            CheckTheCheckboxAndClickCommentsLinkFor(row);
            DeleteAllComments();
        }

        private void _testEntityComment(IReadOnlyDictionary<string, string> comments, string uniqueName, string columnName = "Code", string groupName = "")
        {
            var row = Validate().AssertEntityExistsInGrid(columnName, uniqueName, groupElementName: groupName);
            //Add commet(s)

            var i = 0;
            if (comments.ContainsKey("Comment"))
            {
                CheckTheCheckboxAndClickCommentsLinkFor(row);
                FillSendAndVerifyComment(comments["Comment"]);
                ++i;
            }
            else
            {
                while (comments.ContainsKey("Comment" + (i + 1)))
                {
                    CheckTheCheckboxAndClickCommentsLinkFor(row);
                    FillSendAndVerifyComment(comments["Comment" + (i + 1)], index: i);
                    ++i;
                }
            }
            row = Validate().AssertEntityExistsInGrid(columnName, uniqueName);
            CheckTheCheckboxAndClickCommentsLinkFor(row);
            while (i-- > 1)
            {
                DeleteComment();
                CheckTheCheckboxAndClickCommentsLinkFor(row);
            }
            EditAndVerifyComment(comments["CommentUpd"]);
            Page.OpenCloseCommentSlider(false);
            row = Validate().AssertEntityExistsInGrid(columnName, uniqueName);
            CheckTheCheckboxAndClickCommentsLinkFor(row);
            DeleteAllComments();
        }

        public void TestEntityComment(Dictionary<string, string> comments, string uniqueName, string columnName = "Code", string groupName = "")
        {
            try
            {
                _testEntityComment(comments, uniqueName, columnName, groupName);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        public void TestEntityComment(Dictionary<string, string> comments, string[] uniqueName, string[] columnName, string[] groupName = null)
        {
            try
            {
                _testEntityComment(comments, uniqueName, columnName, groupName);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        public void ContextFiltering()
        {
            try
            {
                StartStep("Apply simple filtering to all collumns");
                //var rowAll = Page.CountTableRows();
                //if (rowAll == 0)
                //{
                //    Driver.Report.LogSkippedStepWithSkippedTestCase("Entities not found");
                //    return;
                //}
                Page.ContextSearchInGrid("1");
                WebDriver.WaitForReadyState();
                FinishStep();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        public bool RunAutoReconciliationProcess(string arName, ReconciliationModule module,
            string columnName = "Description")
        {
            try
            {
                Page.RunProcess(arName, TestUserName, module, columnName);
            }
            catch
            {
                Page.AddElmahDetail();
                return false;
            }
            return true;
        }


        //Getting 
        public bool GetAutoRecResult(string arName)
        {
            StartStep($"Verify results of '{arName}' - State and Status of autorec run");
            try
            {
                Navigate();
                var tries = 5;
                while (true)
                {
                    Validate().AssertEntityExistsInGrid("Description", arName);
                    try
                    {
                        Page.GetAutoRecStatistic();
                        break;
                    }
                    catch (Exception e)
                    {
                        if (e.Message.StartsWith("Process was finished with follow error message:"))
                            throw;
                        if (tries-- > 0)
                        {
                            StartStep($"Something wrong. Try again ({tries})");
                            FinishStep();
                            Navigate();
                            continue;
                        }
                        throw;
                    }
                }
            }
            catch
            {
                Page.AddElmahDetail();
                return false;
            }
            finally
            {
                FinishStep();
            }
            return true;
        }

        public void UnReconcile(string unrecBy = "", bool cash = true, bool filter = true)
        {
            //set filter:
            //cshRes 0eba3180-e1f8-4a86-b157-dca2a55ef91e
            //rcnRes e5f531e1-8c69-4394-bcdb-5d338a56b9c2
            if (filter)
                CreateFilter(new[] { $"{(cash ? "0eba3180-e1f8-4a86-b157-dca2a55ef91e" : "e5f531e1-8c69-4394-bcdb-5d338a56b9c2")}#Run.ExecutedBy.Code;false;Equal;{Page.CurrentUser}" }, $"by{Page.CurrentUser}");
            //set view:
            CreateViewWithCustomColumnByFullname("DEFVIEW");
            var module = cash ? "Cash" : "Reconciliation";
            StartStep("Remove Job with Results");
            var row = Page.CountTableRows();
            if (row > 0)
            {
                IWebElement removeButton;
                switch (unrecBy)
                {
                    case "ARJob":
                        {
                            var checkBoxAll = WebDriver.GetElementById("checkAllRecords");
                            Page.CheckOrUncheckTheCheckbox("true", checkBoxAll);
                            var classIdentifier = (row > 1) ? "action-selection-nonsingle" : "action-selection-single";
                            removeButton = (row > 1)
                            ?
                                           WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a.{classIdentifier}[title='Remove Rec Jobs with Results by selected Result ({module}) (Run In Batch Task)']") ??
                                           WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a[title='Remove Rec Jobs with Results by selected Result ({module}) (Run In Batch Task)']")
                            :
                                           WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a.{classIdentifier}[title='Remove Rec Job with Results by selected Result ({module}) (Run In Batch Task)']") ??
                                           WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a[title='Remove Rec Job with Results by selected Result ({module}) (Run In Batch Task)']");

                            break;
                        }
                    case "AROper":
                        {
                            var checkBoxAll = WebDriver.GetElementById("checkAllRecords");
                            Page.CheckOrUncheckTheCheckbox("true", checkBoxAll);
                            var classIdentifier = (row > 1) ? "action-selection-nonsingle" : "action-selection-single";
                            removeButton = (row > 1)
                            ?
                                           WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a.{classIdentifier}[title='Remove Rec Operations with Results by selected Result ({module}) (Run In Batch Task)']") ??
                                           WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a[title='Remove Rec Operations with Results by selected Result ({module}) (Run In Batch Task)']")
                            :
                                           WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a.{classIdentifier}[title='Remove Rec Operation with Results by selected Result ({module}) (Run In Batch Task)']") ??
                                           WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a[title='Remove Rec Operation with Results by selected Result ({module}) (Run In Batch Task)']");

                            break;
                        }
                    default:
                        {
                            Page.CheckRowNumber(0, withCheck: true);
                            removeButton = WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a.action-selection-single[title='Remove Rec Operation with Results by selected Result ({module}) (Run In Batch Task)']") ??
                                           WebDriver.GetElementByQuery(GetElementBy.Tagname, $"a[title='Remove Rec Operation with Results by selected Result ({module}) (Run In Batch Task)']");
                            break;
                        }
                }
                removeButton.Click2();
            }
            else Assert.Fail("No Reconciliation result found");
            try
            {
                StartStep("Press dialog 'Yes' button");
                Page.ModalButtonClick();
                int counter = 30;
                while (row == Page.CountTableRows())
                {
                    StartStep("Refresh...");
                    try
                    {
                        DriverWait.Until(d => WebDriver.RefreshButton()).Click2();
                    }
                    finally
                    {
                        FinishStep();
                    }
                    if (row == Page.CountTableRows() && counter <= 0)
                        Assert.Fail($"Entry not Unreconcilied");
                    Page.Pause();
                    counter--;
                }
            }
            finally
            {
                FinishStep();
                FinishStep();
            }

        }

        /// <summary>
        /// Run Import/Undo for File/Folder
        /// </summary>
        /// <param name="value">file or folder fullname is shown in grid</param>
        /// <param name="columnName">collumn name where value is present</param>
        /// <param name="file">what grid is processed: true = File, false = folder </param>
        /// <param name="undo">false = Import, true = Undo</param>
        public void RunFileFolderImportUndo(string value, string columnName, bool file = true, bool undo = false)
        {
            try
            {
                StartStep("Selecting default View and Filter");
                Page.CreateNewOrSelectExistingConfiguration("DEFFLTR", ConfigurationType.Filter);
                Page.CreateNewOrSelectExistingConfiguration("DEFVIEW", ConfigurationType.Layout);
                FinishStep();

                Page.First();
                Validate().AssertEntityExistsInGrid(columnName, value, checkTheCheckbox: true);
                //.btn.btn-default.action-selection-single.configpanel-action
                //import folder - [1]
                //import file   - [3]
                //undo posting  - [2]
                var runButton = WebDriver.GetElementsByQuery(GetElementBy.ClassName, "btn.btn-default.action-selection-single.configpanel-action")[undo ? 2 : file ? 3 : 1];

                //var runButton = WebDriver.GetElementByQuery(GetElementBy.Tagname,
                //    $"a[title='{(undo ? "Undo Posting (Run In Batch Task)" : file ? "Import File" : "Import Folder")} with delete by Condition (Run In Batch Task)']");

                StartStep("Run import");
                runButton.Click2(focusElement: false);

                Page.SelectValueInDropDownList(WebDriver.GetElementByQuery(GetElementBy.ClassName, "k-picker.k-dropdownlist[aria-controls='User_listbox']"), TestUserName);
                var yesButton = WebDriver.GetElementByQuery(GetElementBy.ClassName, "modal-footer").GetElementByQuery(GetElementBy.ClassName, "btn.btn-primary");
                yesButton.Click2(focusElement: false);
                FinishStep();
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
        }

        public void RunBatchTaskByCurrentUser()
        {
            StartStep($"Selecting {TestUserName} and starting Batch task.");
            Page.SelectValueInDropDownList(WebDriver.GetElementById("User_cont").GetElementByQuery(GetElementBy.ClassName, "k-picker.k-dropdownlist"), TestUserName);
            Page.ModalButtonClick();
            FinishStep();
        }

        public void WaitForEntries(string account, string filter, int entriesCount, bool undo = false)
        {
            if (string.IsNullOrEmpty(filter) && string.IsNullOrEmpty(account)) return;
            try
            {
                StartStep("Waiting until entries are imported");
                DateTime startTime = DateTime.Now;
                do
                {
                    CreateFilter(new[] { filter + account }, $"by{account}", false);
                    var countEnrties = Page.CountTableRows();
                    if (undo && countEnrties == 0
                        || !undo && countEnrties == entriesCount) break;
                    var dt2 = DateTime.Now;
                    if ((dt2 - startTime).TotalSeconds > ConfigSettingsReader.DefaultTimeOut())
                    {
                        Assert.Fail($"Entries not imported");
                    }
                    try
                    {
                        StartStep("Refresh...");
                        DriverWait.Until(d => WebDriver.GetElementByClassName("k-pager-refresh")).Click2();
                        Page.Pause();
                    }
                    finally
                    {
                        FinishStep();
                    }
                } while (true);
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
            finally
            {
                FinishStep();
            }
        }

        public void RunProcess(string[] values, string columnName = "Description")
        {
            if (values.Count(value =>
                    Page.GetElementRowNumber(GetElementBy.ClassName, "k-selectable", columnName, value,
                        checkTheCheckbox: true) > -1) == 0) return;
            try
            {
                var runButton = WebDriver.GetElementByQuery(GetElementBy.Tagname,
                    $"a[title='Run Reconciliation Process{(values.Length > 1 ? "es" : "")} (Run In Batch Task)']");
                runButton.Click2();
            }
            finally
            {
            }
            //var dialogDiv = WebDriver.GetElementById("popupDialog");
            try
            {
                Page.SelectValueInDropDownList(WebDriver.GetElementById("User").GetParent(), TestUserName);
            }
            finally
            {
            }
            try
            {
                var yesButton = WebDriver.GetElementByQuery(GetElementBy.ClassName, "modal-footer")
                    .GetElementByQuery(GetElementBy.ClassName, "btn.btn-primary");
                yesButton.Click2();
            }
            finally
            {
            }
        }

        public void SendTransferToBank(string[] values, string columnName = "Description")
        {
            if (!values.Any(value =>
                    Page.GetElementRowNumber(GetElementBy.ClassName, "k-selectable", columnName, value,
                        checkTheCheckbox: true) > -1)) return;
            try
            {
                var runButton = WebDriver.GetElementByQuery(GetElementBy.Tagname,
                    $"a.btn.btn-default.action-selection-single.configpanel-action.send-to-bank-btn");
                runButton.Click2();
            }
            finally
            {
            }
        }

        public void AcceptDeclineTicket(string[] values, string columnName, bool acceptTicket)
        {
            if (!values.Any(value =>
                    Page.GetElementRowNumber(GetElementBy.ClassName, "k-selectable", columnName, value,
                        checkTheCheckbox: true) > -1)) return;
            var actionButton = WebDriver.GetElementByQuery(GetElementBy.Tagname,
                            $"a.btn.btn-secondary.action-selection-multiple.configpanel-action.btn-{(acceptTicket ? "info" : "warning")}");
            actionButton.Click2();
            var yesButton = WebDriver.GetElementByOneAttribute("0", "button", "btn-id");
            yesButton.Click2();
        }

        /// Usage: page.GetGridStatistic("Text", "Description", "Lifecycle Status", "Sent", "Banking Status", "TrnGenerated");
        public void GetGridStatistic(string TaskName, string ColumnName = "Description", params string[] Param)
        {
            StartStep("Get results from grid");
            try
            {
                int repeatTime = 5;
                int row = -1, i = 0, lastIndex = 0;

                while (repeatTime-- > 0)
                {
                    row = Validate().AssertEntityExistsInGrid(ColumnName, TaskName);
                    var subTable = WebDriver.GetTable();
                    i = 0;
                    while (Param.Length > i + 3)
                    {
                        var state = Page.GetTableCellText(row, Param[i++], subTable);
                        if (state.Equals(Param[i++]))
                        {
                            lastIndex = i - 2;
                            continue;
                        }
                        else
                        {
                            //Not equal => try again
                            i = -1;
                            break;
                        }
                    }
                    if (i > -1)
                        break;
                    Navigate();
                    continue;
                }
                if (i == -1)
                {
                    Assert.Fail($"Transfer Sent Failed. '{TaskName}' transfer status '{Param[lastIndex + 1]}' in column '{Param[lastIndex]}' was not found");
                }
            }
            finally
            {
                FinishStep();
            }
        }
        public void GetXEyeTicketsStatistic(string TaskName, string ColumnName = "Description", params string[] Param)
        {
            StartStep("Get results from grid");
            try
            {
                int repeatTime = 5;
                int row = -1, i = 0, lastIndex = 0;

                while (repeatTime-- > 0)
                {
                    row = Validate().AssertEntityExistsInGrid(ColumnName, TaskName);
                    var subTable = WebDriver.GetTable();
                    i = 0;
                    while (Param.Length > i + 1) //3-1
                    {
                        var state = Page.GetTableCellText(row, Param[i++], subTable);
                        if (state.Equals(Param[i++]))
                        {
                            lastIndex = i - 2;
                            continue;
                        }
                        else
                        {
                            //Not equal => try again
                            i = -1;
                            break;
                        }
                    }
                    if (i > -1)
                        break;
                    continue;
                }
                if (i == -1)
                {
                    Assert.Fail($"Check XEye ticket status Failed. '{TaskName}' ticket status '{Param[lastIndex + 1]}' in column '{Param[lastIndex]}' was not found");
                }
            }
            finally
            {
                FinishStep();
            }
        }

        public void ActivationDeactivation(bool active = false)
        {
            try
            {
                StartStep($"{(active ? "A" : "Dea")}ctivate all 'Active' toggles");
                Page.SwitchToggleControl(active, WebDriver.GetElementById("Entity_Active").GetParent());
                var extenderTabs = WebDriver.GetElementsByClassName("extender");

                for (var i = 0; i < extenderTabs.Count; ++i)
                {
                    var extToggle = WebDriver.GetElementById($"Extenders_{i}__Value_Active");
                    if (extToggle != null)
                    {
                        try
                        {
                            var tabName = extenderTabs[i].GetElementTitle();
                            StartStep($"{(active ? "A" : "Dea")}ctivate 'Active' toggle on '{tabName}' tab");
                            Page.WebDriver.GoToTab(tabName);
                            Page.SwitchToggleControl(active, extToggle.GetParent());
                        }
                        finally
                        {
                            FinishStep();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.SkipableFail()) throw;
                Page.AddElmahDetail();
                throw;
            }
            finally
            {
                FinishStep();
            }
        }
    }
}
