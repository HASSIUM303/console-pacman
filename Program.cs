using System;
using System.IO;

class Program
{
   static void Main()
   {
      char[,] map = ReadMap("map.txt");
      DrawMap(map);
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