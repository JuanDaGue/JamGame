using System;

[Serializable]
public class GameModel
{
    public int Score { get; private set; }
    public int Lives { get; private set; }
    public int Level { get; private set; }
    
    // Configuración inicial
    private const int INITIAL_LIVES = 3;
    
    public GameModel()
    {
        ResetGame();
    }
    
    public void ResetGame()
    {
        Score = 0;
        Lives = INITIAL_LIVES;
        Level = 1;
    }
    
    public void AddScore(int points)
    {
        Score += points;
    }
    
    public void LoseLife()
    {
        Lives = Math.Max(0, Lives - 1);
    }
    
    public void AddLife()
    {
        Lives++;
    }
    
    public void NextLevel()
    {
        Level++;
    }
}