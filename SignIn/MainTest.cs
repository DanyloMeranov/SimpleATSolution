using BaseDriver;
using BasePage;
using BaseTests;
using System;
using System.Collections.Generic;
using TestTools;

namespace SignInTests
{
    public class MainTest : BaseTest
    {
        public string ConnectionString { get; private set; }

        public void SignOut() => new SignInTests().SignOutUser();

        public MainTest(string dataFile)
        {
            ConnectionString = RelativeDataPath + dataFile;
        }

        public void SignIn<M, V>(BasePage<M, V> page, bool navigate = true, string userName = "", string userPass = "", bool external = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            try
            {
                var signIn = new SignInTests();
                page.UserUpdate(string.IsNullOrEmpty(userName) ? UserName : userName);
                signIn.InitTests(string.IsNullOrEmpty(userName) ? UserName : userName,
                                 string.IsNullOrEmpty(userPass) ? UserPass : userPass);
                if (navigate)
                    page.NavigateNew(external: external);
            }
            catch (Exception e)
            {
                page.Page.AddElmahDetail();
                throw;
            }
        }

        public void Create<M, V>(BasePage<M, V> page, 
                                List<CardItem> listData, 
                                string checkValue, 
                                string columnName = "Code", 
                                string buttonCreate = "", 
                                string groupName = "", 
                                bool nosave = false,
                                bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.CardItems = listData;
            page.CreateEntity(checkValue, columnName, buttonCreate, groupName, nosave);
        }

        public void CreateNew<M, V>(BasePage<M, V> page, List<CardItem> listData, string[] checkValues, string[] columnNames, bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.CardItems = listData;
            page.CreateEntity(checkValues, columnNames);
        }

        public void CreateWithEmptyForm<M, V>(BasePage<M, V> page, string[] spnLst, string entityName = "", string buttonCreate = "", bool logIn = true, string dialogTextAfterSave = "")
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.CreateEntityWithEmptyForm(spnLst, entityName, buttonCreate, dialogTextAfterSave);
        }

        public void Edit<M, V>(BasePage<M, V> page, 
                               List<CardItem> listData, 
                               string checkBeforeValue, 
                               string checkAfterValue = "", 
                               string columnName = "Code", 
                               bool byIcon = true, 
                               string groupNameBefore = "", 
                               string groupNameAfter = "", 
                               string filter = "DEFFLTR", 
                               bool clearBefore = false, 
                               bool useReversedCards = false, 
                               bool validate = true,
                               bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.CardItems = listData;
            page.EditEntity(checkBeforeValue, checkAfterValue, columnName, byIcon, groupNameBefore, groupNameAfter, filter, clearBefore, useReversedCards, validate);
        }

        public void Edit<M, V>(BasePage<M, V> page, 
                               List<CardItem> listData, 
                               string[] checkBeforeValue, 
                               string[] checkAfterValue, 
                               string[] columnName, 
                               bool byIcon = true, 
                               bool clearBefore = false, 
                               bool useReversedCards = false,
                               bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.CardItems = listData;
            page.EditEntity(checkBeforeValue, checkAfterValue, columnName, byIcon, clearBefore, useReversedCards);
        }

        public void Duplicate<M, V>(BasePage<M, V> page, 
                                    List<CardItem> listData,
                                    string checkBeforeValue, 
                                    string checkAfterValue, 
                                    string columnName = "Code", 
                                    string groupName = "", 
                                    string groupNameAfter = "",
                                    bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.CardItems = listData;
            page.DuplicateEntity(checkBeforeValue, checkAfterValue, columnName, groupName, groupNameAfter);
        }

        public void Duplicate<M, V>(BasePage<M, V> page, 
                                    List<CardItem> listData, 
                                    string[] checkBeforeValue, 
                                    string[] checkAfterValue, 
                                    string[] columnName,
                                    bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.CardItems = listData;
            page.DuplicateEntity(checkBeforeValue, checkAfterValue, columnName);
        }

