using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignInTests;
using TestTools;

namespace SetupTypes.Budget_Category
{
    [TestClass]
    public class BudgetCategoryTests : MainTest
    {
        private const string DataFile = "BudgetCategoryTestData.xml";

        public BudgetCategoryTests() : base(RelativePathForDataFiles + DataFile) { }

        [TestMethod]
        [TestCategory("QUICK")]
        public void BudgetCategoryCreateNew()
        {
            GetDataDictionary(ConnectionString, "CreateNew");
            var page = new BudgetCategoryPage();
            PrepareTest(page);
            page.CreateEntity(GetDataValue("Code"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void BudgetCategoryEditByIcon()
        {
            GetDataDictionary(ConnectionString, "EditByIcon");
            var page = new BudgetCategoryPage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void BudgetCategoryEditByAction()
        {
            GetDataDictionary(ConnectionString, "EditByAction");
            var page = new BudgetCategoryPage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"), byIcon: false);
        }

        [TestMethod]
        public void BudgetCategoryDuplicate()
        {
            GetDataDictionary(ConnectionString, "Duplicate");
            var page = new BudgetCategoryPage();
            PrepareTest(page, TestPurpose.Duplicate);
            page.DuplicateEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void BudgetCategoryViewByIcon()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new BudgetCategoryPage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void BudgetCategoryViewByAction()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new BudgetCategoryPage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"), byIcon: false);
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void BudgetCategoryDeleteByIcon()
        {
            GetDataDictionary(ConnectionString, "DeleteByIcon");
            var page = new BudgetCategoryPage();
            PrepareTest(page);
            page.DeleteEntityByIcon(GetDataValue("Code"));
        }

        [TestMethod]
        public void BudgetCategoryDeleteByBatch()
        {
            GetDataDictionary(ConnectionString, "DeleteByBatch");
            var page = new BudgetCategoryPage();
            PrepareTest(page);
            page.DeleteEntitiesByBatch(new[] { GetDataValue("Code1"), GetDataValue("Code2") });
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void BudgetCategoryCreateWithEmptyForm()
        {
            GetDataDictionary(ConnectionString, "CreateWithEmptyForm");
            var page = new BudgetCategoryPage();
            PrepareTest(page);
            page.CreateEntityWithEmptyForm(GetDataValueArray("ErrorMessage"));
        }

        [TestMethod]
        public void BudgetCategoryDeleteThroughTheCard()
        {
            GetDataDictionary(ConnectionString, "DeleteThroughTheCard");
            var page = new BudgetCategoryPage();
            SignIn(page);
            page.DeleteEntityThroughTheCard(GetDataValue("Code3"));
        }

        [TestMethod]
        public void BudgetCategoryCommentTest()
        {
            GetDataDictionary(ConnectionString, "CommentTest");
            var page = new BudgetCategoryPage();
            SignIn(page);
            page.TestEntityComment(GetCommentsData(), GetDataValue("Code"));
        }

        [TestMethod]
        public void BudgetCategoryCheckThePredefinedButton()
        {
            GetDataDictionary(ConnectionString, "CheckThePredefinedButton");
            var page = new BudgetCategoryPage();
            SignIn(page);
            page.CheckThePredefined(new[] { GetDataValue("Code"), GetDataValue("Code1") });
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void BudgetCategoryContextFiltering()
        {
            var page = new BudgetCategoryPage();
            SignIn(page);
            page.ContextFiltering();
        }

        public void PrepareTest(BudgetCategoryPage page, TestPurpose testPurpose = TestPurpose.Create)
        {
            SignIn(page);

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
                NameOrId = "Entity_Direction",
                FieldType = FieldType.ListBox,
                Value = GetDataValue("Direction"),
                WithInput = false
            });
            page.CardItems.Add(new CardItem
            {
                NameOrId = "Entity_IsPredefined",
                FieldType = FieldType.Toggle,
                Value = GetDataValue("Predefined")
            });
        }

    }
}
