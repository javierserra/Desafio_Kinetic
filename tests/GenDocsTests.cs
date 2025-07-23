using Xunit;
using Desafio_Kinetic.Services;
using Desafio_Kinetic.Models;

public class GenDocsTests
{
    [Fact]
    public void CrearProceso_DeberiaInicializarProcesoCorrectamente()
    {
        var genDocs = new GenDocs();
        string id = "test-proceso";

        genDocs.CrearProceso(id, 5);

        var resultado = genDocs.ObtenerResultado(id);

        Assert.NotNull(resultado);
        Assert.Equal("RUNNING", resultado.Status);
        Assert.Equal(5, resultado.Progress.TotalFiles);
        Assert.Equal(0, resultado.Progress.ProcessedFiles);
    }

    [Fact]
    public void ActualizarProgreso_DeberiaActualizarPalabrasYArchivos()
    {
        var genDocs = new GenDocs();
        string id = "test-progreso";
        genDocs.CrearProceso(id, 1);

        var resultDetails = new ResultDetails
        {
            TotalLines = 10,
            TotalWords = 50,
            MostFrequentWords = new List<string> { "inteligencia", "artificial" }
        };

        genDocs.ActualizarProgreso(id, resultDetails, "doc1.txt");

        var resultado = genDocs.ObtenerResultado(id);

        Assert.Equal(50, resultado.Results.TotalWords);
        Assert.Contains("doc1.txt", resultado.Results.FilesProcessed);
    }
}
