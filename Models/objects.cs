namespace Desafio_Kinetic.Models
{
    public class FileProcessStatus
    {
        public string Status { get; set; } = "PENDING";
        public Dictionary<string, string> Files { get; set; } = new();
    }
    public class Docs
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
    }

    public class ProcessResult
    {
        public string ProcessId { get; set; }
        public string Status { get; set; } // RUNNING, COMPLETED, FAILED, etc.
        public ProgressInfo Progress { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EstimatedCompletion { get; set; }
        public ResultDetails Results { get; set; }
    }

    public class ProgressInfo
    {
        public int TotalFiles { get; set; }
        public int ProcessedFiles { get; set; }
        public int Percentage => TotalFiles == 0 ? 0 : (ProcessedFiles * 100) / TotalFiles;
    }

    public class ResultDetails
    {
        public int TotalWords { get; set; }
        public int TotalLines { get; set; }
        public string Resumen { get; set; }
        public List<string> MostFrequentWords { get; set; } = new();
        public List<string> FilesProcessed { get; set; } = new();

        
        
    }
}
