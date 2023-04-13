namespace TestTools
{
    /// <summary>
    /// Represents card item (control) of the testing entity
    /// </summary>
    public struct CardItem
    {
        public FieldType FieldType;
        public string NameOrId;
        public string AuditId;
        public object Value;
        public string Description;
        public string DescriptionShort;
        public string TabName;
        public DetailTableType SubType;
        public bool SaveAndEdit;
        public bool SaveAs;
        public bool Verify;
        public bool NotVerify;
        public bool ClearSingleSelects;
        public bool WithInput;
        public bool DateWithTime;
        public bool TransactionCode;
    }
}
