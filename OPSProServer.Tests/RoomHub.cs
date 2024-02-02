using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using OPSProServer.Contracts.Events;
using OPSProServer.Contracts.Hubs;
using OPSProServer.Contracts.Models;
using OPSProServer.Hubs;
using OPSProServer.Managers;
using OPSProServer.Models;
using OPSProServer.Services;
using SignalR_UnitTestingSupportCommon.IHubContextSupport;
using SignalR_UnitTestingSupportMSTest.Hubs;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;

namespace OPSProServer.Tests
{
    [TestClass]
    public class RoomHub : HubUnitTestsBase
    {
        private IUserManager _userManager;
        private IRoomManager _roomManager;
        private IFlowManager _resolverManager;
        private ICardService _cardService;
        private IGameRuleService _gameRuleEngine;
        private GameHub _roomHub;
        private GameHub _roomHub2;
        private GameHub _roomHub3;
        private User _user1;
        private User _user2;
        private User _user3;
        private CardInfo _leaderCard;
        private DeckInfo _deckInfo;

        [TestInitialize]
        public void Initialize()
        {
            var mock = new Mock<ILogger<GameHub>>();
            var mockCardServiceLogger = new Mock<ILogger<CardService>>();

            var request = new Mock<IRequest>();
            var mockClients = new Mock<IHubCallerClients>();
            var mockGroupManager = new Mock<IGroupManager>();
            var mockRuleEngine = new Mock<IGameRuleService>();
            var mockHubCallerContext = new Mock<HubCallerContext>();
            mockHubCallerContext.SetupGet(c => c.ConnectionId).Returns("unit_test");
            mockHubCallerContext.SetupGet(c => c.UserIdentifier).Returns("unit_test");

            var mockHubCallerContext2 = new Mock<HubCallerContext>();
            mockHubCallerContext.SetupGet(c => c.ConnectionId).Returns("unit_test2");
            mockHubCallerContext.SetupGet(c => c.UserIdentifier).Returns("unit_test2");

            var mockHubCallerContext3 = new Mock<HubCallerContext>();
            mockHubCallerContext.SetupGet(c => c.ConnectionId).Returns("unit_test3");
            mockHubCallerContext.SetupGet(c => c.UserIdentifier).Returns("unit_test3");

            _userManager = new UserManager();
            _roomManager = new RoomManager();
            _resolverManager = new FlowManager();
            IOptions<OpsPro> options = Options.Create(new OpsPro() { CardsPath = string.Empty });
            _cardService = new CardService(mockCardServiceLogger.Object, options);
            _gameRuleEngine = new GameRuleService(_cardService);
            _roomHub = new GameHub(mock.Object, _cardService, _roomManager, _userManager, _resolverManager, _gameRuleEngine);
            _roomHub2 = new GameHub(mock.Object, _cardService, _roomManager, _userManager, _resolverManager, _gameRuleEngine);
            _roomHub3 = new GameHub(mock.Object, _cardService, _roomManager, _userManager, _resolverManager, _gameRuleEngine);
            AssignToHubRequiredProperties(_roomHub);
            AssignToHubRequiredProperties(_roomHub2);
            AssignToHubRequiredProperties(_roomHub3);

            _user1 = new User("unit_test", "UnitTest");
            _user2 = new User("unit_test2", "UnitTest2");
            _user3 = new User("unit_test3", "UnitTest3");

            _roomHub.Context = mockHubCallerContext.Object;
            _roomHub2.Context = mockHubCallerContext2.Object;
            _roomHub3.Context = mockHubCallerContext3.Object;

            _userManager.AddUser(_user1);
            _userManager.AddUser(_user2);
            _userManager.AddUser(_user3);

            _leaderCard = new CardInfo("leader", new List<string>(), "OP01-001", "L", "LEADER", "Zoro", 5, null, "STRIKE", 5000, 0, new List<string>() { "RED" }, new List<string>() { "Mugiwara" }, new List<string>(), "Test", false, false, false, false, false);
            var cards = new List<CardInfo>();
            for(int i = 0;  i < 50; i++)
            {
                var card = new CardInfo(Guid.NewGuid().ToString(), new List<string>(), "OP01-002", "R", "CHARACTER", "Luffy", 5, null, "STRIKE", 5000, 0, new List<string>() { "RED" }, new List<string>() { "Mugiwara" }, new List<string>(), "Test", false, false, false, false, false);
                cards.Add(card);
            }

            _deckInfo = new DeckInfo("test");
            _deckInfo.AddCard(_leaderCard);
            _deckInfo.Cards.AddRange(cards);
        }

