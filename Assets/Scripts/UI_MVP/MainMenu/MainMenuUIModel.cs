using Base_Classes;

namespace UI.MainMenu
{
    // Holds Main Menu UI state independent of Unity view concerns.
    public class MainMenuUIModel : ModelBase
    {
        public bool IsQueueVisible { get; private set; }
        public string PlayerName { get; private set; } = string.Empty;
        public string QueueStatusText { get; private set; } = "Waiting For Other Player...";

        public void SetQueueVisible(bool isVisible)
        {
            IsQueueVisible = isVisible;
        }

        public void SetPlayerName(string playerName)
        {
            PlayerName = playerName ?? string.Empty;
        }

        public void SetQueueStatus(string queueStatusText)
        {
            QueueStatusText = queueStatusText ?? string.Empty;
        }
    }
}
