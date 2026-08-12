namespace Game_Logic
{
    public class Player
    {
        public PlayerData Data { get; private set; }

        public Player(PlayerData data)
        {
            Data = data;
        }
        
        public void ChangePlayerRating(int rating)
        {
            Data = new PlayerData
            {
                Name = Data.Name,
                Rating = Data.Rating + rating
            };
        }
    }
}