using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignInTests;
using TestTools;

namespace SetupTypes.Account_Format
{

    [TestClass]
    public class AccountFormatTests : MainTest
    {
        private const string DataFile = "AccountFormatTestData.xml";

        public AccountFormatTests() : base(RelativePathForDataFiles + DataFile) { }

        [TestMethod]
        [TestCategory("QUICK")]
        [TestCategory("VICASH")]
        public void AccountFormatCreateNew()
        {
            GetDataDictionary(ConnectionString, "CreateNew");
            var accountFormatPage = new AccountFormatPage();
            PrepareTest(accountFormatPage);
            accountFormatPage.CreateEntity(GetDataValue("Code"));
        }

        [TestMethod]
        [TestCategory("UINightly")]
        [TestCategory("QUICK")]
        [TestCategory("VICASH")]
        public void AccountFormatViewByIcon()
        {
            GetDataDictionary(ConnectionString, "ViewByIcon");
            var accountFormatPage = new AccountFormatPage();
            SignIn(accountFormatPage);
            accountFormatPage.ViewEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void AccountFormatViewByActionLink()
        {
            GetDataDictionary(ConnectionString, "ViewByActionLink");
            var accountFormatPage = new AccountFormatPage();
            SignIn(accountFormatPage);
            accountFormatPage.ViewEntity(GetDataValue("Code"), byIcon: false);
        }


        [TestMethod]
        public void AccountFormatDuplicate()
        {
            GetDataDictionary(ConnectionString, "Duplicate");
            var accountFormatPage = new AccountFormatPage();
            PrepareTest(accountFormatPage, TestPurpose.Duplicate);
            accountFormatPage.DuplicateEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        [TestCategory("VICASH")]
        public void AccountFormatEditByIcon()
        {
            GetDataDictionary(ConnectionString, "EditByIcon");
            var accountFormatPage = new AccountFormatPage();
            PrepareTest(accountFormatPage, TestPurpose.Edit);
            accountFormatPage.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void AccountFormatEditByActionLink()
        {
            GetDataDictionary(ConnectionString, "EditByAction");
            var accountFormatPage = new AccountFormatPage();
            PrepareTest(accountFormatPage, TestPurpose.Edit);
            accountFormatPage.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"), byIcon: false);
        }

        [TestMethod]
        [TestCategory("QUICK")]
        [TestCategory("VICASH")]
        public void AccountFormatDeleteByIcon()
        {
            GetDataDictionary(ConnectionString, "DeleteByIcon");
            var accountFormatPage = new AccountFormatPage();
            SignIn(accountFormatPage);
            accountFormatPage.DeleteEntityByIcon(GetDataValue("Code"));
        }

        [TestMethod]
        public void AccountFormatDeleteByBatch()
        {
            GetDataDictionary(ConnectionString, "DeleteByBatch");
            var accountFormatPage = new AccountFormatPage();
            SignIn(accountFormatPage);
            accountFormatPage.DeleteEntitiesByBatch(GetDataValueArray("Code"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        [TestCategory("VICASH")]
        public void AccountFormatCreateWithEmptyForm()
        {
            GetDataDictionary(ConnectionString, "CreateWithEmptyForm");
            var accountFormatPage = new AccountFormatPage();
            SignIn(accountFormatPage);
            accountFormatPage.CreateEntityWithEmptyForm(GetDataValueArray("ErrorMessage"), "Account Format");
        }

        [TestMethod]
        public void AccountFormatCommentTest()
        {
            GetDataDictionary(ConnectionString, "CommentTest");
            var accountFormatPage = new AccountFormatPage();
            SignIn(accountFormatPage);
            accountFormatPage.TestEntityComment(GetCommentsData(), GetDataValue("Code"));
        }

        [TestMethod]
        public void AccountFormatDeleteThroughTheCard()
        {
            GetDataDictionary(ConnectionString, "DeleteThroughTheCard");
            var accountFormatPage = new AccountFormatPage();
            SignIn(accountFormatPage);
            accountFormatPage.DeleteEntityThroughTheCard(GetDataValue("Code3"));
        }

        [TestMethod]
        [TestCategory("CHBB")]
        public void AccountFormatChangeByBatch()
        {
            GetDataDictionary(ConnectionString, "ChangeByBatch");
            var accountFormatPage = new AccountFormatPage();
            PrepareTest(accountFormatPage, TestPurpose.ChangeByBatch);
            accountFormatPage.ChangeByBatch(GetDataValueArray("Code"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void AccountFormatContextFiltering()
        {
            var page = new AccountFormatPage();
            SignIn(page);
            page.ContextFiltering();
        }

        public void PrepareTest(AccountFormatPage page, TestPurpose testPurpose = TestPurpose.Create)
        {
            SignIn(page);

            if (testPurpose != TestPurpose.ChangeByBatch)
                page.CardItems.Add(new CardItem
                {
                    NameOrId = "Entity_Code",
                    FieldType = FieldType.Text,
                    Value = (testPurpose == TestPurpose.Create) ? GetDataValue("Code") : GetDataValue("CodeUpd")
                });
            page.CardItems.Add(new CardItem
            {
                NameOrId = "Entity_Description",
                FieldType = FieldType.Text,
                Value = GetDataValue("Description")
            });
            page.CardItems.Add(new CardItem
            {
                NameOrId = "Entity_Notes",
                FieldType = FieldType.Text,
                Value = GetDataValue("Notes"),
                NotVerify = true
            });
            page.CardItems.Add(new CardItem
            {
                NameOrId = "Entity_Country",
                FieldType = FieldType.Single,
                Value = GetDataValue("CountryCode"),
                Description = GetDataValue("CountryDescription")
            });
            page.CardItems.Add(new CardItem
            {
                NameOrId = "Entity_DefaultForCountry",
                FieldType = FieldType.Toggle,
                Value = GetDataValue("DefaultForCountry")
            });

            page.CardItems.Add(new CardItem
            {
                NameOrId = "Entity_BBANFormat",
                FieldType = FieldType.Text,
                Value = GetDataValue("LocalFormat")
            });
            var ibanToggle = GetDataValue("IbanIndepended");
            if (ibanToggle != null && (ibanToggle.ToLower() == "yes" || ibanToggle.ToLower() == "true"))
            {
                page.CardItems.Add(new CardItem
                {
                    NameOrId = "Entity_IbanIndependent",
                    FieldType = FieldType.Toggle,
                    Value = ibanToggle
                });
                page.CardItems.Add(new CardItem
                {
                    NameOrId = "Entity_IBANFormat",
                    FieldType = FieldType.Text,
                    Value = GetDataValue("IbanFormat")
                });
            }
            var freeToggle = GetDataValue("FreeFormSupported");
            if (freeToggle == null || (freeToggle.ToLower() != "yes" && freeToggle.ToLower() != "true")) return;
            page.CardItems.Add(new CardItem
            {
                NameOrId = "Entity_FreeFormSupported",
                FieldType = FieldType.Toggle,
                Value = freeToggle
            });
            page.CardItems.Add(new CardItem
            {
                NameOrId = "Entity_FreeFormat",
                FieldType = FieldType.Text,
                Value = GetDataValue("FreeFormat")
            });
        }
    }
}

