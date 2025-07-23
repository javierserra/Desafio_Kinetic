using Desafio_Kinetic.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Desafio_Kinetic.Services;

public class FolderProcessor
{
    private readonly ILogger<FolderProcessor> _logger;
    private readonly GenDocs _genDocs;
    private readonly string _outputBase;
    private readonly ProcessStateStore _stateStore;
    private readonly HashSet<string> _stopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "al", "algo", "alguna", "algunas", "alguno", "algunos", "ante", "antes", "como", "con", "contra",
        "cual", "cuales", "cuando", "cuál", "cuáles", "de", "debe", "debido", "debo", "del", "dela", "desde",
        "donde", "durante", "el", "ella", "ellas", "ello", "ellos", "embargo", "en", "entre", "era", "eres",
        "es", "esa", "esas", "ese", "esos", "estaba", "estaban", "estado", "estados", "está", "están", "estar",
        "estoy", "existe", "existen", "fue", "fuera", "fueron", "ha", "haber", "había", "habían", "hace",
        "hacía", "hacer", "hago", "hasta", "la", "las", "le", "lo", "los", "más", "me", "menos", "mi", "mis",
        "muy", "nada", "ni", "no", "nos", "nosotros", "nuestra", "nuestras", "nuestro", "nuestros", "o",
        "otra", "otras", "otro", "otros", "para", "parece", "por", "por qué", "porque", "prima", "primero",
        "qué", "que", "quien", "quienes", "si", "siendo", "sin", "sobre", "son", "sólo", "su", "sus", "tal",
        "tampoco", "te", "tendrán", "tener", "tengo", "teóricamente", "tiempo", "toda", "todas", "todo", "todos",
        "tu", "tus", "un", "una", "unas", "uno", "unos", "y", "ya", "yo"
    };

    public FolderProcessor(ILogger<FolderProcessor> logger, GenDocs genDocs, ProcessStateStore stateStore, string outputBase = "/app/output")
    {
        _logger = logger;
        _genDocs = genDocs;
        _stateStore = stateStore;
        _outputBase = outputBase;
    }


    public void ProcessFolder(string folderPath)
    {
        _logger.LogInformation("Procesando carpeta: {FolderPath}", folderPath);
        var folderName = new DirectoryInfo(folderPath).Name;
        var outputDir = Path.Combine(_outputBase, folderName);

        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        var files = Directory.GetFiles(folderPath, "*.txt");
        var processId = Guid.NewGuid().ToString();
        _stateStore.SetStatus(processId, "RUNNING");
        _genDocs.CrearProceso(processId, files.Length);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            _stateStore.SetFileStatus(processId, fileName, "PENDING");
        }

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            // Verificamos si el proceso fue detenido
            if (_stateStore.GetStatus(processId) == "STOPPED")
            {
                _logger.LogInformation("Proceso detenido manualmente.");
                return;
            }
            try

            {
                _stateStore.SetFileStatus(processId, fileName, "RUNNING");
                var destFile = Path.Combine(outputDir, Path.GetFileName(file));
                File.Move(file, destFile, overwrite: true);
                var stats = ProcesarArchivo(destFile);
                _genDocs.ActualizarProgreso(processId, stats, Path.GetFileName(file));

                _logger.LogInformation("Archivo procesado: {DestFile}", destFile);
                _stateStore.SetFileStatus(processId, fileName, "COMPLETED");
                // Guardar resumen individual por archivo
                var fileSummaryPath = Path.Combine(Path.GetDirectoryName(destFile)!, Path.GetFileNameWithoutExtension(destFile) + "_summary.json");

                var fileSummaryJson = JsonSerializer.Serialize(
                stats,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                File.WriteAllText(fileSummaryPath, fileSummaryJson);
                _logger.LogInformation("Resumen individual guardado en: {SummaryPath}", fileSummaryPath);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando archivo: {File}", file);
                _stateStore.SetFileStatus(processId, fileName, "FAILED");
                _stateStore.SetStatus(processId, "FAILED");
            }

            // Se relentiza el proceso para poder hacer uso de la gestion de procesos..
            Thread.Sleep(5000);
        }
        if (!new[] { "STOPPED", "FAILED" }.Contains(_stateStore.GetStatus(processId)))
        {
            _stateStore.SetStatus(processId, "COMPLETED");
        }
        // Serializar resultado final
        var resultadoFinal = _genDocs.ObtenerResultado(processId);
        if (resultadoFinal != null)
        {
            var summaryPath = Path.Combine(outputDir, "summary.json");
            var json = System.Text.Json.JsonSerializer.Serialize(resultadoFinal, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(summaryPath, json);
            _logger.LogInformation("Resumen del proceso guardado en: {SummaryPath}", summaryPath);
        }
        _logger.LogInformation("Finalizó el procesamiento de: {FolderPath}", folderPath);
    }

    private ResultDetails ProcesarArchivo(string path)
    {
        var content = File.ReadAllText(path);
        var lines = File.ReadAllLines(path);
        int totalLines = lines.Length;

        var palabras_todas = Regex.Matches(content.ToLower(), @"\b\w+\b")
            .Select(m => m.Value)
            .ToList();
        var palabras = Regex.Matches(content.ToLower(), @"\b\w+\b")
            .Select(m => m.Value)
            .Where(w => !_stopWords.Contains(w))
            .ToList();

        var frecuencia = palabras
            .GroupBy(p => p)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToList();

        var resumen = GenerarResumen(content);

        return new ResultDetails
        {
            TotalWords = palabras.Count,
            TotalLines = totalLines,
            MostFrequentWords = frecuencia,
            Resumen = resumen
        };
    }
            public static string GenerarResumen(string contenido, int cantidadOraciones = 3)
    {
        
        // Expresión regular para dividir en oraciones (básica pero funcional para español)
        var oraciones = Regex.Split(contenido, @"(?<=[.!?])\s+(?=[A-ZÁÉÍÓÚÑ])");

        // Tomar las primeras N oraciones
        string resumen = string.Join(" ", oraciones[..Math.Min(cantidadOraciones, oraciones.Length)]);

        return resumen;
    }
}
