using System;
using System.IO;
using System.Threading;

class Program
{
   static char[,] map;
   static ConsoleKeyInfo pressedKey;
   static int pacmanX = 1;
   static int pacmanY = 1;
   static int score;

   static Program()
   {
      SetMapFromFile("map.txt");
      pressedKey = new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false);
   }
   static void Main()
   {
      Console.CursorVisible = false;

      Task.Run(() =>
      {
         while (true) pressedKey = Console.ReadKey();
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
         Console.SetCursorPosition(32, 0);
         Console.Write("Score: {0}", score);

         Thread.Sleep(500);
      }
   }
   private static void SetMapFromFile(string path)
   {
      string[] file = File.ReadAllLines(path);
      map = new char[file.Length, GetMaxLengthOfLine(file)];

      for (int x = 0; x < map.GetLength(0); x++)
         for (int y = 0; y < map.GetLength(1); y++)
            map[x, y] = file[x][y];
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