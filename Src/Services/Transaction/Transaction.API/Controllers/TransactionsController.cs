using AutoMapper;
using EventBus.Message.Events;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Transaction.API.Features.Commands;
using Transaction.API.Features.Queries;
using Transaction.API.Models;
using Transaction.API.Models.Requests;
using Transaction.API.Models.Responses;
using Transaction.API.Services.Grpc;

namespace Transaction.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly AccountService accountService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publisher;

        public TransactionsController(AccountService accountService, IMapper mapper,
            IMediator mediator, IPublishEndpoint publishEndpoint)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _publisher = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

    }
}
