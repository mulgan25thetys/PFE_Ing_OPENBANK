using AutoMapper;
using MediatR;
using Transaction.API.Models;
using Transaction.API.Services.Interfaces;

namespace Transaction.API.Features.Commands
{
    public class TransactionCmdHandler : IRequestHandler<TransactionCmd, bool>
    {
        private readonly ILogger<TransactionCmdHandler> _logger;
        private readonly IMapper _mapper;
        private readonly ITransactionService _service;

        public TransactionCmdHandler(ILogger<TransactionCmdHandler> logger, IMapper mapper, ITransactionService service)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(TransactionCmd request, CancellationToken cancellationToken)
        {
            var transactionModel = _mapper.Map<TransactionModel>(request);

            var result = await _service.AddTransactionAsync(transactionModel);

            if (result == true)
            {
                _logger.LogInformation($"One Transaction has been created");
            }else{
                _logger.LogError($"Transaction created failed!");
            }
            return result;
        }
    }
}
