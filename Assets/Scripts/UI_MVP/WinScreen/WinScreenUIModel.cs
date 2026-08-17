using Base_Classes;

namespace UI.WinScreen
{
    public class WinScreenUIModel : ModelBase
    {
        public string WinnerText { get; private set; } = string.Empty;

        public void SetWinnerText(string winnerText)
        {
            WinnerText = winnerText ?? string.Empty;
        }
    }
}
