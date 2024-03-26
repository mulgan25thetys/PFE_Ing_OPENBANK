﻿using AutoMapper;
using Grpc.Core;
using User.grpc.Models;
using User.grpc.Protos;
using User.grpc.Services.Interfaces;

namespace User.grpc.Services
{
    public class UserServiceProvider : UserProtoService.UserProtoServiceBase
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ILogger<UserServiceProvider> _logger;

        public UserServiceProvider(IMapper mapper, IUserService userService, ILogger<UserServiceProvider> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Retrieving a user with USER_ID : {request.UserId}");
            UserModel model = await _userService.GetUserModelByIdAsync(request.UserId);

            if (model == null)
            {
                _logger.LogError("OBP-20005: User not found. Please specify a valid value for USER_ID.");
                return null;
            }
            UserData data = new UserData()
            {
                email = model.Email,
                provider = model.Provider,
                providerId = model.Provider_id,
                userId = model.Id,
                userName = model.UserName
            };
            return _mapper.Map<UserResponse>(data);
        }
    }
}
