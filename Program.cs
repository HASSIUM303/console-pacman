using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

class Program
{
    static string MapPath = "maps/map.txt";
    static char[,] map = null!;
    static ConsoleKeyInfo pressedKey;
    static int maxScore;
    static int speed = 500;
    static Pacman pacman = null!;
    static List<Ghost> ghosts = [];

    static void Main()
    {
        LoggingBehavior.Configure();

        try
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            MapPath = SelectMapPath();

            if (!TryMapInit(MapPath)) return;

            pressedKey = new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false);
            maxScore = GetCountOfSymbol('.', map);

            int pacmanStartX = 1, pacmanStartY = 1;
            for (int y = 0; y < map.GetLength(0); y++)
            {
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    if (map[y, x] == ' ')
                    {
                        pacmanStartX = x;
                        pacmanStartY = y;
                        break;
                    }
                }
            }

            pacman = new Pacman(pacmanStartX, pacmanStartY, map, maxScore);

            CreateGhosts();

            Console.Write("Введите скорость для пакмена в миллисекунда: ");
            speed = Convert.ToInt32(Console.ReadLine());

            Console.CursorVisible = false;

            Task.Run(() =>
            {
                while (true) pressedKey = Console.ReadKey(true);
            });


            Console.Clear();
            DrawElements(ConsoleColor.Blue, '#');

            pacman.Draw();
            foreach (Ghost g in ghosts)
                g.Draw();


            bool isGameRunning = true;
            while (isGameRunning)
            {
                HandleInput();

                DrawElements(ConsoleColor.DarkMagenta, '.', ' ');
                pacman.Draw();

                foreach (Ghost g in ghosts)
                    g.MoveAsync().GetAwaiter().GetResult();

                foreach (Ghost g in ghosts)
                    g.Draw();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(map.GetLength(1) + 1, 0);
                Console.Write($"Score: {pacman.Score}/{maxScore}");
                Console.SetCursorPosition(map.GetLength(1) + 1, 1);
                Console.Write($"Pressed Key: {pressedKey.KeyChar}  ");

                if (pacman.IsWin)
                {
                    Console.SetCursorPosition(map.GetLength(1) + 1, map.GetLength(0) - 1);
                    Console.Write("YOU WIN!");
                    isGameRunning = false;
                }

                foreach (Ghost g in ghosts)
                {
                    if (g.CollidesWith(pacman))
                    {
                        Console.SetCursorPosition(map.GetLength(1) + 1, map.GetLength(0) - 1);
                        Console.Write("GAME OVER!");
                        isGameRunning = false;
                        break;
                    }
                }

                Thread.Sleep(speed);
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled error in game loop");
            throw;
        }
        finally
        {
            LoggingBehavior.Close();
        }

        Console.WriteLine("Хотите начать всё сначала? [Y/n]");
        bool stop = Console.ReadLine()?.Trim() switch
        {
            "Y" => true,
            "y" => true,
            null => true,
            "N" => false,
            "n" => false,
            _ => false
        };

        if (stop) Main();

        Console.ReadKey(true);
    }

    private static void CreateGhosts()
    {
        ghosts.Clear();

        ghosts.Add(new(12, 10, map, pacman, ConsoleColor.Red, ghosts));
        ghosts.Add(new(13, 10, map, pacman, ConsoleColor.Magenta, ghosts));
        ghosts.Add(new(14, 10, map, pacman, ConsoleColor.Cyan, ghosts));
        ghosts.Add(new(15, 10, map, pacman, ConsoleColor.Yellow, ghosts));

        Console.WriteLine($"Создано призраков: {ghosts.Count}");
    }

    private static string[] GetMaps()
    {
        string mapsDirectory = Path.Combine(AppContext.BaseDirectory, "maps");

        if (!Directory.Exists(mapsDirectory))
            return [];

        return Directory.GetFiles(mapsDirectory);
    }
    private static string SelectMapPath()
    {
        string[] maps = GetMaps();

        if (maps.Length == 0)
            return Path.Combine(AppContext.BaseDirectory, "maps", "map.txt");

        Console.WriteLine("Доступные карты:");
        for (int i = 0; i < maps.Length; i++)
            Console.WriteLine($"{i + 1}. {Path.GetFileName(maps[i])}");

        while (true)
        {
            Console.Write($"Выберите карту [1-{maps.Length}]: ");

            if (int.TryParse(Console.ReadLine(), out int choice) &&
                choice >= 1 && choice <= maps.Length)
                return maps[choice - 1];

            Console.WriteLine("Некорректный выбор, попробуйте ещё раз.");
        }
    }
    private static bool TryMapInit(string mapPath)
    {
        try
        {
            map = GetMapFromFile(mapPath);
            return true;
        }
        catch (FileNotFoundException)
        {
            if (CreateMap(mapPath))
            {
                Console.WriteLine($"Создан пустой файл карты: {mapPath}");
                Console.WriteLine($"Директория: {Directory.GetCurrentDirectory()}");
                Console.WriteLine("Заполните файл картой и запустите игру снова.");
            }
            Console.ReadKey(true);
            return false;
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException)
        {
            Console.WriteLine($"Ошибка чтения карты: {mapPath}");
            Console.WriteLine($"Директория: {Directory.GetCurrentDirectory()}");
            Console.WriteLine(ex.Message);
            Console.ReadKey(true);
            return false; ;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
            Console.ReadKey(true);
            return false;
        }
    }
    private static int GetCountOfSymbol(char symbol, char[,] array)
    {
        int count = 0;

        foreach (char s in array)
            if (s == symbol)
                count++;

        return count;
    }
    private static bool CreateMap(string path)
    {
        while (true)
        {
            Console.WriteLine("Файл с картой не найден!\n");
            Console.WriteLine("Желаете создать файл map.txt в данной директории? [Y/n]");
            Console.WriteLine(Directory.GetCurrentDirectory() + "\n");

            string? input = Console.ReadLine()?.Trim();

            switch (input)
            {
                case "Y":
                case "y":
                case "":
                case null:
                    using (File.Create(path))
                        return true;
                case "N":
                case "n":
                    return false;
                default:
                    Console.WriteLine("Вы ввели некорректное значение");
                    break;
            }
        }
    }
    private static char[,] GetMapFromFile(string path)
    {
        string[] lines = File.ReadAllLines(path);

        if (lines.Length == 0)
            throw new InvalidDataException("Файл карты пустой.");

        char[,] map = new char[lines.Length, GetMaxLengthOfLine(lines)];

        for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
                map[x, y] = lines[x][y];

        return map;
    }
    private static void DrawElements(ConsoleColor color, params char[] elements)
    {
        ConsoleColor defaultColor = Console.ForegroundColor;
        Console.ForegroundColor = color;

        for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
                if (ElementContains(map[x, y]))
                {
                    Console.SetCursorPosition(y, x);
                    Console.Write(map[x, y]);
                }

        Console.ForegroundColor = defaultColor;

        bool ElementContains(char currentChar)
        {
            foreach (var element in elements)
                if (currentChar == element)
                    return true;

            return false;
        }
    }
    private static void HandleInput()
    {
        int[] direction = GetDirection();
        pacman.Move(direction[0], direction[1]);
    }
    private static int[] GetDirection()
    {
        return pressedKey.Key switch
        {
            ConsoleKey.W => [0, -1],
            ConsoleKey.S => [0, 1],
            ConsoleKey.A => [-1, 0],
            ConsoleKey.D => [1, 0],
            _ => [0, 0]
        };
    }
    private static int GetMaxLengthOfLine(string[] lines)
    {
        int maxLength = lines[0].Length;

        foreach (var line in lines)
            if (line.Length > maxLength)
                maxLength = line.Length;

        return maxLength;
    }
}
