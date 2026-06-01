using System;

class Pacman : IGameCharacter
{
    public int X { get; set; }
    public int Y { get; set; }
    public char Symbol => '@';
    public ConsoleColor Color => ConsoleColor.Yellow;

    private int score;
    private int maxScore;
    private char[,] map;

    public Pacman(int startX, int startY, char[,] map, int maxScore)
    {
        X = startX;
        Y = startY;
        this.map = map;
        this.maxScore = maxScore;
        score = 0;
    }

    public void Draw()
    {
        Console.ForegroundColor = Color;
        Console.SetCursorPosition(X, Y);
        Console.Write(Symbol);
    }

    public void Erase()
    {
        Console.SetCursorPosition(X, Y);
        Console.Write(' ');
    }

    public void Move(int directionX, int directionY)
    {
        int nextX = X + directionX;
        int nextY = Y + directionY;

        if (IsValidMove(nextX, nextY))
        {
            Erase();
            X = nextX;
            Y = nextY;

            if (map[Y, X] == '.')
            {
                score++;
                map[Y, X] = ' ';
            }

            Draw();
        }
    }

    public bool IsValidMove(int x, int y)
    {
        if (x < 0 || x >= map.GetLength(1) || y < 0 || y >= map.GetLength(0))
            return false;

        char cell = map[y, x];
        return cell == ' ' || cell == '.';
    }

    public int Score => score;
    public int MaxScore => maxScore;
    public bool IsWin => score >= maxScore;
}
