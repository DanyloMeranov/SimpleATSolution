using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignInTests;
using TestTools;

namespace SetupTypes.Attachment_Type
{
    [TestClass]
    public class AttachmentTypeTests : MainTest
    {
        private const string DataFile = "AttachmentTypeTestData.xml";

        public AttachmentTypeTests()
        : base(RelativePathForDataFiles + DataFile)
        {
        }

        [TestMethod]
        public void AttachmentTypeCreateNew()
        {
            GetDataDictionary(ConnectionString, "CreateNew");
            var attachmentTypePage = new AttachmentTypePage();
            PrepareTest(attachmentTypePage);
            attachmentTypePage.CreateEntity(GetDataValue("Name"), "Name");
        }

        [TestMethod]
        public void AttachmentTypeEditByIcon()
        {
            GetDataDictionary(ConnectionString, "EditByIcon");
            var attachmentTypePage = new AttachmentTypePage();
            PrepareTest(attachmentTypePage, TestPurpose.Edit);
            attachmentTypePage.EditEntity(GetDataValue("Name"), GetDataValue("NameUpd"), "Name");
        }

        [TestMethod]
        public void AttachmentTypeEditByAction()
        {
            GetDataDictionary(ConnectionString, "EditByAction");
            var attachmentTypePage = new AttachmentTypePage();
            PrepareTest(attachmentTypePage, TestPurpose.Edit);
            attachmentTypePage.EditEntity(GetDataValue("Name"), GetDataValue("NameUpd"), "Name", byIcon: false);
        }

        [TestMethod]
        public void AttachmentTypeDuplicate()
        {
            GetDataDictionary(ConnectionString, "Duplicate");
            var attachmentTypePage = new AttachmentTypePage();
            PrepareTest(attachmentTypePage, TestPurpose.Duplicate);
            attachmentTypePage.DuplicateEntity(GetDataValue("Name"), GetDataValue("NameUpd"), "Name");
        }

        [TestMethod]
        public void AttachmentTypeViewByIcon()
        {
            GetDataDictionary(ConnectionString, "ViewByIcon");
            var attachmentTypePage = new AttachmentTypePage();
            PrepareTest(attachmentTypePage);
            attachmentTypePage.ViewEntity(GetDataValue("Name"), "Name");
        }

        [TestMethod]
        public void AttachmentTypeViewByAction()
        {
            GetDataDictionary(ConnectionString, "ViewByAction");
            var attachmentTypePage = new AttachmentTypePage();
            PrepareTest(attachmentTypePage);
            attachmentTypePage.ViewEntity(GetDataValue("Name"), "Name", byIcon: false);
        }

        [TestMethod]
        public void AttachmentTypeDeleteByIcon()
        {
            GetDataDictionary(ConnectionString, "DeleteByIcon");
            var attachmentTypePage = new AttachmentTypePage();
            PrepareTest(attachmentTypePage);
            attachmentTypePage.DeleteEntityByIcon(GetDataValue("Name"), "Name");
        }

        [TestMethod]
        public void AttachmentTypeDeleteByBatch()
        {
            GetDataDictionary(ConnectionString, "DeleteByBatch");
            var attachmentTypePage = new AttachmentTypePage();
            PrepareTest(attachmentTypePage);
            attachmentTypePage.DeleteEntitiesByBatch(new[] { GetDataValue("Name1"), GetDataValue("Name2") }, "Name");
        }

        [TestMethod]
        public void AttachmentTypeCreateWithEmptyForm()
        {
            GetDataDictionary(ConnectionString, "CreateWithEmptyForm");
            var attachmentTypePage = new AttachmentTypePage();
            SignIn(attachmentTypePage);
            attachmentTypePage.CreateEntityWithEmptyForm(GetDataValueArray("ErrorMessage"));
        }

        [TestMethod]
        public void AttachmentTypeCommentTest()
        {
            GetDataDictionary(ConnectionString, "CommentTest");
            var attachmentTypePage = new AttachmentTypePage();
            SignIn(attachmentTypePage);
            attachmentTypePage.TestEntityComment(GetCommentsData(), GetDataValue("Name"), "Name");
        }

        [TestMethod]
        public void AttachmentTypeDeleteThroughTheCard()
        {
            GetDataDictionary(ConnectionString, "DeleteThroughTheCard");
            var attachmentTypePage = new AttachmentTypePage();
            SignIn(attachmentTypePage);
            attachmentTypePage.DeleteEntityThroughTheCard(GetDataValue("Name3"), "Name");
        }

        [TestMethod]
        public void AttachmentTypeCheckThePredefinedButton()
        {
            GetDataDictionary(ConnectionString, "CheckThePredefinedButton");
            var attachmentTypePage = new AttachmentTypePage();
            SignIn(attachmentTypePage);
            attachmentTypePage.CheckThePredefined(new[] { GetDataValue("Name"), GetDataValue("Name1") }, "Name");
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void AttachmentTypeContextFiltering()
        {
            var page = new AttachmentTypePage();
            SignIn(page);
            page.ContextFiltering();
        }

        public void PrepareTest(AttachmentTypePage attachmentTypePage, TestPurpose testPurpose = TestPurpose.Create)
        {
            SignIn(attachmentTypePage);

            attachmentTypePage.CardItems.Add(new CardItem
            {
                NameOrId = "Entity_Name",
                FieldType = FieldType.Text,
                Value = (testPurpose == TestPurpose.Edit|| testPurpose == TestPurpose.Duplicate)
                ? GetDataValue("NameUpd") : GetDataValue("Name")
            });
            attachmentTypePage.CardItems.Add(new CardItem
            {
                NameOrId = "Entity_Description",
                FieldType = FieldType.Text,
                Value = GetDataValue("Description"),
            });
            attachmentTypePage.CardItems.Add(new CardItem
            {
                NameOrId = "Entity_Extension",
                FieldType = FieldType.Text,
                Value = GetDataValue("Extension"),
            });
        }
    }
}
