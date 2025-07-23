using System.Collections.Concurrent;
using System.Text.Json;
using Desafio_Kinetic.Models;
namespace Desafio_Kinetic.Services
{
    public class ProcessStateStore
    {
        private readonly string _filePath;
        private Dictionary<string, FileProcessStatus> _state = new();

        public ProcessStateStore(string basePath = "/app/input")
        {
            Directory.CreateDirectory(basePath); // Asegura que exista el directorio
            _filePath = Path.Combine(basePath, "state.json");
            Load();
        }


        public void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _state = JsonSerializer.Deserialize<Dictionary<string, FileProcessStatus>>(json)
                         ?? new Dictionary<string, FileProcessStatus>();
            }
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(_state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        public string? GetStatus(string processId)
        {
            if (_state.TryGetValue(processId, out var status))
                return status.Status;

            return null;
        }
        public void SetStatus(string processId, string status)
        {
            if (!_state.ContainsKey(processId))
                _state[processId] = new FileProcessStatus();

            _state[processId].Status = status;
            Save();
        }

        public void SetFileStatus(string processId, string fileName, string fileStatus)
        {
            if (!_state.ContainsKey(processId))
                _state[processId] = new FileProcessStatus();

            _state[processId].Files[fileName] = fileStatus;
            Save();
        }

        public FileProcessStatus? GetProcessStatus(string processId)
        {
            _state.TryGetValue(processId, out var status);
            return status;
        }
        public Dictionary<string, FileProcessStatus> GetAllStatus()
        {
            return new Dictionary<string, FileProcessStatus>(_state);
        }
        
    }
}