using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController(DbContext? dbContext) : ControllerBase
{
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return dbContext is not null
            ? [$"Can connect: {dbContext.Database.CanConnect()} "]
            : new List<string> { "No context " };
    }

    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}