namespace TaskApi.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
        private static readonly List<string> Tasks = new()
        {
            "task 1",
            "task 2",
            "task 3",
            "task 4",
        };


        [HttpGet("/")]
        public IActionResult AllTasks()
        {
            return Ok(Tasks);
        }
    }
}