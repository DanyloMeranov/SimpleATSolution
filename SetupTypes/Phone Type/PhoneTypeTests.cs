using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignInTests;
using TestTools;

namespace SetupTypes.Phone_Type
{
    [TestClass]
    public class PhoneTypeTests : MainTest
    {
        private const string DataFile = "PhoneTypeTestData.xml";

        public PhoneTypeTests() : base(RelativePathForDataFiles + DataFile) { }

        [TestMethod]
        public void PhoneTypeCreateNew()
        {
            GetDataDictionary(ConnectionString, "CreateNew");
            var page = new PhoneTypePage();
            PrepareTest(page);
            page.CreateEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void PhoneTypeEditByIcon()
        {
            GetDataDictionary(ConnectionString, "EditByIcon");
            var page = new PhoneTypePage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void PhoneTypeEditByAction()
        {
            GetDataDictionary(ConnectionString, "EditByAction");
            var page = new PhoneTypePage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"), byIcon: false);
        }

        [TestMethod]
        public void PhoneTypeDuplicate()
        {
            GetDataDictionary(ConnectionString, "Duplicate");
            var page = new PhoneTypePage();
            PrepareTest(page, TestPurpose.Duplicate);
            page.DuplicateEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void PhoneTypeViewByIcon()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new PhoneTypePage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void PhoneTypeViewByAction()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new PhoneTypePage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"), byIcon: false);
        }

        [TestMethod]
        public void PhoneTypeDeleteByIcon()
        {
            GetDataDictionary(ConnectionString, "DeleteByIcon");
            var page = new PhoneTypePage();
            PrepareTest(page);
            page.DeleteEntityByIcon(GetDataValue("Code"));
        }

        [TestMethod]
        public void PhoneTypeDeleteByBatch()
        {
            GetDataDictionary(ConnectionString, "DeleteByBatch");
            var page = new PhoneTypePage();
            PrepareTest(page);
            page.DeleteEntitiesByBatch(new[] { GetDataValue("Code1"), GetDataValue("Code2") });
        }

        [TestMethod]
        public void PhoneTypeCreateWithEmptyForm()
        {
            GetDataDictionary(ConnectionString, "CreateWithEmptyForm");
            var page = new PhoneTypePage();
            PrepareTest(page);
            page.CreateEntityWithEmptyForm(GetDataValueArray("ErrorMessage"));
        }

        [TestMethod]
        public void PhoneTypeCommentTest()
        {
            GetDataDictionary(ConnectionString, "CommentTest");
            var page = new PhoneTypePage();
            SignIn(page);
            page.TestEntityComment(GetCommentsData(), GetDataValue("Code"));
        }

        [TestMethod]
        public void PhoneTypeDeleteThroughTheCard()
        {
            GetDataDictionary(ConnectionString, "DeleteThroughTheCard");
            var page = new PhoneTypePage();
            SignIn(page);
            page.DeleteEntityThroughTheCard(GetDataValue("Code3"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void PhoneTypeContextFiltering()
        {
            var page = new PhoneTypePage();
            SignIn(page);
            page.ContextFiltering();
        }

        public void PrepareTest(PhoneTypePage page, TestPurpose testPurpose = TestPurpose.Create)
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
