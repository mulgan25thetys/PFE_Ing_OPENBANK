﻿using OcelotApiGW.API.Utils.Interfaces;
using System;

namespace OcelotApiGW.API.Middlewares
{
    public class JwtMiddleware : IMiddleware
    {
        private readonly IJwtUtils jwtUtils;

        public JwtMiddleware(IJwtUtils jwtUtils)
        {
            this.jwtUtils = jwtUtils ?? throw new ArgumentNullException(nameof(jwtUtils));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Get the token from the Authorization header
            var bearer = context.Request.Headers["Authorization"].ToString();
            var token = bearer.Replace("Bearer ", string.Empty);

            if (!string.IsNullOrEmpty(token))
            {
                // Verify the token using the IJwtBuilder
                var userId = jwtUtils.ValidateToken(token);

                if (userId != null)
                {
                    // Store the userId in the HttpContext items for later use
                    context.Items["userId"] = userId;
                }
                else
                {
                    // If token or userId are invalid, send 401 Unauthorized status
                    context.Response.StatusCode = 401;
                }
            }

            // Continue processing the request
            await next(context);
        }
    }
}
