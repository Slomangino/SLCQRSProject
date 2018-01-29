﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSpork.DataAccess.DataAccess;
using UrbanSpork.DataAccess.DataTransferObjects;
using UrbanSpork.Domain.Interfaces;

namespace UrbanSpork.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private UrbanDbContext _context;
        private IUserManager _userManager;

        public UserRepository(UrbanDbContext context, IUserManager userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Task<UserDTO> GetSingleUser(int id)
        {
            var foo = _context.Users
                .Single(b => b.UserID == id);

            return Task.FromResult(Mapper.Map<UserDTO>(foo));
        }

        public void CreateUser(Users message)
        {
            _userManager.CreateNewUser();
            _context.Users.Add(message);
            _context.SaveChanges();
            //_context.FindAsync(message);
            //return Task.FromResult(message);
        }
    }
}
