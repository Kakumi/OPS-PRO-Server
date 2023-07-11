using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using OPSProServer.Contracts.Hubs;
using OPSProServer.Hubs;
using OPSProServer.Managers;

namespace OPSProServer.Tests
{
    [TestClass]
    public class UserHub
    {
        private IUserManager _userManager;
        private IRoomManager _roomManager;
        private IUserHub _userHub;

        [TestInitialize]
        public void Initialize()
        {
            var mock = new Mock<ILogger<GameHub>>();

            var request = new Mock<IRequest>();
            var mockClients = new Mock<IHubCallerClients>();
            var mockGroupManager = new Mock<IGroupManager>();
            var mockHubCallerContext = new Mock<HubCallerContext>();

            mockHubCallerContext.SetupGet(c => c.ConnectionId).Returns("unit_test");
            mockHubCallerContext.SetupGet(c => c.UserIdentifier).Returns("unit_test");

            _userManager = new UserManager();
            _roomManager = new RoomManager();
            _userHub = new GameHub(mock.Object, _roomManager, _userManager)
            {
                Context = mockHubCallerContext.Object,
                Clients = mockClients.Object,
                Groups = mockGroupManager.Object
            };
        }

        [TestMethod]
        public void TestRegister()
        {
            var playerName = "UnitTest";
            var guid = _userHub.Register(playerName);
            var user = _userManager.GetUser(guid);

            Assert.IsNotNull(guid);
            Assert.IsNotNull(user);
            Assert.IsNotNull(user.Username);
            Assert.IsNotNull(user.Created);
            Assert.IsNotNull(user.Id);
            Assert.IsNotNull(user.ConnectionId);
            Assert.AreEqual(playerName, user.Username);
            Assert.AreEqual("unit_test", user.ConnectionId);
        }
    }
}