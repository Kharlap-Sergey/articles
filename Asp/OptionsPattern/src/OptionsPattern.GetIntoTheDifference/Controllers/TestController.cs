using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace OptionsPattern.GetIntoTheDifference.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IOptionsSnapshot<TestOptions> _testOptionsSnapshot;
        private readonly IOptions<TestOptions> _testOptions;
        private readonly IOptionsMonitor<TestOptions> _optionsMonitor;
        private readonly IOptionsMonitor<TestNamedOptions> _namedOptionsMonitor;
        private readonly IOptionsMonitor<TestValidationOptions> _validationOptionsMonitor;

        public TestController(
            IOptionsSnapshot<TestOptions> testOptionsSnapshot,
            IOptions<TestOptions> testOptions,
            IOptionsMonitor<TestOptions> optionsMonitor,
            IOptionsMonitor<TestNamedOptions> namedOptionsMonitor,
            IOptionsMonitor<TestValidationOptions> validationOptionsMonitor
            )
        {
            _testOptionsSnapshot = testOptionsSnapshot;
            _testOptions = testOptions;
            _optionsMonitor = optionsMonitor;
            _namedOptionsMonitor = namedOptionsMonitor;
            _validationOptionsMonitor = validationOptionsMonitor;
        }


        [HttpGet("validation")]
        public async Task<IActionResult> GetValidationOptions()
        {
            var result = _validationOptionsMonitor.CurrentValue;
            return Ok(result);
        }

        [HttpGet("named")]
        public async Task<IActionResult> GetNamedOptions()
        {
            var result = new
            {
                Name1 = _namedOptionsMonitor.Get("Name1"),
                Name2 = _namedOptionsMonitor.Get("Name2")
            };
            return Ok(result);
        }

        [HttpGet("nowait")]
        public async Task<IActionResult> InvokeWithouWait()
        {
            var result = new
            {
                Options = _testOptions.Value,
                OptionsSnapshot = _testOptionsSnapshot.Value,
                OptionsMonitor = _optionsMonitor.CurrentValue
            };
            return Ok(
                result
                );
        }

        [HttpGet("wait")]
        public async Task<IActionResult> IndexAsync()
        {
            var beforeChange = new
            {
                Options = _testOptions.Value,
                OptionsSnapshot = _testOptionsSnapshot.Value,
                OptionsMonitor = _optionsMonitor.CurrentValue
            };

            //change the value of appsettings.json file
            //code to waite for 1 minte
            await Task.Delay(20000);

            var afterChange = new
            {
                Options = _testOptions.Value,
                OptionsSnapshot = _testOptionsSnapshot.Value,
                OptionsMonitor = _optionsMonitor.CurrentValue
            };

            var result = new { beforeChange, afterChange };
            return Ok(
                result
                );
        }
    }
}
