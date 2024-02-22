using MediatR;
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
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly AccountService accountService;
        private readonly IMediator _mediator;

        public TransactionsController(AccountService accountService,IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        [Route("add-withdrawal")]
        [HttpPost]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(bool), 400)]
        [ProducesResponseType(typeof(bool), 500)]
        public async Task<ActionResult<bool>> AddWithdrawal(WithDrawalRequest request)
        {
            Random random = new Random();
            TransactionCmd transactionCmd = new TransactionCmd() { Trans_Id = random.NextInt64(10000000000), Trans_Amount = request.Trans_Amount,
            Trans_Author = request.Trans_Author, Trans_Description = request.Trans_Description, Trans_Created_At = DateTime.Now,
            Trans_Updated_At = DateTime.Now, Trans_Type = TRANS_TYPE.WITHDRAWAL.ToString(), Trans_Status = TRANS_STATUS.WAITING.ToString()};

            var debitedAccount = await accountService.GetAccountDataAsync(request.Trans_Debited_Acc);
            transactionCmd.Trans_Debited_Acc = debitedAccount.Accnumber;

            return await _mediator.Send(transactionCmd);   
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
                Trans_Id = random.NextInt64(10000000000),
                Trans_Amount = request.Trans_Amount,
                Trans_Author = request.Trans_Author,
                Trans_Description = request.Trans_Description,
                Trans_Created_At = DateTime.Now,
                Trans_Updated_At = DateTime.Now,
                Trans_Type = TRANS_TYPE.DEPOSIT.ToString(),
                Trans_Status = TRANS_STATUS.WAITING.ToString(),
                Trans_Credited_Acc = creditedAccount.Accnumber,
            };

            return await _mediator.Send(transactionCmd);
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
                Trans_Id = random.NextInt64(10000000000),
                Trans_Amount = request.Trans_Amount,
                Trans_Author = request.Trans_Author,
                Trans_Description = request.Trans_Description,
                Trans_Created_At = DateTime.Now,
                Trans_Updated_At = DateTime.Now,
                Trans_Type = TRANS_TYPE.DEPOSIT.ToString(),
                Trans_Status = TRANS_STATUS.WAITING.ToString(),
                Trans_Credited_Acc = creditedAccount.Accnumber,
                Trans_Debited_Acc = debitedAccount.Accnumber,
            };

            return await _mediator.Send(transactionCmd);
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
