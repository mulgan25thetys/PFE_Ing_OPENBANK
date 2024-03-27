﻿using Account.Grpc.Protos;
using Account.Grpc.Services.Interfaces;
using AutoMapper;
using Grpc.Core;

namespace Account.Grpc.Services
{
    public class AccountServiceProvider : AccountProtoService.AccountProtoServiceBase
    {
        private readonly IAccountService _service;
        private readonly ILogger<AccountServiceProvider> _logger;
        private readonly IMapper _mapper;

        public AccountServiceProvider(IAccountService service, ILogger<AccountServiceProvider> logger, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public override async Task<AccountObject> GetAccount(GetAccountRequest request, ServerCallContext context)
        {
            var account = await _service.GetAccount(request.Id);
            if (account.Id == null)
            {
                _logger.LogInformation($"Failed to retrieve Account by id {request.Id}");
                return new AccountObject();
            }
            else
            {
                _logger.LogInformation($"Account is retrieved by id {request.Id}");
                return _mapper.Map<AccountObject>(account);
            }
            
        }
    }
}
