using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SensoringAPI.Attributes;
using System.Threading.Tasks;

namespace SensoringAPI.Tests.Attributes;

[TestClass]
public class ApiPasswordAuthorizeAttributeTests
{
    private const string ReadPassword = "read123";
    private const string WritePassword = "write123";

    private AuthorizationFilterContext CreateContext(string providedPassword = null)
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Auth:ReadPassword"]).Returns(ReadPassword);
        configMock.Setup(c => c["Auth:WritePassword"]).Returns(WritePassword);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(configMock.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProviderMock.Object
        };

        if (providedPassword != null)
            httpContext.Request.Headers["Api-Password"] = providedPassword;

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
        };

        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }


    [TestMethod]
    public async Task NoPasswordProvided_ReturnsUnauthorized()
    {
        var attr = new ApiPasswordAuthorizeAttribute();
        var context = CreateContext();

        await attr.OnAuthorizationAsync(context);

        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
    }

    [TestMethod]
    public async Task WrongReadPassword_ReturnsUnauthorized()
    {
        var attr = new ApiPasswordAuthorizeAttribute();
        var context = CreateContext("wrong");

        await attr.OnAuthorizationAsync(context);

        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
    }

    [TestMethod]
    public async Task CorrectReadPassword_AllowsAccess()
    {
        var attr = new ApiPasswordAuthorizeAttribute();
        var context = CreateContext(ReadPassword);

        await attr.OnAuthorizationAsync(context);

        Assert.IsNull(context.Result);
    }

    [TestMethod]
    public async Task CorrectWritePassword_AllowsAccess_WhenWriteRequiredIsTrue()
    {
        var attr = new ApiPasswordAuthorizeAttribute(writeRequired: true);
        var context = CreateContext(WritePassword);

        await attr.OnAuthorizationAsync(context);

        Assert.IsNull(context.Result);
    }

    [TestMethod]
    public async Task WrongWritePassword_ReturnsUnauthorized_WhenWriteRequiredIsTrue()
    {
        var attr = new ApiPasswordAuthorizeAttribute(writeRequired: true);
        var context = CreateContext("read123");

        await attr.OnAuthorizationAsync(context);

        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
    }
}
