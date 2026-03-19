using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

class Program
{
   static string MapPath = "maps/map.txt";
   static char[,] map = null!;
   static ConsoleKeyInfo pressedKey;
   static int pacmanX = 1;
   static int pacmanY = 1;
   static int score;
   static int maxScore;
   static int speed = 500;

   static void Main()
   {
      Directory.SetCurrentDirectory(AppContext.BaseDirectory);

      MapPath = SelectMapPath();

      if (!TryMapInit(MapPath)) return;

      pressedKey = new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false);
      maxScore = GetCountOfSymbol('.', map);

      Console.Write("Введите скорость для пакмена в миллисекунда: ");
      speed = Convert.ToInt32(Console.ReadLine());

      Console.CursorVisible = false;

      Task.Run(() =>
      {
         while (true) pressedKey = Console.ReadKey(true);
      });

      Console.Clear();
      DrawElements(ConsoleColor.Blue, '#');

      while (true)
      {
         HandleInput();

         DrawElements(ConsoleColor.DarkMagenta, '.', ' ');

         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.SetCursorPosition(pacmanX, pacmanY);
         Console.Write("@");

         Console.ForegroundColor = ConsoleColor.Red;
         Console.SetCursorPosition(map.GetLength(1) + 1, 0);
         Console.Write($"Score: {score}/{maxScore}");
         Console.SetCursorPosition(map.GetLength(1) + 1, 1);
         Console.Write($"Pressed Key: {pressedKey.KeyChar}  ");

         if (score >= maxScore)
         {
            Console.SetCursorPosition(map.GetLength(1) + 1, map.GetLength(0) - 1);
            Console.Write("YOU WIN!");
            break;
         }

         Thread.Sleep(speed);
      }

      Console.ReadKey(true);
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
      bool isWorking = true;
      try
      {
         map = GetMapFromFile(mapPath);
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
         isWorking = false;
      }
      catch (Exception ex) when (ex is InvalidDataException or IOException)
      {
         Console.WriteLine($"Ошибка чтения карты: {mapPath}");
         Console.WriteLine($"Директория: {Directory.GetCurrentDirectory()}");
         Console.WriteLine(ex.Message);
         Console.ReadKey(true);
         isWorking = false; ;
      }
      catch (Exception)
      { isWorking = false; }

      return isWorking;
   }
   private static int GetCountOfSymbol(char symbol, char[,] array)
   {
      int result = 0;

      foreach (var s in array)
         if (s == symbol) result++;

      return result;
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
      string[] file = File.ReadAllLines(path);

      if (file.Length == 0)
         throw new InvalidDataException("Файл карты пустой.");

      char[,] map = new char[file.Length, GetMaxLengthOfLine(file)];

      for (int x = 0; x < map.GetLength(0); x++)
         for (int y = 0; y < map.GetLength(1); y++)
            map[x, y] = file[x][y];

      return map;
   }
   private static void DrawElements(ConsoleColor color, params char[] elements)
   {
      ConsoleColor defaultColor = Console.ForegroundColor;
      Console.ForegroundColor = color;

      for (int x = 0; x < map.GetLength(0); x++)
      {
         for (int y = 0; y < map.GetLength(1); y++)
         {
            if (ElementContains(map[x, y]))
            {
               Console.SetCursorPosition(y, x);
               Console.Write(map[x, y]);
            }
         }
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

      int nextX = pacmanX + direction[0];
      int nextY = pacmanY + direction[1];

      char nextCell = map[nextY, nextX];

      if (nextCell == ' ' || nextCell == '.')
      {
         pacmanX = nextX;
         pacmanY = nextY;

         if (nextCell == '.')
         {
            score++;
            map[nextY, nextX] = ' ';
         }
      }
   }
   private static int[] GetDirection()
   {
      int[] direction = [0, 0];

      if (pressedKey.Key == ConsoleKey.W) direction[1] = -1;
      else if (pressedKey.Key == ConsoleKey.S) direction[1] = 1;
      else if (pressedKey.Key == ConsoleKey.A) direction[0] = -1;
      else if (pressedKey.Key == ConsoleKey.D) direction[0] = 1;

      return direction;
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
