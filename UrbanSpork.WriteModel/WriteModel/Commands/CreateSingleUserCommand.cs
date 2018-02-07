﻿using System;
using UrbanSpork.Common.DataTransferObjects;
using UrbanSpork.CQRS.Interfaces.WriteModel;

namespace UrbanSpork.WriteModel.Commands
{
    public class CreateSingleUserCommand : ICommand<UserDTO>
    {
        public UserInputDTO _input { get; set; }

        public CreateSingleUserCommand(UserInputDTO input)
        {
            _input = input;
        }
    }
}
