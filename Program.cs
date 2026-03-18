using System;
using System.IO;
using System.Threading;

class Program
{
   private const string MapFileName = "map.txt";
   static char[,] map = null!;
   static ConsoleKeyInfo pressedKey;
   static int pacmanX = 1;
   static int pacmanY = 1;
   static int score;
   static int maxScore;

   static void Main()
   {
      Console.CursorVisible = false;

      Directory.SetCurrentDirectory(AppContext.BaseDirectory);
      try
      {
         map = GetMapFromFile(MapFileName);
      }
      catch (FileNotFoundException)
      {
         if (CreateMap(MapFileName))
         {
            Console.WriteLine($"Создан пустой файл карты: {MapFileName}");
            Console.WriteLine($"Директория: {Directory.GetCurrentDirectory()}");
            Console.WriteLine("Заполните файл картой и запустите игру снова.");
         }
         Console.ReadKey(true);
         return;
      }
      catch (Exception ex) when (ex is InvalidDataException or IOException)
      {
         Console.WriteLine($"Ошибка чтения карты: {MapFileName}");
         Console.WriteLine($"Директория: {Directory.GetCurrentDirectory()}");
         Console.WriteLine(ex.Message);
         Console.ReadKey(true);
         return;
      }

      pressedKey = new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false);
      maxScore = GetCountOfSymbol('.', map);

      Task.Run(() =>
      {
         while (true) pressedKey = Console.ReadKey(true);
      });

      while (true)
      {
         Console.Clear();

         HandleInput();

         Console.ForegroundColor = ConsoleColor.Blue;
         DrawMap();

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

         Thread.Sleep(500);
      }

      Console.ReadKey();
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
               Console.WriteLine("Вы ввели некоректное значение");
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
   private static void DrawMap()
   {
      for (int x = 0; x < map.GetLength(0); x++)
      {
         for (int y = 0; y < map.GetLength(1); y++)
            Console.Write(map[x, y]);
         Console.WriteLine();
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
