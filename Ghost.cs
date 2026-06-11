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
    private readonly List<Ghost> ghosts;
    private static readonly HttpClient httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(2)
    };
    private readonly string serviceName;

    public Ghost(
        int startX, int startY,
        char[,] map,
        Pacman pacman,
        ConsoleColor color,
        List<Ghost> ghosts,
        string serviceName = "http://localhost:8080")
    {
        X = startX;
        Y = startY;
        this.map = map;
        this.pacman = pacman;
        this.ghosts = ghosts;
        Color = color;
        this.serviceName = serviceName;
    }

    public void Draw()
    {
        Console.ForegroundColor = Color;
        Console.SetCursorPosition(X, Y);
        Console.Write(Symbol);
        Console.ResetColor();
    }
    public void Erase()
    {
    }

    public async Task MoveAsync()
    {
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

        int nextX = X;
        int nextY = Y;
        bool usedServiceMove = false;
        bool serviceAvailable = false;

        try
        {
            string json = JsonSerializer.Serialize(request);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{serviceName}/path", content);
            serviceAvailable = response.IsSuccessStatusCode;

            if (serviceAvailable)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                PathResponse? pathResponse = JsonSerializer.Deserialize<PathResponse>(responseJson);

                if (pathResponse != null && pathResponse.Found)
                {
                    if (IsValidMove(pathResponse.NextX, pathResponse.NextY))
                    {
                        nextX = pathResponse.NextX;
                        nextY = pathResponse.NextY;
                        usedServiceMove = true;
                    }
                }
            }
        }
        catch (Exception)
        {
            serviceAvailable = false;
        }

        if (!serviceAvailable || !usedServiceMove)
        {
            MoveSimple(ref nextX, ref nextY);
        }

        if (IsValidMove(nextX, nextY))
        {
            X = nextX;
            Y = nextY;
        }
    }

    private void MoveSimple(ref int nextX, ref int nextY)
    {
        int dx = pacman.X - X;
        int dy = pacman.Y - Y;

        if (Math.Abs(dx) > Math.Abs(dy))
            nextX += dx > 0 ? 1 : -1;
        else
            nextY += dy > 0 ? 1 : -1;

        if (!IsValidMove(nextX, nextY))
        {
            int[][] directions =
            [
                [1, 0],
                [-1, 0],
                [0, 1],
                [0, -1]
            ];

            foreach (int[] dir in directions)
            {
                int testX = X + dir[0];
                int testY = Y + dir[1];

                if (IsValidMove(testX, testY))
                {
                    nextX = testX;
                    nextY = testY;
                    return;
                }
            }
        }
    }

    private bool IsValidMove(int x, int y)
    {
        if (x < 0 || x >= map.GetLength(1) || y < 0 || y >= map.GetLength(0))
            return false;

        if (IsOccupiedByOtherGhost(x, y))
            return false;

        char cell = map[y, x];
        return cell == ' ' || cell == '.';
    }

    private bool IsOccupiedByOtherGhost(int x, int y)
    {
        foreach (Ghost ghost in ghosts)
        {
            if (!ReferenceEquals(ghost, this) && ghost.X == x && ghost.Y == y)
                return true;
        }

        return false;
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
