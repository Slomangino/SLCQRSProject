
using UrbanSpork.CQRS.Events;
using System;
using UrbanSpork.Common.DataTransferObjects;

namespace UrbanSpork.DataAccess.Events.Users
{
    public class UserCreatedEvent : IEvent
    {
        public UserDTO UserDTO{ get; set; }
        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTime TimeStamp { get; set; }

        public UserCreatedEvent() { }

        public UserCreatedEvent(UserDTO userDTO)
        {
            //TimeStamp = DateTime.UtcNow;
            Id = userDTO.UserID;
            UserDTO = userDTO;
            UserDTO.DateCreated = DateTime.Now;
        }
    }
}