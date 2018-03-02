﻿using System;
using System.Collections.Generic;
using System.Text;
using UrbanSpork.Common.DataTransferObjects.Permission;
using UrbanSpork.CQRS.Events;
using UrbanSpork.CQRS.WriteModel.Command;

namespace UrbanSpork.WriteModel.Commands
{
    public class EnablePermissionCommand : ICommand<PermissionDTO>
    {
        public Guid Id;

        public EnablePermissionCommand(Guid id)
        {
            Id = id;
        }
    }
}