        public void ChangeByBatch<M, V>(BasePage<M, V> page, 
                                        List<CardItem> listData, 
                                        string[] checkValues, 
                                        string columnName = "Code", 
                                        string[] groupValues = null,
                                        bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.CardItems = listData;
            page.ChangeByBatch(checkValues, columnName, groupValues);
        }

        public void ChangeByBatch<M, V>(BasePage<M, V> page, 
                                        List<CardItem> listData, 
                                        string[][] checkBeforeValues, 
                                        string[][] checkAfterValues, 
                                        string[] columnName, 
                                        string[] groupValues = null,
                                        bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.CardItems = listData;
            page.ChangeByBatch(checkBeforeValues, checkAfterValues, columnName, groupValues);
        }

        public void View<M, V>(BasePage<M, V> page, 
                               List<CardItem> listData, 
                               string checkValue, 
                               string columnName = "Code", 
                               bool byIcon = true, 
                               string title = "View", 
                               string groupName = "", 
                               bool noReturn = false, 
                               bool check = false,
                               bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.CardItems = listData;
            page.ViewEntity(checkValue, columnName, byIcon, title, groupName, noReturn, check);
        }

        public void View<M, V>(BasePage<M, V> page, 
                               List<CardItem> listData, 
                               string[] checkValue, 
                               string[] columnName, 
                               bool byIcon = true, 
                               string title = "View", 
                               string[] groupNames = null, 
                               bool editMode = false,
                               bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.CardItems = listData;
            page.ViewMultiplyColumnEntity(checkValue, columnName, byIcon, title, groupNames, editMode);
        }

        public void Delete<M, V>(BasePage<M, V> page, 
                                 string checkValue, 
                                 string columnName = "Code", 
                                 string groupName = "", 
                                 string[] errMessages = null, 
                                 bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.DeleteEntityByIcon(checkValue, columnName, groupName, errMessages);
        }
        
        public void Delete<M, V>(BasePage<M, V> page, string[] checkBeforeValue, string[] columnName, bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.DeleteMultiplyColumnEntityByIcon(checkBeforeValue, columnName);
        }
        
        public void Delete<M, V>(BasePage<M, V> page, 
                                 string[] checkValues, 
                                 string columnName = "Code", 
                                 string[] groupValues = null, 
                                 string[] errMessages = null, 
                                 bool logIn = true)
                    where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.DeleteEntitiesByBatch(checkValues, columnName, groupValues, errMessages);
        }

        public void Delete<M, V>(BasePage<M, V> page, string[][] checkBeforeValue, string[] columnName, bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.DeleteMultiplyColumnEntitiesByAction(checkBeforeValue, columnName);
        }

        public void DeleteThroughCard<M, V>(BasePage<M, V> page, 
                                            string checkValues, 
                                            string columnName = "Code", 
                                            string groupName = "", 
                                            string[] errMessages = null,
                                            bool logIn = true)
                    where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.DeleteEntityThroughTheCard(checkValues, columnName, groupName, errMessages);
        }

        public void DeleteThroughCard<M, V>(BasePage<M, V> page, string[] checkBeforeValue, string[] columnName, bool logIn = true)
                    where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.DeleteMultiplyColumnsThroughTheCard(checkBeforeValue, columnName);
        }
        
        public void Comment<M, V>(BasePage<M, V> page, 
                                  Dictionary<string, string> comments, 
                                  string uniqueName, 
                                  string columnName = "Code", 
                                  string groupName = "",
                                  bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.TestEntityComment(comments, uniqueName, columnName, groupName);
        }

        public void Comment<M, V>(BasePage<M, V> page, 
                                  Dictionary<string, string> comments, 
                                  string[] uniqueName, 
                                  string[] columnName, 
                                  string[] groupName = null,
                                  bool logIn = true)
                    where M : BasePageElementMap, new()
        where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.TestEntityComment(comments, uniqueName, columnName, groupName);
        }

        public void ContextFiltering<M, V>(BasePage<M, V> page, bool logIn = true)
            where M : BasePageElementMap, new()
            where V : BasePageValidator<M>, new()
        {
            if (logIn)
                SignIn(page);
            else
                page.NavigateNew(external: true);
            page.ContextFiltering();
        }
    }
}
