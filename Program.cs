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
            SetName = "outputDir",
            HelpText = "The directory to put the generated test output into.")]
        public string OutputDirectory { get; set; }

        [Option('s', "sample-size",
            SetName = "sampleSize",
            Default = 10000,
            HelpText = "The size of the randomized output to generate.")]
        public int SampleSize { get; set; }

        [Option('c', "combination",
            SetName = "combo",
            Default = "***",
            HelpText = "The bitwise combination to generate where we have hi/mid/lo for the dividend and the divisor.")]
        public string Combination { get; set; }
    }

    enum Op
    {
        Div,
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    var generator = CombinationGenerator.Parse(o.Combination);
                    var data = generator.Generate(o.SampleSize);
                    foreach (var combo in data.Keys)
                    {
                        var values = data[combo];
                        string filename =
                            $"{Path.GetFullPath(o.OutputDirectory)}{Path.DirectorySeparatorChar}{o.Operation}_{combo}.csv";

                        // Write out the CSV header
                        using StreamWriter writer = new StreamWriter(filename);
                        writer.WriteLine("D1,D2,Result,Error");
                        for (int i = 0; i < values.Count; i++)
                        {
                            decimal result = 0;
                            string error = "";
                            var tuple = values[i];
                            switch (o.Operation)
                            {
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