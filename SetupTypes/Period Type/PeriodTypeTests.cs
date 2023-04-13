using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignInTests;
using TestTools;

namespace SetupTypes.Period_Type
{
    [TestClass]
    public class PeriodTypeTests : MainTest
    {
        private const string DataFile = "PeriodTypeTestData.xml";

        public PeriodTypeTests() : base(RelativePathForDataFiles + DataFile) { }

        [TestMethod]
        public void PeriodTypeCreateNew()
        {
            GetDataDictionary(ConnectionString, "CreateNew");
            var page = new PeriodTypePage();
            PrepareTest(page);
            page.CreateEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void PeriodTypeEditByIcon()
        {
            GetDataDictionary(ConnectionString, "EditByIcon");
            var page = new PeriodTypePage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void PeriodTypeEditByAction()
        {
            GetDataDictionary(ConnectionString, "EditByAction");
            var page = new PeriodTypePage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"), byIcon: false);
        }

        [TestMethod]
        public void PeriodTypeDuplicate()
        {
            GetDataDictionary(ConnectionString, "Duplicate");
            var page = new PeriodTypePage();
            PrepareTest(page, TestPurpose.Duplicate);
            page.DuplicateEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void PeriodTypeViewByIcon()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new PeriodTypePage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void PeriodTypeViewByAction()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new PeriodTypePage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"), byIcon: false);
        }

        [TestMethod]
        public void PeriodTypeDeleteByIcon()
        {
            GetDataDictionary(ConnectionString, "DeleteByIcon");
            var page = new PeriodTypePage();
            PrepareTest(page);
            page.DeleteEntityByIcon(GetDataValue("Code"));
        }

        [TestMethod]
        public void PeriodTypeDeleteByBatch()
        {
            GetDataDictionary(ConnectionString, "DeleteByBatch");
            var page = new PeriodTypePage();
            PrepareTest(page);
            page.DeleteEntitiesByBatch(new[] { GetDataValue("Code1"), GetDataValue("Code2") });
        }

        [TestMethod]
        public void PeriodTypeCreateWithEmptyForm()
        {
            GetDataDictionary(ConnectionString, "CreateWithEmptyForm");
            var page = new PeriodTypePage();
            PrepareTest(page);
            page.CreateEntityWithEmptyForm(GetDataValueArray("ErrorMessage"));
        }

        [TestMethod]
        public void PeriodTypeCommentTest()
        {
            GetDataDictionary(ConnectionString, "CommentTest");
            var page = new PeriodTypePage();
            SignIn(page);
            page.TestEntityComment(GetCommentsData(), GetDataValue("Code"));
        }

        [TestMethod]
        public void PeriodTypeDeleteThroughTheCard()
        {
            GetDataDictionary(ConnectionString, "DeleteThroughTheCard");
            var page = new PeriodTypePage();
            SignIn(page);
            page.DeleteEntityThroughTheCard(GetDataValue("Code3"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void PeriodTypeContextFiltering()
        {
            var page = new PeriodTypePage();
            SignIn(page);
            page.ContextFiltering();
        }

        public void PrepareTest(PeriodTypePage page, TestPurpose testPurpose = TestPurpose.Create)
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
        }

    }
}
