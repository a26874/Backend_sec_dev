using Microsoft.AspNetCore.Mvc;

namespace Backend_sec_dev.Shared.Constants
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
    }
}
