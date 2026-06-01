using System;

interface IGameCharacter
{
    int X { get; set; }
    int Y { get; set; }
    char Symbol { get; }
    ConsoleColor Color { get; }

    void Draw();
    void Erase();
}
