# SpanCsvParser

This repository contains an efficient CSV file parser implementation leveraging `System.Buffers` and `System.IO.Pipelines` for high-performance memory management and large data processing.

## Features

- **Asynchronous Processing:** The main method, `ParseAsync`, enables asynchronous CSV file processing for better performance and scalability.
- **Customizable Callback:** A delegate (`ReadCallback`) is used to populate objects with the parsed CSV values, offering great flexibility.
- **Memory Management:** Utilizes array pools (`ArrayPool<T>`) to minimize allocations and improve performance.
- **Column Configuration:** Allows you to specify the number of columns expected in the CSV file.

## How to Use

1. **Define the object type for mapping:**
   The `TObjeto` type should be a struct that represents the format of each CSV row.

2. **Implement the Callback:**
   The callback method should map column values to the respective fields in the object.

3. **Call the `ParseAsync` method:**
   Provide the CSV file as a `Stream`, the callback, the number of columns, and a cancellation token if needed.

Usage example:
```csharp
struct CsvRow {
    public int Id;
    public string Name;
}

SpanCsvParser<CsvRow>.ReadCallback callback = (ref ReadOnlySpan<byte> field, ref int columnIndex, ref CsvRow row) =>
{
    switch (columnIndex)
    {
        case 0:
            row.Id = int.Parse(field);
            break;
        case 1:
            row.Name = Encoding.UTF8.GetString(field);
            break;
    }
};

using var stream = File.OpenRead("file.csv");
var result = await SpanCsvParser<CsvRow>.ParseAsync(stream, callback, numberOfColumns: 2, CancellationToken.None);

if (result.IsSuccess)
{
    var (rows, total) = result.Value;
    Console.WriteLine($"Processed {total} rows.");
}
else
{
    Console.WriteLine($"Error: {result.Errors[0].Message}");
}

```

# Requirements
.NET 9 or higher

# Dependencies:

[FluentResults](https://github.com/altmann/FluentResults)

#Benchmarks

Compared against [CsvHelper](https://github.com/JoshClose/CsvHelper) and common csv parser.

The benchmark was ran against files that had:
	- 1 line
	- 50 lines
	- 500 lines
	- 5000 lines
	
![Benchmark](/images/Benchmark.png)

#Contributions
Contributions are welcome! Feel free to open issues or submit pull requests.

#License
This project is licensed under the MIT License.