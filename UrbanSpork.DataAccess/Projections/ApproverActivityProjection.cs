﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public class ApproverActivityProjection : IProjection
    {
        private readonly UrbanDbContext _context;

        public ApproverActivityProjection() { }

        public ApproverActivityProjection(UrbanDbContext context)
        {
            _context = context;
        }

        [Key]
        public Guid Id { get; set; }
        public Guid ById { get; set; }
        public Guid ForId { get; set; }
        public string ForFirstName { get; set; }
        public string ForLastName { get; set; }
        public string TruncatedEventType { get; set; }

        [Column(TypeName = "json")]
        public string PermissionList { get; set; }

        public async Task ListenForEvents(IEvent @event)
        {
            ApproverActivityProjection approverActivity = new ApproverActivityProjection();
            List<CondensedPermissionDetail> permissionList = new List<CondensedPermissionDetail>();
            switch (@event)
            {
                case UserUpdatedEvent userUpdated:
                    approverActivity = _context.ApproverActivityProjection
                                        .SingleOrDefault(a => a.ForId == userUpdated.Id) 
                                        ?? new ApproverActivityProjection();

                    EFHelper.InsertOrUpdate(approverActivity);

                    approverActivity.Id = Id.Equals(Guid.Empty) ? new Guid() : approverActivity.Id;

                    approverActivity.ForFirstName = userUpdated.FirstName;

                    approverActivity.ForLastName = userUpdated.LastName;

                    _context.Entry(approverActivity).Property(a => a.ForLastName).IsModified = true;
                    _context.Entry(approverActivity).Property(a => a.ForFirstName).IsModified = true;
                    _context.ApproverActivityProjection.Add(approverActivity);
                    break;
                case PermissionInfoUpdatedEvent permissionInfoUpdated:
                    IEnumerable<ApproverActivityProjection> aAList = 
                         _context.ApproverActivityProjection.ToList().Where(p =>
                            JsonConvert.DeserializeObject<CondensedPermissionDetail>(p.PermissionList)
                                .PermissionId == permissionInfoUpdated.Id);

                    //aAList = aAList.Where(p => 
                    //    JsonConvert.DeserializeObject<CondensedPermissionDetail>(p.PermissionList)
                    //        .PermissionId == permissionInfoUpdated.Id);

                    foreach (var aActivity in aAList)
                    {
                        _context.Entry(aActivity).State = EntityState.Modified;

                        permissionList =
                            JsonConvert.DeserializeObject<List<CondensedPermissionDetail>>(aActivity.PermissionList);

                        var pDetail = permissionList.FirstOrDefault(a =>
                            a.PermissionId == permissionInfoUpdated.Id);

                        var oldValue = pDetail;

                        pDetail.PermissionName = permissionInfoUpdated.Name;

                        int index = permissionList.IndexOf(oldValue);

                        if (index != -1)
                        {
                            permissionList[index] = pDetail;
                        }

                        _context.Entry(aActivity).Property(a => a.PermissionList).IsModified = true;
                        _context.ApproverActivityProjection.Update(aActivity);
                    }
                    break;
                case UserPermissionGrantedEvent permissionGranted:
                    
                    _context.ApproverActivityProjection.Add(approverActivity);

                    approverActivity.ForId = permissionGranted.ForId;

                    approverActivity.ById = permissionGranted.ById;
                    
                    approverActivity.ForFirstName = _context.UserDetailProjection
                        .Where(a => a.UserId == permissionGranted.ForId)
                        .Select(p => p.FirstName).Single();

                    approverActivity.ForLastName = _context.UserDetailProjection
                        .Where(a => a.UserId == permissionGranted.ForId)
                        .Select(p => p.LastName).Single();

                    approverActivity.TruncatedEventType = "Granted";

                    foreach (var permission in permissionGranted.PermissionsToGrant)
                    {
                        permissionList.Add(new CondensedPermissionDetail
                        {
                            PermissionId = permission.Key,
                            TimeStamp = permission.Value.RequestDate,
                            PermissionName = _context.PermissionDetailProjection
                                .Where(a => a.PermissionId == permission.Key)
                                .Select(p => p.Name).Single()
                        });
                    }

                    approverActivity.PermissionList = JsonConvert.SerializeObject(permissionList);

                    foreach (var prop in _context.Entry(approverActivity).Properties)
                    {
                        prop.IsModified = true;
                    }
                    //_context.Entry(approverActivity).Property(a => a.ForLastName).IsModified = true;
                    //_context.Entry(approverActivity).Property(a => a.ForFirstName).IsModified = true;
                    //_context.Entry(approverActivity).Property(a => a.ForId).IsModified = true;
                    //_context.Entry(approverActivity).Property(a => a.PermissionList).IsModified = true;
                    //_context.Entry(approverActivity).Property(a => a.TruncatedEventType);
                    _context.ApproverActivityProjection.Update(approverActivity);
                    break;
                case UserPermissionRevokedEvent permissionRevoked:
                    _context.ApproverActivityProjection.Add(approverActivity);

                    approverActivity.ForId = permissionRevoked.ForId;

                    approverActivity.ById = permissionRevoked.ById;

                    approverActivity.ForFirstName = _context.UserDetailProjection
                        .Where(a => a.UserId == permissionRevoked.ForId)
                        .Select(p => p.FirstName).ToString();

                    approverActivity.ForLastName = _context.UserDetailProjection
                        .Where(a => a.UserId == permissionRevoked.ForId)
                        .Select(p => p.LastName).ToString();

                    approverActivity.TruncatedEventType = "Revoked";

                    foreach (var permission in permissionRevoked.PermissionsToRevoke)
                    {
                        permissionList.Add(new CondensedPermissionDetail
                        {
                            PermissionId = permission.Key,
                            TimeStamp = permission.Value.RequestDate,
                            PermissionName = _context.PermissionDetailProjection
                                .Where(a => a.PermissionId == permission.Key)
                                .Select(p => p.Name).ToString()
                        });
                    }

                    PermissionList = JsonConvert.SerializeObject(permissionList);

                    _context.ApproverActivityProjection.Update(approverActivity);
                    break;
                case UserPermissionRequestDeniedEvent permissionDenied:
                    _context.ApproverActivityProjection.Add(approverActivity);

                    approverActivity.ForId = permissionDenied.ForId;

                    approverActivity.ById = permissionDenied.ById;

                    approverActivity.ForFirstName = _context.UserDetailProjection
                        .Where(a => a.UserId == permissionDenied.ForId)
                        .Select(p => p.FirstName).ToString();

                    approverActivity.ForLastName = _context.UserDetailProjection
                        .Where(a => a.UserId == permissionDenied.ForId)
                        .Select(p => p.LastName).ToString();

                    approverActivity.TruncatedEventType = "Denied";

                    foreach (var permission in permissionDenied.PermissionsToDeny)
                    {
                        permissionList.Add(new CondensedPermissionDetail
                        {
                            PermissionId = permission.Key,
                            TimeStamp = permission.Value.RequestDate,
                            PermissionName = _context.PermissionDetailProjection
                                .Where(a => a.PermissionId == permission.Key)
                                .Select(p => p.Name).ToString()
                        });
                    }

                    PermissionList = JsonConvert.SerializeObject(permissionList);
                    _context.ApproverActivityProjection.Update(approverActivity);
                    break;
            }
            await _context.SaveChangesAsync();
        }
    }
}