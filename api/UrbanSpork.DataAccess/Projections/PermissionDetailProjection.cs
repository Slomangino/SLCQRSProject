﻿using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using UrbanSpork.CQRS.Events;
using UrbanSpork.DataAccess.DataAccess;
using UrbanSpork.DataAccess.Events;

namespace UrbanSpork.DataAccess.Projections
{
    public class PermissionDetailProjection : IProjection
    {
        private readonly UrbanDbContext _context;

        public PermissionDetailProjection() { }

        public PermissionDetailProjection(UrbanDbContext context)
        {
            _context = context;
        }

        [Key]
        public Guid PermissionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string Image { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime DateCreated { get; set; }

        public async Task ListenForEvents(IEvent @event)
        {
            PermissionDetailProjection perm = new PermissionDetailProjection();
            switch (@event)
            {
                case PermissionCreatedEvent pc:
                    perm.PermissionId = pc.Id;
                    perm.Name = pc.Name;
                    perm.Description = pc.Description;
                    perm.IsActive = pc.IsActive;
                    perm.DateCreated = pc.TimeStamp;
                    perm.Image = pc.Image;

                    await _context.PermissionDetailProjection.AddAsync(perm);
                    break;

                case PermissionInfoUpdatedEvent pu:
                    perm = await _context.PermissionDetailProjection.SingleAsync(a => a.PermissionId == pu.Id);
                    _context.Attach(perm);

                    perm.Name = pu.Name;
                    perm.Description = pu.Description;
                    perm.Image = pu.Image;
                    _context.Entry(perm).Property(a => a.Name).IsModified = true;
                    _context.Entry(perm).Property(a => a.Description).IsModified = true;
                    _context.Entry(perm).Property(a => a.Image).IsModified = true;

                    _context.PermissionDetailProjection.Update(perm);
                    break;

                case PermissionDisabledEvent pd:
                    perm = await _context.PermissionDetailProjection.SingleAsync(a => a.PermissionId == pd.Id);
                    _context.Attach(perm);

                    perm.IsActive = pd.IsActive;
                    _context.Entry(perm).Property(a => a.IsActive).IsModified = true;

                    _context.PermissionDetailProjection.Update(perm);
                    break;

                case PermissionEnabledEvent pe:
                    perm = await _context.PermissionDetailProjection.SingleAsync(a => a.PermissionId == pe.Id);
                    _context.Attach(perm);

                    perm.IsActive = pe.IsActive;
                    _context.Entry(perm).Property(a => a.IsActive).IsModified = true;

                    _context.PermissionDetailProjection.Update(perm);
                    break;
                }
            
            await _context.SaveChangesAsync();
        }
    }
}
