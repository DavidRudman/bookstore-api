using Bookstore.Application.Import;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Bookstore.Application.Jobs
{
    [DisallowConcurrentExecution]   //never let two imports overlap
    public class BookImportJob : IJob
    {
        private readonly IBookImporter importer;
        private readonly ILogger<BookImportJob> logger;

        public BookImportJob(IBookImporter importer, ILogger<BookImportJob> logger)
        {
            this.importer = importer;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Scheduled import triggered");
            var result = await importer.ImportAsync(context.CancellationToken);
            this.logger.LogInformation("Scheduled import done: {Imported} imported, {Skipped} skipped",
                result.Imported, result.Skipped);
        }
    }
}
