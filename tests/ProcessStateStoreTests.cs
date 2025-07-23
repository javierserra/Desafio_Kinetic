using Xunit;
using System.IO;
using Desafio_Kinetic.Services;

public class ProcessStateStoreTests
{
    [Fact]
    public void SetAndGetStatus_DeberiaPersistirEstadoCorrectamente()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempPath);

        var store = new ProcessStateStore(tempPath);
        store.SetStatus("proc123", "RUNNING");

        var estado = store.GetStatus("proc123");
        Assert.Equal("RUNNING", estado);
    }

    [Fact]
    public void SetFileStatus_DeberiaAsignarEstadoArchivo()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempPath);

        var store = new ProcessStateStore(tempPath);
        store.SetFileStatus("proc001", "archivo.txt", "PENDING");

        var estado = store.GetProcessStatus("proc001");
        Assert.Equal("PENDING", estado!.Files["archivo.txt"]);
    }
}
