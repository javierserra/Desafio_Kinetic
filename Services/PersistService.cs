using System.Collections.Concurrent;
using Desafio_Kinetic.Models;

namespace Desafio_Kinetic.Services
{
       public class GenDocs
    {
        // Almacena los resultados por ID de proceso
        private static readonly ConcurrentDictionary<string, ProcessResult> _procesos = new();

        public ProcessResult? ObtenerResultado(string id) =>
            _procesos.TryGetValue(id, out var result) ? result : null;

        public void CrearProceso(string id, int totalFiles)
        {
            _procesos[id] = new ProcessResult
            {
                ProcessId = id,
                Status = "RUNNING",
                Progress = new ProgressInfo
                {
                    TotalFiles = totalFiles,
                    ProcessedFiles = 0
                },
                StartedAt = DateTime.UtcNow,
                EstimatedCompletion = DateTime.UtcNow.AddMinutes(totalFiles), // Aproximado
                Results = new ResultDetails()
            };
        }

        public void ActualizarProgreso(string id, ResultDetails resultadoParcial, string archivo)
        {
            if (_procesos.TryGetValue(id, out var result))
            {
                result.Progress.ProcessedFiles += 1;
                result.Results.TotalWords += resultadoParcial.TotalWords;
                result.Results.TotalLines += resultadoParcial.TotalLines;
                result.Results.FilesProcessed.Add(archivo);

                // Actualizar palabras frecuentes
                result.Results.MostFrequentWords = CombinarFrecuencias(
                    result.Results.MostFrequentWords, resultadoParcial.MostFrequentWords
                );

                if (result.Progress.ProcessedFiles == result.Progress.TotalFiles)
                {
                    result.Status = "COMPLETED";
                }
            }
        }

        private List<string> CombinarFrecuencias(List<string> actuales, List<string> nuevas)
        {
            var counter = actuales.Concat(nuevas)
                .GroupBy(w => w)
                .ToDictionary(g => g.Key, g => g.Count());

            return counter.OrderByDescending(p => p.Value)
                          .Take(10)
                          .Select(p => p.Key)
                          .ToList();
        }
    }
}
