using AutoMapper;
using EventBus.Message.Events;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Transaction.API.Features.Commands;
using Transaction.API.Features.Queries;
using Transaction.API.Models;
using Transaction.API.Models.Requests;
using Transaction.API.Models.Responses;
using Transaction.API.Services.Grpc;
using TRANS_STATUS = Transaction.API.Models.TRANS_STATUS;

namespace Transaction.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
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

        [Route("add-withdrawal")]
        [HttpPost]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(bool), 400)]
        [ProducesResponseType(typeof(bool), 500)]
        public async Task<ActionResult<bool>> AddWithdrawal(WithDrawalRequest request)
        {
            Random random = new Random();
            TransactionCmd transactionCmd = new TransactionCmd() { TransId = random.NextInt64(10000000000), Amount = request.Trans_Amount,
            Author = request.Trans_Author, Description = request.Trans_Description, CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now, Type = TRANS_TYPE.WITHDRAWAL.ToString(), Status = TRANS_STATUS.WAITING.ToString()};

            var debitedAccount = await accountService.GetAccountDataAsync(request.Trans_Debited_Acc);
            transactionCmd.DebitedAcc = debitedAccount.Accnumber;

            TransactionModel result = await _mediator.Send(transactionCmd);
            if (result != null )
            {
                var eventMessage = _mapper.Map<AccountEvent>(debitedAccount);

                eventMessage.BALANCE = debitedAccount.Balance - request.Trans_Amount;
                eventMessage.TRANSACTIONEVENTID = result.TRANSID;
                await _publisher.Publish(eventMessage);
                return true;
            }
            return false;
        }
        [Route("add-deposit")]
        [HttpPost]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(bool), 400)]
        [ProducesResponseType(typeof(bool), 500)]
        public async Task<ActionResult<bool>> AddDeposit(DepositRequest request)
        {
            var creditedAccount = await accountService.GetAccountDataAsync(request.Trans_Credited_Acc);
            Random random = new Random();
            TransactionCmd transactionCmd = new TransactionCmd()
            {
                TransId = random.NextInt64(10000000000),
                Amount = request.Trans_Amount,
                Author = request.Trans_Author,
                Description = request.Trans_Description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Type = TRANS_TYPE.DEPOSIT.ToString(),
                Status = TRANS_STATUS.WAITING.ToString(),
                CreditedAcc = creditedAccount.Accnumber,
            };

            TransactionModel result = await _mediator.Send(transactionCmd);
            if (result != null)
            {
                var eventMessage = _mapper.Map<AccountEvent>(creditedAccount);
                eventMessage.BALANCE = creditedAccount.Balance + request.Trans_Amount;
                eventMessage.TRANSACTIONEVENTID = result.TRANSID;
                await _publisher.Publish(eventMessage);
                return true;
            }
            return false;
        }
        [Route("transfert-account")]
        [HttpPost]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(bool), 400)]
        [ProducesResponseType(typeof(bool), 500)]
        public async Task<ActionResult<bool>> AddTransfert(TransfertAccountsRequest request)
        {
            var creditedAccount = await accountService.GetAccountDataAsync(request.Trans_Credited_Acc);
            var debitedAccount = await accountService.GetAccountDataAsync(request.Trans_Debited_Acc);
            Random random = new Random();
            TransactionCmd transactionCmd = new TransactionCmd()
            {
                TransId = random.NextInt64(10000000000),
                Amount = request.Trans_Amount,
                Author = request.Trans_Author,
                Description = request.Trans_Description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Type = TRANS_TYPE.TRANSFERT.ToString(),
                Status = TRANS_STATUS.WAITING.ToString(),
                CreditedAcc = creditedAccount.Accnumber,
                DebitedAcc = debitedAccount.Accnumber,
            };

            TransactionModel result = await _mediator.Send(transactionCmd);
            if (result != null)
            { //debited account
                var eventMessage = _mapper.Map<AccountEvent>(debitedAccount);
                eventMessage.BALANCE = debitedAccount.Balance - request.Trans_Amount;
                eventMessage.TRANSACTIONEVENTID = result.TRANSID;
                await _publisher.Publish(eventMessage);
                System.Threading.Thread.Sleep(1000);
                //credited account
                var eventMessage2 = _mapper.Map<AccountEvent>(creditedAccount);
                eventMessage2.BALANCE = creditedAccount.Balance + request.Trans_Amount;
                eventMessage2.TRANSACTIONEVENTID = result.TRANSID;
                await _publisher.Publish(eventMessage2);
                return true;
            }
            return false;
        }

        [HttpGet("{transactionId}", Name = "GetTransactionById")]
        [ProducesResponseType(typeof(TransactionModel), 200)]
        [ProducesResponseType(typeof(TransactionModel), 404)]
        public async Task<ActionResult<TransactionModel>> GetTransactionById(long transactionId)
        {
            return await _mediator.Send(new GetTransactionCmd() { TransactionId = transactionId });
        }

        [HttpGet]
        [ProducesResponseType(typeof(TransactionList), 200)]
        public async Task<ActionResult<TransactionList>> GetAllTransactionByAccount(long account_number)
        {
            return await _mediator.Send(new GetAccountTransactions() { Account_Number = account_number });
        }
    }
}
