﻿using AutoMapper;
using System;
using System.Threading.Tasks;
using UrbanSpork.Common.DataTransferObjects;
using UrbanSpork.CQRS.Interfaces;
using UrbanSpork.DataAccess.Repositories;
using UrbanSpork.WriteModel.Commands;
using CQRSLite.WriteModel.CommandHandler;

namespace UrbanSpork.WriteModel.CommandHandlers
{
    public class CreateSingleUserCommandHandler : ICommandHandler<CreateSingleUserCommand, UserDTO>
    {
        
        private IUserManager _userManager;

        public CreateSingleUserCommandHandler(IUserManager userManager)
        {
           
            _userManager = userManager;
        }

        public async Task<UserDTO> Handle(CreateSingleUserCommand command)
        {
            var userDTO = await _userManager.CreateNewUser(command._input);
            return userDTO;
        }
    }
}
