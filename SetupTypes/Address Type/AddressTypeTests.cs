using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignInTests;
using TestTools;

namespace SetupTypes.Address_Type
{
    [TestClass]
    public class AddressTypeTests : MainTest
    {
        private const string DataFile = "AddressTypeTestData.xml";

        public AddressTypeTests()
            : base(RelativePathForDataFiles + DataFile)
        {
        }

        [TestMethod]
        public void AddressTypeCreateNew()
        {
            GetDataDictionary(ConnectionString, "CreateNew");
            var page = new AddressTypePage();
            PrepareTest(page);
            page.CreateEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void AddressTypeEditByIcon()
        {
            GetDataDictionary(ConnectionString, "EditByIcon");
            var page = new AddressTypePage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void AddressTypeEditByAction()
        {
            GetDataDictionary(ConnectionString, "EditByAction");
            var page = new AddressTypePage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"), byIcon: false);
        }

        [TestMethod]
        public void AddressTypeDuplicate()
        {
            GetDataDictionary(ConnectionString, "Duplicate");
            var page = new AddressTypePage();
            PrepareTest(page, TestPurpose.Duplicate);
            page.DuplicateEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void AddressTypeViewByIcon()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new AddressTypePage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void AddressTypeViewByAction()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new AddressTypePage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"), byIcon: false);
        }

        [TestMethod]
        public void AddressTypeDeleteByIcon()
        {
            GetDataDictionary(ConnectionString, "DeleteByIcon");
            var page = new AddressTypePage();
            PrepareTest(page);
            page.DeleteEntityByIcon(GetDataValue("Code"));
        }

        [TestMethod]
        public void AddressTypeDeleteByBatch()
        {
            GetDataDictionary(ConnectionString, "DeleteByBatch");
            var page = new AddressTypePage();
            PrepareTest(page);
            page.DeleteEntitiesByBatch(new[] { GetDataValue("Code1"), GetDataValue("Code2") });
        }

        [TestMethod]
        public void AddressTypeCreateWithEmptyForm()
        {
            GetDataDictionary(ConnectionString, "CreateWithEmptyForm");
            var page = new AddressTypePage();
            PrepareTest(page);
            page.CreateEntityWithEmptyForm(GetDataValueArray("ErrorMessage"));
        }

        [TestMethod]
        public void AddressTypeCommentTest()
        {
            GetDataDictionary(ConnectionString, "CommentTest");
            var page = new AddressTypePage();
            SignIn(page);
            page.TestEntityComment(GetCommentsData(), GetDataValue("Code"));
        }

        [TestMethod]
        public void AddressTypeDeleteThroughTheCard()
        {
            GetDataDictionary(ConnectionString, "DeleteThroughTheCard");
            var page = new AddressTypePage();
            SignIn(page);
            page.DeleteEntityThroughTheCard(GetDataValue("Code3"));
        }

        [TestMethod]
        [TestCategory("CHBB")]
        public void AddressTypeChangeByBatch()
        {
            GetDataDictionary(ConnectionString, "ChangeByBatch");
            var page = new AddressTypePage();
            PrepareTest(page, testPurpose: TestPurpose.ChangeByBatch);
            page.ChangeByBatch(GetDataValueArray("Code"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void AddressTypeContextFiltering()
        {
            var page = new AddressTypePage();
            SignIn(page);
            page.ContextFiltering();
        }

        public void PrepareTest(AddressTypePage page, TestPurpose testPurpose = TestPurpose.Create)
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
                NameOrId = "Entity_CanBeMultiple",
                FieldType = FieldType.Toggle,
                Value = GetDataValue("CanBeMultiple")
            });
        }

    }
}
