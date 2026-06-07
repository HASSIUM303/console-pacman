using System;
using System.Text;
using System.Text.Json;

class Ghost : IGameCharacter
{
    public int X { get; set; }
    public int Y { get; set; }
    public char Symbol => 'G';
    public ConsoleColor Color { get; }

    private readonly char[,] map;
    private readonly Pacman pacman;
    private static readonly HttpClient httpClient = new();
    private readonly string serviceName;
    private readonly int ghostId;

    public Ghost(int startX, int startY,
        char[,] map,
        Pacman pacman,
        ConsoleColor color,
        int ghostId,
        string serviceName = "http://localhost:8080")
    {
        X = startX;
        Y = startY;
        this.map = map;
        this.pacman = pacman;
        Color = color;
        this.ghostId = ghostId;
        this.serviceName = serviceName;
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

    public async Task MoveAsync()
    {
        Erase();

        var request = new
        {
            ghostX = X,
            ghostY = Y,
            pacmanX = pacman.X,
            pacmanY = pacman.Y,
            map = MapToIntArray(),
            mapWidth = map.GetLength(1),
            mapHeight = map.GetLength(0)
        };

        try
        {
            string json = JsonSerializer.Serialize(request);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{serviceName}/path", content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                PathResponse? pathResponse = JsonSerializer.Deserialize<PathResponse>(responseJson);

                if (pathResponse != null && pathResponse.Found)
                {
                    X = pathResponse.NextX;
                    Y = pathResponse.NextY;
                }
            }
        }
        catch (Exception)
        {
            MoveSinple();
        }

        Draw();
    }

    private void MoveSinple()
    {
        int dx = pacman.X - X;
        int dy = pacman.Y - Y;

        int nextX = X;
        int nextY = Y;

        if (Math.Abs(dx) > Math.Abs(dy))
            nextX += dx > 0 ? 1 : -1;
        else
            nextY += dy > 0 ? 1 : -1;

        if (IsValidMove(nextX, nextY))
        {
            X = nextX;
            Y = nextY;
        }
    }

    private bool IsValidMove(int x, int y)
    {
        if (x < 0 || x >= map.GetLength(1) || y < 0 || y >= map.GetLength(0))
            return false;

        char cell = map[y, x];
        return cell == ' ' || cell == '.';
    }

    private int[][] MapToIntArray()
    {
        int[][] result = new int[map.GetLength(0)][];

        for (int i = 0; i < map.GetLength(0); i++)
        {
            result[i] = new int[map.GetLength(1)];
            for (int j = 0; j < map.GetLength(1); j++)
            {
                result[i][j] = map[i, j] == '#' ? 1 : 0;
            }
        }

        return result;
    }

    public bool CollidesWith(Pacman pacman)
    {
        return X == pacman.X && Y == pacman.Y;
    }
}

class PathResponse
{
    public int NextX { get; set; }
    public int NextY { get; set; }
    public bool Found { get; set; }
}
