/* 
inputs: 
deck size, default 60
min copies, default 1 
max copies, default 4 
max mulls, default 2
How many you want, default 1 

outputs:
Copies  0 mull  1 mull  2 mull
1       5%      10%     15%
2       10%     20%     30%
...

1. Collect inputs
2. validate inputs
3. calculate odds table
4. output table
*/

using CommandLine;

namespace MtgOdds
{
    class Program
    {

        public class Options
        {
            [Option('d', "deck", Default = 60)]
            public int DeckSize { get; set; }

            [Option('n', "min", Default = 1)]
            public int MinCopies { get; set; }

            [Option('x', "max", Default = 4)]
            public int MaxCopies { get; set; }

            [Option('m', "mulls", Default = 2)]
            public int MaxMulls { get; set; }

            [Option('c', "desire", Default = 1)]
            public int DesireCount { get; set; }

        }

        static void Main(string[] args)
        {
            (int deckSize, int minCopies, int maxCopies, int maxMulls, int desireCount) = GetInputs(args);
            if (!ValidateInputs(deckSize, minCopies, maxCopies, maxMulls, desireCount))
            {
                return;
            }
            //Console.WriteLine($"deck: {deckSize} mincopies:{minCopies}max copies:{maxCopies}max mulls:{maxMulls} desire count:{desireCount}");
            var table = CalcOdds(deckSize, minCopies, maxCopies, maxMulls, desireCount);
            OutputTable(table, minCopies);
        }


        static (int deckSize, int minCopies, int maxCopies, int maxMulls, int desireCount) GetInputs(string[] args)
        {
            Options? opt = null;

            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                opt = o;
            });

            if (opt == null)
            {
                throw new Exception("invalid args");
            }

            return (opt.DeckSize, opt.MinCopies, opt.MaxCopies, opt.MaxMulls, opt.DesireCount);
        }

        static bool ValidateInputs(int deckSize, int minCopies, int maxCopies, int maxMulls, int desireCount)
        {
            if (minCopies > maxCopies)
            {
                Console.WriteLine("Min copies greater than max");
                return false;
            }
            if (deckSize < maxCopies)
            {
                Console.WriteLine("Check deck size");
                return false;
            }
            if (desireCount > minCopies)
            {
                Console.WriteLine("Not enough copies");
                return false;
            }
            return true;
        }

        static double[,] CalcOdds(int deckSize, int minCopies, int maxCopies, int maxMulls, int desireCount)
        {
            var odds = new double[maxMulls + 1, maxCopies - minCopies + 1];

            for (int copyCount = 0; copyCount < odds.GetLength(1); copyCount++)
            {

                for (int cardCount = desireCount; cardCount <= Math.Min(maxCopies, 7); cardCount++)
                {
                    var M = minCopies + copyCount;
                    var k = cardCount;
                    var N = deckSize;
                    var n = 7;

                    // Formula from https://steemit.com/steemstem/@aximot/will-you-win-the-skiing-trip-with-one-of-your-friends-hypergeometric-distribution-basics
                    odds[0, copyCount] +=
                        (GetBinCoeff(M, k) * GetBinCoeff(N - M, n - k)
                        /
                        GetBinCoeff(N, n));
                }

                for (int mullCount = 1; mullCount < odds.GetLength(0); mullCount++)
                {
                    odds[mullCount, copyCount] = 1 - Math.Pow(1 - odds[0, copyCount], mullCount + 1);
                }
            }

            return odds;
        }

        static double GetBinCoeff(long N, long K)
        {
            // This function gets the total number of unique combinations based upon N and K.
            // N is the total number of items.
            // K is the size of the group.
            // Total number of unique combinations = N! / ( K! (N - K)! ).
            // This function is less efficient, but is more likely to not overflow when N and K are large.
            // Taken from:  http://blog.plover.com/math/choose.html
            //
            long r = 1;
            long d;
            if (K > N) return 0;
            for (d = 1; d <= K; d++)
            {
                r *= N--;
                r /= d;
            }
            return r;
        }

        static void OutputTable(double[,] table, int minCopies)
        {
            Console.Write($"Copies  ");
            for (int mull = 0; mull < table.GetLength(0); mull++)
            {
                Console.Write($"Mull {mull}  ");
            }
            Console.WriteLine();

            for (int copies = 0; copies < table.GetLength(1); copies++)
            {
                Console.Write($"{copies + minCopies,-8}");
                for (int mull = 0; mull < table.GetLength(0); mull++)
                {
                    Console.Write($"{table[mull, copies],-8:P2}");
                }
                Console.WriteLine();
            }
        }
    }
}