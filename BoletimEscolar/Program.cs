using System.Text.Json;
using System.Globalization;
using BoletimEscolar.Models;
using System.Text.Encodings.Web;

class Program
{
    static string dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data");

    static void Main()
    {
        Directory.CreateDirectory(dataFolder);
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Menu();
    }

    static void Menu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("===== Boletim Escolar ======\n ");

            Console.WriteLine("1 - Criar um novo boletim");
            Console.WriteLine("2 - Listar boletins salvos");
            Console.WriteLine("3 - Abrir boletim salvo ");
            Console.WriteLine("0 - Sair");
            Console.Write("\nEscolha Uma opção: ");

            var opt = Console.ReadLine() ?? "";

            switch (opt.Trim())
            {
                case "1":
                    CreateReportCard();
                    break;

                case "2":
                    ListSavedReportCards();
                    break;

                case "3":
                    OpenSavedReportCard();
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Opção Invalida. Pressione qualquer tecla para retornar");
                    Console.ReadKey();
                    break;
            }

        }
    }

    // Criação Boletim
    static void CreateReportCard()
    {
        Console.Clear();
        var rc = new ReportCard();

        Console.Write("Nome do aluno: ");
        rc.StudentName = ReadNonEmptyString();

        Console.WriteLine("Quantas matérias deseja adicionar? ");
        int qtd = ReadIntGreaterOrEqual(1);

        for (int i = 0; i < qtd; i++)
        {
            Console.WriteLine($"\n--- Matéria {i + 1} ---");
            Console.Write("Nome da Matéria: ");
            string name = ReadNonEmptyString();

            var subject = new Subject { Name = name };

            Console.WriteLine("Quantas notas para essa matéria? (ex: 2, 3 ou 4):");
            int notas = ReadIntGreaterOrEqual(1);

            for (int j = 0; j < notas; j++)
            {
                Console.Write($"Nota{j + 1}:");
                double n = ReadDoubleInRange(0.0, 10.0);
                subject.Grades.Add(n);
            }

            rc.Subjects.Add(subject);
        }

        //Exibe Resumo

        Console.Clear();
        DisplayReportCard(rc);

        //Pergunta se salva

        Console.Write("\nDeseja salvar esse boletim? (S/N): ");
        var ans = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
        if (ans == "S" || ans == "SIM")
        {
            SaveReportCard(rc);
            Console.WriteLine("Boletim salvo com sucesso !");
        }

        else
        {
            Console.WriteLine("Boletim não salvo.");
        }

        Console.WriteLine("\nPressione qualquer tecla para voltar ao menu.");
        Console.ReadKey();
    }

    static void DisplayReportCard(ReportCard rc)
    {
        Console.WriteLine($"Boletim de: {rc:StudentName}");
        Console.WriteLine($"Criado em (UTC): {rc.CreatedAt:u}");
        Console.WriteLine($"Média geral: {rc.OverallAverage:F2}");
        Console.WriteLine($"Aprovadas: {rc.SubjectsApproved} | Recuperação: {rc.SubjectsInRecovery} | Reprovadas: {rc.SubjectsFailed}\n");

        if (rc.Subjects == null || rc.Subjects.Count == 0)
        {
            Console.WriteLine("Nenhuma matéria cadastrada.");
            return;
        }

        foreach (Subject s in rc.Subjects)
        {
            if (s.Average >= 7.0) Console.ForegroundColor = ConsoleColor.Green;

            else if (s.Average >= 5.0) Console.ForegroundColor = ConsoleColor.Yellow;

            else Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"Matéria: {s.Name}");
            Console.WriteLine($"Notas {string.Join(",", s.Grades.Select(g => g.ToString("F2", CultureInfo.InvariantCulture)))}");
            Console.WriteLine($"Média: {s.Average:F2} | Situação: {s.Situation}\n");
        }

        Console.ResetColor();
    }

    static void SaveReportCard(ReportCard rc)
    {
        string safeName = MakeFileNamesSafe(rc.StudentName);
        string fileName = $"{safeName}_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
        string path = Path.Combine(dataFolder, fileName);

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(rc, options);

        File.WriteAllText(path, json);
    }

    static void ListSavedReportCards()
    {
        Console.Clear();
        var files = Directory.GetFiles(dataFolder, "*.json").OrderByDescending(f => f).ToArray();
        if (files.Length == 0)
        {
            Console.WriteLine("Nenhum boletim salvo ainda.");
            Console.WriteLine("\nPressione qualquer tecla para voltar.");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Boletins salvos: \n");
        for (int i = 0; i < files.Length; i++)
        {
            Console.WriteLine($"{i + 1} - {Path.GetFileName(files[i])}");
        }

        Console.WriteLine("\nPressione qualquer tecla para voltar.");
        Console.ReadKey();
    }

    static void OpenSavedReportCard()
    {
        Console.Clear();
        var files = Directory.GetFiles(dataFolder, "*.json").OrderByDescending(f => f).ToArray();
        if (files.Length == 0)
        {
            Console.WriteLine("Nenhum boletim salvo.");
            Console.WriteLine("\nPressione qualquer tecla para voltar.");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Escolha o boletim para abrir:\n");
        for (int i = 0; i < files.Length; i++)
        {
            Console.WriteLine($"{i + 1} - {Path.GetFileName(files[i])}");
        }

        Console.Write("\nNúmero: ");
        int idx = ReadIntInRange(1, files.Length) - 1;
        string filePath = files[idx];

        try
        {
            string json = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("Erro: O arquivo está vazio ou corrompido.");
                Console.WriteLine("\nPressione qualquer tecla para voltar.");
                Console.ReadKey();
                return;
            }
            Console.Clear();
            Console.WriteLine($"--- Boletim --- : {Path.GetFileName(filePath)} ----");

            var jsonObject = JsonSerializer.Deserialize<object>(json);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string formattedJson = JsonSerializer.Serialize(jsonObject, options);

            Console.WriteLine(formattedJson);
            Console.WriteLine("--------------------");
            Console.WriteLine("\nConteúdo acima lido do arquivo.");
            Console.WriteLine("Pressione qualquer tecla para carregar o boletim ");
            Console.WriteLine("  ");
            Console.ReadKey();

            var rc = JsonSerializer.Deserialize<ReportCard>(json);

            if (rc == null)
            {
                Console.WriteLine("\nErro: O arquivo JSON contém 'null' ou não pode ser direcionado para um Boletim");
            }
            else
            {
                Console.Clear();
                DisplayReportCard(rc);
            }
        }

        catch (System.Text.Json.JsonException jsonEx)
        {
            Console.Clear();
            Console.WriteLine($"ERRO DE SINTAXE NO ARQUIVO JSON: {Path.GetFileName(filePath)}");
            Console.WriteLine("O arquivo não é um JSON válido e não pode ser lido.");
            Console.WriteLine($"Detalhe do erro: {jsonEx.Message}");
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Ocorreu um erro inesperdo ao ler o arquivo: {ex.Message}");
        }
        Console.WriteLine("\nPressione qualquer tecla para voltar.");
        Console.ReadKey();
    }



    //----------- Metodos utilitarios para entrada --------------------

    static string ReadNonEmptyString()
    {
        while (true)
        {
            var s = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
            Console.WriteLine("Valor Invalido. Digite novamente: ");
        }
    }

    static int ReadIntGreaterOrEqual(int min)
    {
        while (true)
        {
            var s = Console.ReadLine() ?? "";
            if (int.TryParse(s, out int v) && v >= min) return v;
            Console.Write($"Digite um número inteiro >= {min}: ");
        }
    }

    static int ReadIntInRange(int min, int max)
    {
        while (true)
        {
            var s = Console.ReadLine() ?? "";
            if (int.TryParse(s, out int v) && v >= min && v <= max) return v;
            Console.Write($"Digite um número entre {min} e {max}: ");
        }
    }

    static double ReadDoubleInRange(double min, double max)
    {
        while (true)
        {
            var s = Console.ReadLine() ?? "";
            if (double.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out double v) ||
                double.TryParse(s, NumberStyles.Number, CultureInfo.CurrentCulture, out v))
            {
                if (v >= min && v <= max) return v;
            }
            Console.Write($"Digite um valor entre {min} e {max}: ");
        }
    }

    static string MakeFileNamesSafe(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '-');
        return name.Replace(' ', '-');
    }
}