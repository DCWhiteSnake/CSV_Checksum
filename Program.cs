// See https://aka.ms/new-console-template for more information
// use this to map CHIP_0007 to CSVFormat
using AutoMapper;
using CsvHelper;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using static System.Console;

namespace CSV_Checksum;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            string pathToFolder = args[0];
            string pathToCSV = Path.Combine(args[0], args[1]);

            // Automapper Config
            var csvrecords = new List<CSVFormat>();
            var config = new MapperConfiguration(cfg =>
            cfg.CreateMap<CHIP_0007, CSVFormat>());
            var mapper = new Mapper(config);

            string csvHeader = "";
            List<string> checksums = new List<string>();
            List<CSVFormat> csvFormats = new List<CSVFormat>();

            CreateJsonFiles(pathToFolder, pathToCSV, mapper, csvFormats);

            FlushToFile(pathToCSV, csvFormats);

            Console.ForegroundColor = ConsoleColor.Green;
            WriteLine($"Completed without errors, check {pathToFolder} for the generated files");

            Console.ForegroundColor = default;
        }
        catch (IndexOutOfRangeException ex)
        {
            WriteLine("Too many inputs, or too few inputs. Check that you pass 2 arguments");
        }
    }

    private static void CreateJsonFiles(string pathToFolder, string pathToCSV, Mapper mapper, List<CSVFormat> csvFormats)
    {
        // Calculate checksum for each new JSON file
        using (var filestream = new FileStream(pathToCSV, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 20, true))
        using (var reader = new StreamReader(filestream))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<CHIP_0007>();
            foreach (var item in records)
            {
                var json = JsonSerializer.Serialize(item);
                var sha_256 = string.Empty;
                var pathToCHIPJson = Path.Combine(pathToFolder, item.name ?? $"undefined_{Guid.NewGuid()}.json");

                // create the json
                using (var writer = new StreamWriter(pathToCHIPJson))
                {
                    writer.WriteLine(json);
                }
                // generate sha_256 for this particular file
                using (var filestream2 = new FileStream(pathToCHIPJson, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    sha_256 = GetChecksum(filestream2);
                    //checksums.Add(sha_256);
                }

                var csvRecord = mapper.Map<CHIP_0007, CSVFormat>(item);
                csvRecord.Hash = sha_256;
                //var csvRecordIEnumerable = new List<CSVFormat>() { csvRecord };
                csvFormats.Add(csvRecord);

            }
        }
    }

    private static void FlushToFile(string pathToCSV, List<CSVFormat> csvFormats)
    {
        using (var reader_csv_2 = new StreamWriter(pathToCSV))
        using (var csv_2 = new CsvWriter(reader_csv_2, CultureInfo.InvariantCulture))
        {
            //csv_2.WriteHeader<CSVFormat>();
            csv_2.WriteRecords(csvFormats);
        }
    }

    private static string GetChecksum(FileStream stream)
    {
        var sha = new SHA256Managed();
        byte[] checksum = sha.ComputeHash(stream);
        return BitConverter.ToString(checksum).Replace("-", String.Empty);
    }
}