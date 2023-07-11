using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using OPSProServer.Contracts.Hubs;
using OPSProServer.Contracts.Models;
using OPSProServer.Hubs;
using OPSProServer.Managers;
using System.Diagnostics;

namespace OPSProServer.Tests
{
    [TestClass]
    public class RoomHub
    {
        private IUserManager _userManager;
        private IRoomManager _roomManager;
        private IRoomHub _roomHub;
        private User _user1;
        private User _user2;
        private User _user3;

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
            _roomHub = new GameHub(mock.Object, _roomManager, _userManager)
            {
                Context = mockHubCallerContext.Object,
                Clients = mockClients.Object,
                Groups = mockGroupManager.Object
            };

            _user1 = new User("unit_test", "UnitTest");
            _user2 = new User("unit_test2", "UnitTest2");
            _user3 = new User("unit_test3", "UnitTest3");

            _userManager.AddUser(_user1);
            _userManager.AddUser(_user2);
            _userManager.AddUser(_user3);
        }

        [TestMethod]
        public async Task TestCreateNormal()
        {
            var description = "Has description";
            var success = await _roomHub.CreateRoom(_user1.Id, null, description);

            Assert.IsTrue(success);

            var room = _roomManager.GetRoom(_user1);

            Assert.IsNotNull(room);
            Assert.IsNotNull(room.Created);
            Assert.IsNotNull(room.Creator);
            Assert.IsNotNull(room.Description);
            Assert.IsNull(room.Opponent);
            Assert.IsNull(room.Password);
            Assert.IsNull(room.Game);
        }

        [TestMethod]
        public async Task TestCreatePassword()
        {
            var password = "unit_test_586";
            var success = await _roomHub.CreateRoom(_user1.Id, password, null);

            Assert.IsTrue(success);

            var room = _roomManager.GetRoom(_user1);

            Assert.IsNotNull(room);
            Assert.IsNotNull(room.Created);
            Assert.IsNotNull(room.Creator);
            Assert.IsNotNull(room.Password);
            Assert.IsNull(room.Description);
            Assert.IsNull(room.Opponent);
            Assert.IsNull(room.Game);
            Assert.AreEqual(password, room.Password);
        }
    }
}