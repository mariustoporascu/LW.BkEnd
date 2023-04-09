using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LW.DocProces
{
	public class AuthenticationRequirementOperationFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var authorizeAttributes = context.MethodInfo?.DeclaringType?.GetCustomAttributes(true)
				.Union(context.MethodInfo.GetCustomAttributes(true))
				.OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

			if (authorizeAttributes?.Any() ?? false)
			{
				operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
				operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

				var securityRequirement = new OpenApiSecurityRequirement();
				var scheme = new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } };
				securityRequirement.Add(scheme, authorizeAttributes.Select(attr => attr.Policy).Distinct().ToList());

				operation.Security = new List<OpenApiSecurityRequirement> { securityRequirement };
			}
		}
	}
}
