using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Challenger.API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly ConcurrentDictionary<int, Moto> _motos = new();
    private static int _nextId = 1;

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    // GET: /WeatherForecast - Listar todas as motos
    [HttpGet(Name = "GetAllMotos")]
    public IEnumerable<Moto> Get()
    {
        return _motos.Values.OrderBy(m => m.Id);
    }

    // GET: /WeatherForecast/5 - Buscar moto por ID
    [HttpGet("{id}", Name = "GetMotoById")]
    public ActionResult<Moto> GetById(int id)
    {
        if (!_motos.TryGetValue(id, out var moto))
        {
            return NotFound($"Moto com id {id} não encontrada");
        }
        return Ok(moto);
    }

    // POST: /WeatherForecast - Criar nova moto
    [HttpPost(Name = "CreateMoto")]
    public ActionResult<Moto> Create([FromBody] Moto moto)
    {
        if (moto == null)
        {
            return BadRequest("Dados da moto são obrigatórios");
        }

        // Validar placa única
        if (_motos.Values.Any(m => m.Placa == moto.Placa))
        {
            return Conflict($"Já existe uma moto com a placa {moto.Placa}");
        }

        moto.Id = _nextId++;
        moto.DataCriacao = DateTime.UtcNow;
        moto.Status = StatusMoto.DISPONIVEL;

        _motos[moto.Id] = moto;
        return CreatedAtAction(nameof(GetById), new { id = moto.Id }, moto);
    }

    // PUT: /WeatherForecast/5 - Atualizar moto
    [HttpPut("{id}", Name = "UpdateMoto")]
    public ActionResult<Moto> Update(int id, [FromBody] Moto motoAtualizada)
    {
        if (!_motos.TryGetValue(id, out var motoExistente))
        {
            return NotFound($"Moto com id {id} não encontrada");
        }

        // Validar placa única (exceto ela mesma)
        if (_motos.Values.Any(m => m.Id != id && m.Placa == motoAtualizada.Placa))
        {
            return Conflict($"Já existe outra moto com a placa {motoAtualizada.Placa}");
        }

        motoExistente.Ano = motoAtualizada.Ano;
        motoExistente.Modelo = motoAtualizada.Modelo;
        motoExistente.Placa = motoAtualizada.Placa;
        motoExistente.Status = motoAtualizada.Status;
        motoExistente.DataAtualizacao = DateTime.UtcNow;

        return Ok(motoExistente);
    }

    // DELETE: /WeatherForecast/5 - Deletar moto
    [HttpDelete("{id}", Name = "DeleteMoto")]
    public ActionResult Delete(int id)
    {
        if (!_motos.TryRemove(id, out _))
        {
            return NotFound($"Moto com id {id} não encontrada");
        }
        return NoContent();
    }
}

// Classe Moto dentro do mesmo arquivo (para simplificar)
public class Moto
{
    public int Id { get; set; }
    public int Ano { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public StatusMoto Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

// Enum StatusMoto dentro do mesmo arquivo (para simplificar)
public enum StatusMoto
{
    DISPONIVEL, 
    EM_USO, 
    EM_MANUTENCAO, 
    INATIVA
}
