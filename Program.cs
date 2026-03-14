using System;
using System.IO;

class Program
{
   static void Main()
   {
      char[,] map = ReadMap("map.txt");
      
      while (true)
      {
         Console.Clear();

         Console.ForegroundColor = ConsoleColor.Blue;
         DrawMap(map);

         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.SetCursorPosition(1, 1);
         Console.Write("@");

         Thread.Sleep(1000);
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
   private static int GetMaxLengthOfLine(string[] lines)
   {
      int maxLength = lines[0].Length;

      foreach (var line in lines)
         if (line.Length > maxLength)
            maxLength = line.Length;

      return maxLength;
   }
}