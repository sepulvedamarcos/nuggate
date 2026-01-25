using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace nuggate.Controllers;

/// <summary>
/// Unico controlador de la aplicación
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NuggateController : ControllerBase
{
    private readonly string _packagePath = Path.Combine(Directory.GetCurrentDirectory(), "Packages");

    /// <summary>
    /// Obtiene el índice de servicios de la API de NuGet
    /// </summary>
    /// <remarks>
    /// Este método devuelve un JSON que describe los servicios disponibles en la API de NuGet.
    /// </remarks>
    /// <response code="200">Estructura consulta de servidor de Nuget Local</response>
    [HttpGet("index.json")]
    public IActionResult GetServiceIndex()
    {
        var json = $"{{ \"version\": \"3.0.0\", \"resources\": [";
        json += $"{{ \"@id\": \"{Request.Scheme}://{Request.Host}/api/nuggate/package/\", \"@type\": \"PackageBaseAddress/3.0.0\" }},";
        json += $"{{ \"@id\": \"{Request.Scheme}://{Request.Host}/api/nuggate/search\", \"@type\": \"SearchQueryService/3.0.0\" }},";
        json += $"{{ \"@id\": \"{Request.Scheme}://{Request.Host}/api/nuggate/upload\", \"@type\": \"PackagePublish/2.0.0\" }}";
        json += "]}";

        return Content(json, "application/json");
    }

    /// <summary>
    /// Obtiene el índice de versiones de un paquete específico
    /// </summary>
    /// <param name="id">Nombre del paquete</param>
    /// <param name="index">Nombre del archivo index.json</param>
    /// <response code="200">Índice de versiones del paquete solicitado</response>
    [HttpGet("package/{id}/{index}")]
    public IActionResult GetPackage(string id, string index)
    {
        var packageDirectory = Path.Combine(_packagePath, id);
        var indexFilePath = Path.Combine(packageDirectory, index);

        if (System.IO.File.Exists(indexFilePath))
        {
            var content = System.IO.File.ReadAllText(indexFilePath);
            return Content(content, "application/json");
        }

        return NotFound($"Package {id} version {index} no encontrado");
    }

    /// <summary>
    /// Obtiene el archivo .nupkg de un paquete específico
    /// </summary>
    /// <remarks>
    /// Este método devuelve el archivo .nupkg solicitado si existe en el servidor.
    /// </remarks>
    /// <param name="id">Nombre del paquete</param>
    /// <param name="version">versión del paquete buscado</param>
    /// <param name="fileName">Nombre completo del paquete</param>
    /// <response code="200">Archivo .nupkg del paquete solicitado</response>
    [HttpGet("package/{id}/{version}/{fileName}")]
    public IActionResult GetPackage2(string id, string version, string fileName)
    {
        var packageFileName = $"{id}.{version}.nupkg";
        var packageFilePath = Path.Combine(_packagePath, id, packageFileName);

        if (System.IO.File.Exists(packageFilePath))
        {
            var bytes = System.IO.File.ReadAllBytes(packageFilePath);
            return File(bytes, "application/octet-stream", packageFileName);
        }

        return NotFound($"Package {id} version {version} no encontrado");
    }

    /// <summary>
    /// metodo para subir un paquete y actualizar el archivo index.json
    /// </summary>
    /// <remarks>
    /// Este método permite subir un archivo .nupkg al servidor y actualizar el archivo index.json correspondiente.
    /// </remarks>
    /// <response code="200">Archivo .nupkg subido correctamente</response>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadPackage()
    {
        if (Request.Form.Files.Count == 0)
            return BadRequest("No se ha subido ningún archivo.");

        var file = Request.Form.Files[0];

        // Verificar si el archivo tiene la extensión correcta
        if (!file.FileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Tipo de archivo inválido. Solo se permiten archivos .nupkg.");

        // Obtener el nombre del archivo sin la extensión
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);

        // Contar los segmentos desde el final y tomar el último para la versión y el resto para el ID del paquete
        var segments = fileNameWithoutExtension.Split('.');
        if (segments.Length < 4) // Asegurarse de que haya al menos tres segmentos para ID del paquete y versión
            return BadRequest("Formato de nombre de archivo de paquete inválido. Formato esperado: <PackageId>.<Version>.nupkg");

        // Los últimos 3 segmentos definen el inicio de la versión
        var versionStartIndex = segments.Length - 3;
        var packageId = string.Join(".", segments.Take(versionStartIndex));
        var version = string.Join(".", segments.Skip(versionStartIndex));

        // Validar la versión utilizando un formato semántico simple
        if (!Version.TryParse(version, out _))
            return BadRequest("Formato de versión inválido en el nombre del archivo de paquete.");

        // Crear el directorio para el paquete
        var packageDirectory = Path.Combine(_packagePath, packageId);
        if (!Directory.Exists(packageDirectory))
        {
            Directory.CreateDirectory(packageDirectory);
        }

        var filePath = Path.Combine(packageDirectory, file.FileName);

        // Validar si el archivo ya existe
        if (System.IO.File.Exists(filePath))
            return BadRequest("El archivo ya existe en el servidor.");

        // Guardar el archivo .nupkg en el directorio
        try
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al guardar el paquete: {ex.Message}");
        }

        // Actualizar o crear el archivo index.json correspondiente
        var indexFilePath = Path.Combine(packageDirectory, "index.json");
        List<string> versions = new List<string>();

        try
        {
            if (System.IO.File.Exists(indexFilePath))
            {
                var indexJson = System.IO.File.ReadAllText(indexFilePath);
                var indexData = JsonSerializer.Deserialize<PackageIndex>(indexJson);

                // Verificar si indexData y Versions no son nulos antes de acceder
                if (indexData != null && indexData.Versions != null)
                {
                    versions = indexData.Versions;
                    if (!versions.Contains(version))
                    {
                        versions.Add(version);
                        var newIndexData = new PackageIndex { Versions = versions.OrderBy(v => v).ToList() };
                        var newIndexJson = JsonSerializer.Serialize(newIndexData, new JsonSerializerOptions { WriteIndented = true });
                        System.IO.File.WriteAllText(indexFilePath, newIndexJson);
                    }
                }
                else
                {
                    // Inicializar versiones si el archivo JSON estaba vacío o malformado
                    versions.Add(version);
                    var newIndexData = new PackageIndex { Versions = versions.OrderBy(v => v).ToList() };
                    var newIndexJson = JsonSerializer.Serialize(newIndexData, new JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(indexFilePath, newIndexJson);
                }
            }
            else
            {
                versions.Add(version);
                var newIndexData = new PackageIndex { Versions = versions.OrderBy(v => v).ToList() };
                var newIndexJson = JsonSerializer.Serialize(newIndexData, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(indexFilePath, newIndexJson);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el archivo index.json: {ex.Message}");
        }

        return Ok($"El paquete {file.FileName} se ha subido y registrado correctamente.");
    }

    /// <summary>
    /// Método para buscar paquetes en el servidor
    /// </summary>
    /// <remarks>
    /// Este método permite buscar paquetes en el servidor por nombre.
    /// </remarks>
    /// <param name="q">Nombre de paquete a buscar</param>
    /// <param name="skip">Salto</param>
    /// <param name="take">Cantidad</param>
    /// <response code="200">Lista de paquetes encontrados</response>
    [HttpGet("search")]
    public IActionResult Search([FromQuery] string q = "", [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var packageFiles = Directory.GetDirectories(_packagePath) // Obtener todos los directorios de paquetes
            .SelectMany(d => Directory.GetFiles(d, "*.nupkg")) // Obtener todos los archivos .nupkg en cada directorio
            .ToList();

        var packages = packageFiles.Select(file =>
        {
            var fileInfo = new FileInfo(file);
            var fileName = fileInfo.Name;
            int lastDotIndex = fileName.LastIndexOf('.');
            int secondLastDotIndex = fileName.LastIndexOf('.', lastDotIndex - 1);
            int thirdLastDotIndex = fileName.LastIndexOf('.', secondLastDotIndex - 1);
            var version = "";
            var nombre = "";

            if (thirdLastDotIndex != -1)
            {
                version = fileName.Substring(thirdLastDotIndex - 1, (lastDotIndex + 1) - thirdLastDotIndex);
                nombre = fileName.Substring(0, thirdLastDotIndex - 2);
            }
            else
            {
                version = "No se encontró el patrón esperado.";
                nombre = "No se encontró el patrón esperado.";
            }

            return new
            {
                Id = nombre,
                nombre = fileName,
                Version = version,
                Description = "",
                Authors = new[] { "Author" },
            };
        })
        .Where(p => string.IsNullOrEmpty(q) || p.Id.Contains(q, StringComparison.OrdinalIgnoreCase))
        .Skip(skip)
        .Take(take)
        .ToList();

        var result = new
        {
            totalHits = packages.Count,
            data = packages
        };

        return Ok(result);
    }

    // metodo para eliminar un archivo del directorio y de los archivos index.json
    /// <summary>
    /// Método para eliminar un paquete del servidor
    /// </summary>
    /// <param name="id">Nombre del paquete</param>
    /// <param name="version">Versión del paquete</param>
    /// <response code="200">Paquete eliminado correctamente</response>
    /// [HttpDelete("package/{id}/{version}")]
    ///     
    ///     
    [HttpDelete("delete/{id}/{version}")]
    public IActionResult DeletePackage(string id, string version)
    {
        var packageDirectory = Path.Combine(_packagePath, id);
        var packageFileName = $"{id}.{version}.nupkg";
        var packageFilePath = Path.Combine(packageDirectory, packageFileName);
        var indexFilePath = Path.Combine(packageDirectory, "index.json");

        // Verificar si el paquete existe
        if (!System.IO.File.Exists(packageFilePath))
            return NotFound($"Package {id} version {version} no encontrado");

        // Eliminar el archivo .nupkg
        System.IO.File.Delete(packageFilePath);

        // Actualizar el archivo index.json
        if (System.IO.File.Exists(indexFilePath))
        {
            var indexJson = System.IO.File.ReadAllText(indexFilePath);
            var indexData = JsonSerializer.Deserialize<PackageIndex>(indexJson);

            if (indexData != null && indexData.Versions != null)
            {
                indexData.Versions.Remove(version);
                var newIndexJson = JsonSerializer.Serialize(indexData, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(indexFilePath, newIndexJson);
            }
        }

        return Ok($"El paquete {id} versión {version} ha sido eliminado correctamente.");
    }
}

/// <summary>
/// Clase que representa la estructura de un archivo index.json
/// </summary>
public class PackageIndex
{
    /// <summary>
    /// Lista de versiones del paquete
    /// </summary>
    [JsonPropertyName("versions")]
    public List<string> Versions { get; set; } = new List<string>();
}
