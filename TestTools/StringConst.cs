using System.Collections.Generic;

namespace TestTools
{
    public static class StringConst
    {
        public const string GeneralTab = "General information";
        public const string General2Tab = "General Information";
        public const string General_Tab = "General";
        public const string IdentifiersTab = "Identifiers";
        public const string AddressesTab = "Addresses";
        public const string PersonTab = "Person";
        public const string ContactInfoTab = "Contact information";
        public const string AdditionalInfoTab = "Additional Info";
        public const string OperationTypeTab = "Operation";
        public const string OperationsTab = "Operations";
        public const string DelegatorsTab = "Delegators";
        public const string DelegationsTab = "Delegations";
        public const string SettingsTab = "Settings";
        public const string AccountsTab = "Accounts";
        public const string SecurityTab = "Security";
        public const string FieldsTab = "Fields";
        public const string PermissionsTab = "Access Permissions";
        public const string UsersTab = "Users";
        public const string ContactsTab = "Contacts";
        public const string RolesTab = "Roles";
        public const string PartiesTab = "Parties";
        public const string AuthEmplTab = "Authorized Employees";
        public const string EntitiesTab = "Entities";
        public const string OptionsTab = "Options";
        public const string FormatTab = "Format";
        public const string PeriodTab = "Period";
        public const string GroupingTab = "Groupings";
        public const string RowsTab = "Rows";
        public const string ColorsTab = "Colors";
        public const string FlowsInfoTab = "Flows information";
        public const string FlowInfoTab = "Flow information";
        public const string ProcessesTab = "Processes";
        public const string CancelRulTab = "Cancellation Rules";
        public const string ReconRulTab = "Reconciliation Rules";
        public const string GenRulTab = "Generation Rules";
        public const string ReconCorrTab = "Reconciliation only correspondences";
        public const string CharacteristicsTab = "Characteristics";
        public const string MainContactTab = "Main contact";
        public const string PredefCurrTab = "Predefined currencies";

        public const string UDFTab = "UDFs";

        public const string CashFlows = "Cash Flows";
        public const string IntercoFlows = "Interco Flows";
        public const string ZBASettings = "ZBA Settings";

        public const string FilterTab = "Filter";
        public const string ExportOptionsTab = "Export Options";

        //Extenders
        public const string BRMTab = "Bank Delegation of Authorities";
        public const string CashTab = "Cash";
        public const string ReconTab = "Reconciliation";
        public const string DITab = "Debts & Investments";
        public const string ReceiptsTab = "Receipts";

        public static IDictionary<string, string> EntityClassId;

