using Microsoft.VisualStudio.TestTools.UnitTesting;
using BaseDriver;
using TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

//[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace BaseTests
{
    public class BaseTest
    {
        //public const string ProviderInvariantString = "Microsoft.VisualStudio.TestTools.DataSource.XML";
        public const string RelativeDataPath = "|DataDirectory|\\";
        public const string RelativePathForDataFiles = "DataFiles\\";
        public const string RelativePathForPerformanceDataFiles = "PerformanceData\\";
        public IReadOnlyCollection<Dictionary<string, string>> DataDictionaryCollection;

        public string UserName {get; set;}
        public string UserPass { get; set; }

        private string userFlag;

        protected Driver MainDriver;
        protected bool Initialized { get; private set; } = false;

        private Guid DriverId { get; }

        public BaseTest()
        {
            DriverId = new Guid();
        }

        #region Private members

        private string GetSuiteName()
        {
            var fullClassName = TestContext.FullyQualifiedTestClassName;
            var words = fullClassName.Split('.');

            var suiteName = words.Length > 1 ? words[^2] : null;
            return string.IsNullOrEmpty(suiteName) ? "UnnamedSuite" : suiteName;
        }

        private string GetProjectName => TestContext.FullyQualifiedTestClassName.Split('.')[0];

        internal void CloseReports()
        {
            //Report.FinishTestCase();
            //Report.FinishSuite();
        }

        #endregion

        protected IWebDriver TestDriver;
        protected WebDriverWait TestDriverWait;
        //protected static AllureNextReport Report => Driver.Report;

        [TestInitialize]
        [ClassInitialize]
        public void SetupTest()
        {
            if (Initialized) return;
            MainDriver = new Driver();
            int tNumber;
            if (ConfigSettingsReader.MultiUser && (tNumber = FileProcessor.TryToCreateFile($"{ConfigSettingsReader.ReportPath}\\User", GetSuiteName())) > 0)
            {
                UserName = $"User{tNumber:00}";
                UserPass = $"P@ssword{tNumber++:00}";
                userFlag = $"{ConfigSettingsReader.ReportPath}\\{UserName}";
            }
            else
            {
                UserName = ConfigSettingsReader.TestUserName;
                UserPass = ConfigSettingsReader.TestUserPass;
            }
            //Report.StartSuite(GetSuiteName(), GetProjectName);
            //Report.StartTestCase(TestContext.TestName, UserName);
            Initialized = true;

            try
            {
                var driverInstance = MainDriver.StartBrowser(DriverId);
                TestDriver = Driver.Instance.Browser;
                TestDriverWait = Driver.Instance.BrowserWait;
            }
            catch{
                //Report.LogFailedStepWithFailedTestCase(e);
                throw;
            }
        }

        public void AddIssue(string Name = "", string Url = "")
        {
            var IssueName = Name;
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Url))
                IssueName = Url.Split('/')[^1];
            //Report.AddIssue(GetSuiteName(), TestContext.TestName, IssueName, Url);
        }

        [TestCleanup]
        [ClassCleanup]
        public void TeardownTest()
        {
            if (!Initialized) return;
            if (!string.IsNullOrEmpty(userFlag))
                try
                {
                    StartStep($"Try to delete '{userFlag}'");
                    StartStep($"{(FileProcessor.TryToDeleteFile(userFlag) ? "Done!" : "Fail...")}");
                    FinishStep();
                }
                catch (Exception)
                {
                    StartStep("Fail...");
                    FinishStep();
                }
                finally
                {
                    FinishStep();
                }
            try
            {
                CloseReports();
            }
            catch (Exception) { }
            finally
            {
                MainDriver.StopBrowser();
                Initialized = false;
            }
        }

        public void RestartDriver()
        {
            TeardownTest();
            SetupTest();
        }

        public static void StartStep(string startMessage)
        {
            //Report.StartStep(startMessage);
        }

        public static void FinishStep()
        {
            //AllureNext//Report.FinishStep();
        }

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Dictionary for storing data values from files
        /// </summary>
        public Dictionary<string, string> DataValue { get; set; }

        public void GetDataDictionaryCollection(string fileName, string sectionName)
        {
            DataDictionaryCollection = XmlProcessor.ReadData(fileName.Replace("|DataDirectory|", Environment.CurrentDirectory), sectionName);
        }

        public bool GetDataDictionary(int index = 0)
        {
            if (index < 0) return false;
            if(DataDictionaryCollection==null) Assert.Fail("Data collection are not preloaded!");
            DataValue = new Dictionary<string, string>();
            var arrayDict = DataDictionaryCollection.ToArray();
            DataValue = arrayDict[index];
            return true;
        }


        public int GetDataDictionary(string fileName, string sectionName, int index = 0)
        {
            try
            {
                DataValue = new Dictionary<string, string>();
                GetDataDictionaryCollection(fileName, sectionName);
                DataValue = DataDictionaryCollection.ToArray()[index];
                return DataDictionaryCollection.Count;
            }
            catch {
                //Driver.Report.LogFailedStepWithFailedTestCase(e);
                throw;
            }
        }

        public string[][] GetDataMassArray(string[] keys)
        {
            var ret = new List<string[]>();
            var i = 1;
            var value = GetDataValue(keys[0] + i);
            while (value != null)
            {
                ret.Add(keys.Select(t => GetDataValue(t + i)).ToArray());
                value = GetDataValue(keys[0] + ++i);
            }
            return ret.ToArray();
        }

        public string[] GetDataValueArray(string baseString)
        {
            var ret = new List<string>();
            if (DataValue == null) return ret.ToArray();
            var i = 1;
            var value = GetDataValue(baseString + i);
            while (value != null)
            {
                ret.Add(value);
                value = GetDataValue(baseString + ++i);
            }

            if (i > 1) return ret.ToArray();

            value = GetDataValue(baseString);
            if (value == null)
                return ret.ToArray();

            ret.Add(value);
            return ret.ToArray();
        }

        /// <summary>
        /// Method gets value from Data dictionary, 
        /// if value is not found method returns Empty string
        /// </summary>
        /// <param name="keyIndex">Index name in Data dictionary</param>
        /// <returns></returns>
        public string GetDataValue(string keyIndex)
        {
            return (DataValue == null) ? string.Empty : DataValue.ContainsKey(keyIndex) ? DataValue[keyIndex] : null;
        }

        /// <summary>
        /// Method gets Identifiers from DataFile
        /// </summary>
        /// <returns>List of Identifiers</returns>
        public List<Dictionary<string, string>> GetDetailTableData(DetailTableType type, string additional = "")
        {
            var list = new List<Dictionary<string, string>>();
            string suffix = type switch
            {
                DetailTableType.AccountIdentifiers or DetailTableType.Identifiers => "Identifiers" + additional,
                DetailTableType.Phones => "Phones" + additional,
                DetailTableType.NetAddresses => "NetAddresses" + additional,
                DetailTableType.FixedCost => "FixedCost" + additional,
                DetailTableType.UnitCost => "UnitCost" + additional,
                DetailTableType.Amount => "Amount" + additional,
                DetailTableType.Steps => "Steps" + additional,
                DetailTableType.CalculationType => "CalculationType" + additional,
                DetailTableType.ZBASettings => "ZBASettings" + additional,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
            var numberOfRecords = GetDataValue($"NumberOf{suffix}");
            if (string.IsNullOrEmpty(numberOfRecords)) numberOfRecords = "0";
            var keys = new List<string>();
            switch (type)
            {
                case DetailTableType.AccountIdentifiers:
                    {
                        keys.Add("Main");
                        keys.Add("Local");
                        keys.Add("Iban");
                        keys.Add("FreeForm");
                        break;
                    }
                case DetailTableType.Phones:
                case DetailTableType.Identifiers:
                    {
                        keys.Add("Main");
                        keys.Add("Type");
                        keys.Add("Text");
                        break;
                    }
                case DetailTableType.NetAddresses:
                    {
                        keys.Add("Main");
                        keys.Add("Type");
                        keys.Add("Text");
                        keys.Add("Comment");
                        break;
                    }
                case DetailTableType.FixedCost:
                case DetailTableType.UnitCost:
                    {
                        keys.Add("StepFee");
                        keys.Add("Ceiling");
                        break;
                    }
                case DetailTableType.Steps:
                    {
                        keys.Add("FixedAmmount");
                        keys.Add("MarkupPercentage");
                        keys.Add("RateIndex");
                        keys.Add("RateIndexPart");
                        keys.Add("Ceiling");
                        break;
                    }
                case DetailTableType.Amount:
                    {
                        keys.Add("Rate");
                        keys.Add("Ceiling");
                        break;
                    }
                case DetailTableType.CalculationType:
                    {
                        keys.Add("Amount");
                        keys.Add("Percentage");
                        keys.Add("Ceiling");
                        break;
                    }
                case DetailTableType.ZBASettings:
                    {
                        keys.Add("Counterparty");
                        keys.Add("Text");
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            var maxNum = Convert.ToInt32(numberOfRecords);
            if (maxNum == 0)
                list.Add(AddValues(keys, "", suffix));

            for (var i = maxNum; i > 0; --i)
            {
                list.Add(AddValues(keys, i.ToString(), suffix));
            }

            return list;
        }

        public List<Dictionary<string, string>> GetAddressesData()
        {
            var keys = new List<string>
            {
                "AddressesMain", "Type", "CountryCode", "CountryDescription", "Zip", "Province", "City", "Street", "Building", "Apartment", "AddressLine1_", "AddressLine2_", "Comment"
            };
            var maxNum = Convert.ToInt32(GetDataValue("NumberOfAddresses"));
            var listAddresses = new List<Dictionary<string, string>>(); //List<Dictionary<string, string>>()
            for (var i = maxNum; i > 0; --i)
            {
                listAddresses.Add(AddValues(keys, i.ToString()));
            }
            return listAddresses;
        }

        public List<Dictionary<string, string>> GetCorrespondenceData()
        {
            var listOperatons = new List<Dictionary<string, string>>();
            var j = 0;
            var useInRecCsh = !string.IsNullOrEmpty(GetDataValue("UseInReconciliation")) &&
                              (GetDataValue("UseInReconciliation").ToLower() == "true" ||
                               GetDataValue("UseInReconciliation").ToLower() == "yes");
            while (GetDataValue($"LeftMap{++j}") != null)
            {
                var valuesDictionary = new Dictionary<string, string>
                {
                    {$"LeftMap{j}", GetDataValue($"LeftMap{j}")},
                    {$"RightMap{j}", GetDataValue($"RightMap{j}")}
                };
                if (!useInRecCsh) { 
                    valuesDictionary.Add($"LeftGroupingType{j}", GetDataValue($"LeftGroupingType{j}"));
                    valuesDictionary.Add($"RightGroupingType{j}", GetDataValue($"RightGroupingType{j}"));
                }

                listOperatons.Add(valuesDictionary);
            }
            return listOperatons;
        }

        public List<Dictionary<string, string>> GetOperationsData(string order = "")
        {
            var keys = new List<string>
            {
                "OperationTypeCode", "OperationTypeDescription", "SignatureTypeCode", "SignatureTypeDescription", "AmountLimitMin", "AmountLimitMax", "CurrencyCode", "CurrencyDescription", "Period"
            };
            var maxNum = Convert.ToInt32(GetDataValue($"NumberOfOperationSections{order}"));
            var listOperatons = new List<Dictionary<string, string>>();
            for (var j = maxNum; j > 0; --j)
            {
                listOperatons.Add(AddValues(keys, $"{order}{j}"));
            }
            return listOperatons;
        }

        public List<Dictionary<string, string>> GetFilesData(string numberOfAttachments = "0")
        {
            var keys = new List<string>
            {
                "AttachmentTypeName", "AttachmentTypeDescription", "AttachmentRoleCode", "AttachmentRoleDescription", "FileToAttach", "FileDescription"
            };
            var maxNum = Convert.ToInt32(numberOfAttachments);
            var listFiles = new List<Dictionary<string, string>>();
            if (maxNum == 0)
            {
                listFiles.Add(AddValues(keys, ""));
                var b = true;
                foreach (var fi in keys)
                    b &= listFiles[0][fi] != null;
                if(!b)
                    listFiles = new List<Dictionary<string, string>>();
            }
            else
                for (var j = maxNum; j > 0; --j)
                {
                    listFiles.Add(AddValues(keys, j.ToString()));
                }
            return listFiles;
        }

        public List<Dictionary<string, string>> GetLettersData()
        {
            var keys = new List<string>
            {
                "CreationDate", "Contact", "ContactDescription", "Comment"
            };
            var maxNum = Convert.ToInt32(GetDataValue("NumberOfLettersSections"));
            var listLetters = new List<Dictionary<string, string>>();
            for (var j = 1; j <= maxNum; ++j)
            {
                listLetters.Add(AddValues(keys, j.ToString()));
            }
            return listLetters;
        }

        public List<Dictionary<string, string>> GetFixedCostData()
        {
            var keys = new List<string>
            {
                "StepFee", "Celling",
            };
            var maxNum = Convert.ToInt32(GetDataValue("NumberOfLettersSections"));
            var listLetters = new List<Dictionary<string, string>>();
            for (var j = 1; j <= maxNum; ++j)
            {
                listLetters.Add(AddValues(keys, j.ToString()));
            }
            return listLetters;
        }

        public Dictionary<string, string> GetFieldParameterData(int ind)
        {
            var keys = new List<string>
            {
                "FieldType", "FieldShift", "FieldLength", "FieldMandatory", "FieldIncluded", "FieldFormat", "FieldDefValue", "FieldConverter"
            };
            var retDict = AddValues(keys, ind.ToString());
            retDict.Add("UsedIn", GetDataValue("UsedIn"));
            return retDict;
        }

        public List<Dictionary<string, string>> GetReconciliationRulesData(string prefix = "")
        {
            var listRules = new List<Dictionary<string, string>>();
            var maxNum = Convert.ToInt32(GetDataValue($"NumberOf{prefix}RulesSections"));

            var keys = new List<string>
            {
                "Priority", "ExcludeFromProcess", "RuleCode", "RuleDescription", "RedefineOptions"
            };
            for (var j = 1; j <= maxNum; ++j)
            {
                listRules.Add(AddValues(keys, j.ToString(), prefix));
            }
            return listRules;
        }

        public List<Dictionary<string, string>> GetCalendarRulesData()
        {
            var listRules = new List<Dictionary<string, string>>();
            var maxNum = Convert.ToInt32(GetDataValue("NumberOfCalendarRulesSections"));
            var keys = new List<string>
            {
                "RuleDescription", "RuleDay", "RuleDate", "RuleType"
            };
            for (var j = 1; j <= maxNum; ++j)
            {
                listRules.Add(AddValues(keys, j.ToString()));
            }
            return listRules;
        }

        public List<string> GetArray(string startName)
        {
            var values = new List<string>();
            var i = 1;
            while (true)
            {
                var val = GetDataValue($"{startName}{i}");
                if (string.IsNullOrEmpty(val)) break;
                values.Add(val);
                i++;
            }
            return values;
        }

        public List<Dictionary<string, string>> GetRecRuleListViewsData(string numberOfListViews, string prefix)
        {
            var listCriterias = new List<Dictionary<string, string>>();
            var maxNum = Convert.ToInt32(numberOfListViews);
            var isFilter = prefix.Contains("Filter");
            for (var j = 1; j <= maxNum; ++j)
            {
                var valuesDictionary = new Dictionary<string, string>();

                var keys = new List<string>
                {
                    "CriteriaType", "Property", "Description"
                };

                if (!isFilter)
                    keys.Add("Name");

                var f = GetDataValue(prefix + "F" + j);

                if (f != null && bool.Parse(f))
                {
                    keys.AddRange(new[] {"F", "FType"});

                    var criteriaType = GetDataValue(prefix + "CriteriaType" + j).ToLower();
                    var funcType = GetDataValue(prefix + "FType" + j).ToLower();

                    switch (criteriaType)
                    {
                        case "string":
                            keys.AddRange(funcType == "substring" ? new[] {"StartPosition", "Length"} : new[] {"TrimingSymbol", "Option"});
                            break;
                        case "number":
                            if (funcType == "round")
                            {
                                keys.Add("Length");
                            }
                            break;
                        case "date":
                            keys.Add("DatePart");
                            if (funcType == "datediff")
                            {
                                keys.Add("EndDate");
                            }
                            break;
                        default:
                            Assert.Fail($"Getting criterias data: wrong criteria type - '{criteriaType}'");
                            break;
                    }
                }

                if (isFilter)
                    keys.AddRange(new[] {"Operator", "FltrValue", "UseCriterion"});

                keys.ForEach(x => valuesDictionary.Add(prefix + x, GetDataValue(prefix + x + j)));

                listCriterias.Add(valuesDictionary);
            }
            return listCriterias;
        }


        public List<Dictionary<string, string>> GetCriteriaDependenciesData(string numberOfCriteriaDependencies)
        {
            var listCriteriaDeps = new List<Dictionary<string, string>>();
            var maxNum = Convert.ToInt32(numberOfCriteriaDependencies);
            var keys = new List<string>
            {
                "DependencyLeft", "DependencyRight", "Operation", "Optional"
            };
            for (var j = 1; j <= maxNum; ++j)
            {
                listCriteriaDeps.Add(AddValues(keys, j.ToString()));
            }
            return listCriteriaDeps;
        }

        public Dictionary<string, string> AddValues(List<string> keys, string index, string prefix = "")
        {
            var valuesDictionary = new Dictionary<string, string>();
            keys.ForEach(key => valuesDictionary.Add(key, GetDataValue($"{prefix}{key}{index}")));
            return valuesDictionary;
        }

        public Dictionary<string, string> GetCommentsData()
        {
            var commentsDictionary = new Dictionary<string, string>();

            if (DataValue.ContainsKey("Comment1"))
            {
                var i = 1;
                while (DataValue.ContainsKey("Comment" + i))
                {
                    commentsDictionary.Add("Comment" + i, GetDataValue("Comment" + i));
                    ++i;
                }
            }
            else
                commentsDictionary.Add("Comment", GetDataValue("Comment"));
            commentsDictionary.Add("CommentUpd", GetDataValue("CommentUpd"));
            return commentsDictionary;
        }

        public Dictionary<string, string> GetFlagsEnumValues(string componentName)
        {
            var flagEnumValues = new Dictionary<string, string>();
            var i = 0;
            while (DataValue.ContainsKey(componentName + ++i))
            {
                flagEnumValues.Add(componentName + i, GetDataValue(componentName + i));
            }
            return flagEnumValues;
        }
        public static void WaitForSomeMinutes()
        {
            int number = ConfigSettingsReader.WaitForMinutes;
            var tm1 = DateTime.Now;
            while (tm1.Minute % number > 0)
            {
                if(tm1.Second >= 10 && tm1.Second < 60)
                {
                    tm1.Pause((60 - tm1.Second) * 1000);
                    tm1 = DateTime.Now;
                    continue;
                }
                if (tm1.Minute % number < number - 1)
                    tm1.Pause(60000);
                else if (tm1.Minute % number == number - 1 && tm1.Second < 30)
                    tm1.Pause(30000);
                else if (tm1.Minute % number == number - 1 && tm1.Second < 45)
                    tm1.Pause(15000);
                else if (tm1.Minute % number == number - 1 && tm1.Second < 59)
                    tm1.Pause(1000);
                tm1 = DateTime.Now;
            }
        }
    }
}
