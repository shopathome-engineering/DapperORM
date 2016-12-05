namespace ShopAtHome.DapperORMCore
{
    public interface IConnectionStringConfig
    {
        string TransactionProcessing { get; }

        string DataProcessing { get; }

        string LiveSahSelect { get; }
        string ReportSahSelect { get; }

        string LiveMMS { get; }
        string ReportMMS { get; }
    }
}
