using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3 || args.Length % 2 == 0)
        {
            Console.WriteLine("Error: You must provide an odd number of unique moves, at least 3.");
            return;
        }

        var uniqueMoves = new HashSet<string>(args);
        if (uniqueMoves.Count != args.Length)
        {
            Console.WriteLine("Error: Moves must be unique.");
            return;
        }

        var moves = args;
        int n = moves.Length;
        int p = n / 2;
        var random = new Random();

        while (true)
        {
            var key = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            string keyHex = BitConverter.ToString(key).Replace("-", "").ToLower();

            int computerMoveIndex = random.Next(n);
            string computerMove = moves[computerMoveIndex];

            string hmac = Hmac(key, computerMove);
            Console.WriteLine($"HMAC: {hmac}");

            Console.WriteLine("Available moves:");
            for (int i = 0; i < moves.Length; i++)
            {
                Console.WriteLine($"{i + 1} - {moves[i]}");
            }
            Console.WriteLine("0 - exit");
            Console.WriteLine("? - help");

            while (true)
            {
                Console.Write("Enter your move: ");
                var input = Console.ReadLine().Trim();

                if (input == "0")
                {
                    Console.WriteLine("You exit the game");
                    return; 
                }

                if (input == "?" || input.ToLower() == "help")
                {
                    PrintHelp(moves);
                    continue;
                }

                if (!int.TryParse(input, out int userMoveIndex) || userMoveIndex < 1 || userMoveIndex > n)
                {
                    Console.WriteLine("Invalid move. Please enter a valid number corresponding to your move.");
                    continue; 
                }

                userMoveIndex--; 
                string userMove = moves[userMoveIndex];

                Console.WriteLine($"Your move: {userMove}");
                Console.WriteLine($"Computer move: {computerMove}");

                if (userMove == computerMove)
                {
                    Console.WriteLine("Draw!");
                }
                else
                {
                    int result = (userMoveIndex - computerMoveIndex + n) % n;
                    if (result <= p && result != 0)
                    {
                        Console.WriteLine("You win!");
                    }
                    else
                    {
                        Console.WriteLine("You lose!");
                    }
                }

                Console.WriteLine($"HMAC key: {keyHex}");
                break;
            }

            break;
        }
    }

    static void PrintHelp(string[] moves)
    {
        int n = moves.Length;
        Console.WriteLine("+-------------+" + new string('-', n * 8 - 1) + "+");
        Console.Write("| v User\\PC > |");
        foreach (var move in moves)
        {
            Console.Write($" {move,-5} |");
        }
        Console.WriteLine();
        Console.WriteLine("+-------------+" + new string('-', n * 8 - 1) + "+");

        for (int i = 0; i < n; i++)
        {
            Console.Write($"| {moves[i],-11} |");
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                    Console.Write(" Draw  |");
                else if ((i - j + n) % n <= n / 2 && (i - j + n) % n != 0)
                    Console.Write(" Win   |");
                else
                    Console.Write(" Lose  |");
            }
            Console.WriteLine();
            Console.WriteLine("+-------------+" + new string('-', n * 8 - 1) + "+");
        }
    }

    static string Hmac(byte[] key, string message)
    {
        using (var hmac = new HMACSHA256(key))
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] hashBytes = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
