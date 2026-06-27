namespace Dianty.Models;

internal class GameManager
{
    public class Game
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsEnable { get; set; }
        public Mode StrengthMode { get; set; }
        public int OutputStrength { get; set; }
    }

    public enum Mode
    {
        Max, Add
    }
}