        [TestMethod]
        public async Task CreateRoomNormal()
        {
            var description = "Has description";
            var success = await _roomHub.CreateRoom(null, description);

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
        public async Task CreateRoomWithPassword()
        {
            var password = "unit_test_586";
            var success = await _roomHub.CreateRoom(password, null);

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

        [TestMethod]
        public async Task GetRooms()
        {
            var success = await _roomHub.CreateRoom(null, null);

            Assert.IsTrue(success);

            var list = await _roomHub.GetRooms();

            Assert.IsNotNull(list);
            Assert.AreEqual(list.Count, _roomManager.GetRooms().Count);
            Assert.AreEqual(list.Count, 1);
        }

        [TestMethod]
        public async Task GetRoom()
        {
            var success = await _roomHub.CreateRoom(null, null);

            Assert.IsTrue(success);

            var room = await _roomHub.GetRoom();

            Assert.IsNotNull(room);
            Assert.IsNotNull(room.Created);
            Assert.IsNotNull(room.Creator);
            Assert.IsFalse(room.UsePassword);
            Assert.IsNull(room.Description);
            Assert.IsNull(room.Opponent);
            Assert.AreEqual(room.Creator.Id, _user1.Id);
            Assert.AreEqual(room.Creator.Username, _user1.Username);
        }

        [TestMethod]
        public async Task JoinRoomNormal()
        {
            var success = await _roomHub.CreateRoom(null, null);

            Assert.IsTrue(success);

            var room = _roomManager.GetRoom(_user1);

            Assert.IsNotNull(room);

            room.SetOpponent(_user3);

            var joined = await _roomHub2.JoinRoom(room.Id, null);

            Assert.IsFalse(joined);

            room.SetOpponent(null);

            joined = await _roomHub2.JoinRoom(room.Id, null);

            Assert.IsTrue(joined);

            Assert.IsNotNull(room.Created);
            Assert.IsNotNull(room.Creator);
            Assert.IsNotNull(room.Opponent);
            Assert.IsFalse(room.UsePassword);
            Assert.IsNull(room.Description);
            Assert.AreEqual(room.Creator.Id, _user1.Id);
            Assert.AreEqual(room.Creator.Username, _user1.Username);
            Assert.AreEqual(room.Opponent.Id, _user2.Id);
            Assert.AreEqual(room.Opponent.Username, _user2.Username);
        }

        [TestMethod]
        public async Task JoinRoomWithPassword()
        {
            var success = await _roomHub.CreateRoom("123", null);

            Assert.IsTrue(success);

            var room = _roomManager.GetRoom(_user1);

            Assert.IsNotNull(room);

            var joined = await _roomHub2.JoinRoom(room.Id, "1234");

            Assert.IsFalse(joined);

            joined = await _roomHub2.JoinRoom(room.Id, "123");

            Assert.IsTrue(joined);

            Assert.IsNotNull(room.Created);
            Assert.IsNotNull(room.Creator);
            Assert.IsNotNull(room.Opponent);
            Assert.IsTrue(room.UsePassword);
            Assert.IsNull(room.Description);
            Assert.AreEqual(room.Creator.Id, _user1.Id);
            Assert.AreEqual(room.Creator.Username, _user1.Username);
            Assert.AreEqual(room.Opponent.Id, _user2.Id);
            Assert.AreEqual(room.Opponent.Username, _user2.Username);
        }

        [TestMethod]
        public async Task LeaveRoomAsCreator()
        {
            var success = await _roomHub.CreateRoom(null, null);

            Assert.IsTrue(success);

            var room = _roomManager.GetRoom(_user1);

            Assert.IsNotNull(room);

            var joined = await _roomHub2.JoinRoom(room.Id, null);

            Assert.IsTrue(joined);
            Assert.IsNotNull(room.Opponent);

            var left = await _roomHub.LeaveRoom();

            ClientsGroupMock.Verify(
                x => x.SendCoreAsync(
                    nameof(IRoomHubEvent.RoomDeleted),
                    new object[] { },
                    It.IsAny<CancellationToken>())
            );

            Assert.IsTrue(left);

            var newRoomCreator = _roomManager.GetRoom(room.Id);
            var newRoomOpponent = _roomManager.GetRoom(_user2);

            Assert.IsNull(newRoomCreator);
            Assert.IsNull(newRoomOpponent);
        }

        [TestMethod]
        public async Task LeaveRoomAsOpponent()
        {
            var success = await _roomHub.CreateRoom(null, null);

            Assert.IsTrue(success);

            var room = _roomManager.GetRoom(_user1);

            Assert.IsNotNull(room);

            var joined = await _roomHub2.JoinRoom(room.Id, null);

            Assert.IsTrue(joined);
            Assert.IsNotNull(room.Opponent);

            var left = await _roomHub2.LeaveRoom();

            ClientsGroupMock.Verify(
                x => x.SendCoreAsync(
                    nameof(IRoomHubEvent.RoomUpdated),
                    new object[] { room },
                    It.IsAny<CancellationToken>())
            );

            Assert.IsTrue(left);
            Assert.IsNull(room.Opponent);
        }

        [TestMethod]
        public async Task ExcludeAsCreator()
        {
            var success = await _roomHub.CreateRoom(null, null);

            Assert.IsTrue(success);

            var room = _roomManager.GetRoom(_user1);

            Assert.IsNotNull(room);

            var joined = await _roomHub2.JoinRoom(room.Id, null);

            Assert.IsTrue(joined);
            Assert.IsNotNull(room.Opponent);

            var excluded = await _roomHub.Exclude(_user2.Id, room.Id);

            ClientsGroupMock.Verify(
                x => x.SendCoreAsync(
                    nameof(IRoomHubEvent.RoomUpdated),
                    new object[] { room },
                    It.IsAny<CancellationToken>())
            );

            ClientsClientMock.Verify(
                x => x.SendCoreAsync(
                    nameof(IRoomHubEvent.RoomExcluded),
                    new object[] { },
                    It.IsAny<CancellationToken>())
            );

            Assert.IsTrue(excluded);
            Assert.IsNull(room.Opponent);
        }

        [TestMethod]
        public async Task ExcludeAsOpponent()
        {
            var success = await _roomHub.CreateRoom(null, null);

            Assert.IsTrue(success);

            var room = _roomManager.GetRoom(_user1);

            Assert.IsNotNull(room);

            var joined = await _roomHub2.JoinRoom(room.Id, null);

            Assert.IsTrue(joined);
            Assert.IsNotNull(room.Opponent);

            var excluded = await _roomHub.Exclude(_user2.Id, room.Id);
            var excluded2 = await _roomHub.Exclude(_user3.Id, room.Id);

            Assert.IsFalse(excluded);
            Assert.IsFalse(excluded2);
        }

        [TestMethod]
        public async Task SetReady()
        {
            var success = await _roomHub.CreateRoom(null, null);

            Assert.IsTrue(success);

            var room = _roomManager.GetRoom(_user1);

            Assert.IsNotNull(room);

            var joined = await _roomHub2.JoinRoom(room.Id, null);

            Assert.IsTrue(joined);
            Assert.IsNotNull(room.Opponent);

            var readyEmpty = await _roomHub.SetReady(null, null);
            var ready = await _roomHub.SetReady(_deckInfo.Name, _deckInfo.Cards.Select(x => x.Id).ToList());

            ClientsGroupMock.Verify(
                x => x.SendCoreAsync(
                    nameof(IRoomHubEvent.RoomUpdated),
                    new object[] { room },
                    It.IsAny<CancellationToken>())
            );

            Assert.IsFalse(readyEmpty);
            Assert.IsTrue(ready);
            Assert.AreEqual(room.Creator.Deck, _deckInfo);
        }
    }
}