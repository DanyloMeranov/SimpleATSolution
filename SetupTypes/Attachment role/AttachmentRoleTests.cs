using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignInTests;
using TestTools;

namespace SetupTypes.Attachment_role
{
    [TestClass]
    public class AttachmentRoleTests : MainTest
    {
        private const string DataFile = "AttachmentRoleTestData.xml";

        public AttachmentRoleTests()
        : base(RelativePathForDataFiles + DataFile)
        {
        }

        [TestMethod]
        public void AttachmentRoleCreateNew()
        {
            GetDataDictionary(ConnectionString, "CreateNew");
            var page = new AttachmentRolePage();
            PrepareTest(page);
            page.CreateEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void AttachmentRoleEditByIcon()
        {
            GetDataDictionary(ConnectionString, "EditByIcon");
            var page = new AttachmentRolePage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void AttachmentRoleEditByAction()
        {
            GetDataDictionary(ConnectionString, "EditByAction");
            var page = new AttachmentRolePage();
            PrepareTest(page, TestPurpose.Edit);
            page.EditEntity(GetDataValue("Code"), GetDataValue("CodeUpd"), byIcon: false);
        }

        [TestMethod]
        public void AttachmentRoleDuplicate()
        {
            GetDataDictionary(ConnectionString, "Duplicate");
            var page = new AttachmentRolePage();
            PrepareTest(page, TestPurpose.Duplicate);
            page.DuplicateEntity(GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void AttachmentRoleViewByIcon()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new AttachmentRolePage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"));
        }

        [TestMethod]
        public void AttachmentRoleViewByAction()
        {
            GetDataDictionary(ConnectionString, "View");
            var page = new AttachmentRolePage();
            PrepareTest(page);
            page.ViewEntity(GetDataValue("Code"), byIcon: false);
        }

        [TestMethod]
        public void AttachmentRoleDeleteByIcon()
        {
            GetDataDictionary(ConnectionString, "DeleteByIcon");
            var page = new AttachmentRolePage();
            PrepareTest(page);
            page.DeleteEntityByIcon(GetDataValue("Code"));
        }

        [TestMethod]
        public void AttachmentRoleDeleteByBatch()
        {
            GetDataDictionary(ConnectionString, "DeleteByBatch");
            var page = new AttachmentRolePage();
            PrepareTest(page);
            page.DeleteEntitiesByBatch(new[] { GetDataValue("Code1"), GetDataValue("Code2") });
        }

        [TestMethod]
        public void AttachmentRoleCreateWithEmptyForm()
        {
            GetDataDictionary(ConnectionString, "CreateWithEmptyForm");
            var page = new AttachmentRolePage();
            PrepareTest(page);
            page.CreateEntityWithEmptyForm(GetDataValueArray("ErrorMessage"));
        }

        [TestMethod]
        public void AttachmentRoleCommentTest()
        {
            GetDataDictionary(ConnectionString, "CommentTest");
            var page = new AttachmentRolePage();
            SignIn(page);
            page.TestEntityComment(GetCommentsData(), GetDataValue("Code"));
        }

        [TestMethod]
        public void AttachmentRoleDeleteThroughTheCard()
        {
            GetDataDictionary(ConnectionString, "DeleteThroughTheCard");
            var page = new AttachmentRolePage();
            SignIn(page);
            page.DeleteEntityThroughTheCard(GetDataValue("Code3"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void AttachmentRoleContextFiltering()
        {
            var page = new AttachmentRolePage();
            SignIn(page);
            page.ContextFiltering();
        }

        public void PrepareTest(AttachmentRolePage page, TestPurpose testPurpose = TestPurpose.Create)
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
