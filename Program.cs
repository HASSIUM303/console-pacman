using System;
using System.IO;

class Program
{
   static void Main()
   {
      char[,] map = null;

      string[] file = File.ReadAllLines("map.txt");

      foreach (var line in file)
         Console.WriteLine(line);
   }
}