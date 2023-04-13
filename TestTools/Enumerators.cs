namespace TestTools
{
    public enum BrowserTypes
    {
        Firefox,
        InternetExplorer,
        Chrome,
        NotSet
    }

    /// <summary>
    /// The purpose of test, for ex. Create, Edit.
    /// </summary>
    public enum TestPurpose
    {
        Create,
        Edit,
        ChangeByBatch,
        Duplicate,
        View,
        ClearBeforeFill,
        Audit
    }

    public enum LayoutTabs
    {
        Columns,
        Grouping,
        Sorting,
        Detail,
        Aggregate,
        GroupingAggregate,
        Chart
    }

    public enum FieldType
    {
        CheckBox,
        Toggle,
        ListBox,
        Radio,
        RadioAsButton,
        Text,
        Password,
        Date,
        SlidingDate,
        Numeric,
        Single,
        Multi,
        DetailTable,
        ListView,
        EntityPropertiesTree,
        SelectedPropertiesTree,
        RowsTree,
        TreeListBox,
        FieldsParameter,
        Extender,
        Select,
        FlagsEnum,
        File,
        Color,
        KendoMultiSelect,
        ChkBoxList,
        Label
    }

    public enum ConfigurationType
    {
        Filter,
        Layout,
        CPWFilter,
        CPWLayout
    }

    public enum DetailTableType
    {
        AccountIdentifiers,
        Identifiers,
        Phones,
        NetAddresses,
        Amount,
        FixedCost,
        UnitCost,
        CalculationType,
        Steps,
        ZBASettings
    }

    /// <summary>
    /// Get table on the page by
    /// </summary>
    public enum GetElementBy
    {
        /// <summary>
        /// For example "k-selectable"
        /// </summary>
        ClassName,

        /// <summary>
        /// For example "role=treegrid"
        /// </summary>
        AttributeWithValue,

        /// <summary>
        /// For example "table"
        /// </summary>
        Tagname,

        /// <summary>
        /// For example #Area
        /// </summary>
        Id
    }

    public enum ReconciliationModule
    {
        Cash,
        Reconciliation
    }
}
