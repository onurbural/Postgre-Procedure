using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;
using Postgre_Procedures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

[Keyless]
public class Prosedur
{
    public string SchemaName { get; set; }
    public string ProcedureName { get; set; }
}


[Keyless]
public class Fonksiyon
{
    public string SchemaName { get; set; }
    public string FunctionName { get; set; }
}

public static class Prosedurler
{
    private static readonly HashSet<string> _procedures = new HashSet<string>();

    public static void ProsedurEkle(string semaIsmi, string prosedurIsmi)
    {
        var tamProsedurIsmi = $"{semaIsmi}.{prosedurIsmi}";
        if (!_procedures.Contains(tamProsedurIsmi))
        {
            _procedures.Add(tamProsedurIsmi);
        }
    }

    public static IEnumerable<string> ProsedurleriGetir()
    {
        return _procedures;
    }
}

public static class Fonksiyonlar
{
    private static readonly HashSet<string> _functions = new HashSet<string>();

    public static void FonksiyonEkle(string semaIsmi, string fonksiyonIsmi)
    {
        var tamFonksiyonIsmi = $"{semaIsmi}.{fonksiyonIsmi}";
        if (!_functions.Contains(tamFonksiyonIsmi))
        {
            _functions.Add(tamFonksiyonIsmi);
        }
    }

    public static IEnumerable<string> FonksiyonlariGetir()
    {
        return _functions;
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        using (var context = new PROCEDURContext())
        {
            string prosedurSorgu = @"
            SELECT routine_schema AS SchemaName, routine_name AS ProcedureName 
            FROM information_schema.routines 
            WHERE routine_type = 'PROCEDURE';";

            var prosedurler = await context.Set<Prosedur>().FromSqlRaw(prosedurSorgu).ToListAsync();
            foreach (var prosedur in prosedurler)
            {
                Prosedurler.ProsedurEkle(prosedur.SchemaName, prosedur.ProcedureName);
            }

            string fonksiyonSorgu = @"
            SELECT ns.nspname AS SchemaName, p.proname AS FunctionName 
            FROM pg_proc p
            INNER JOIN pg_namespace ns ON p.pronamespace = ns.oid
            WHERE ns.nspname = 'public';";

            var fonksiyonlar = await context.Set<Fonksiyon>().FromSqlRaw(fonksiyonSorgu).ToListAsync();
            foreach (var fonksiyon in fonksiyonlar)
            {
                Fonksiyonlar.FonksiyonEkle(fonksiyon.SchemaName, fonksiyon.FunctionName);
            }

            Console.WriteLine("Mevcut prosedürler:");
            foreach (var mevcutProsedur in Prosedurler.ProsedurleriGetir())
            {
                Console.WriteLine(mevcutProsedur);
            }

            await Console.Out.WriteLineAsync("\n\n\n");

            Console.WriteLine("Mevcut fonksiyonlar:");
            foreach (var mevcutFonksiyon in Fonksiyonlar.FonksiyonlariGetir())
            {
                Console.WriteLine(mevcutFonksiyon);
            }

            Console.WriteLine("Bir prosedür veya fonksiyon seçin (yazın):");
            var seciliItem = Console.ReadLine();
            var secilenProsedur = Prosedurler.ProsedurleriGetir().FirstOrDefault(p => p == seciliItem);
            var secilenFonksiyon = Fonksiyonlar.FonksiyonlariGetir().FirstOrDefault(f => f == seciliItem);

            if (secilenProsedur != null)
            {
                await ProseduruCalıstır(secilenProsedur);
            }
            else if (secilenFonksiyon != null)
            {
                await FonksiyonuCalistir(secilenFonksiyon);
            }
            else
            {
                Console.WriteLine("Geçersiz prosedür veya fonksiyon adı.");
            }
        }
    }

    private static async Task ProseduruCalıstır(string prosedurIsmi)
    {
        string connectionString = "Host=localhost;Username=postgres;Password=123;Database=PROCEDUR;";
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();

            var commandText = $"CALL {prosedurIsmi}();";
            using (var command = new NpgsqlCommand(commandText, conn))
            {
                try
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            var resultList = new List<Dictionary<string, object>>();

                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var result = reader.GetValue(i);

                                    if (result is object[] objects)
                                    {
                                        row[$"Veri"] = objects;
                                    }
                                    else
                                    {
                                        row[$"Veri"] = result;
                                    }
                                }
                                resultList.Add(row);
                            }

                            var jsonResult = JsonConvert.SerializeObject(resultList, Newtonsoft.Json.Formatting.Indented);
                            Console.WriteLine(jsonResult);
                        }
                        else
                        {
                            Console.WriteLine("Prosedürden herhangi bir satır dönmedi.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Prosedür yürütülürken bir hata oluştu: {ex.Message}");
                }
            }
        }
    }



    private static async Task FonksiyonuCalistir(string fonksiyonIsmi)
    {
        string connectionString = "Host=localhost;Username=postgres;Password=123;Database=PROCEDUR;";
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();

            var commandText = $"SELECT {fonksiyonIsmi}();";
            using (var command = new NpgsqlCommand(commandText, conn))
            {
                try
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            var resultList = new List<Dictionary<string, object>>();

                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var result = reader.GetValue(i);

                                    if (result is object[] objects)
                                    {
                                        row[$"Veri"] = objects;
                                    }
                                    else
                                    {
                                        row[$"Veri"] = result;
                                    }
                                }
                                resultList.Add(row);
                            }

                            var jsonResult = JsonConvert.SerializeObject(resultList, Newtonsoft.Json.Formatting.Indented);
                            Console.WriteLine(jsonResult);
                        }
                        else
                        {
                            Console.WriteLine("Fonksiyondan herhangi bir satır dönmedi.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fonksiyon yürütülürken bir hata oluştu: {ex.Message}");
                }
            }
        }
    }
}