        static StringConst()
        {
            EntityClassId = new Dictionary<string, string>
            {
                { "1e3d1ffc-76b6-4148-bf4e-b390a6d41bc4", "BRM.AccAuthorization.BL.Setup.Account" },
                { "6a244f63-3997-4207-9148-1fb3dfccb249", "BRM.AccAuthorization.BL.Setup.Account2Auth" },
                { "97d52944-a0ab-4fba-9acf-80113b055e3b", "BRM.AccAuthorization.BL.Setup.Company2Auth" },
                { "ee8d40ee-b28f-4142-b0de-e68ab3e13229", "BRM.AccAuthorization.BL.Setup.Employee2Auth" },
                { "9b3ce076-c9dd-4f97-acb7-6e9af381cefa", "BRM.Cash.BL.Setup.CashRcnPrm" },
                { "803f4eb4-411b-46d2-b82d-1731a6b7cbb6", "BRM.Cash.BL.Setup.Account2Cash" },
                { "b5efae14-a7fd-4146-98fb-de4e18b1b6cd", "BRM.Cash.BL.Setup.CashRcnProcessParams" },
                { "3efadd07-0d91-4fde-a42b-95dbd81c2ded", "BRM.Cash.BL.Setup.AvailabilityCondition" },
                { "ff9e7140-9a84-4982-bb2e-b134cdf9524b", "BRM.Cash.BL.Setup.CommissionRule" },
                { "c4e70219-45eb-413b-b1ff-f1922272904c", "BRM.Cash.BL.Setup.Commission" },
                { "757dd3f5-46b7-49cd-bc62-77675732b790", "BRM.Cash.BL.Setup.CashGridFilter" },
                { "64c9c4ba-1f6a-4db2-a41d-72b611c0dd00", "BRM.Cash.BL.Setup.CashGridStructure" },
                { "1641f36e-cee0-48a1-a7e6-aa18215ac384", "BRM.Cash.BL.Setup.CashGridCode" },
                { "d59d60f6-8a81-4a05-834d-b5857782587c", "BRM.Cash.BL.Setup.CreditInt"},
                { "14287988-0abe-41c1-8665-02112e7fd359", "BRM.Cash.BL.Setup.DebitInt" },
                { "f8363c27-b88d-4859-8eba-286214a21778", "BRM.Cash.BL.Setup.CashDiscrepancyLimit" },
                { "b9a28631-16a1-48a9-8739-0f27b9d5681e", "BRM.Cash.BL.Setup.GeneralFeeIdentificationRule" },
                { "90d07d43-5d2b-438c-a055-3a7be81ccb87", "BRM.Cash.BL.Setup.GeneralFee" },
                { "b1b70f16-c18e-4d26-a6ab-22b5a6fcf997", "BRM.Cash.BL.Setup.CashRcnProcess" },
                { "385222cd-386e-422e-93b0-20b99653ead0", "BRM.Cash.BL.Setup.CashRcnRule" },
                { "3ebc452c-da6e-4cbd-8413-60e4201ca565", "BRM.Cash.BL.Setup.FlowCategory" },
                { "6c3732f1-de84-49ad-8fd8-a3bb621d56e0", "BRM.Cash.BL.Setup.FlowCode" },
                { "a2670447-29b1-48c7-9bd8-ae4c0ff2702c", "BRM.Cash.BL.Setup.FlowSubcategory" },
                { "0eba3180-e1f8-4a86-b157-dca2a55ef91e", "BRM.Cash.BL.Data.RcnBnkFlowResult" },
                { "e262b397-4a1c-46f0-ad7f-dac715591ddb", "BRM.Cash.BL.Data.AccountBalanceBnk" },
                { "6b7053ff-5845-4d11-89f6-07697cf2ca6f", "BRM.Cash.BL.Data.AccountBalanceCf" },
                { "bc6010f2-8b07-40ca-a574-91f335c68e0d", "BRM.Cash.BL.Data.BankTransaction" },
                { "01e43279-9bba-432d-83e6-5dd2ec56c1b0", "BRM.Cash.BL.Data.CashFlow" },
                { "fc48c4ce-f7b0-42f5-b977-a0c2725dde74", "BRM.Cash.BL.Data.CorrespondenceTable.Btc2FlowCorrespondenceIdentificRule" },
                { "ff6a0fda-fcd8-4bf1-97f0-df617b6d1a9e", "BRM.Cash.BL.Data.CorrespondenceTable.Btc2FlowCorrespondence" },
                { "6d47a442-8fab-4797-a5f5-d25009e1aab7", "BRM.Cash.BL.Data.CashFlowWithSepa" },
                { "9852dc57-2eb4-4d1a-b0c7-38dd83c492b4", "BRM.GeneralSetup.BL.Bank" },
                { "e5bfdd4a-7a69-4a35-be8d-61417a5f5b2a", "BRM.GeneralSetup.BL.Company" },
                { "e3c6f970-71d9-46d9-8fdd-62e8e10b8a23", "BRM.GeneralSetup.BL.CurrencyRate" },
                { "4b5009fd-6b9e-436b-b2cf-3b141649d66f", "BRM.GeneralSetup.BL.Employee" },
                { "c70ea908-461d-4f85-b2d8-1a7500d67d32", "BRM.GeneralSetup.BL.InterestRate"},
                { "0c89df3b-d83d-4b6b-acf8-f24e4a1737b8", "BRM.GeneralSetup.BL.BankArea.BnkFileStatementEntry" },
                { "7b97eb76-c720-4c8c-95ed-24d1ab212754", "BRM.GeneralSetup.BL.BankArea.BnkFileStatement" },
                { "4dd47cf7-3754-45b4-8a8e-a147aac68620", "BRM.Reconciliation.BL.Setup.Account2Rec" },
                { "26f7eb0a-0736-4025-81ab-adad1dcab2ff", "BRM.Reconciliation.BL.Data.AccountBalance"},
                { "e5f531e1-8c69-4394-bcdb-5d338a56b9c2", "BRM.Reconciliation.BL.Data.RcnBnkGlResult" },
                { "bade56db-219f-4a91-b120-9e5550936820", "BRM.Reconciliation.BL.Data.GlStatement" },
                { "a98bea42-ae3f-44c7-95bb-e2fa8b834c4b", "BRM.Reconciliation.BL.Data.BnkStatement" },
                { "d370854b-1655-4550-b4df-538c3f37a421", "NN.Core.Tech.BL.ExportImport.EntityFileLink" },
                { "ddf025e1-1399-4456-8210-78fb8a645330", "NN.Core.Tech.BL.BatchTasks.HeavyJob" },
                { "13364ee4-1a99-4e56-9805-963ca89af675", "NN.Core.Tech.BL.CopySchemeConfig.CopyScheme" },
                { "2beb46dd-9a03-483e-bd19-f048228a02ae", "NN.Core.Tech.BL.Comments.CommentRoot" },
            };
        }
    }
}
