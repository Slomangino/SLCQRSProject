﻿using AutoMapper;
using UrbanSpork.CQRS.Events;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UrbanSpork.DataAccess.DataAccess;
using UrbanSpork.DataAccess.Events.Users;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UrbanSpork.Common;

namespace UrbanSpork.DataAccess.Projections
{
    public class UserDetailProjection : IProjection
    {
        private readonly UrbanDbContext _context;

        public UserDetailProjection() { }

        public UserDetailProjection(UrbanDbContext context)
        {
            _context = context;
        }

        [Key]
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }

        [Column(TypeName = "json")]
        public string PermissionList { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime DateCreated { get; set; }

        public async Task ListenForEvents(IEvent @event)
        {
            UserDetailProjection user = new UserDetailProjection();
            switch (@event) { 
                case UserCreatedEvent uc:
                    user.UserId = uc.Id;
                    user.FirstName = uc.FirstName;
                    user.LastName = uc.LastName;
                    user.Email = uc.Email;
                    user.Position = uc.Position;
                    user.Department = uc.Department;
                    user.IsActive = uc.IsActive;
                    user.IsAdmin = uc.IsAdmin;
                    user.PermissionList = JsonConvert.SerializeObject(uc.PermissionList);
                    user.DateCreated = uc.TimeStamp;

                    _context.UserDetailProjection.Add(user);
                    break;
                case UserUpdatedEvent uu:
                    user = await _context.UserDetailProjection.SingleAsync(b => b.UserId == uu.Id);
                    _context.UserDetailProjection.Attach(user);

                    user.FirstName = uu.FirstName;
                    user.LastName = uu.LastName;
                    user.Email = uu.Email;
                    user.Position = uu.Position;
                    user.Department = uu.Department;
                    user.IsAdmin = uu.IsAdmin;
                    _context.Entry(user).Property(a => a.FirstName).IsModified = true;
                    _context.Entry(user).Property(a => a.LastName).IsModified = true;
                    _context.Entry(user).Property(a => a.Email).IsModified = true;
                    _context.Entry(user).Property(a => a.Position).IsModified = true;
                    _context.Entry(user).Property(a => a.Department).IsModified = true;
                    _context.Entry(user).Property(a => a.IsAdmin).IsModified = true;

                    _context.UserDetailProjection.Update(user);
                    break;
                case UserDisabledEvent ud:
                    user = await _context.UserDetailProjection.SingleAsync(b => b.UserId == ud.Id);
                    _context.UserDetailProjection.Attach(user);

                    user.IsActive = ud.IsActive;
                    _context.Entry(user).Property(a => a.IsActive).IsModified = true;

                    _context.UserDetailProjection.Update(user);
                    break;
                case UserEnabledEvent ue:
                    user = await _context.UserDetailProjection.SingleAsync(b => b.UserId == ue.Id);
                    _context.UserDetailProjection.Attach(user);

                    user.IsActive = ue.IsActive;
                    _context.Entry(user).Property(a => a.IsActive).IsModified = true;

                    _context.UserDetailProjection.Update(user);
                    break;
                case UserPermissionsRequestedEvent upr:
                    user = await _context.UserDetailProjection.SingleAsync(b => b.UserId == upr.Id);
                    _context.UserDetailProjection.Attach(user);

                    var permList = JsonConvert.DeserializeObject<Dictionary<Guid, PermissionRequest>>(user.PermissionList);
                    foreach (var request in upr.Requests)
                    {
                        request.Value.RequestDate = upr.TimeStamp; // not a good fix, updates projection but not the aggregate
                        permList[request.Key] = request.Value;
                    }

                    user.PermissionList = JsonConvert.SerializeObject(permList);

                    _context.Entry(user).Property(a => a.PermissionList).IsModified = true;

                    _context.UserDetailProjection.Update(user);
                    break;
            }

            await _context.SaveChangesAsync();
        }
    }
}
