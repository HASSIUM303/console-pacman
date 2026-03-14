using System;
using System.IO;

class Program
{
   static void Main()
   {
      Console.CursorVisible = false;

      char[,] map = ReadMap("map.txt");
      ConsoleKeyInfo pressedKey = new ConsoleKeyInfo('w', ConsoleKey.W, false, false, false);

      int pacmanX = 1;
      int pacmanY = 1;

      while (true)
      {
         Console.Clear();

         Console.ForegroundColor = ConsoleColor.Blue;
         DrawMap(map);

         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.SetCursorPosition(pacmanX, pacmanY);
         Console.Write("@");

         Console.ForegroundColor = ConsoleColor.Red;
         Console.SetCursorPosition(32, 0);
         Console.Write(pressedKey.KeyChar);

         pressedKey = Console.ReadKey();

         HandleInput(pressedKey, ref pacmanX, ref pacmanY);
      }
   }
   private static char[,] ReadMap(string path)
   {
      string[] file = File.ReadAllLines("map.txt");
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
   private static void HandleInput(ConsoleKeyInfo pressedKey, ref int pacmanX, ref int pacmanY)
   {
      
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