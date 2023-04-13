using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignInTests;
using TestTools;

namespace SetupTypes.Identifier_Type
{
    [TestClass]
    public class IdentifierTypeTests : MainTest
    {
        private const string DataFile = "IdentifierTypeTestData.xml";

        public IdentifierTypeTests() : base(RelativePathForDataFiles + DataFile) { }

        [TestMethod]
        public void IdentifierTypeCreateNew()
        {
            GetDataDictionary(ConnectionString, "CreateNew");
            var page = new IdentifierTypePage();
            PrepareTest(page);
            page.CreateEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void IdentifierTypeEditByIcon()
        {
            GetDataDictionary(ConnectionString, "EditByIcon");
            var page = new IdentifierTypePage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void IdentifierTypeEditByAction()
        {
            GetDataDictionary(ConnectionString, "EditByAction");
            var page = new IdentifierTypePage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"), byIcon: false);
        }

        [TestMethod]
        public void IdentifierTypeDuplicate()
        {
            GetDataDictionary(ConnectionString, "Duplicate");
            var page = new IdentifierTypePage();
            PrepareTest(page, TestPurpose.Duplicate);
            page.DuplicateEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void IdentifierTypeViewByIcon()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new IdentifierTypePage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void IdentifierTypeViewByAction()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new IdentifierTypePage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"), byIcon: false);
        }

        [TestMethod]
        public void IdentifierTypeDeleteByIcon()
        {
            GetDataDictionary(ConnectionString, "DeleteByIcon");
            var page = new IdentifierTypePage();
            PrepareTest(page);
            page.DeleteEntityByIcon(GetDataValue("Code"));
        }

        [TestMethod]
        public void IdentifierTypeDeleteByBatch()
        {
            GetDataDictionary(ConnectionString, "DeleteByBatch");
            var page = new IdentifierTypePage();
            PrepareTest(page);
            page.DeleteEntitiesByBatch(new[] { GetDataValue("Code1"), GetDataValue("Code2") });
        }

        [TestMethod]
        public void IdentifierTypeCreateWithEmptyForm()
        {
            GetDataDictionary(ConnectionString, "CreateWithEmptyForm");
            var page = new IdentifierTypePage();
            PrepareTest(page);
            page.CreateEntityWithEmptyForm(GetDataValueArray("ErrorMessage"));
        }

        [TestMethod]
        public void IdentifierTypeCommentTest()
        {
            GetDataDictionary(ConnectionString, "CommentTest");
            var page = new IdentifierTypePage();
            SignIn(page);
            page.TestEntityComment(GetCommentsData(), GetDataValue("Code"));
        }

        [TestMethod]
        public void IdentifierTypeDeleteThroughTheCard()
        {
            GetDataDictionary(ConnectionString, "DeleteThroughTheCard");
            var page = new IdentifierTypePage();
            SignIn(page);
            page.DeleteEntityThroughTheCard(GetDataValue("Code3"));
        }

        [TestMethod]
        [TestCategory("CHBB")]
        public void IdentifierTypeChangeByBatch()
        {
            GetDataDictionary(ConnectionString, "ChangeByBatch");
            var page = new IdentifierTypePage();
            PrepareTest(page, testPurpose: TestPurpose.ChangeByBatch);
            page.ChangeByBatch(GetDataValueArray("Code"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void IdentifierTypeContextFiltering()
        {
            var page = new IdentifierTypePage();
            SignIn(page);
            page.ContextFiltering();
        }

        public void PrepareTest(IdentifierTypePage page, TestPurpose testPurpose = TestPurpose.Create)
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
                NameOrId = "Entity_MaxLength",
                FieldType = FieldType.Numeric,
                Value = GetDataValue("MaxLength")
            });
            page.CardItems.Add(new CardItem
            {
                NameOrId = "Entity_Mask",
                FieldType = FieldType.Text,
                Value = GetDataValue("Mask")
            });
        }

    }
}
