﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
    public class PendingRequestsProjection : IProjection
    {
        public readonly UrbanDbContext _context;

        public PendingRequestsProjection() { }

        public PendingRequestsProjection(UrbanDbContext context)
        {
            _context = context;
        }

        public Guid PermissionId { get; set; }
        public Guid ForId { get; set; }
        public Guid ById { get; set; }
        public string RequestType { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime DateOfRequest { get; set; }

        public async Task ListenForEvents(IEvent @event)
        {
            switch (@event)
            {
                case UserPermissionsRequestedEvent upr:
                    foreach (var r in upr.Requests)
                    {
                        var row = new PendingRequestsProjection
                        {
                            PermissionId = r.Key,
                            ForId = r.Value.RequestedFor,
                            ById = r.Value.RequestedBy,
                            RequestType = r.Value.EventType,
                            DateOfRequest = r.Value.RequestDate
                        };

                        //if a request of that type does not already exist for that user and permission add it to the list
                        if (!_context.PendingRequestsProjection.Any(a =>
                            a.PermissionId == row.PermissionId && a.ForId == row.ForId && a.RequestType == row.RequestType))
                        {
                            await _context.PendingRequestsProjection.AddAsync(row);
                        }
                    }
                    break;
                case PermissionDiabledEvent pd:
                    var list = _context.PendingRequestsProjection.Where(a => a.PermissionId == pd.Id);
                    _context.PendingRequestsProjection.RemoveRange(list);
                    break;
                case UserPermissionRequestDeniedEvent pd:
                    var pending = await _context.PendingRequestsProjection.SingleAsync(a => a.PermissionId == pd.PermissionId && a.ForId == pd.ForId);
                    _context.PendingRequestsProjection.Remove(pending);
                    break;
                case UserPermissionGrantedEvent pg:
                    var requests = _context.PendingRequestsProjection.Where(a => a.ForId == pg.ForId && pg.PermissionsToGrant.ContainsKey(a.PermissionId));
                    if (requests.Any())
                    {
                        foreach (var r in requests)
                        {
                            _context.PendingRequestsProjection.Remove(r);
                        }
                    }
                    break;
            }

            await _context.SaveChangesAsync();
        }
    }
}
