using Microsoft.AspNetCore.Mvc;

namespace DMG.DataReconciliation.api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class DataReconciliationController : ControllerBase
	{

		private readonly ILogger<DataReconciliationController> _logger;

		public DataReconciliationController(ILogger<DataReconciliationController> logger)
		{
			_logger = logger;
		}

		[HttpGet(Name = "GetReconDataCSV")]
		public IEnumerable<DataReconciliation> Get()
		{
			return Enumerable.Range(1, 5).Select(index => new DataReconciliation
			{
				//Projection = d
				//CSV = DateTime.Now.AddDays(index)
			})
			.ToArray();
		}
	}
}