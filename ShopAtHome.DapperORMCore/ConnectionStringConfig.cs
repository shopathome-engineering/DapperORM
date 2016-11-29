namespace ShopAtHome.DapperORMCore
{
    public class ConnectionStringConfig : IConnectionStringConfig
    {
        public string TransactionProcessing => "TransactionProcessing";

        public string DataProcessing => "DataProcessing";

        public string LiveSahSelect => "LiveSahSelect";
        public string ReportSahSelect => "ReportSahSelect";

        public string LiveMMS => "LiveMMS";
        public string ReportMMS => "ReportMMS";
    }
}
