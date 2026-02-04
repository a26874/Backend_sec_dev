using Microsoft.AspNetCore.Mvc;

namespace Backend_sec_dev.Shared.Constants
{
    /// <summary>
    /// Base endpoint controller
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
    }
}
