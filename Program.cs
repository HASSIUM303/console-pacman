using System;
using System.IO;
using System.Threading;

class Program
{
   static void Main()
   {
      Console.CursorVisible = false;

      char[,] map = ReadMap("map.txt");
      ConsoleKeyInfo pressedKey = new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false);

      Task.Run(() =>
      {
         while (true) pressedKey = Console.ReadKey();
      });

      int pacmanX = 1;
      int pacmanY = 1;
      int score = 0;

      while (true)
      {
         Console.Clear();

         HandleInput(pressedKey, ref pacmanX, ref pacmanY, map, ref score);

         Console.ForegroundColor = ConsoleColor.Blue;
         DrawMap(map);

         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.SetCursorPosition(pacmanX, pacmanY);
         Console.Write("@");

         Console.ForegroundColor = ConsoleColor.Red;
         Console.SetCursorPosition(32, 0);
         Console.Write("Score: {0}", score);

         Thread.Sleep(500);
      }
   }
   private static char[,] ReadMap(string path)
   {
      string[] file = File.ReadAllLines(path);
      char[,] map = new char[file.Length, GetMaxLengthOfLine(file)];

      for (int x = 0; x < map.GetLength(0); x++)
         for (int y = 0; y < map.GetLength(1); y++)
            map[x, y] = file[x][y];

      return map;
   }
   public static void DrawMap(char[,] map)
   {
      for (int x = 0; x < map.GetLength(0); x++)
      {
         for (int y = 0; y < map.GetLength(1); y++)
            Console.Write(map[x, y]);
         Console.WriteLine();
      }
   }
   private static void HandleInput(ConsoleKeyInfo pressedKey, ref int pacmanX, ref int pacmanY, char[,] map, ref int score)
   {
      int[] direction = GetDirection(pressedKey);

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
   private static int[] GetDirection(ConsoleKeyInfo pressedKey)
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