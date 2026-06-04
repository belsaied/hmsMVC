using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HMS.PL.Authorization
{
    public class PatientOwnershipRequirement : IAuthorizationRequirement { }

    public class PatientOwnershipHandler : AuthorizationHandler<PatientOwnershipRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PatientOwnershipHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PatientOwnershipRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext!;
            var role = context.User.FindFirstValue(ClaimTypes.Role);

            // Non-patient roles pass freely
            if (role != "Patient")
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Support both {id} (PatientsController) and {patientId} (sub-resource controllers)
            var routeValues = httpContext.Request.RouteValues;
            var rawId = routeValues.TryGetValue("id", out var id1) ? id1
                      : routeValues.TryGetValue("patientId", out var id2) ? id2
                      : null;

            if (rawId is null || !int.TryParse(rawId.ToString(), out var requestedId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var claim = context.User.FindFirstValue("patient_id");
            if (int.TryParse(claim, out var ownId) && ownId == requestedId)
                context.Succeed(requirement);
            else
                context.Fail(new AuthorizationFailureReason(this,
                    "You can only access your own patient record."));

            return Task.CompletedTask;
        }
    }
}
