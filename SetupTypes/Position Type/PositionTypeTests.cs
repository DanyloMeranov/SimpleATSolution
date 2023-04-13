using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignInTests;
using System.Collections.Generic;
using TestTools;

namespace SetupTypes.Position_Type
{
    [TestClass]
    public class PositionTypeTests : MainTest
    {
        private const string DataFile = "PositionTypeTestData.xml";

        public PositionTypeTests() : base(RelativePathForDataFiles + DataFile) { }

        [TestMethod]
        [TestCategory("QUICK")]
        public void PositionTypeCreateNew()
        {
            Create(new PositionTypePage(), PrepareData("CreateNew"), GetDataValue("Code"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void PositionTypeEditByIcon()
        {
            Edit(new PositionTypePage(), PrepareData("EditByIcon", TestPurpose.Edit), GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        public void PositionTypeEditByAction()
        {
            Edit(new PositionTypePage(), PrepareData("EditByAction", TestPurpose.Edit), GetDataValue("Code"), GetDataValue("CodeUpd"), byIcon: false);
        }

        [TestMethod]
        public void PositionTypeDuplicate()
        {
            Duplicate(new PositionTypePage(), PrepareData("Duplicate", TestPurpose.Duplicate), GetDataValue("Code"), GetDataValue("CodeUpd"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void PositionTypeViewByIcon()
        {
            View(new PositionTypePage(), PrepareData("View"), GetDataValue("Code"));
        }

        [TestMethod]
        public void PositionTypeViewByAction()
        {
            View(new PositionTypePage(), PrepareData("View"), GetDataValue("Code"), byIcon: false);
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void PositionTypeDeleteByIcon()
        {
            GetDataDictionary(ConnectionString, "DeleteByIcon");
            Delete(new PositionTypePage(), GetDataValue("Code"));
        }

        [TestMethod]
        public void PositionTypeDeleteByBatch()
        {
            GetDataDictionary(ConnectionString, "DeleteByBatch");
            Delete(new PositionTypePage(), GetDataValueArray("Code"));
        }

        [TestMethod]
        public void PositionTypeDeleteThroughTheCard()
        {
            GetDataDictionary(ConnectionString, "DeleteThroughTheCard");
            DeleteThroughCard(new PositionTypePage(), GetDataValue("Code"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void PositionTypeCommentTest()
        {
            GetDataDictionary(ConnectionString, "CommentTest");
            Comment(new PositionTypePage(), GetCommentsData(), GetDataValue("Code"));
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void PositionTypeContextFiltering()
        {
            ContextFiltering(new PositionTypePage());
        }

        [TestMethod]
        [TestCategory("QUICK")]
        public void PositionTypeCreateWithEmptyForm()
        {
            GetDataDictionary(ConnectionString, "CreateWithEmptyForm");
            CreateWithEmptyForm(new PositionTypePage(), GetDataValueArray("ErrorMessage"));
        }

        public List<CardItem> PrepareData(string section, TestPurpose testPurpose = TestPurpose.Create)
        {
            GetDataDictionary(ConnectionString, section);
            return new List<CardItem>
            {
                new CardItem
                {
                    NameOrId = "Entity_Code",
                    FieldType = FieldType.Text,
                    Value = (testPurpose == TestPurpose.Create) ? GetDataValue("Code") : GetDataValue("CodeUpd")
                },
                new CardItem
                {
                    NameOrId = "Entity_Description",
                    FieldType = FieldType.Text,
                    Value = GetDataValue("Description")
                },
                new CardItem
                {
                    NameOrId = "Entity_Notes",
                    FieldType = FieldType.Text,
                    Value = GetDataValue("Notes")
                }
            };
        }
    }
}
