namespace BalancedScorecard.Enums
{
    /// <summary>
    /// En/- decryption process.
    /// </summary>
    public enum CryptoAction
    {
        Encrypt,
        Decrypt
    }

    /// <summary>
    /// Component that gets rendered as body of Layout.
    /// </summary>
    public enum PageComponent
    {
        Overview,
        Administration,
        Finances
    }

    /// <summary>
    /// Button that raises an OnButtonClick event.
    /// </summary>
    public enum RaisingButton
    {
        SidebarToggleButton,
        LoadDataButton
    }

    public enum KPI
    {
        Undefined,
        OrderVolume,
        Revenue
    }

    public enum PlotType
    {
        Bar,
        Pie
    }
}
