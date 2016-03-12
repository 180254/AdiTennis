using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using AdiTennis.Logic;

namespace AdiTennis.StageAbstract.Menus
{
    internal class MenuTemplate : IEnumerable<MenuItem>
    {
        public List<MenuItem> Items = new List<MenuItem>();

        private readonly ConsoleKeyInfo _defaultUnusableKey = new ConsoleKeyInfo('X', ConsoleKey.X, false, false, false);

        private readonly string[] _additionalMessage = new string[2];
        private readonly ConsoleColor _messageFirstLineColor;
        private int _selectedMenuItem;

        public MenuTemplate(string firstLineMessage, string secondLineMessage)
            : this(firstLineMessage, secondLineMessage, GameState.Config.MsgColorError)
        {
        }

        public MenuTemplate(string firstLineMessage, string secondLineMessage, ConsoleColor messageFirstLineColor)
        {
            _additionalMessage[0] = firstLineMessage;
            _additionalMessage[1] = secondLineMessage;
            _messageFirstLineColor = messageFirstLineColor;
        }

        public void Go()
        {
            Items[0].IsSelected = true;
            var pressedKey = _defaultUnusableKey;

            do
            {
                Console.Clear();
                PrintLogo();

                Console.CursorTop = GameState.Config.MsgCursorTop;
                Console.CursorLeft = GameState.Config.MsgCursorLeft;
                Console.ForegroundColor = _messageFirstLineColor;
                Console.WriteLine(_additionalMessage[0]);
                Console.ForegroundColor = GameState.Config.MsgColorStandard;
                Console.CursorLeft = GameState.Config.MsgCursorLeft;
                Console.WriteLine(_additionalMessage[1]);
                Console.Write("\n\n");
                Console.ResetColor();

                Items[_selectedMenuItem].IsSelected = false;
                if (pressedKey.Key == ConsoleKey.UpArrow)
                    _selectedMenuItem = (_selectedMenuItem > 0 ? _selectedMenuItem - 1 : Items.Count - 1);
                if (pressedKey.Key == ConsoleKey.DownArrow)
                    _selectedMenuItem = (_selectedMenuItem == Items.Count - 1 ? 0 : _selectedMenuItem + 1);
                Items[_selectedMenuItem].IsSelected = true;


                foreach (var menuItem in Items)
                {
                    Console.CursorLeft = GameState.Config.MsgCursorLeft;
                    if (menuItem.IsSelected)
                    {
                        Console.ForegroundColor = GameState.Config.MsgColorStandard;
                        Console.CursorLeft = Console.CursorLeft - 4;
                        Console.Write(" -> ");
                    }
                    Console.WriteLine(menuItem.Name);
                    Console.ResetColor();
                }


                do
                {
                    while (!Console.KeyAvailable)
                    {
                        Thread.Sleep(GameState.Config.FpsAsMilisecondsSleep);
                    }

                    pressedKey = Console.ReadKey();
                    if (pressedKey.Key == ConsoleKey.UpArrow || pressedKey.Key == ConsoleKey.DownArrow ||
                        pressedKey.Key == ConsoleKey.Enter) continue;
                    Console.CursorLeft = 0;
                    Console.Write(' ');
                    Console.CursorLeft = 0;
                } while (
                    !(pressedKey.Key == ConsoleKey.UpArrow || pressedKey.Key == ConsoleKey.DownArrow ||
                      pressedKey.Key == ConsoleKey.Enter));
            } while (pressedKey.Key != ConsoleKey.Enter);
        }

        public static void PrintLogo()
        {
            Console.WriteLine(" _______ ______  _____ _______ _______ __   _ __   _ _____ _______\n" +
                              "|_____| |     \\   |      |    |______ | \\  | | \\  |   |   |______\n" +
                              "|     | |_____/ __|__    |    |______ |  \\_| |  \\_| __|__ ______|\n"
                );
        }


        public void Add(MenuItem menuItem)
        {
            Items.Add(menuItem);
        }

        public IEnumerator<MenuItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}