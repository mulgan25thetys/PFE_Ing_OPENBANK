﻿using Identity.API.Applications.Dtos;
using Identity.API.Applications.Models.Responses;

namespace Identity.API.Services.Interfaces
{
    public interface IEntitlementService
    {
        public Task<EntitlementResponse> AddEntitlementForUserAsync(string userId, EntitlementRequest request);
        public Task<bool> DeleteEntitlementAsync(string entitlement_id);
        public Entitlements GetAllEntitlements();
    }
}
