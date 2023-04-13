using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignInTests;
using TestTools;

namespace SetupTypes.Net_Address_Type
{
    [TestClass]
    public class NetAddressTypeTests : MainTest
    {
        private const string DataFile = "NetAddressTypeTestData.xml";

        public NetAddressTypeTests() : base(RelativePathForDataFiles + DataFile) { }

        [TestMethod]
        public void NetAddressTypeCreateNew()
        {
            GetDataDictionary(ConnectionString, "CreateNew");
            var page = new NetAddressTypePage();
            PrepareTest(page);
            page.CreateEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void NetAddressTypeEditByIcon()
        {
            GetDataDictionary(ConnectionString, "EditByIcon");
            var page = new NetAddressTypePage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void NetAddressTypeEditByAction()
        {
            GetDataDictionary(ConnectionString, "EditByAction");
            var page = new NetAddressTypePage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"), byIcon: false);
        }

        [TestMethod]
        public void NetAddressTypeDuplicate()
        {
            GetDataDictionary(ConnectionString, "Duplicate");
            var page = new NetAddressTypePage();
            PrepareTest(page, TestPurpose.Duplicate);
            page.DuplicateEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void NetAddressTypeViewByIcon()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new NetAddressTypePage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void NetAddressTypeViewByAction()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new NetAddressTypePage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"), byIcon: false);
        }

        [TestMethod]
        public void NetAddressTypeDeleteByIcon()
        {
            GetDataDictionary(ConnectionString, "DeleteByIcon");
            var page = new NetAddressTypePage();
            PrepareTest(page);
            page.DeleteEntityByIcon(GetDataValue("Code"));
        }

        [TestMethod]
        public void NetAddressTypeDeleteByBatch()
        {
            GetDataDictionary(ConnectionString, "DeleteByBatch");
            var page = new NetAddressTypePage();
            PrepareTest(page);
            page.DeleteEntitiesByBatch(new[] { GetDataValue("Code1"), GetDataValue("Code2") });
        }

        [TestMethod]
        public void NetAddressTypeCreateWithEmptyForm()
        {
            GetDataDictionary(ConnectionString, "CreateWithEmptyForm");
            var page = new NetAddressTypePage();
            PrepareTest(page);
            page.CreateEntityWithEmptyForm(GetDataValueArray("ErrorMessage"));
        }

        [TestMethod]
        public void NetAddressTypeCommentTest()
        {
            GetDataDictionary(ConnectionString, "CommentTest");
            var page = new NetAddressTypePage();
            SignIn(page);
            page.TestEntityComment(GetCommentsData(), GetDataValue("Code"));
        }

        [TestMethod]
        public void NetAddressDeleteThroughTheCard()
        {
            GetDataDictionary(ConnectionString, "DeleteThroughTheCard");
            var page = new NetAddressTypePage();
            SignIn(page);
            page.DeleteEntityThroughTheCard(GetDataValue("Code3"));
        }

        [TestMethod]
        [TestCategory("CHBB")]
        public void NetAddressChangeByBatch()
        {
            GetDataDictionary(ConnectionString, "ChangeByBatch");
            var page = new NetAddressTypePage();
            PrepareTest(page, testPurpose: TestPurpose.ChangeByBatch);
            page.ChangeByBatch(GetDataValueArray("Code"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void NetAddressTypeContextFiltering()
        {
            var page = new NetAddressTypePage();
            SignIn(page);
            page.ContextFiltering();
        }

        public void PrepareTest(NetAddressTypePage page, TestPurpose testPurpose = TestPurpose.Create)
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
                NameOrId = "Entity_Mask",
                FieldType = FieldType.Text,
                Value = GetDataValue("Mask")
            });
        }

    }
}
