using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Desafio_Kinetic.Services;
using System.IO;

public class FolderProcessorTests
{
    [Fact]
    public void ProcesarArchivo_DeberiaRetornarEstadisticasBasicas()
{
    // Arrange
    string tempFile = Path.GetTempFileName();
    File.WriteAllText(tempFile, "La inteligencia artificial es el futuro. La inteligencia puede aprender.");

    var logger = Mock.Of<ILogger<FolderProcessor>>();
    var genDocs = new Mock<GenDocs>();

    // Usamos un directorio temporal real
    var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDir);
    var stateStore = new ProcessStateStore(tempDir);

    var processor = new FolderProcessor(logger, genDocs.Object, stateStore);

    // Act
    var resultado = processor.GetType()
        .GetMethod("ProcesarArchivo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
        .Invoke(processor, new object[] { tempFile }) as Desafio_Kinetic.Models.ResultDetails;

    // Assert
    Assert.NotNull(resultado);
    Assert.True(resultado!.TotalLines > 0);
    Assert.True(resultado!.TotalWords > 0);
    Assert.NotEmpty(resultado.MostFrequentWords);
    Assert.False(string.IsNullOrWhiteSpace(resultado.Resumen));
}
}
