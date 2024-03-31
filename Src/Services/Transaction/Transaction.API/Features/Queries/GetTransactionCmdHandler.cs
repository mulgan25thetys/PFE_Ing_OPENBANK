using MediatR;
using Transaction.API.Models;
using Transaction.API.Services.Interfaces;

namespace Transaction.API.Features.Queries
{
    public class GetTransactionCmdHandler : IRequestHandler<GetTransactionCmd, TransactionModel>
    {
        private readonly ILogger<GetTransactionCmdHandler> _logger;
        private readonly ITransactionService _service;

        public GetTransactionCmdHandler(ILogger<GetTransactionCmdHandler> logger, ITransactionService service)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task<TransactionModel> Handle(GetTransactionCmd request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving of transaction ...");
            //var transaction = await _service.GetTransactionAsync(request.TransactionId);

            _logger.LogInformation($"Transaction with id: {request.TransactionId} is retrieved");
            return null;
        }
    }
}
