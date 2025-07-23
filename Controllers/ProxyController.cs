using Microsoft.AspNetCore.Mvc;
using Desafio_Kinetic.Services;
using Hangfire;
using System.ComponentModel.DataAnnotations;

namespace Desafio_Kinetic.Controllers
{
    [ApiController]
    [Route("api/processes")]
    [Produces("application/json")]
    public class JobController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ProcessStateStore _stateStore;

        public JobController(ProcessStateStore stateStore, IServiceProvider serviceProvider)
        {
            _stateStore = stateStore;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Inicia el procesamiento de carpetas dentro de un path.
        /// </summary>
        /// <param name="request">Ruta raíz con subcarpetas a procesar</param>
        /// <returns>Mensaje de confirmación y cantidad de jobs lanzados</returns>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult StartJobs([FromBody] StartProcessRequest request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                if (!Directory.Exists(request.RootPath))
                    return BadRequest("El directorio especificado no existe.");

                var subfolders = Directory.GetDirectories(request.RootPath);
                int launchedJobs = 0;

                foreach (var folder in subfolders)
                {
                    var txtFiles = Directory.GetFiles(folder, "*.txt");
                    if (txtFiles.Length == 0) continue;

                    BackgroundJob.Enqueue<FolderProcessor>(processor =>
                        processor.ProcessFolder(folder));
                    launchedJobs++;
                }

                return Ok(new
                {
                    message = $"Se lanzaron {launchedJobs} jobs.",
                    totalSubfolders = subfolders.Length
                });
            }
            catch (Exception ex)
            {
                return Problem(title: "Error interno", detail: ex.Message, statusCode: 500);
            }
        }

        /// <summary>
        /// Marca un proceso como detenido.
        /// </summary>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult StopProcess(string id)
        {
            _stateStore.SetStatus(id, "STOPPED");
            return Ok(new { message = $"Proceso {id} marcado como STOPPED." });
        }

        /// <summary>
        /// Obtiene el estado de un proceso.
        /// </summary>
        [HttpGet("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetState(string id)
        {
            var state = _stateStore.GetStatus(id);
            return Ok(new { processId = id, state });
        }

        /// <summary>
        /// Lista el estado de todos los procesos.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllStates()
        {
            return Ok(_stateStore.GetAllStatus());
        }

        /// <summary>
        /// Obtiene el resultado de un proceso.
        /// </summary>
        [HttpGet("{id}/result")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetResult(string id)
        {
            try
            {
                var genDocs = _serviceProvider.GetRequiredService<GenDocs>();
                var result = genDocs.ObtenerResultado(id);

                if (result == null)
                    return NotFound("Proceso no encontrado.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(title: "Error al obtener resultado", detail: ex.Message, statusCode: 500);
            }
        }
    }

    /// <summary>
    /// Solicitud para iniciar procesos por carpeta.
    /// </summary>
    public class StartProcessRequest
    {
        /// <example>/app/input</example>
        [Required(ErrorMessage = "El campo RootPath es obligatorio.")]
        public string RootPath { get; set; }
    }
}
