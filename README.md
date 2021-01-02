# decfuzzgen

Fuzzer to generate CSV based tests for decimal operations.

## Usage

```
./decfuzzgen <op> --output <outputDir> [--sample-size <sampleSize>] [--combination <combo>] 
```

Arguments:
* `op` - the decimal operation to output
* `outputDir` - the directory to put test output
* `sampleSize` - the size of the randomized output to generate. By default this is 10,000.
* `combo` - the bitwise combination to generate. e.g. 100 will generate decimals with only
   the high portion set.