﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UrbanSpork.Common;
using UrbanSpork.CQRS.Events;
using UrbanSpork.DataAccess.DataAccess;
using UrbanSpork.DataAccess.Events;
using UrbanSpork.DataAccess.Events.Users;

namespace UrbanSpork.DataAccess.Projections
{
    public class DashboardProjection : IProjection
    {
        private readonly UrbanDbContext _context;

        public DashboardProjection() { }

        public DashboardProjection(UrbanDbContext context)
        {
            _context = context;
        }

        [Key]
        public Guid PermissionId { get; set; }
        public int ActiveUsers { get; set; }
        public int PendingRequests { get; set; }
        public string SystemName { get; set; }
        public string LogoUrl { get; set; }

        public async Task ListenForEvents(IEvent @event)
        {
            DashboardProjection dBProjection = new DashboardProjection();

            switch (@event)
            {
                case PermissionInfoUpdatedEvent pue:
                    dBProjection = await _context.DashBoardProjection.SingleAsync(a => a.PermissionId == pue.Id);
                    _context.Attach(dBProjection);

                    dBProjection.SystemName = pue.Name;
                    dBProjection.LogoUrl = pue.Image;

                    _context.Entry(dBProjection).Property(a => a.SystemName).IsModified = true;
                    _context.Entry(dBProjection).Property(a => a.LogoUrl).IsModified = true;

                    _context.DashBoardProjection.Update(dBProjection);
                    break;
                case PermissionCreatedEvent pce:
                    dBProjection.PermissionId = pce.Id;
                    dBProjection.SystemName = pce.Name;
                    dBProjection.ActiveUsers = 0;
                    dBProjection.PendingRequests = 0;
                    dBProjection.LogoUrl = pce.Image;
                    _context.DashBoardProjection.Add(dBProjection);
                    break;
                case UserPermissionsRequestedEvent pre:
                    //increment pending requests
                    var rowsToBeUpdated = await _context.DashBoardProjection.Where(a => pre.Requests.ContainsKey(a.PermissionId)).ToListAsync();
                    foreach (var entry in rowsToBeUpdated)
                    {
                        entry.PendingRequests++;

                        _context.Entry(entry).Property(a => a.PendingRequests).IsModified = true;
                        _context.Update(entry);
                    }
                    break;
                case UserPermissionRequestDeniedEvent prd:
                    //decrement pending requests
                    var rows = await _context.DashBoardProjection.Where(a => prd.PermissionsToDeny.ContainsKey(a.PermissionId)).ToListAsync();
                    foreach (var entry in rows)
                    {
                        if (entry.PendingRequests <= 1)
                        {
                            entry.PendingRequests = 0;
                        }
                        else
                        {
                            entry.PendingRequests-- ;
                        }
                        _context.Entry(entry).Property(a => a.PendingRequests).IsModified = true;
                        _context.Update(entry);
                    }
                    break;
                case UserPermissionGrantedEvent pge:
                    //increment active users, decrement pending requests
                    var rs = await _context.DashBoardProjection.Where(a => pge.PermissionsToGrant.ContainsKey(a.PermissionId)).ToListAsync();
                    var user = await _context.UserDetailProjection.Where(a => a.UserId == pge.ForId).SingleOrDefaultAsync();
                    var permissionList =
                        JsonConvert.DeserializeObject<Dictionary<Guid, DetailedUserPermissionInfo>>(
                            user.PermissionList);

                    foreach (var entry in rs)
                    {
                        entry.ActiveUsers++;
                        _context.Entry(entry).Property(a => a.ActiveUsers).IsModified = true;
                        
                        //no need to go further because their list did not contain a definition for this permission meaning it was never previously requested.
                        if (!permissionList.ContainsKey(entry.PermissionId))
                        {
                            _context.Update(entry);
                            break;
                        }

                        // checks previous state of that permission
                        if (string.Equals(permissionList[entry.PermissionId].PermissionStatus, "Requested"))
                        {
                            if (entry.PendingRequests <= 1)
                            {
                                entry.PendingRequests = 0;
                            }
                            else
                            {
                                entry.PendingRequests--;
                            }
                        }
                        _context.Entry(entry).Property(a => a.PendingRequests).IsModified = true;
                        _context.Update(entry);
                    }
                    break;
                case UserPermissionRevokedEvent upr:
                    //decrement active users
                    var r = await _context.DashBoardProjection.Where(a => upr.PermissionsToRevoke.ContainsKey(a.PermissionId)).ToListAsync();
                    foreach (var entry in r)
                    {
                        if (entry.ActiveUsers <= 1)
                        {
                            entry.ActiveUsers = 0;
                        }
                        else
                        {
                            entry.ActiveUsers--;
                        }

                        _context.Entry(entry).Property(a => a.ActiveUsers).IsModified = true;
                        _context.Update(entry);
                    }
                    break;

                case UserCreatedEvent uc:
                    if (uc.PermissionList.Any())
                    {
                        var rowsToChange = await _context.DashBoardProjection.Where(a => uc.PermissionList.ContainsKey(a.PermissionId)).ToListAsync();
                        foreach (var row in rowsToChange)
                        {
                            row.ActiveUsers++;
                        }
                    }
                    break;

            }
            await _context.SaveChangesAsync();
        }
    }
}