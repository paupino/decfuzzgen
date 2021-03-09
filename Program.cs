using System;
using System.IO;
using CommandLine;

namespace DecimalFuzzGenerator
{
    class Options
    {
        [Value(0, MetaName = "operation",
            HelpText = "The operation to generate fuzz for.",
            Required = true)]
        public Op Operation { get; set; }

        [Option('o', "output",
            HelpText = "The directory to put the generated test output into.")]
        public string OutputDirectory { get; set; }

        [Option('s', "sample-size",
            Default = 10000,
            HelpText = "The size of the randomized output to generate.")]
        public int SampleSize { get; set; }

        [Option('c', "combination",
            Default = "***",
            HelpText = "The bitwise combination to generate where we have hi/mid/lo for the dividend and the divisor.")]
        public string Combination { get; set; }
        
        [Option("overwrite",
            Default = false,
            HelpText = "If set and a file already exists, it will be overwritten.")]
        public bool OverWrite { get; set; }
    }

    enum Op
    {
        Add,
        Div,
        Mul,
        Rem,
        Sub,
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    string outDir = $"{Path.GetFullPath(o.OutputDirectory)}{Path.DirectorySeparatorChar}";
                    Console.WriteLine("Operation: {0}", o.Operation);
                    Console.WriteLine("Output directory: {0}", outDir);
                    Console.WriteLine("Sample Size: {0}", o.SampleSize);
                    Console.WriteLine("Combination: {0}", o.Combination);
                    
                    var generator = CombinationGenerator.Parse(o.Combination);
                    var data = generator.Generate(o.SampleSize);
                    foreach (var combo in data.Keys)
                    {
                        var values = data[combo];
                        string filename =
                            $"{outDir}{o.Operation}_{combo}.csv";
                        bool exists = File.Exists(filename);

                        // Write out the CSV header
                        using StreamWriter writer = new StreamWriter(filename, !o.OverWrite);
                        if (o.OverWrite || !exists) 
                            writer.WriteLine("D1,D2,Result,Error");
                        for (int i = 0; i < values.Count; i++)
                        {
                            decimal result = 0;
                            string error = "";
                            var tuple = values[i];
                            switch (o.Operation)
                            {
                                case Op.Add:
                                    try
                                    {
                                        result = tuple.Item1 + tuple.Item2;
                                    }
                                    catch (OverflowException)
                                    {
                                        error = "overflow";
                                    }
                                    break;
                                case Op.Div:
                                    try
                                    {
                                        result = tuple.Item1 / tuple.Item2;
                                    }
                                    catch (DivideByZeroException)
                                    {
                                        error = "divide_by_zero";
                                    }
                                    catch (OverflowException)
                                    {
                                        error = "overflow";
                                    }

                                    break;
                                case Op.Mul:
                                    try
                                    {
                                        result = tuple.Item1 * tuple.Item2;
                                    }
                                    catch (OverflowException)
                                    {
                                        error = "overflow";
                                    }
                                    break;
                                case Op.Rem:
                                    try
                                    {
                                        result = tuple.Item1 % tuple.Item2;
                                    }
                                    catch (DivideByZeroException)
                                    {
                                        error = "divide_by_zero";
                                    }
                                    catch (OverflowException)
                                    {
                                        error = "overflow";
                                    }

                                    break;
                                case Op.Sub:
                                    try
                                    {
                                        result = tuple.Item1 - tuple.Item2;
                                    }
                                    catch (OverflowException)
                                    {
                                        error = "overflow";
                                    }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            writer.WriteLine($"{tuple.Item1},{tuple.Item2},{result},{error}");
                        }
                    }
                });
        }
    }
}