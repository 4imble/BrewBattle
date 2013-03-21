using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace BrewBattle.Hubs
{
    [HubName("brewBattleHub")]
    public class BrewBattleHub : Hub
    {
        public static IList<ConnectedUser> CurrentUsers = new List<ConnectedUser>();
        public static IList<ConnectedUser> PrevUsers = new List<ConnectedUser>();
        public static Random rnd = new Random();

        public void Roll(string drinkname, string comment)
        {
            ConnectedUser user = GetConnectedUser();
            string encodedDrink = HttpUtility.HtmlEncode(drinkname);
            string encodedComment = HttpUtility.HtmlEncode(comment);
            int rollNumber = PlayerRoll(user);

            Clients.All.addRoll(user.Name, rollNumber.ToString(), encodedDrink, Truncate(encodedComment, 40));
            SendMessage(user.Name + " has rolled.");
            user.HasRolled = true;
            user.RollNumber = rollNumber;

            TriggerAllRolled();
        }

        int PlayerRoll(ConnectedUser user)
        {
            int roll = rnd.Next(1000) + 1;
            ConnectedUser lastLoser = PrevUsers.OrderBy(x => x.RollNumber).FirstOrDefault();
            if (lastLoser != null && user.Name == lastLoser.Name)
            {
                int bonusroll = rnd.Next(1000) + 1;
                SendMessage(user.Name + " gets bonus roll for losing!");
                Clients.Caller.addMessage(string.Format("Roll: {0}, Bonus: {1}", roll, bonusroll));
                return roll > bonusroll ? roll : bonusroll;
            }

            return roll;
        }

        public void SendMessage(string message)
        {
            string encodedMessage = HttpUtility.HtmlEncode(message);
            Clients.All.addMessage(encodedMessage);
        }

        public void ResetTimer()
        {
            Clients.All.resetTimer();
        }

        public override Task OnConnected()
        {
            foreach (ConnectedUser connectedUser in CurrentUsers)
            {
                Clients.Caller.addMessage(connectedUser.Name + " has previously connected.");
            }

            var user = new ConnectedUser {ConnectionId = Context.ConnectionId, Name = Context.User.Identity.Name};
            CurrentUsers.Add(user);

            SendMessage(user.Name + " has connected.");
            ResetTimer();
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            ConnectedUser user = GetConnectedUser();
            if (user != null && !user.HasRolled) CurrentUsers.Remove(user);
            SendMessage(Context.User.Identity.Name + " has disconnected.");
            return base.OnDisconnected();
        }

        public override Task OnReconnected()
        {
            SendMessage(Context.User.Identity.Name + " has reconnected.");
            return base.OnReconnected();
        }

        ConnectedUser GetConnectedUser()
        {
            return CurrentUsers.SingleOrDefault(x => x.ConnectionId == Context.ConnectionId);
        }

        void TriggerAllRolled()
        {
            if (!CurrentUsers.All(x => x.HasRolled)) return;

            SendMessage("Game over!");
            PrevUsers.Clear();
            foreach (ConnectedUser connectedUser in CurrentUsers)
            {
                PrevUsers.Add(connectedUser);
            }
            CurrentUsers.Clear();
        }

        string Truncate(string source, int length)
        {
            if (source.Length > length)
            {
                source = source.Substring(0, length);
            }
            return source;
        }

        public class ConnectedUser
        {
            public string Name { get; set; }
            public string ConnectionId { get; set; }
            public bool HasRolled { get; set; }
            public int RollNumber { get; set; }
        }
    }
}