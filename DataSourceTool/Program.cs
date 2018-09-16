using System;

namespace DataSourceTool
{
    class Program
    {
        static void Main(string[] args)
        {
            new Exporter().Run(args).Wait();
        }
    }
}
