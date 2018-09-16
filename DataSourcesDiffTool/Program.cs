using System;

namespace DataSourcesDiffTool
{
    class Program
    {
        static void Main(string[] args)
        {
            new Exporter().Run(args).Wait();
        }
    }
}
