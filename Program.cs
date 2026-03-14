using System;
using System.IO;

class Program
{
   static void Main()
   {
      
   }
   private static char[,] ReadMap(string path)
   {
      string[] file = File.ReadAllLines("map.txt");
      char[,] map = new char[GetMaxLengthOfLine(file), file.Length];
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