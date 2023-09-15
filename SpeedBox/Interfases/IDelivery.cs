namespace SpeedBox.Interfases
{
    public interface IDelivery
    {
        Task LogIn();
        Task<string> GetShippingCost(int weight, int length, int width, int height, string cityFrom, string cityTo);
        Task<Dictionary<string, int>> GetCodeCity(string cityFrom, string cityTo);
    }
}
