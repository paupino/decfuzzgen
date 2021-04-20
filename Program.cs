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
                    
                    var lhsGenerator = CombinationGenerator.Parse(o.Combination);
                    var rhsGenerator = CombinationGenerator.Parse("***");
                    //Console.Write("Generating samples...");
                    //var data = generator.Generate(o.SampleSize);
                    //Console.WriteLine("Finished");
                    foreach (var lhsCombo in lhsGenerator.Combinations)
                    {
                        foreach (var rhsCombo in rhsGenerator.Combinations)
                        {
                            if (lhsCombo.IsZero && rhsCombo.IsZero)
                                continue;
                            Console.WriteLine("Generating {0} samples for {1}_{2}...", o.SampleSize, lhsCombo, rhsCombo);
                            var lhsValues = lhsGenerator.Generate(lhsCombo, o.SampleSize);
                            var rhsValues = rhsGenerator.Generate(rhsCombo, o.SampleSize);
                            string filename =
                                $"{outDir}{o.Operation}_{lhsCombo}_{rhsCombo}.csv";
                            bool exists = File.Exists(filename);

                            // Write out the CSV header
                            Console.WriteLine("Writing to {0}...", filename);
                            using StreamWriter writer = new StreamWriter(filename, !o.OverWrite);
                            if (o.OverWrite || !exists) 
                                writer.WriteLine("D1,D2,Result,Error");
                            for (int i = 0; i < Math.Min(lhsValues.Count, rhsValues.Count); i++)
                            {
                                decimal result = 0;
                                string error = "";
                                var lhs = lhsValues[i];
                                var rhs = rhsValues[i];
                                switch (o.Operation)
                                {
                                    case Op.Add:
                                        try
                                        {
                                            result = lhs + rhs;
                                        }
                                        catch (OverflowException)
                                        {
                                            error = "overflow";
                                        }
                                        break;
                                    case Op.Div:
                                        try
                                        {
                                            result = lhs / rhs;
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
                                            result = lhs * rhs;
                                        }
                                        catch (OverflowException)
                                        {
                                            error = "overflow";
                                        }
                                        break;
                                    case Op.Rem:
                                        try
                                        {
                                            result = lhs % rhs;
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
                                            result = lhs - rhs;
                                        }
                                        catch (OverflowException)
                                        {
                                            error = "overflow";
                                        }
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                writer.WriteLine($"{lhs},{rhs},{result},{error}");
                            }
                        }
                    }
                });
        }
    }
}