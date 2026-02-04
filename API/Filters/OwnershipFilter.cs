/*
*	<copyright file="OwnershipFilter">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/27/2025 7:05:32 PM</date>
*	<description></description>
**/


using Backend_sec_dev.Application.Interfaces;

namespace Backend_sec_dev.API.Filters
{
    public class OwnershipFilter : IEndpointFilter
    {
        //step 5 meh
        private readonly IOwnershipService ownershipService;

        public OwnershipFilter(IOwnershipService ownershipService)
        {
            this.ownershipService = ownershipService;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {

            return await next(context);
        }
    }
}