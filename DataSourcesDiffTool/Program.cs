using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using Microsoft.Extensions.Logging;

namespace DataSourcesDiffTool
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run().Wait();
        }

        private async Task Run()
        {
            ILogger logger = new ConsoleLogger();

            IDataSource source1 = new MHArmory.AthenaAssDataSource.DataSource(logger, null, null);
            var source2 = new MHArmory.MhwDbDataSource.DataSource(null, true);

            IArmorPiece[] armorPieces = await source1.GetArmorPieces();
            ISkill[] skills = await source1.GetSkills();

            source2.ConvertArmorPieces(armorPieces, Path.Combine(AppContext.BaseDirectory, "armor.c.json"));
            source2.ConvertSkills(skills, Path.Combine(AppContext.BaseDirectory, "skills.c.json"));
        }

        private async Task Run2()
        {
            ILogger logger = new ConsoleLogger();

            IDataSource source1 = new MHArmory.AthenaAssDataSource.DataSource(logger, null, null);
            IDataSource source2 = new MHArmory.MhwDbDataSource.DataSource(null, true);

            Task<IArmorPiece[]> armorPieces1Task = source1.GetArmorPieces();
            Task<IArmorPiece[]> armorPieces2Task = source2.GetArmorPieces();

            await Task.WhenAll(armorPieces1Task, armorPieces2Task);

            IArmorPiece[] armorPieces1 = armorPieces1Task.Result;
            IArmorPiece[] armorPieces2 = armorPieces2Task.Result;

            //ISkill[] testSkills = skills.Except(skills2, new LambdaEqualityComparer<ISkill>((x, y) => x.Name == y.Name)).ToArray();
            IArmorPiece[] testArmors = armorPieces1.Except(armorPieces2, new LambdaEqualityComparer<IArmorPiece>((x, y) => x.Name == y.Name)).ToArray();

            foreach (IArmorPiece a2 in armorPieces2)
            {
                if (armorPieces2.Any(a1 => a1.Name == a2.Name))
                    continue;

                (IArmorPiece armorPiece, int distance)[] bestByNameDistance = armorPieces1
                    .Select(a1 => (armorPiece: a1, distance: ComputeDamereauLevensheinDistance(a1.Name, a2.Name)))
                    .OrderBy(x => x.distance)
                    //.Select(x => x.Armor)
                    .ToArray();

                if (bestByNameDistance[0].armorPiece.Name != a2.Name)
                {
                }

                Console.WriteLine(bestByNameDistance[0].armorPiece.Name);
            }

            foreach (IArmorPiece a1 in armorPieces1)
            {
                if (armorPieces2.Any(a2 => a2.Name == a1.Name))
                    continue;

                (IArmorPiece armorPiece, int distance)[] bestByNameDistance = armorPieces2
                    .Select(a2 => (armor: a2, distance: ComputeDamereauLevensheinDistance(a2.Name, a1.Name)))
                    .OrderBy(x => x.distance)
                    //.Select(x => x.Armor)
                    .ToArray();

                if (bestByNameDistance[0].armorPiece.Name != a1.Name)
                {
                }

                Console.WriteLine(bestByNameDistance[0].armorPiece.Name);
            }
        }

        public static int ComputeDamereauLevensheinDistance(string s, string t)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                if (string.IsNullOrWhiteSpace(t))
                    return 0;

                return t.Length;
            }

            if (string.IsNullOrWhiteSpace(t))
                return s.Length;

            int n = s.Length;
            int m = t.Length;

            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++)
                ;
            for (int j = 1; j <= m; d[0, j] = j++)
                ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;

                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }

            return d[n, m];
        }
    }

    public class LambdaEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> equals;
        private readonly Func<T, int> getHashCode;

        public LambdaEqualityComparer(Func<T, T, bool> equals)
            : this(equals, _ => 0)
        {
        }

        public LambdaEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            this.equals = equals;
            this.getHashCode = getHashCode;
        }

        public bool Equals(T x, T y)
        {
            return equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return getHashCode(obj);
        }
    }

    public class ConsoleLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
        }
    }
}
